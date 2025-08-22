using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WSB_Management.Components;
using WSB_Management.Data;
using WSB_Management.Models;

namespace WSB_Management;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var connectionString = builder.Configuration.GetConnectionString("MySQLConnection");
        builder.Services.AddDbContext<WSBRacingDbContext>(options =>
            options.UseMySQL(connectionString ?? throw new InvalidOperationException("MySQLConnection not configured.")));

        builder.Services.AddBlazorBootstrap();
        builder.Services.AddIdentity<Personal, IdentityRole<int>>(options =>
        {
            options.SignIn.RequireConfirmedAccount = false;
        })
        .AddEntityFrameworkStores<WSBRacingDbContext>()
        .AddDefaultTokenProviders();

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/login";
            options.AccessDeniedPath = "/login";
        });

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        builder.Services.AddAntiforgery();

        var app = builder.Build();

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
