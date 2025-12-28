using Microsoft.EntityFrameworkCore;
using WSB_Management.Data;
using WSB_Management.Models;

namespace WSB_Management.Services;

/// <summary>
/// Service für alle Race/Event Management Operationen
/// </summary>
public class RaceService
{
    private readonly IDbContextFactory<WSBRacingDbContext> _factory;

    public RaceService(IDbContextFactory<WSBRacingDbContext> factory)
    {
        _factory = factory;
    }

    #region Event Operations

    /// <summary>
    /// Lädt ein Event mit allen notwendigen Relationen
    /// </summary>
    public async Task<Event?> GetEventWithDetailsAsync(long eventId)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Events
            .Include(e => e.CustomerEvents)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == eventId);
    }

    /// <summary>
    /// Lädt alle Events für die Auswahl
    /// </summary>
    public async Task<List<Event>> GetAllEventsAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Events
            .OrderByDescending(e => e.Validfrom)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// Lädt die Tagespreise für ein Event
    /// </summary>
    public async Task<List<EventDayPrice>> GetEventDayPricesAsync(long eventId)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.EventDayPrices
            .Where(p => p.EventId == eventId)
            .OrderBy(p => p.Date)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// Erstellt oder aktualisiert Tagespreise für ein Event
    /// </summary>
    public async Task SaveEventDayPricesAsync(long eventId, List<EventDayPrice> prices)
    {
        await using var db = await _factory.CreateDbContextAsync();
        
        // Bestehende löschen
        var existing = await db.EventDayPrices.Where(p => p.EventId == eventId).ToListAsync();
        db.EventDayPrices.RemoveRange(existing);
        
        // Neue hinzufügen
        foreach (var price in prices)
        {
            price.EventId = eventId;
            db.EventDayPrices.Add(price);
        }
        
        await db.SaveChangesAsync();
    }

    #endregion

    #region Participation Operations

    /// <summary>
    /// Lädt alle Teilnahmen für ein Event mit vollständigen Kundendaten
    /// </summary>
    public async Task<List<CostumerEvent>> GetEventParticipationsAsync(long eventId)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.CustomerEvents
            .Include(p => p.Customer).ThenInclude(c => c.Contact)
            .Include(p => p.Customer).ThenInclude(c => c.Address).ThenInclude(a => a!.Country)
            .Include(p => p.Customer).ThenInclude(c => c.Bike).ThenInclude(b => b!.BikeType).ThenInclude(bt => bt!.Brand)
            .Include(p => p.Customer).ThenInclude(c => c.Gruppe)
            .Include(p => p.Customer).ThenInclude(c => c.Transponder)
            .Include(p => p.Transponder)
            .Include(p => p.Bike).ThenInclude(b => b!.BikeType).ThenInclude(bt => bt!.Brand)
            .Where(p => p.Event.Id == eventId)
            .OrderBy(p => p.ParticipationDate)
            .ThenBy(p => p.Customer.Contact.Surname)
            .ToListAsync();
    }

    /// <summary>
    /// Gruppiert Teilnahmen nach Kunde für Übersichtsdarstellung
    /// </summary>
    public List<CustomerEventSummary> GroupParticipationsByCustomer(List<CostumerEvent> participations, Event currentEvent)
    {
        return participations
            .GroupBy(p => p.Customer.Id)
            .Select(g =>
            {
                var first = g.First();
                var days = g.Select(p => p.ParticipationDate.Date).Distinct().ToList();
                return new CustomerEventSummary
                {
                    CustomerId = first.Customer.Id,
                    Customer = first.Customer,
                    ParticipationDays = days,
                    PrimaryParticipation = first,
                    AllParticipations = g.ToList(),
                    HasAllDays = currentEvent?.GetEventDays().All(d => days.Contains(d.Date)) ?? false
                };
            })
            .OrderBy(s => s.Customer.Contact?.Surname)
            .ThenBy(s => s.Customer.Contact?.Firstname)
            .ToList();
    }

    /// <summary>
    /// Speichert oder aktualisiert eine Teilnahme
    /// </summary>
    public async Task<CostumerEvent> SaveParticipationAsync(CostumerEvent participation)
    {
        await using var db = await _factory.CreateDbContextAsync();
        
        if (participation.Id == 0)
        {
            db.CustomerEvents.Add(participation);
        }
        else
        {
            db.CustomerEvents.Update(participation);
        }
        
        await db.SaveChangesAsync();
        return participation;
    }

    /// <summary>
    /// Fügt eine neue Teilnahme hinzu
    /// </summary>
    public async Task<CostumerEvent> AddParticipationAsync(CostumerEvent participation)
    {
        await using var db = await _factory.CreateDbContextAsync();
        db.CustomerEvents.Add(participation);
        await db.SaveChangesAsync();
        return participation;
    }

    /// <summary>
    /// Löscht eine Teilnahme
    /// </summary>
    public async Task DeleteParticipationAsync(long participationId)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var entity = await db.CustomerEvents.FindAsync(participationId);
        if (entity != null)
        {
            db.CustomerEvents.Remove(entity);
            await db.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Speichert alle Tagesbuchungen für einen Kunden bei einem Event
    /// </summary>
    public async Task SaveCustomerDayBookingsAsync(long eventId, long customerId, List<DateTime> bookedDays, ParticipationStatus status = ParticipationStatus.Registered)
    {
        await using var db = await _factory.CreateDbContextAsync();
        
        // Bestehende Buchungen des Kunden für dieses Event laden
        var existing = await db.CustomerEvents
            .Where(ce => ce.Event.Id == eventId && ce.Customer.Id == customerId)
            .ToListAsync();
        
        var customer = await db.Customers.FindAsync(customerId);
        var ev = await db.Events.FindAsync(eventId);
        
        if (customer == null || ev == null) return;
        
        // Tage entfernen die nicht mehr gebucht sind
        var toRemove = existing.Where(e => !bookedDays.Contains(e.ParticipationDate.Date)).ToList();
        db.CustomerEvents.RemoveRange(toRemove);
        
        // Neue Tage hinzufügen
        var existingDates = existing.Select(e => e.ParticipationDate.Date).ToList();
        foreach (var day in bookedDays.Where(d => !existingDates.Contains(d.Date)))
        {
            db.CustomerEvents.Add(new CostumerEvent
            {
                Customer = customer,
                Event = ev,
                ParticipationDate = day.Date,
                Status = status,
                RegistrationDate = DateTime.Now
            });
        }
        
        await db.SaveChangesAsync();
    }

    #endregion

    #region Payment & Finance Operations

    /// <summary>
    /// Berechnet den Gesamtbetrag für einen Kunden bei einem Event
    /// </summary>
    public async Task<CustomerFinanceSummary> CalculateCustomerFinancesAsync(long customerId, long eventId)
    {
        await using var db = await _factory.CreateDbContextAsync();
        
        var summary = new CustomerFinanceSummary { CustomerId = customerId, EventId = eventId };
        
        // Prüfen ob Jahreskarte vorhanden
        var seasonPass = await db.SeasonPasses
            .Where(sp => sp.CustomerId == customerId && sp.Year == DateTime.Now.Year && sp.IsActive && sp.IsPaid)
            .FirstOrDefaultAsync();
        
        summary.HasSeasonPass = seasonPass != null;
        
        // Tagespreise laden
        var dayPrices = await db.EventDayPrices.Where(p => p.EventId == eventId).ToListAsync();
        
        // Gebuchte Tage laden
        var participations = await db.CustomerEvents
            .Where(ce => ce.Customer.Id == customerId && ce.Event.Id == eventId)
            .ToListAsync();
        
        summary.BookedDays = participations.Select(p => p.ParticipationDate.Date).ToList();
        
        // Preis berechnen
        decimal totalPrice = 0;
        foreach (var part in participations.Where(p => p.Status != ParticipationStatus.Absent))
        {
            var dayPrice = dayPrices.FirstOrDefault(dp => dp.Date.Date == part.ParticipationDate.Date);
            if (summary.HasSeasonPass)
            {
                totalPrice += dayPrice?.SeasonPassPrice ?? 0;
            }
            else
            {
                // Prüfen ob Sonderpreis vorhanden
                totalPrice += part.SpecialPrice ?? dayPrice?.StandardPrice ?? 220.00m;
            }
        }
        summary.TotalToPay = totalPrice;
        
        // Zahlungen laden
        var payments = await db.Payments
            .Where(p => p.CustomerId == customerId && p.EventId == eventId && p.Status == PaymentStatus.Completed)
            .ToListAsync();
        
        summary.TotalPaid = payments.Sum(p => p.Amount);
        summary.OpenAmount = summary.TotalToPay - summary.TotalPaid;
        
        // Mahnstatus prüfen
        var remindedPayment = await db.Payments
            .Where(p => p.CustomerId == customerId && p.EventId == eventId && p.Status == PaymentStatus.Reminded)
            .FirstOrDefaultAsync();
        summary.IsReminded = remindedPayment != null;
        
        // Boxbuchungen
        var boxBookings = await db.BoxBookings
            .Include(bb => bb.Box)
            .Where(bb => bb.CustomerId == customerId && bb.EventId == eventId)
            .ToListAsync();
        summary.BoxBookings = boxBookings;
        summary.BoxTotal = boxBookings.Sum(bb => bb.Price);
        
        return summary;
    }

    /// <summary>
    /// Speichert eine Zahlung
    /// </summary>
    public async Task<Payment> SavePaymentAsync(Payment payment)
    {
        await using var db = await _factory.CreateDbContextAsync();
        
        if (payment.Id == 0)
        {
            db.Payments.Add(payment);
        }
        else
        {
            db.Payments.Update(payment);
        }
        
        await db.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// Lädt alle Zahlungen eines Kunden für ein Event
    /// </summary>
    public async Task<List<Payment>> GetCustomerPaymentsAsync(long customerId, long? eventId = null)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var query = db.Payments.Where(p => p.CustomerId == customerId);
        
        if (eventId.HasValue)
        {
            query = query.Where(p => p.EventId == eventId.Value);
        }
        
        return await query.OrderByDescending(p => p.PaymentDate).ToListAsync();
    }

    #endregion

    #region Waiver Operations

    /// <summary>
    /// Prüft ob ein Kunde eine gültige Verzichtserklärung für das aktuelle Jahr hat
    /// </summary>
    public async Task<bool> HasValidWaiverAsync(long customerId, int? year = null)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var checkYear = year ?? DateTime.Now.Year;
        
        return await db.WaiverDeclarations
            .AnyAsync(w => w.CustomerId == customerId && w.Year == checkYear && w.IsValid);
    }

    /// <summary>
    /// Erstellt eine Verzichtserklärung für einen Kunden
    /// </summary>
    public async Task<WaiverDeclaration> CreateWaiverAsync(long customerId, WaiverType type = WaiverType.InPerson, string? recordedBy = null)
    {
        await using var db = await _factory.CreateDbContextAsync();
        
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

    #endregion

    #region Box Operations

    /// <summary>
    /// Lädt alle verfügbaren Boxen
    /// </summary>
    public async Task<List<Box>> GetAllBoxesAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Boxes.Where(b => b.IsActive).OrderBy(b => b.Number).ToListAsync();
    }

    /// <summary>
    /// Lädt die Boxbelegung für ein Event
    /// </summary>
    public async Task<List<BoxBooking>> GetEventBoxBookingsAsync(long eventId)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.BoxBookings
            .Include(bb => bb.Box)
            .Include(bb => bb.Customer).ThenInclude(c => c.Contact)
            .Where(bb => bb.EventId == eventId)
            .OrderBy(bb => bb.Box.Number)
            .ToListAsync();
    }

    /// <summary>
    /// Prüft ob eine Box für ein Event noch freie Kapazität hat
    /// </summary>
    public async Task<int> GetBoxAvailableCapacityAsync(long boxId, long eventId)
    {
        await using var db = await _factory.CreateDbContextAsync();
        
        var box = await db.Boxes.FindAsync(boxId);
        if (box == null) return 0;
        
        var currentBookings = await db.BoxBookings.CountAsync(bb => bb.BoxId == boxId && bb.EventId == eventId);
        return box.MaxCapacity - currentBookings;
    }

    /// <summary>
    /// Bucht eine Box für einen Kunden
    /// </summary>
    public async Task<BoxBooking?> BookBoxAsync(long boxId, long customerId, long eventId, decimal price)
    {
        var capacity = await GetBoxAvailableCapacityAsync(boxId, eventId);
        if (capacity <= 0) return null;
        
        await using var db = await _factory.CreateDbContextAsync();
        
        var booking = new BoxBooking
        {
            BoxId = boxId,
            CustomerId = customerId,
            EventId = eventId,
            Price = price,
            BookingDate = DateTime.Now
        };
        
        db.BoxBookings.Add(booking);
        await db.SaveChangesAsync();
        
        return booking;
    }

    #endregion

    #region StartNumber Operations

    /// <summary>
    /// Lädt alle Startnummern für ein Event
    /// </summary>
    public async Task<List<StartNumber>> GetEventStartNumbersAsync(long eventId)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.StartNumbers
            .Include(sn => sn.Customer).ThenInclude(c => c.Contact)
            .Where(sn => sn.EventId == eventId)
            .OrderBy(sn => sn.Number)
            .ToListAsync();
    }

    /// <summary>
    /// Weist eine Startnummer zu oder aktualisiert sie
    /// </summary>
    public async Task<StartNumber> AssignStartNumberAsync(long customerId, long eventId, string number)
    {
        await using var db = await _factory.CreateDbContextAsync();
        
        var existing = await db.StartNumbers
            .FirstOrDefaultAsync(sn => sn.CustomerId == customerId && sn.EventId == eventId);
        
        if (existing != null)
        {
            existing.Number = number;
            existing.AssignedDate = DateTime.Now;
        }
        else
        {
            existing = new StartNumber
            {
                CustomerId = customerId,
                EventId = eventId,
                Number = number,
                IsAssigned = true,
                AssignedDate = DateTime.Now
            };
            db.StartNumbers.Add(existing);
        }
        
        await db.SaveChangesAsync();
        return existing;
    }

    /// <summary>
    /// Prüft ob eine Startnummer bereits vergeben ist
    /// </summary>
    public async Task<bool> IsStartNumberTakenAsync(long eventId, string number, long? excludeCustomerId = null)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var query = db.StartNumbers.Where(sn => sn.EventId == eventId && sn.Number == number);
        
        if (excludeCustomerId.HasValue)
        {
            query = query.Where(sn => sn.CustomerId != excludeCustomerId.Value);
        }
        
        return await query.AnyAsync();
    }

    #endregion

    #region Laptime & Group Operations

    /// <summary>
    /// Ermittelt die Referenz-Rundenzeit für einen Kunden
    /// </summary>
    public async Task<TimeSpan?> GetReferenceLaptimeAsync(long customerId, long eventId, int? referenceYear = null)
    {
        await using var db = await _factory.CreateDbContextAsync();
        
        // Erst die Eventreferenz suchen
        var eventLaptime = await db.LaptimeReferences
            .Where(lr => lr.CustomerId == customerId && lr.EventId == eventId)
            .OrderByDescending(lr => lr.Year)
            .FirstOrDefaultAsync();
        
        if (eventLaptime != null)
        {
            return eventLaptime.BestLaptime;
        }
        
        // Sonst aus dem CustomerEvent des letzten Jahres
        var lastYearEvent = await db.CustomerEvents
            .Where(ce => ce.Customer.Id == customerId && ce.Event.Id == eventId)
            .OrderByDescending(ce => ce.ParticipationDate)
            .FirstOrDefaultAsync();
        
        return lastYearEvent?.Laptime;
    }

    /// <summary>
    /// Berechnet die empfohlene Gruppe basierend auf der Rundenzeit
    /// </summary>
    public async Task<Gruppe?> CalculateRecommendedGroupAsync(TimeSpan laptime)
    {
        await using var db = await _factory.CreateDbContextAsync();
        
        var groups = await db.Gruppes
            .Where(g => g.MaxTimelap.HasValue)
            .OrderBy(g => g.MaxTimelap)
            .ToListAsync();
        
        foreach (var group in groups)
        {
            if (laptime <= group.MaxTimelap!.Value)
            {
                return group;
            }
        }
        
        // Letzte Gruppe für langsame Fahrer
        return groups.LastOrDefault();
    }

    /// <summary>
    /// Speichert eine neue Bestzeit
    /// </summary>
    public async Task SaveLaptimeReferenceAsync(long customerId, long eventId, TimeSpan laptime, bool verified = false)
    {
        await using var db = await _factory.CreateDbContextAsync();
        
        var existing = await db.LaptimeReferences
            .FirstOrDefaultAsync(lr => lr.CustomerId == customerId && lr.EventId == eventId && lr.Year == DateTime.Now.Year);
        
        if (existing != null)
        {
            // Nur speichern wenn besser
            if (laptime < existing.BestLaptime)
            {
                existing.BestLaptime = laptime;
                existing.RecordDate = DateTime.Now;
                existing.IsVerified = verified;
            }
        }
        else
        {
            db.LaptimeReferences.Add(new LaptimeReference
            {
                CustomerId = customerId,
                EventId = eventId,
                BestLaptime = laptime,
                Year = DateTime.Now.Year,
                RecordDate = DateTime.Now,
                IsVerified = verified
            });
        }
        
        await db.SaveChangesAsync();
    }

    #endregion

    #region Season Pass Operations

    /// <summary>
    /// Prüft ob ein Kunde eine aktive Jahreskarte hat
    /// </summary>
    public async Task<SeasonPass?> GetActiveSeasonPassAsync(long customerId, int? year = null)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var checkYear = year ?? DateTime.Now.Year;
        
        return await db.SeasonPasses
            .Where(sp => sp.CustomerId == customerId && sp.Year == checkYear && sp.IsActive && sp.IsPaid)
            .FirstOrDefaultAsync();
    }

    #endregion

    #region Cup Operations

    /// <summary>
    /// Lädt alle aktiven Cups
    /// </summary>
    public async Task<List<RaceCup>> GetActiveCupsAsync(int? year = null)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var checkYear = year ?? DateTime.Now.Year;
        
        return await db.RaceCups
            .Where(c => c.IsActive && c.Year == checkYear)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Lädt die Cup-Teilnahmen eines Kunden
    /// </summary>
    public async Task<List<CustomerCupParticipation>> GetCustomerCupParticipationsAsync(long customerId)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.CustomerCupParticipations
            .Include(cp => cp.RaceCup)
            .Where(cp => cp.CustomerId == customerId && cp.IsActive)
            .ToListAsync();
    }

    #endregion

    #region Statistics

    /// <summary>
    /// Lädt Statistiken für ein Event
    /// </summary>
    public async Task<EventStatistics> GetEventStatisticsAsync(long eventId)
    {
        await using var db = await _factory.CreateDbContextAsync();
        
        var ev = await db.Events.FindAsync(eventId);
        if (ev == null) return new EventStatistics();
        
        var participations = await db.CustomerEvents
            .Include(ce => ce.Customer).ThenInclude(c => c.Bike).ThenInclude(b => b!.BikeType).ThenInclude(bt => bt!.Brand)
            .Include(ce => ce.Customer).ThenInclude(c => c.Gruppe)
            .Where(ce => ce.Event.Id == eventId)
            .ToListAsync();
        
        var stats = new EventStatistics
        {
            EventId = eventId,
            EventName = ev.Name,
            MaxCapacity = ev.maxPersons,
            TotalRegistrations = participations.Count(p => p.Status == ParticipationStatus.Registered),
            TotalWaitlist = participations.Count(p => p.Status == ParticipationStatus.Waitlist),
            TotalAbsent = participations.Count(p => p.Status == ParticipationStatus.Absent),
            TotalAdditionalBikes = participations.Count(p => p.Status == ParticipationStatus.AdditionalBike)
        };
        
        // Gruppiere nach Tag
        foreach (var day in ev.GetEventDays())
        {
            var dayStats = new DayStatistics
            {
                Date = day,
                Registered = participations.Count(p => p.ParticipationDate.Date == day.Date && p.Status == ParticipationStatus.Registered),
                Waitlist = participations.Count(p => p.ParticipationDate.Date == day.Date && p.Status == ParticipationStatus.Waitlist),
                Absent = participations.Count(p => p.ParticipationDate.Date == day.Date && p.Status == ParticipationStatus.Absent),
                AdditionalBikes = participations.Count(p => p.ParticipationDate.Date == day.Date && p.Status == ParticipationStatus.AdditionalBike)
            };
            stats.DayStatistics.Add(dayStats);
        }
        
        // Gruppiere nach Marke
        stats.BrandDistribution = participations
            .Where(p => p.Customer?.Bike?.BikeType?.Brand != null)
            .GroupBy(p => p.Customer!.Bike!.BikeType!.Brand!.Name)
            .ToDictionary(g => g.Key, g => g.Count());
        
        // Gruppiere nach Gruppe
        stats.GroupDistribution = participations
            .Where(p => p.Customer?.Gruppe != null)
            .GroupBy(p => p.Customer!.Gruppe!.Name)
            .ToDictionary(g => g.Key, g => g.Count());
        
        return stats;
    }

    #endregion

    #region Email Operations

    /// <summary>
    /// Protokolliert eine gesendete Email
    /// </summary>
    public async Task<EmailLog> LogEmailAsync(long customerId, long? eventId, string recipientEmail, 
        EmailType type, string? subject = null, string? notes = null, string? sentBy = null)
    {
        await using var db = await _factory.CreateDbContextAsync();
        
        var log = new EmailLog
        {
            CustomerId = customerId,
            EventId = eventId,
            RecipientEmail = recipientEmail,
            Type = type,
            Subject = subject,
            Notes = notes,
            SentBy = sentBy,
            SentDate = DateTime.Now,
            Status = EmailStatus.Sent
        };
        
        db.EmailLogs.Add(log);
        await db.SaveChangesAsync();
        
        return log;
    }

    /// <summary>
    /// Lädt die Email-Historie eines Kunden
    /// </summary>
    public async Task<List<EmailLog>> GetCustomerEmailHistoryAsync(long customerId)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.EmailLogs
            .Where(e => e.CustomerId == customerId)
            .OrderByDescending(e => e.SentDate)
            .ToListAsync();
    }

    #endregion

    #region Season Reset

    /// <summary>
    /// Setzt Daten für die neue Saison zurück
    /// </summary>
    public async Task ResetForNewSeasonAsync(int newYear)
    {
        await using var db = await _factory.CreateDbContextAsync();
        
        // Jahreskarten deaktivieren
        var oldPasses = await db.SeasonPasses.Where(sp => sp.Year < newYear).ToListAsync();
        foreach (var pass in oldPasses)
        {
            pass.IsActive = false;
        }
        
        // Verzichtserklärungen bleiben erhalten, sind aber nur für das jeweilige Jahr gültig
        
        await db.SaveChangesAsync();
    }

    #endregion
}

