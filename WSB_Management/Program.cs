using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.EntityFrameworkCore;
using WSB_Management.Components;
using WSB_Management.Data;

namespace WSB_Management
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Database configuration
            var connectionString = builder.Configuration.GetConnectionString("MySQLConnection");
            builder.Services.AddDbContext<WSBRacingDbContext>(options =>
                options.UseMySQL(connectionString ?? throw new InvalidOperationException("MySQLConnection not configured.")));

            // Razor Components and interactivity
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            // Authentication and Authorization
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = "CustomAuth";
                options.DefaultChallengeScheme = "CustomAuth";
                options.DefaultAuthenticateScheme = "CustomAuth";
            })
            .AddCustomAuth();

            builder.Services.AddAuthorization();
            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
            builder.Services.AddScoped<CustomAuthStateProvider>();
            builder.Services.AddScoped<ProtectedSessionStorage>();


            builder.Services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-CSRF-TOKEN";
            });

            builder.Services.AddHttpContextAccessor();

            var app = builder.Build();

            // Database migration
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WSBRacingDbContext>();
                db.Database.Migrate();
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}