using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using WSB_Management.Data;

namespace WSB_Management.Components.Pages
{
    public partial class Login
    {
        [Inject]
        private NavigationManager NavigationManager { get; set; }

        [Inject]
        private AuthenticationStateProvider AuthProvider { get; set; }

        [Inject]
        private WSBRacingDbContext Context { get; set; }

        [Required(ErrorMessage = "Username ist erforderlich")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Passwort ist erforderlich")]
        public string Password { get; set; } = string.Empty;

        protected string passwordInputType = "password";
        protected string togglePasswordIcon = "👁️";
        protected string ErrorMessage = string.Empty;

        private CustomAuthStateProvider CustomAuthProvider => (CustomAuthStateProvider)AuthProvider;
        private async Task LoginUser()
        {
            ErrorMessage = string.Empty;

            try
            {
                // Replace this with your actual user validation logic
                var user = await Context.Personals.FirstOrDefaultAsync(u =>
                    u.Username == Username && u.Password == Password);

                if (user != null)
                {
                    CustomAuthProvider.MarkUserAsAuthenticated(
                        user.Id.ToString(),
                        user.Username,
                        user.Position);

                    NavigationManager.NavigateTo("/Home", true);
                }
                else
                {
                    ErrorMessage = "Ungültige Anmeldedaten";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Ein Fehler ist aufgetreten: " + ex.Message;
            }
        }
    }
}