using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using WSB_Management.Data;
using WSB_Management.Models;

namespace WSB_Management.Services
{
    public class AuthService
    {
        private readonly WSBRacingDbContext _context;
        private readonly ILogger<AuthService> _logger;

        public AuthService(WSBRacingDbContext context, ILogger<AuthService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<AdminUser?> AuthenticateAsync(string email, string password)
        {
            var user = await _context.AdminUsers
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.IsActive);

            if (user == null)
            {
                _logger.LogWarning("Login fehlgeschlagen: Benutzer {Email} nicht gefunden oder inaktiv", email);
                return null;
            }

            if (!VerifyPassword(password, user.PasswordHash))
            {
                _logger.LogWarning("Login fehlgeschlagen: Falsches Passwort für {Email}", email);
                return null;
            }

            // Update last login (UTC für PostgreSQL timestamp with time zone)
            user.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Benutzer {Email} erfolgreich angemeldet", email);
            return user;
        }

        public async Task<List<AdminUser>> GetAllUsersAsync()
        {
            return await _context.AdminUsers.OrderBy(u => u.LastName).ThenBy(u => u.FirstName).ToListAsync();
        }

        public async Task<AdminUser?> GetUserByIdAsync(long id)
        {
            return await _context.AdminUsers.FindAsync(id);
        }

        public async Task<AdminUser?> GetUserByEmailAsync(string email)
        {
            return await _context.AdminUsers.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<bool> CreateUserAsync(AdminUser user, string password)
        {
            // Check if email already exists
            if (await _context.AdminUsers.AnyAsync(u => u.Email.ToLower() == user.Email.ToLower()))
            {
                _logger.LogWarning("Benutzer mit E-Mail {Email} existiert bereits", user.Email);
                return false;
            }

            user.PasswordHash = HashPassword(password);
            user.CreatedAt = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

            _context.AdminUsers.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Neuer Benutzer {Email} erstellt", user.Email);
            return true;
        }

        public async Task<bool> UpdateUserAsync(AdminUser user, string? newPassword = null)
        {
            var existingUser = await _context.AdminUsers.FindAsync(user.Id);
            if (existingUser == null)
            {
                return false;
            }

            // Check if email is taken by another user
            if (await _context.AdminUsers.AnyAsync(u => u.Email.ToLower() == user.Email.ToLower() && u.Id != user.Id))
            {
                _logger.LogWarning("E-Mail {Email} wird bereits von einem anderen Benutzer verwendet", user.Email);
                return false;
            }

            existingUser.Username = user.Username;
            existingUser.Email = user.Email;
            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.IsActive = user.IsActive;
            existingUser.IsSuperAdmin = user.IsSuperAdmin;

            if (!string.IsNullOrEmpty(newPassword))
            {
                existingUser.PasswordHash = HashPassword(newPassword);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Benutzer {Email} aktualisiert", user.Email);
            return true;
        }

        public async Task<bool> DeleteUserAsync(long id)
        {
            var user = await _context.AdminUsers.FindAsync(id);
            if (user == null)
            {
                return false;
            }

            // Prevent deleting the last super admin
            if (user.IsSuperAdmin)
            {
                var superAdminCount = await _context.AdminUsers.CountAsync(u => u.IsSuperAdmin && u.Id != id);
                if (superAdminCount == 0)
                {
                    _logger.LogWarning("Kann den letzten Super-Admin nicht löschen");
                    return false;
                }
            }

            _context.AdminUsers.Remove(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Benutzer {Email} gelöscht", user.Email);
            return true;
        }

        public async Task<bool> ChangePasswordAsync(long userId, string currentPassword, string newPassword)
        {
            var user = await _context.AdminUsers.FindAsync(userId);
            if (user == null)
            {
                return false;
            }

            if (!VerifyPassword(currentPassword, user.PasswordHash))
            {
                _logger.LogWarning("Passwortänderung fehlgeschlagen: Aktuelles Passwort falsch für {Email}", user.Email);
                return false;
            }

            user.PasswordHash = HashPassword(newPassword);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Passwort für Benutzer {Email} geändert", user.Email);
            return true;
        }

        public async Task EnsureDefaultAdminExistsAsync()
        {
            if (!await _context.AdminUsers.AnyAsync())
            {
                var defaultAdmin = new AdminUser
                {
                    Username = "admin",
                    Email = "admin@wsb-sport.at",
                    FirstName = "System",
                    LastName = "Administrator",
                    IsActive = true,
                    IsSuperAdmin = true,
                    CreatedAt = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified)
                };
                defaultAdmin.PasswordHash = HashPassword("admin123"); // Default password, should be changed!

                _context.AdminUsers.Add(defaultAdmin);
                await _context.SaveChangesAsync();

                _logger.LogWarning("Standard-Admin-Benutzer erstellt: admin@wsb-sport.at / admin123 - Bitte Passwort ändern!");
            }
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private static bool VerifyPassword(string password, string hash)
        {
            var passwordHash = HashPassword(password);
            return passwordHash == hash;
        }
    }
}
