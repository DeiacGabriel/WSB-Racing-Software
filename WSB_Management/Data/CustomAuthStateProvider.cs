using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Claims;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ProtectedSessionStorage _sessionStorage;
    private ClaimsPrincipal _currentUser = new ClaimsPrincipal(new ClaimsIdentity());

    public CustomAuthStateProvider(ProtectedSessionStorage sessionStorage)
    {
        _sessionStorage = sessionStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (_sessionStorage is null)
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        try
        {
            var storedUser = await _sessionStorage.GetAsync<ClaimsPrincipal>("authUser");
            _currentUser = storedUser.Success ? storedUser.Value : new ClaimsPrincipal(new ClaimsIdentity());
            return new AuthenticationState(_currentUser);
        }
        catch (InvalidOperationException)
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    public async void MarkUserAsAuthenticated(string id, string username, string role)
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, id),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role)
        }, "CustomAuth");

        _currentUser = new ClaimsPrincipal(identity);

        var userSession = new UserSession
        {
            Id = id,
            Username = username,
            Role = role
        };

        await _sessionStorage.SetAsync("authUser", userSession);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async void MarkUserAsLoggedOut()
    {
        _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
        await _sessionStorage.DeleteAsync("authUser");
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
class UserSession
{
    public string Id { get; set; }
    public string Username { get; set; }
    public string Role { get; set; }
}
