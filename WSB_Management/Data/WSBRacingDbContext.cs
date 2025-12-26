using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WSB_Management.Models;

namespace WSB_Management.Data;
public class WSBRacingDbContext : IdentityDbContext<Personal, Position, int>
{
    public DbSet<Address> Addresses { get; set; }
    public DbSet<Bike> Bikes { get; set; }
    public DbSet<BikeType> BikeTypes { get; set; }
    public DbSet<Brand> Brands { get; set; }
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<Country> Countries { get; set; }
    public DbSet<Cup> Cups { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<CostumerEvent> CustomerEvents { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Klasse> Klasses { get; set; }
    public DbSet<Transponder> Transponders { get; set; }
    public DbSet<Personal> Personals { get; set; }
    public DbSet<Position> Positions { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<Gruppe> Gruppes { get; set; }
    
    public WSBRacingDbContext(DbContextOptions<WSBRacingDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Map all DateTime properties to 'timestamp without time zone'
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            if (clrType == null) continue;

            var dateProps = clrType.GetProperties()
                .Where(p => p.PropertyType == typeof(DateTime));

            foreach (var prop in dateProps)
            {
                builder.Entity(clrType)
                    .Property(prop.Name)
                    .HasColumnType("timestamp without time zone");
            }
        }
    }
}

