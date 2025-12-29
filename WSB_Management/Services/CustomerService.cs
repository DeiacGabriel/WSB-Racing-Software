using Microsoft.EntityFrameworkCore;
using WSB_Management.Data;
using WSB_Management.Models;

namespace WSB_Management.Services;

public class CustomerService
{
    private readonly IDbContextFactory<WSBRacingDbContext> _dbFactory;

    public CustomerService(IDbContextFactory<WSBRacingDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<List<Customer>> GetCustomersAsync(CancellationToken ct = default)
    {
        using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await db.Customers
            .Include(c => c.Address).ThenInclude(a => a.Country)
            .Include(c => c.Gruppe)
            .Include(c => c.Contact)
            .Include(c => c.NotfallContact)
            .Include(c => c.Transponder)
            .Include(c => c.Bike).ThenInclude(b => b.BikeType).ThenInclude(bt => bt.Brand)
            .Include(c => c.Bike).ThenInclude(b => b.BikeType).ThenInclude(bt => bt.Klasse)
            .AsNoTracking()
            .OrderBy(c => c.Contact!.Surname)
            .ToListAsync(ct);
    }

    public async Task<List<Country>> GetCountriesAsync(CancellationToken ct = default)
    {
        using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await db.Countries.AsNoTracking().OrderBy(c => c.Longtxt).ToListAsync(ct);
    }

    public async Task<List<Brand>> GetBrandsAsync(CancellationToken ct = default)
    {
        using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await db.Brands.AsNoTracking().OrderBy(b => b.Name).ToListAsync(ct);
    }

    public async Task<List<Transponder>> GetTranspondersAsync(CancellationToken ct = default)
    {
        using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await db.Transponders.AsNoTracking().OrderBy(t => t.Number).ToListAsync(ct);
    }

    public async Task<List<Gruppe>> GetGruppenAsync(CancellationToken ct = default)
    {
        using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await db.Gruppes.AsNoTracking().OrderBy(g => g.Name).ToListAsync(ct);
    }

    public async Task<long> DeleteCustomerAsync(long id, CancellationToken ct = default)
    {
        using var db = await _dbFactory.CreateDbContextAsync(ct);
        var entity = await db.Customers.FindAsync(new object?[] { id }, ct);
        if (entity == null) return 0;
        db.Customers.Remove(entity);
        await db.SaveChangesAsync(ct);
        return id;
    }
    public async Task<WaiverDeclaration> CreateWaiverAsync(long customerId, WaiverType type = WaiverType.InPerson, string? recordedBy = null)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        
        var waiver = new WaiverDeclaration
        {
            CustomerId = customerId,
            Year = DateTime.Now.Year,
            SignedDate = DateTime.Now,
            Type = type,
            RecordedBy = recordedBy,
            IsValid = true
        };
        
        db.WaiverDeclarations.Add(waiver);
        await db.SaveChangesAsync();
        
        return waiver;
    }
}
