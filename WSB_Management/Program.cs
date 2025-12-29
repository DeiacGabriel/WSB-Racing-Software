using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using WSB_Management.Components;
using WSB_Management.Data;
using WSB_Management.Models;
using WSB_Management.Services;

namespace WSB_Management;

public class Program
{
    public static async Task Main(string[] args)
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
        builder.Services.AddScoped<AuthService>();
        builder.Services.AddScoped<CustomAuthStateProvider>();
        builder.Services.AddScoped<AuthenticationStateProvider>(provider => 
            provider.GetRequiredService<CustomAuthStateProvider>());
        
        // Cookie Authentication (minimal, nur für Authorization Challenge)
        builder.Services.AddAuthentication("Cookies")
            .AddCookie("Cookies", options =>
            {
                options.LoginPath = "/login";
                options.LogoutPath = "/logout";
                options.AccessDeniedPath = "/login";
            });
        
        builder.Services.AddAuthorization();

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        builder.Services.AddAntiforgery();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WSBRacingDbContext>();
            db.Database.Migrate();
            
            await SeedMasterDataAsync(db);
            
            var authService = scope.ServiceProvider.GetRequiredService<AuthService>();
            await authService.EnsureDefaultAdminExistsAsync();
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

        await app.RunAsync();
    }
    
    /// <summary>
    /// Erstellt Testdaten für alle Stammdaten wenn noch keine vorhanden sind
    /// </summary>
    private static async Task SeedMasterDataAsync(WSBRacingDbContext db)
    {
        // Helper für PostgreSQL-kompatible DateTime
        static DateTime LocalDateTime(int year, int month, int day) 
            => DateTime.SpecifyKind(new DateTime(year, month, day), DateTimeKind.Unspecified);
        
        // === LÄNDER ===
        if (!db.Countries.Any())
        {
            var countries = new List<Country>
            {
                new() { Shorttxt = "AT", Longtxt = "Österreich" },
                new() { Shorttxt = "DE", Longtxt = "Deutschland" },
                new() { Shorttxt = "CH", Longtxt = "Schweiz" },
                new() { Shorttxt = "IT", Longtxt = "Italien" },
                new() { Shorttxt = "CZ", Longtxt = "Tschechien" },
                new() { Shorttxt = "SK", Longtxt = "Slowakei" },
                new() { Shorttxt = "HU", Longtxt = "Ungarn" },
                new() { Shorttxt = "SI", Longtxt = "Slowenien" },
                new() { Shorttxt = "HR", Longtxt = "Kroatien" },
                new() { Shorttxt = "PL", Longtxt = "Polen" },
                new() { Shorttxt = "NL", Longtxt = "Niederlande" },
                new() { Shorttxt = "BE", Longtxt = "Belgien" },
                new() { Shorttxt = "FR", Longtxt = "Frankreich" },
                new() { Shorttxt = "ES", Longtxt = "Spanien" },
                new() { Shorttxt = "GB", Longtxt = "Großbritannien" }
            };
            db.Countries.AddRange(countries);
            await db.SaveChangesAsync();
        }

        // === MARKEN (Brands) ===
        if (!db.Brands.Any())
        {
            var brands = new List<Brand>
            {
                new() { Name = "Honda" },
                new() { Name = "Yamaha" },
                new() { Name = "Kawasaki" },
                new() { Name = "Suzuki" },
                new() { Name = "Ducati" },
                new() { Name = "BMW" },
                new() { Name = "KTM" },
                new() { Name = "Aprilia" },
                new() { Name = "Triumph" },
                new() { Name = "MV Agusta" },
                new() { Name = "Husqvarna" },
                new() { Name = "Harley-Davidson" }
            };
            db.Brands.AddRange(brands);
            await db.SaveChangesAsync();
        }

        // === KLASSEN ===
        if (!db.Klasses.Any())
        {
            var klasses = new List<Klasse>
            {
                new() { Bezeichnung = "Supersport 300" },
                new() { Bezeichnung = "Supersport 600" },
                new() { Bezeichnung = "Superbike 1000" },
                new() { Bezeichnung = "Naked Bike" },
                new() { Bezeichnung = "Enduro" },
                new() { Bezeichnung = "Supermoto" },
                new() { Bezeichnung = "Vintage" },
                new() { Bezeichnung = "E-Bike" }
            };
            db.Klasses.AddRange(klasses);
            await db.SaveChangesAsync();
        }

        // === MOTORRADTYPEN (BikeTypes) ===
        if (!db.BikeTypes.Any())
        {
            var honda = db.Brands.First(b => b.Name == "Honda");
            var yamaha = db.Brands.First(b => b.Name == "Yamaha");
            var kawasaki = db.Brands.First(b => b.Name == "Kawasaki");
            var suzuki = db.Brands.First(b => b.Name == "Suzuki");
            var ducati = db.Brands.First(b => b.Name == "Ducati");
            var bmw = db.Brands.First(b => b.Name == "BMW");
            var ktm = db.Brands.First(b => b.Name == "KTM");
            var aprilia = db.Brands.First(b => b.Name == "Aprilia");

            var ss300 = db.Klasses.First(k => k.Bezeichnung == "Supersport 300");
            var ss600 = db.Klasses.First(k => k.Bezeichnung == "Supersport 600");
            var sbk1000 = db.Klasses.First(k => k.Bezeichnung == "Superbike 1000");
            var naked = db.Klasses.First(k => k.Bezeichnung == "Naked Bike");

            var bikeTypes = new List<BikeType>
            {
                // Supersport 300
                new() { Name = "CBR300R", BrandId = honda.Id, KlasseId = ss300.Id },
                new() { Name = "YZF-R3", BrandId = yamaha.Id, KlasseId = ss300.Id },
                new() { Name = "Ninja 400", BrandId = kawasaki.Id, KlasseId = ss300.Id },
                new() { Name = "RC 390", BrandId = ktm.Id, KlasseId = ss300.Id },
                
                // Supersport 600
                new() { Name = "CBR600RR", BrandId = honda.Id, KlasseId = ss600.Id },
                new() { Name = "YZF-R6", BrandId = yamaha.Id, KlasseId = ss600.Id },
                new() { Name = "ZX-6R", BrandId = kawasaki.Id, KlasseId = ss600.Id },
                new() { Name = "GSX-R600", BrandId = suzuki.Id, KlasseId = ss600.Id },
                new() { Name = "RS 660", BrandId = aprilia.Id, KlasseId = ss600.Id },
                
                // Superbike 1000
                new() { Name = "CBR1000RR-R Fireblade", BrandId = honda.Id, KlasseId = sbk1000.Id },
                new() { Name = "YZF-R1", BrandId = yamaha.Id, KlasseId = sbk1000.Id },
                new() { Name = "ZX-10R", BrandId = kawasaki.Id, KlasseId = sbk1000.Id },
                new() { Name = "GSX-R1000", BrandId = suzuki.Id, KlasseId = sbk1000.Id },
                new() { Name = "Panigale V4", BrandId = ducati.Id, KlasseId = sbk1000.Id },
                new() { Name = "S1000RR", BrandId = bmw.Id, KlasseId = sbk1000.Id },
                new() { Name = "RSV4", BrandId = aprilia.Id, KlasseId = sbk1000.Id },
                
                // Naked Bike
                new() { Name = "CB1000R", BrandId = honda.Id, KlasseId = naked.Id },
                new() { Name = "MT-09", BrandId = yamaha.Id, KlasseId = naked.Id },
                new() { Name = "Z900", BrandId = kawasaki.Id, KlasseId = naked.Id },
                new() { Name = "Monster", BrandId = ducati.Id, KlasseId = naked.Id },
                new() { Name = "S1000R", BrandId = bmw.Id, KlasseId = naked.Id },
                new() { Name = "Duke 890", BrandId = ktm.Id, KlasseId = naked.Id }
            };
            db.BikeTypes.AddRange(bikeTypes);
            await db.SaveChangesAsync();
        }

        // === GRUPPEN ===
        if (!db.Gruppes.Any())
        {
            var gruppen = new List<Gruppe>
            {
                new() { Name = "Anfänger", MaxTimelap = TimeSpan.FromMinutes(2).Add(TimeSpan.FromSeconds(30)) },
                new() { Name = "Fortgeschrittene", MaxTimelap = TimeSpan.FromMinutes(2) },
                new() { Name = "Schnelle", MaxTimelap = TimeSpan.FromMinutes(1).Add(TimeSpan.FromSeconds(45)) },
                new() { Name = "Experten", MaxTimelap = TimeSpan.FromMinutes(1).Add(TimeSpan.FromSeconds(30)) },
                new() { Name = "Profi", MaxTimelap = TimeSpan.FromMinutes(1).Add(TimeSpan.FromSeconds(15)) },
                new() { Name = "Rennfahrer", MaxTimelap = null }
            };
            db.Gruppes.AddRange(gruppen);
            await db.SaveChangesAsync();
        }

        // === TRANSPONDER ===
        if (!db.Transponders.Any())
        {
            var transponders = new List<Transponder>();
            for (int i = 1; i <= 50; i++)
            {
                transponders.Add(new Transponder 
                { 
                    Bezeichung = $"Transponder {i:D3}", 
                    Number = $"TR-{1000 + i}" 
                });
            }
            db.Transponders.AddRange(transponders);
            await db.SaveChangesAsync();
        }

        // === TEAMS ===
        if (!db.Teams.Any())
        {
            var teams = new List<Team>
            {
                new() { Name = "WSB Racing Team" },
                new() { Name = "Red Bull Racing" },
                new() { Name = "Monster Energy Racing" },
                new() { Name = "Alpinestars Team" },
                new() { Name = "Dainese Racing" },
                new() { Name = "Pirelli Racing Team" },
                new() { Name = "Bridgestone Racing" },
                new() { Name = "Privateers United" },
                new() { Name = "Track Day Heroes" },
                new() { Name = "Weekend Warriors" }
            };
            db.Teams.AddRange(teams);
            await db.SaveChangesAsync();
        }

        // === BIKES (Fahrzeuge ohne Typ für allgemeine Nutzung) ===
        if (!db.Bikes.Any())
        {
            var bikeTypes = db.BikeTypes.ToList();
            var bikes = new List<Bike>();
            
            if (bikeTypes.Any())
            {
                foreach (var bikeType in bikeTypes.Take(10))
                {
                    bikes.Add(new Bike { BikeTypeId = bikeType.Id });
                }
            }
            else
            {
                // Falls keine BikeTypes existieren, erstelle Bikes ohne Typ
                for (int i = 0; i < 5; i++)
                {
                    bikes.Add(new Bike());
                }
            }
            db.Bikes.AddRange(bikes);
            await db.SaveChangesAsync();
        }

        // === BEISPIELKUNDEN ===
        if (!db.Customers.Any())
        {
            var austria = db.Countries.FirstOrDefault(c => c.Shorttxt == "AT");
            var germany = db.Countries.FirstOrDefault(c => c.Shorttxt == "DE");
            var gruppe1 = db.Gruppes.FirstOrDefault();
            var gruppe2 = db.Gruppes.Skip(1).FirstOrDefault();
            var gruppe3 = db.Gruppes.Skip(2).FirstOrDefault();
            var bike1 = db.Bikes.FirstOrDefault();
            var bike2 = db.Bikes.Skip(1).FirstOrDefault();
            var transponder1 = db.Transponders.FirstOrDefault();
            var transponder2 = db.Transponders.Skip(1).FirstOrDefault();
            var team1 = db.Teams.FirstOrDefault();

            // Prüfe ob alle benötigten Stammdaten vorhanden sind
            if (austria == null || germany == null || gruppe1 == null || bike1 == null || transponder1 == null)
            {
                Console.WriteLine("Kunden-Testdaten konnten nicht erstellt werden - fehlende Stammdaten");
                Console.WriteLine("Stammdaten erfolgreich initialisiert");
                return;
            }

            var customers = new List<Customer>
            {
                new()
                {
                    Title = "Herr",
                    Contact = new Contact { Firstname = "Max", Surname = "Mustermann", Phonenumber = "+43 664 1234567" },
                    Birthdate = LocalDateTime(1985, 5, 15),
                    Mail = "max.mustermann@example.com",
                    Newsletter = true,
                    Validfrom = LocalDateTime(2024, 1, 1),
                    Startnumber = "42",
                    Address = new Address { Street = "Hauptstraße 1", Zip = "1010", City = "Wien", Country = austria },
                    NotfallContact = new Contact { Firstname = "Anna", Surname = "Mustermann", Phonenumber = "+43 664 7654321" },
                    Sponsor = "Red Bull",
                    UID = "ATU12345678",
                    Guthaben = 250.00,
                    LastGuthabenAdd = LocalDateTime(2024, 12, 1),
                    LastGuthabenAddNumber = 100.00,
                    GuthabenComment = "Weihnachtsbonus",
                    Preisgeld = 500.00,
                    Gratisfahrer = 2,
                    Schurke = 0,
                    VerzichtOk = true,
                    Gruppe = gruppe3 ?? gruppe1,
                    letzteBuchung = LocalDateTime(2024, 12, 20),
                    letzterEinkauf = LocalDateTime(2024, 12, 15),
                    Bike = bike1,
                    Transponder = transponder1,
                    BestTime = TimeSpan.FromMinutes(1).Add(TimeSpan.FromSeconds(42)),
                    S8S = "Ja",
                    Speeddays = "5"
                },
                new()
                {
                    Title = "Frau",
                    Contact = new Contact { Firstname = "Maria", Surname = "Musterfrau", Phonenumber = "+43 660 9876543" },
                    Birthdate = LocalDateTime(1990, 8, 22),
                    Mail = "maria.musterfrau@example.com",
                    Newsletter = true,
                    Validfrom = LocalDateTime(2024, 3, 1),
                    Startnumber = "88",
                    Address = new Address { Street = "Ringstraße 5", Zip = "8010", City = "Graz", Country = austria },
                    NotfallContact = new Contact { Firstname = "Peter", Surname = "Musterfrau", Phonenumber = "+43 660 1234567" },
                    Sponsor = "",
                    UID = "",
                    Guthaben = 100.00,
                    LastGuthabenAdd = LocalDateTime(2024, 11, 15),
                    LastGuthabenAddNumber = 50.00,
                    GuthabenComment = "",
                    Preisgeld = 0,
                    Gratisfahrer = 0,
                    Schurke = 0,
                    VerzichtOk = true,
                    Gruppe = gruppe2 ?? gruppe1,
                    letzteBuchung = LocalDateTime(2024, 12, 10),
                    letzterEinkauf = LocalDateTime(2024, 12, 5),
                    Bike = bike2 ?? bike1,
                    Transponder = transponder2 ?? transponder1,
                    BestTime = TimeSpan.FromMinutes(1).Add(TimeSpan.FromSeconds(55)),
                    S8S = "Nein",
                    Speeddays = "3"
                },
                new()
                {
                    Title = "Herr",
                    Contact = new Contact { Firstname = "Thomas", Surname = "Huber", Phonenumber = "+49 170 1234567" },
                    Birthdate = LocalDateTime(1978, 2, 10),
                    Mail = "thomas.huber@example.de",
                    Newsletter = false,
                    Validfrom = LocalDateTime(2023, 6, 1),
                    Startnumber = "7",
                    Address = new Address { Street = "Bayerstraße 42", Zip = "80335", City = "München", Country = germany },
                    NotfallContact = new Contact { Firstname = "Lisa", Surname = "Huber", Phonenumber = "+49 170 7654321" },
                    Sponsor = "BMW Motorrad",
                    UID = "DE123456789",
                    Guthaben = 500.00,
                    LastGuthabenAdd = LocalDateTime(2024, 10, 1),
                    LastGuthabenAddNumber = 200.00,
                    GuthabenComment = "Sponsoring",
                    Preisgeld = 1500.00,
                    Gratisfahrer = 5,
                    Schurke = 0,
                    VerzichtOk = true,
                    Gruppe = gruppe3 ?? gruppe1,
                    letzteBuchung = LocalDateTime(2024, 12, 18),
                    letzterEinkauf = LocalDateTime(2024, 12, 18),
                    Bike = bike1,
                    Transponder = transponder1,
                    BestTime = TimeSpan.FromMinutes(1).Add(TimeSpan.FromSeconds(35)),
                    S8S = "Ja",
                    Speeddays = "12"
                }
            };
            
            // Team zuweisen - nur wenn gruppe3 existiert
            
            db.Customers.AddRange(customers);
            await db.SaveChangesAsync();
            
            // Team aktualisieren mit Chef - nur wenn Team existiert
            if (team1 != null)
            {
                team1.TeamChef = customers[0];
                team1.Members = new List<Customer> { customers[0], customers[2] };
                await db.SaveChangesAsync();
            }
        }

        Console.WriteLine("✓ Stammdaten erfolgreich initialisiert");
    }
}
