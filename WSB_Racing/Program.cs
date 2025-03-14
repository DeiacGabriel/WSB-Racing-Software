using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WSB_Racing.Data;
using WSB_Racing.Models;

namespace WSB_Racing
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("MySQLConnection");
            builder.Services.AddDbContext<WSBRacingDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));



            // Add Identity services
            builder.Services.AddIdentity<Personal, Position>()
                .AddEntityFrameworkStores<WSBRacingDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication(); 
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Login}/{id?}");

            app.Run();
        }
    }
}
