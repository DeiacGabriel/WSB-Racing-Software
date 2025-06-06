using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using WSB_Management.Data;
using WSB_Management.Models;

namespace WSB_Management.Components.Pages
{
    public partial class Login
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = string.Empty;
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
        private string ErrorMessage = string.Empty;
        [Inject]
        private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
        [Inject]
        private NavigationManager Navigation { get; set; } = default!;
        private async Task HandleLogin()
        {
            ErrorMessage = string.Empty;

            if (AuthenticationStateProvider is WebsiteAuthenticator authenticator)
            {
                var success = await authenticator.LoginAsync(Username, Password);

                if (success)
                {
                    Navigation.NavigateTo("/Home", forceLoad: true);
                }
                else
                {
                    ErrorMessage = "Invalid username or password";
                }
            }
        }
    }
}