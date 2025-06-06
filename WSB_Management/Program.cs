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

            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();
            /*
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Cookies";
                options.DefaultChallengeScheme = "Cookies";
            })
            .AddCookie("Cookies", options =>
            {
                options.Cookie.Name = "WSB_Management.Auth";
                options.ExpireTimeSpan = TimeSpan.FromDays(1);
                options.SlidingExpiration = true;
            });

            builder.Services.AddScoped<ProtectedLocalStorage>();
            builder.Services.AddScoped<LoggedInUserService>();
            builder.Services.AddScoped<WebsiteAuthenticator>();
            builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
                sp.GetRequiredService<WebsiteAuthenticator>());
            */
            builder.Services.AddAntiforgery();

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

            /*
            app.UseAuthentication();
            app.UseAuthorization();
            */
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}