#region DTOs

public class CustomerEventSummary
{
    public long CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public List<DateTime> ParticipationDays { get; set; } = new();
    public CostumerEvent PrimaryParticipation { get; set; } = null!;
    public List<CostumerEvent> AllParticipations { get; set; } = new();
    public bool HasAllDays { get; set; }
}

public class CustomerFinanceSummary
{
    public long CustomerId { get; set; }
    public long EventId { get; set; }
    public bool HasSeasonPass { get; set; }
    public List<DateTime> BookedDays { get; set; } = new();
    public decimal TotalToPay { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal OpenAmount { get; set; }
    public bool IsReminded { get; set; }
    public List<BoxBooking> BoxBookings { get; set; } = new();
    public decimal BoxTotal { get; set; }
}

public class EventStatistics
{
    public long EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public int MaxCapacity { get; set; }
    public int TotalRegistrations { get; set; }
    public int TotalWaitlist { get; set; }
    public int TotalAbsent { get; set; }
    public int TotalAdditionalBikes { get; set; }
    public int FreeSpots => MaxCapacity - TotalRegistrations;
    public double OccupancyPercent => MaxCapacity > 0 ? (double)TotalRegistrations * 100 / MaxCapacity : 0;
    public List<DayStatistics> DayStatistics { get; set; } = new();
    public Dictionary<string, int> BrandDistribution { get; set; } = new();
    public Dictionary<string, int> GroupDistribution { get; set; } = new();
}

public class DayStatistics
{
    public DateTime Date { get; set; }
    public int Registered { get; set; }
    public int Waitlist { get; set; }
    public int Absent { get; set; }
    public int AdditionalBikes { get; set; }
    public int Free => 100 - Registered; // Annahme max 100 pro Tag, sollte konfigurierbar sein
}

#endregion
