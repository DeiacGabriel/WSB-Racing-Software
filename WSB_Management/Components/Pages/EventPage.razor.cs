using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using WSB_Management.Data;
using WSB_Management.Models;
using WSB_Management.Services;

namespace WSB_Management.Components.Pages;

public partial class EventPage : IDisposable
{
    [Inject] private IDbContextFactory<WSBRacingDbContext> DbFactory { get; set; } = default!;
    [Inject] private EventService EventService { get; set; } = default!;
    
    private bool _isDisposed;
    private CancellationTokenSource? _cts = new();
    
    // Data
    private List<Event> Events { get; set; } = new();
    private Event? SelectedEvent { get; set; }
    private bool IsNewEvent { get; set; }
    private string? Message { get; set; }
    
    // Filter
    private string? SearchText { get; set; }
    private string? StatusFilter { get; set; }
    
    // Statistics
    private int ActiveEventsCount => Events.Count(e => DateTime.Today >= e.Validfrom.Date && DateTime.Today <= e.Validuntil.Date);
    private int UpcomingEventsCount => Events.Count(e => e.Validfrom.Date > DateTime.Today);
    private int TotalCapacity => Events.Sum(e => e.maxPersons);
    
    // DatePicker Helpers
    private DateTime? ValidFromPicker
    {
        get => SelectedEvent?.Validfrom;
        set { if (SelectedEvent != null && value.HasValue) SelectedEvent.Validfrom = value.Value; }
    }
    
    private DateTime? ValidUntilPicker
    {
        get => SelectedEvent?.Validuntil;
        set { if (SelectedEvent != null && value.HasValue) SelectedEvent.Validuntil = value.Value; }
    }
    
    // Filtered Events
    private IEnumerable<Event> FilteredEvents
    {
        get
        {
            var query = Events.AsEnumerable();
            
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.ToLower();
                query = query.Where(e => 
                    e.Name?.ToLower().Contains(search) == true ||
                    e.Id.ToString().Contains(search));
            }
            
            if (!string.IsNullOrWhiteSpace(StatusFilter))
            {
                query = StatusFilter switch
                {
                    "aktiv" => query.Where(e => DateTime.Today >= e.Validfrom.Date && DateTime.Today <= e.Validuntil.Date),
                    "kommend" => query.Where(e => e.Validfrom.Date > DateTime.Today),
                    "beendet" => query.Where(e => e.Validuntil.Date < DateTime.Today),
                    _ => query
                };
            }
            
            return query.OrderByDescending(e => e.Validfrom);
        }
    }
    
    protected override async Task OnInitializedAsync()
    {
        await LoadEventsAsync();
    }
    
    private async Task LoadEventsAsync()
    {
        try
        {
            await using var db = await DbFactory.CreateDbContextAsync(_cts?.Token ?? CancellationToken.None);
            Events = await db.Events
                .AsNoTracking()
                .OrderByDescending(e => e.Validfrom)
                .ToListAsync(_cts?.Token ?? CancellationToken.None);
        }
        catch (Exception ex)
        {
            Message = $"Fehler beim Laden: {ex.Message}";
        }
    }
    
    private void AddNewEvent()
    {
        SelectedEvent = new Event
        {
            Validfrom = DateTime.Today,
            Validuntil = DateTime.Today.AddDays(1),
            Vat = 20.0
        };
        IsNewEvent = true;
        Message = null;
    }
    
    private void SelectEvent(Event ev)
    {
        SelectedEvent = new Event
        {
            Id = ev.Id,
            Name = ev.Name,
            Validfrom = ev.Validfrom,
            Validuntil = ev.Validuntil,
            maxPersons = ev.maxPersons,
            Vat = ev.Vat
        };
        IsNewEvent = false;
        Message = null;
    }
    
    private void CloseDetail()
    {
        SelectedEvent = null;
        IsNewEvent = false;
        Message = null;
    }
    
    private async Task SaveEvent()
    {
        if (_isDisposed || SelectedEvent == null) return;
        
        try
        {
            if (string.IsNullOrWhiteSpace(SelectedEvent.Name))
            {
                Message = "Fehler: Bitte Eventname eingeben.";
                return;
            }
            
            await using var db = await DbFactory.CreateDbContextAsync(_cts?.Token ?? CancellationToken.None);
            
            if (IsNewEvent)
            {
                db.Events.Add(SelectedEvent);
            }
            else
            {
                var existing = await db.Events.FindAsync(new object[] { SelectedEvent.Id }, _cts?.Token ?? CancellationToken.None);
                if (existing != null)
                {
                    existing.Name = SelectedEvent.Name;
                    existing.Validfrom = SelectedEvent.Validfrom;
                    existing.Validuntil = SelectedEvent.Validuntil;
                    existing.maxPersons = SelectedEvent.maxPersons;
                    existing.Vat = SelectedEvent.Vat;
                }
            }
            
            await db.SaveChangesAsync(_cts?.Token ?? CancellationToken.None);
            
            Message = $"Event '{SelectedEvent.Name}' gespeichert!";
            EventService.NotifyEventsChanged();
            
            await LoadEventsAsync();
            CloseDetail();
        }
        catch (Exception ex)
        {
            Message = $"Fehler beim Speichern: {ex.Message}";
        }
    }
    
    private async Task DeleteEvent(long id)
    {
        if (_isDisposed) return;
        
        try
        {
            await using var db = await DbFactory.CreateDbContextAsync(_cts?.Token ?? CancellationToken.None);
            var ev = await db.Events.FindAsync(new object[] { id }, _cts?.Token ?? CancellationToken.None);
            
            if (ev != null)
            {
                db.Events.Remove(ev);
                await db.SaveChangesAsync(_cts?.Token ?? CancellationToken.None);
                
                EventService.NotifyEventsChanged();
                
                if (SelectedEvent?.Id == id)
                    CloseDetail();
                    
                await LoadEventsAsync();
            }
        }
        catch (Exception ex)
        {
            Message = $"Fehler beim Löschen: {ex.Message}";
        }
    }
    
    private static string GetEventStatus(Event ev)
    {
        var today = DateTime.Today;
        if (today >= ev.Validfrom.Date && today <= ev.Validuntil.Date)
            return "Aktiv";
        if (ev.Validfrom.Date > today)
            return "Kommend";
        return "Beendet";
    }
    
    private static MudBlazor.Color GetStatusColor(string status)
    {
        return status switch
        {
            "Aktiv" => MudBlazor.Color.Success,
            "Kommend" => MudBlazor.Color.Info,
            "Beendet" => MudBlazor.Color.Default,
            _ => MudBlazor.Color.Default
        };
    }
    
    public void Dispose()
    {
        _isDisposed = true;
        try { _cts?.Cancel(); } catch { }
        _cts?.Dispose();
        _cts = null;
        GC.SuppressFinalize(this);
    }
}
