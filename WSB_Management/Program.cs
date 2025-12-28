using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using WSB_Management.Components;
using WSB_Management.Data;
using WSB_Management.Models;
using WSB_Management.Services;

namespace WSB_Management;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var connectionString = builder.Configuration.GetConnectionString("PostgresConnection");
        builder.Services.AddDbContext<WSBRacingDbContext>(options =>
            options.UseNpgsql(connectionString ?? throw new InvalidOperationException("PostgresConnection not configured.")));
        builder.Services.AddDbContextFactory<WSBRacingDbContext>(
            options => options.UseNpgsql(connectionString ?? throw new InvalidOperationException("PostgresConnection not configured.")),
            ServiceLifetime.Scoped);
            
        builder.Services.AddBlazorBootstrap();
        builder.Services.AddMudServices();
        
        // Services
        builder.Services.AddScoped<EventService>();
        builder.Services.AddScoped<CustomerService>();
        builder.Services.AddScoped<RaceService>();
        builder.Services.AddScoped<MasterDataService>();
        
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
        app.MapStaticAssets();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseAntiforgery();

        app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

        app.Run();
    }
}
