using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WSB_Racing.Models;

namespace WSB_Racing.Data;

public class WSBRacingDbContext : IdentityDbContext<Personal>
{
    public WSBRacingDbContext(DbContextOptions<WSBRacingDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Address> Addresses { get; set; }
    public virtual DbSet<Bike> Bikes { get; set; }
    public virtual DbSet<Brand> Brands { get; set; }
    public virtual DbSet<Contact> Contacts { get; set; }
    public virtual DbSet<Country> Countries { get; set; }
    public virtual DbSet<Cup> Cups { get; set; }
    public virtual DbSet<Customer> Customers { get; set; }
    public virtual DbSet<CustomerBike> CustomerBikes { get; set; }
    public virtual DbSet<CustomerCup> CustomerCups { get; set; }
    public virtual DbSet<CustomerEvent> CustomerEvents { get; set; }
    public virtual DbSet<Event> Events { get; set; }
    public virtual DbSet<Transponder> Transponders { get; set; }
    public DbSet<Personal> Personals { get; set; }
    public DbSet<Position> Positions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasAnnotation("Relational:DisableMigrationsLocking", true);

        modelBuilder.Entity<Address>().ToTable("address");
        modelBuilder.Entity<Bike>().ToTable("bike");
        modelBuilder.Entity<Brand>().ToTable("brand");
        modelBuilder.Entity<Contact>().ToTable("contact");
        modelBuilder.Entity<Country>().ToTable("country");
        modelBuilder.Entity<Cup>().ToTable("cup");
        modelBuilder.Entity<Customer>().ToTable("customer");
        modelBuilder.Entity<CustomerBike>().ToTable("customer_bike");
        modelBuilder.Entity<CustomerCup>().ToTable("customer_cup");
        modelBuilder.Entity<CustomerEvent>().ToTable("customer_event");
        modelBuilder.Entity<Event>().ToTable("event");
        modelBuilder.Entity<Transponder>().ToTable("transponder");

        modelBuilder.Entity<Address>()
            .HasOne(a => a.Country)
            .WithMany(c => c.Addresses)
            .HasForeignKey(a => a.Countryid)
            .OnDelete(DeleteBehavior.Restrict); 

        modelBuilder.Entity<Address>()
            .HasOne(a => a.Customer)
            .WithMany(c => c.Addresses)
            .HasForeignKey(a => a.Customerid)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Bike>()
            .HasOne(b => b.Brand)
            .WithMany(brand => brand.Bikes)
            .HasForeignKey(b => b.Brandid)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Contact>()
            .HasOne(c => c.Customer)
            .WithMany(cust => cust.Contacts)
            .HasForeignKey(c => c.Customerid)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CustomerBike>()
            .HasOne(cb => cb.Customer)
            .WithMany(c => c.CustomerBikes)
            .HasForeignKey(cb => cb.Customerid)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CustomerBike>()
            .HasOne(cb => cb.Bike)
            .WithMany(b => b.CustomerBikes)
            .HasForeignKey(cb => cb.Bikeid)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CustomerCup>()
            .HasOne(cc => cc.Customer)
            .WithMany(c => c.CustomerCups)
            .HasForeignKey(cc => cc.Customerid)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CustomerCup>()
            .HasOne(cc => cc.Cup)
            .WithMany(c => c.CustomerCups)
            .HasForeignKey(cc => cc.Cupid)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CustomerEvent>()
            .HasOne(ce => ce.Customer)
            .WithMany(c => c.CustomerEvents)
            .HasForeignKey(ce => ce.Customerid)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CustomerEvent>()
            .HasOne(ce => ce.Event)
            .WithMany(e => e.CustomerEvents)
            .HasForeignKey(ce => ce.Eventid)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CustomerEvent>()
            .HasOne(ce => ce.Bike)
            .WithMany(b => b.CustomerEvents)
            .HasForeignKey(ce => ce.Bikeid)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CustomerEvent>()
            .HasOne(ce => ce.Transponder)
            .WithMany(t => t.CustomerEvents)
            .HasForeignKey(ce => ce.Transponderid)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
