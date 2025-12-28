using Microsoft.EntityFrameworkCore;
using WSB_Management.Data;
using WSB_Management.Models;

namespace WSB_Management.Services;

/// <summary>
/// Service für alle Stammdaten-Operationen (Brands, Countries, Groups, BikeTypes, Transponder)
/// </summary>
public class MasterDataService
{
    private readonly IDbContextFactory<WSBRacingDbContext> _factory;

    public MasterDataService(IDbContextFactory<WSBRacingDbContext> factory)
    {
        _factory = factory;
    }

    #region Brand Operations

    public async Task<List<Brand>> GetBrandsAsync(CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        return await db.Brands
            .AsNoTracking()
            .OrderBy(b => b.Name)
            .ToListAsync(ct);
    }

    public async Task<Brand?> GetBrandByIdAsync(long id, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        return await db.Brands
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id, ct);
    }

    public async Task<Brand> SaveBrandAsync(Brand brand, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        
        if (brand.Id == 0)
        {
            db.Brands.Add(brand);
        }
        else
        {
            var existing = await db.Brands.FindAsync(new object[] { brand.Id }, ct);
            if (existing != null)
            {
                existing.Name = brand.Name;
            }
        }
        
        await db.SaveChangesAsync(ct);
        return brand;
    }

    public async Task DeleteBrandAsync(long id, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var brand = await db.Brands.FindAsync(new object[] { id }, ct);
        if (brand != null)
        {
            db.Brands.Remove(brand);
            await db.SaveChangesAsync(ct);
        }
    }

    #endregion

    #region Country Operations

    public async Task<List<Country>> GetCountriesAsync(CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        return await db.Countries
            .AsNoTracking()
            .OrderBy(c => c.Longtxt)
            .ToListAsync(ct);
    }

    public async Task<Country?> GetCountryByIdAsync(long id, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        return await db.Countries
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<Country> SaveCountryAsync(Country country, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        
        if (country.Id == 0)
        {
            db.Countries.Add(country);
        }
        else
        {
            var existing = await db.Countries.FindAsync(new object[] { country.Id }, ct);
            if (existing != null)
            {
                existing.Shorttxt = country.Shorttxt;
                existing.Longtxt = country.Longtxt;
                existing.FlagPath = country.FlagPath;
            }
        }
        
        await db.SaveChangesAsync(ct);
        return country;
    }

    public async Task DeleteCountryAsync(long id, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var country = await db.Countries.FindAsync(new object[] { id }, ct);
        if (country != null)
        {
            db.Countries.Remove(country);
            await db.SaveChangesAsync(ct);
        }
    }

    #endregion

    #region Gruppe Operations

    public async Task<List<Gruppe>> GetGruppenAsync(CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        return await db.Gruppes
            .AsNoTracking()
            .OrderBy(g => g.Name)
            .ToListAsync(ct);
    }

    public async Task<Gruppe?> GetGruppeByIdAsync(long id, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        return await db.Gruppes
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == id, ct);
    }

    public async Task<Gruppe> SaveGruppeAsync(Gruppe gruppe, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        
        if (gruppe.Id == 0)
        {
            db.Gruppes.Add(gruppe);
        }
        else
        {
            var existing = await db.Gruppes.FindAsync(new object[] { gruppe.Id }, ct);
            if (existing != null)
            {
                existing.Name = gruppe.Name;
                existing.MaxTimelap = gruppe.MaxTimelap;
            }
        }
        
        await db.SaveChangesAsync(ct);
        return gruppe;
    }

    public async Task DeleteGruppeAsync(long id, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var gruppe = await db.Gruppes.FindAsync(new object[] { id }, ct);
        if (gruppe != null)
        {
            db.Gruppes.Remove(gruppe);
            await db.SaveChangesAsync(ct);
        }
    }

    public async Task<int> AutoAssignCustomersToGroupsAsync(CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        
        var gruppen = await db.Gruppes.OrderBy(g => g.MaxTimelap).ToListAsync(ct);
        var customers = await db.Customers
            .Include(c => c.Gruppe)
            .Where(c => c.BestTime.HasValue)
            .ToListAsync(ct);
        
        int assignedCount = 0;
        
        foreach (var customer in customers)
        {
            if (!customer.BestTime.HasValue) continue;
            
            var passendGruppe = gruppen.FirstOrDefault(g => 
                g.MaxTimelap.HasValue && customer.BestTime.Value <= g.MaxTimelap.Value);
            
            if (passendGruppe != null && customer.Gruppe?.Id != passendGruppe.Id)
            {
                customer.Gruppe = passendGruppe;
                assignedCount++;
            }
        }
        
        await db.SaveChangesAsync(ct);
        return assignedCount;
    }

    #endregion

    #region Transponder Operations

    public async Task<List<Transponder>> GetTranspondersAsync(CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        return await db.Transponders
            .AsNoTracking()
            .OrderBy(t => t.Number)
            .ToListAsync(ct);
    }

    public async Task<Transponder?> GetTransponderByIdAsync(long id, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        return await db.Transponders
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<Transponder> SaveTransponderAsync(Transponder transponder, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        
        if (transponder.Id == 0)
        {
            db.Transponders.Add(transponder);
        }
        else
        {
            var existing = await db.Transponders.FindAsync(new object[] { transponder.Id }, ct);
            if (existing != null)
            {
                existing.Bezeichung = transponder.Bezeichung;
                existing.Number = transponder.Number;
            }
        }
        
        await db.SaveChangesAsync(ct);
        return transponder;
    }

    public async Task DeleteTransponderAsync(long id, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var transponder = await db.Transponders.FindAsync(new object[] { id }, ct);
        if (transponder != null)
        {
            db.Transponders.Remove(transponder);
            await db.SaveChangesAsync(ct);
        }
    }

    #endregion

    #region Klasse Operations

    public async Task<List<Klasse>> GetKlassenAsync(CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        return await db.Klasses
            .AsNoTracking()
            .OrderBy(k => k.Bezeichnung)
            .ToListAsync(ct);
    }

    public async Task<Klasse> SaveKlasseAsync(Klasse klasse, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        
        if (klasse.Id == 0)
        {
            db.Klasses.Add(klasse);
        }
        else
        {
            var existing = await db.Klasses.FindAsync(new object[] { klasse.Id }, ct);
            if (existing != null)
            {
                existing.Bezeichnung = klasse.Bezeichnung;
            }
        }
        
        await db.SaveChangesAsync(ct);
        return klasse;
    }

    public async Task DeleteKlasseAsync(long id, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var klasse = await db.Klasses.FindAsync(new object[] { id }, ct);
        if (klasse != null)
        {
            db.Klasses.Remove(klasse);
            await db.SaveChangesAsync(ct);
        }
    }

    #endregion

    #region BikeType Operations

    public async Task<List<BikeType>> GetBikeTypesAsync(CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        return await db.BikeTypes
            .Include(bt => bt.Brand)
            .Include(bt => bt.Klasse)
            .AsNoTracking()
            .OrderBy(bt => bt.Brand!.Name)
            .ThenBy(bt => bt.Name)
            .ToListAsync(ct);
    }

    public async Task<BikeType> SaveBikeTypeAsync(BikeType bikeType, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        
        if (bikeType.Id == 0)
        {
            db.BikeTypes.Add(bikeType);
        }
        else
        {
            var existing = await db.BikeTypes.FindAsync(new object[] { bikeType.Id }, ct);
            if (existing != null)
            {
                existing.Name = bikeType.Name;
                existing.BrandId = bikeType.BrandId;
                existing.KlasseId = bikeType.KlasseId;
            }
        }
        
        await db.SaveChangesAsync(ct);
        return bikeType;
    }

    public async Task DeleteBikeTypeAsync(long id, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var bikeType = await db.BikeTypes.FindAsync(new object[] { id }, ct);
        if (bikeType != null)
        {
            db.BikeTypes.Remove(bikeType);
            await db.SaveChangesAsync(ct);
        }
    }

    #endregion

    #region Customer Operations

    public async Task<List<Customer>> GetCustomersAsync(CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        return await db.Customers
            .Include(c => c.Address).ThenInclude(a => a!.Country)
            .Include(c => c.Gruppe)
            .Include(c => c.Contact)
            .Include(c => c.NotfallContact)
            .Include(c => c.Transponder)
            .Include(c => c.Bike).ThenInclude(b => b!.BikeType).ThenInclude(bt => bt!.Brand)
            .Include(c => c.Bike).ThenInclude(b => b!.BikeType).ThenInclude(bt => bt!.Klasse)
            .AsNoTracking()
            .OrderBy(c => c.Contact!.Surname)
            .ToListAsync(ct);
    }

    public async Task<Customer?> GetCustomerByIdAsync(long id, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        return await db.Customers
            .Include(c => c.Address).ThenInclude(a => a!.Country)
            .Include(c => c.Gruppe)
            .Include(c => c.Contact)
            .Include(c => c.NotfallContact)
            .Include(c => c.Transponder)
            .Include(c => c.Bike).ThenInclude(b => b!.BikeType).ThenInclude(bt => bt!.Brand)
            .Include(c => c.Bike).ThenInclude(b => b!.BikeType).ThenInclude(bt => bt!.Klasse)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<Customer> SaveCustomerAsync(Customer customer, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        
        if (customer.Id == 0)
        {
            customer.Validfrom = DateTime.UtcNow;
            
            // Attach related entities
            if (customer.Gruppe?.Id > 0)
                db.Attach(customer.Gruppe);
            if (customer.Transponder?.Id > 0)
                db.Attach(customer.Transponder);
            if (customer.Address?.Country?.Id > 0)
                db.Attach(customer.Address.Country);
            if (customer.Bike?.BikeType?.Id > 0)
                db.Attach(customer.Bike.BikeType);
                
            db.Customers.Add(customer);
            await db.SaveChangesAsync(ct);
            return customer;
        }
        
        var existing = await db.Customers
            .Include(c => c.Address)
            .Include(c => c.Contact)
            .Include(c => c.NotfallContact)
            .Include(c => c.Bike)
            .Include(c => c.Gruppe)
            .Include(c => c.Transponder)
            .FirstOrDefaultAsync(c => c.Id == customer.Id, ct);
            
        if (existing == null) return customer;
        
        // Update alle Felder
        existing.Title = customer.Title;
        existing.Mail = customer.Mail;
        existing.Birthdate = customer.Birthdate;
        existing.BestTime = customer.BestTime;
        existing.Startnumber = customer.Startnumber;
        existing.UID = customer.UID;
        existing.Sponsor = customer.Sponsor;
        existing.Guthaben = customer.Guthaben;
        existing.LastGuthabenAdd = customer.LastGuthabenAdd;
        existing.LastGuthabenAddNumber = customer.LastGuthabenAddNumber;
        existing.GuthabenComment = customer.GuthabenComment;
        existing.Preisgeld = customer.Preisgeld;
        existing.Gratisfahrer = customer.Gratisfahrer;
        existing.Schurke = customer.Schurke;
        existing.VerzichtOk = customer.VerzichtOk;
        existing.S8S = customer.S8S;
        existing.Speeddays = customer.Speeddays;
        existing.letzteBuchung = customer.letzteBuchung;
        existing.letzterEinkauf = customer.letzterEinkauf;
        
        // Gruppe update
        if (customer.Gruppe?.Id > 0)
        {
            existing.Gruppe = await db.Gruppes.FindAsync(new object[] { customer.Gruppe.Id }, ct) ?? existing.Gruppe;
        }
        
        // Transponder update
        if (customer.Transponder?.Id > 0)
        {
            existing.Transponder = await db.Transponders.FindAsync(new object[] { customer.Transponder.Id }, ct) ?? existing.Transponder;
        }
        
        // Contact
        if (existing.Contact != null && customer.Contact != null)
        {
            existing.Contact.Firstname = customer.Contact.Firstname;
            existing.Contact.Surname = customer.Contact.Surname;
            existing.Contact.Phonenumber = customer.Contact.Phonenumber;
        }
        
        // NotfallContact
        if (existing.NotfallContact != null && customer.NotfallContact != null)
        {
            existing.NotfallContact.Firstname = customer.NotfallContact.Firstname;
            existing.NotfallContact.Surname = customer.NotfallContact.Surname;
            existing.NotfallContact.Phonenumber = customer.NotfallContact.Phonenumber;
        }
        
        // Address
        if (existing.Address != null && customer.Address != null)
        {
            existing.Address.Street = customer.Address.Street;
            existing.Address.Zip = customer.Address.Zip;
            existing.Address.City = customer.Address.City;
            if (customer.Address.Country?.Id > 0)
            {
                existing.Address.Country = await db.Countries.FindAsync(new object[] { customer.Address.Country.Id }, ct);
            }
        }
        
        // Bike
        if (customer.Bike?.BikeType?.Id > 0)
        {
            if (existing.Bike == null)
            {
                existing.Bike = new Bike();
            }
            existing.Bike.BikeType = await db.BikeTypes
                .Include(bt => bt.Brand)
                .Include(bt => bt.Klasse)
                .FirstOrDefaultAsync(bt => bt.Id == customer.Bike.BikeType.Id, ct);
        }
        
        await db.SaveChangesAsync(ct);
        return existing;
    }

    public async Task DeleteCustomerAsync(long id, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var customer = await db.Customers.FindAsync(new object[] { id }, ct);
        if (customer != null)
        {
            db.Customers.Remove(customer);
            await db.SaveChangesAsync(ct);
        }
    }

    #endregion

    #region Team Operations

    public async Task<List<Team>> GetTeamsAsync(CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        return await db.Teams
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .ToListAsync(ct);
    }

    #endregion

    #region Cup Operations

    public async Task<List<Cup>> GetCupsAsync(CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        return await db.Cups
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
    }

    #endregion

    #region Statistics

    public async Task<MasterDataStatistics> GetStatisticsAsync(CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        
        return new MasterDataStatistics
        {
            TotalCustomers = await db.Customers.CountAsync(ct),
            TotalBrands = await db.Brands.CountAsync(ct),
            TotalCountries = await db.Countries.CountAsync(ct),
            TotalGruppen = await db.Gruppes.CountAsync(ct),
            TotalTransponder = await db.Transponders.CountAsync(ct),
            TotalBikeTypes = await db.BikeTypes.CountAsync(ct),
            TotalKlassen = await db.Klasses.CountAsync(ct),
            CustomersWithGroup = await db.Customers.Include(c => c.Gruppe).Where(c => c.Gruppe != null && c.Gruppe.Id > 0).CountAsync(ct),
            CustomersWithTransponder = await db.Customers.Include(c => c.Transponder).Where(c => c.Transponder != null && c.Transponder.Id > 0).CountAsync(ct)
        };
    }

    #endregion
}

public class MasterDataStatistics
{
    public int TotalCustomers { get; set; }
    public int TotalBrands { get; set; }
    public int TotalCountries { get; set; }
    public int TotalGruppen { get; set; }
    public int TotalTransponder { get; set; }
    public int TotalBikeTypes { get; set; }
    public int TotalKlassen { get; set; }
    public int CustomersWithGroup { get; set; }
    public int CustomersWithTransponder { get; set; }
}
