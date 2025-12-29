using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Claims;
using WSB_Management.Models;

namespace WSB_Management.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ProtectedSessionStorage _sessionStorage;
        private readonly AuthService _authService;
        private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        public CustomAuthStateProvider(ProtectedSessionStorage sessionStorage, AuthService authService)
        {
            _sessionStorage = sessionStorage;
            _authService = authService;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var userSessionResult = await _sessionStorage.GetAsync<UserSession>("UserSession");
                var userSession = userSessionResult.Success ? userSessionResult.Value : null;

                if (userSession == null)
                {
                    return new AuthenticationState(_anonymous);
                }

                var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userSession.UserId.ToString()),
                    new Claim(ClaimTypes.Name, userSession.UserName),
                    new Claim(ClaimTypes.Email, userSession.Email),
                    new Claim("FullName", userSession.FullName),
                    new Claim(ClaimTypes.Role, userSession.IsSuperAdmin ? "SuperAdmin" : "Admin")
                }, "CustomAuth"));

                return new AuthenticationState(claimsPrincipal);
            }
            catch
            {
                return new AuthenticationState(_anonymous);
            }
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            var user = await _authService.AuthenticateAsync(email, password);
            if (user == null)
            {
                return false;
            }

            var userSession = new UserSession
            {
                UserId = user.Id,
                UserName = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                IsSuperAdmin = user.IsSuperAdmin
            };

            await _sessionStorage.SetAsync("UserSession", userSession);

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("FullName", user.FullName),
                new Claim(ClaimTypes.Role, user.IsSuperAdmin ? "SuperAdmin" : "Admin")
            }, "CustomAuth"));

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
            return true;
        }

        public async Task LogoutAsync()
        {
            await _sessionStorage.DeleteAsync("UserSession");
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
        }

        public async Task<UserSession?> GetCurrentUserAsync()
        {
            try
            {
                var result = await _sessionStorage.GetAsync<UserSession>("UserSession");
                return result.Success ? result.Value : null;
            }
            catch
            {
                return null;
            }
        }
    }

    public class UserSession
    {
        public long UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public bool IsSuperAdmin { get; set; }
    }
}
