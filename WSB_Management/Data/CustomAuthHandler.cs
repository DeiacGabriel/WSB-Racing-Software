using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace WSB_Management.Data
{
    public class CustomAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly CustomAuthStateProvider _authStateProvider;

        public CustomAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            CustomAuthStateProvider authStateProvider)
            : base(options, logger, encoder, clock)
        {
            _authStateProvider = authStateProvider;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            return AuthenticateResult.Success(new AuthenticationTicket(
                authState.User,
                "CustomAuth"));
        }
    }

    public static class CustomAuthExtensions
    {
        public static AuthenticationBuilder AddCustomAuth(this AuthenticationBuilder builder)
        {
            return builder.AddScheme<AuthenticationSchemeOptions, CustomAuthHandler>(
                "CustomAuth", null);
        }
    }
}