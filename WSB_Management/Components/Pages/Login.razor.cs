using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using WSB_Management.Models;

namespace WSB_Management.Components.Pages
{
    public partial class Login
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;

        protected string ErrorMessage { get; set; } = string.Empty;

        [Inject] private SignInManager<Personal> SignInManager { get; set; } = default!;
        [Inject] private NavigationManager Navigation { get; set; } = default!;
        [Inject] private ILogger<Login> Logger { get; set; } = default!;

        protected async Task HandleLogin()
        {
            var result = await SignInManager.PasswordSignInAsync(
                userName: Username,
                password: Password,
                isPersistent: false,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                Logger.LogInformation("User {Username} logged in successfully.", Username);
                Navigation.NavigateTo("/");
            }
            else
            {
                ErrorMessage = "Login fehlgeschlagen. Bitte überprüfe deine Eingaben.";
                Logger.LogWarning("Login failed for user {Username}.", Username);
            }
        }
    }
}
