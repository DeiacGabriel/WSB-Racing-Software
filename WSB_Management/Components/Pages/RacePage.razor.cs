using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using WSB_Management.Components.Dialogs;
using WSB_Management.Data;
using WSB_Management.Models;
using WSB_Management.Services;

namespace WSB_Management.Components.Pages;

public partial class RacePage : ComponentBase
{
    #region DI / Parameter
    [Inject] public IDbContextFactory<WSBRacingDbContext> DbFactory { get; set; } = default!;
    [Inject] public RaceService RaceService { get; set; } = default!;
    [Inject] public IDialogService DialogService { get; set; } = default!;
    [Inject] public NavigationManager NavigationManager { get; set; } = default!;
    [Parameter] public long EventId { get; set; }
    #endregion

    #region State
    private Event? CurrentEvent { get; set; }
    private List<Event> AllEvents { get; set; } = new();
    private List<CostumerEvent> EventParticipations { get; set; } = new();
    private List<CustomerEventSummary> CustomerSummaries { get; set; } = new();
    private List<EventDayPrice> DayPrices { get; set; } = new();
    private List<Gruppe> Gruppen { get; set; } = new();
    private List<Transponder> Transponders { get; set; } = new();
    private List<Customer> AllCustomers { get; set; } = new();
    private HashSet<long> ValidWaiverCustomerIds { get; set; } = new();
    
    private EventStatistics Statistics { get; set; } = new();
    private CustomerEventSummary? SelectedSummary { get; set; }
    
    private string SearchText { get; set; } = string.Empty;
    private ParticipationStatus? StatusFilter { get; set; }
    private string LaptimeString { get; set; } = string.Empty;
    
    private bool _isLoading = false;
    private bool _initialLoadDone = false;
    #endregion

    #region Computed Properties
    private IEnumerable<CustomerEventSummary> FilteredCustomers
    {
        get
        {
            var result = CustomerSummaries.AsEnumerable();
            
            // Status filter
            if (StatusFilter.HasValue)
            {
                result = result.Where(s => s.PrimaryParticipation?.Status == StatusFilter.Value);
            }
            
            // Search filter
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.ToLower();
                result = result.Where(s =>
                    (s.Customer?.Contact?.Surname?.ToLower().Contains(search) ?? false) ||
                    (s.Customer?.Contact?.Firstname?.ToLower().Contains(search) ?? false) ||
                    (s.Customer?.Startnumber?.ToLower().Contains(search) ?? false) ||
                    (s.Customer?.Mail?.ToLower().Contains(search) ?? false));
            }
            
            return result;
        }
    }
    #endregion

    #region Lifecycle
    protected override async Task OnParametersSetAsync()
    {
        if (_isLoading) return;
        
        if (!_initialLoadDone || CurrentEvent?.Id != EventId)
        {
            _isLoading = true;
            try
            {
                await LoadAllDataAsync();
                _initialLoadDone = true;
            }
            finally
            {
                _isLoading = false;
            }
        }
    }
    #endregion

    #region Data Loading
    private async Task LoadAllDataAsync()
    {
        await using var db = await DbFactory.CreateDbContextAsync();
        
        // Load all events for dropdown
        AllEvents = await RaceService.GetAllEventsAsync();
        
        // Load current event
        if (EventId > 0)
        {
            CurrentEvent = await RaceService.GetEventWithDetailsAsync(EventId);
        }
        
        if (CurrentEvent == null) return;
        
        // Load supporting data
        Gruppen = await db.Gruppes.AsNoTracking().ToListAsync();
        Transponders = await db.Transponders.AsNoTracking().ToListAsync();
        AllCustomers = await db.Customers
            .Include(c => c.Contact)
            .AsNoTracking()
            .ToListAsync();
        
        // Load day prices
        DayPrices = await RaceService.GetEventDayPricesAsync(EventId);
        
        // Load participations
        EventParticipations = await RaceService.GetEventParticipationsAsync(EventId);
        
        // Group by customer
        CustomerSummaries = RaceService.GroupParticipationsByCustomer(EventParticipations, CurrentEvent);
        
        // Load statistics
        Statistics = await RaceService.GetEventStatisticsAsync(EventId);
        
        // Load valid waivers
        await LoadValidWaiversAsync();
    }
    
    private async Task LoadValidWaiversAsync()
    {
        ValidWaiverCustomerIds.Clear();
        foreach (var summary in CustomerSummaries)
        {
            if (await RaceService.HasValidWaiverAsync(summary.CustomerId))
            {
                ValidWaiverCustomerIds.Add(summary.CustomerId);
            }
        }
    }
    
    private async Task RefreshDataAsync()
    {
        EventParticipations = await RaceService.GetEventParticipationsAsync(EventId);
        CustomerSummaries = RaceService.GroupParticipationsByCustomer(EventParticipations, CurrentEvent!);
        Statistics = await RaceService.GetEventStatisticsAsync(EventId);
        await LoadValidWaiversAsync();
        StateHasChanged();
    }
    #endregion

    #region Event Handlers
    private async Task OnEventSelected(long newEventId)
    {
        if (newEventId != EventId)
        {
            NavigationManager.NavigateTo($"/events/{newEventId}");
        }
    }
    
    private void FilterByStatus(ParticipationStatus? status)
    {
        StatusFilter = status;
    }
    
    private void SelectCustomer(CustomerEventSummary summary)
    {
        SelectedSummary = summary;
        
        // Set laptime string if available
        var firstPart = summary.AllParticipations.FirstOrDefault();
        if (firstPart?.Laptime != TimeSpan.Zero)
        {
            LaptimeString = firstPart.Laptime.ToString(@"m\:ss\.fff");
        }
        else
        {
            LaptimeString = string.Empty;
        }
    }
    
    private void ToggleDay(DateTime day, bool isBooked)
    {
        if (SelectedSummary == null) return;
        
        if (isBooked)
        {
            if (!SelectedSummary.ParticipationDays.Contains(day.Date))
            {
                SelectedSummary.ParticipationDays.Add(day.Date);
            }
        }
        else
        {
            SelectedSummary.ParticipationDays.Remove(day.Date);
        }
    }
    
    private async Task SaveCurrentCustomer()
    {
        if (SelectedSummary == null || CurrentEvent == null) return;
        
        // Save day bookings
        await RaceService.SaveCustomerDayBookingsAsync(
            CurrentEvent.Id, 
            SelectedSummary.CustomerId, 
            SelectedSummary.ParticipationDays);
        
        // Parse and save laptime
        if (!string.IsNullOrWhiteSpace(LaptimeString) && TimeSpan.TryParse(LaptimeString, out var laptime))
        {
            await RaceService.SaveLaptimeReferenceAsync(SelectedSummary.CustomerId, CurrentEvent.Id, laptime);
        }
        
        await RefreshDataAsync();
        SelectedSummary = null;
    }
    
    private async Task DeleteCustomerParticipations(CustomerEventSummary summary)
    {
        var result = await DialogService.ShowMessageBox(
            "Löschen bestätigen",
            $"Möchten Sie alle Buchungen von {summary.Customer?.Contact?.Surname} {summary.Customer?.Contact?.Firstname} wirklich löschen?",
            yesText: "Ja, löschen",
            cancelText: "Abbrechen");
        
        if (result == true)
        {
            foreach (var part in summary.AllParticipations)
            {
                await RaceService.DeleteParticipationAsync(part.Id);
            }
            await RefreshDataAsync();
        }
    }
    #endregion

    #region Dialog Handlers
    private async Task OpenKassa(CustomerEventSummary summary)
    {
        var parameters = new DialogParameters<KassaDialog>
        {
            { x => x.Customer, summary.Customer },
            { x => x.CurrentEvent, CurrentEvent },
            { x => x.CustomerParticipations, summary.AllParticipations },
            { x => x.AvailableTransponders, Transponders },
            { x => x.DayPrices, DayPrices }
        };
        
        var options = new DialogOptions 
        { 
            MaxWidth = MaxWidth.Large, 
            FullWidth = true,
            CloseButton = true
        };
        
        var dialog = await DialogService.ShowAsync<KassaDialog>("Kassa", parameters, options);
        var result = await dialog.Result;
        
        if (!result!.Canceled)
        {
            await RefreshDataAsync();
        }
    }
    
    private async Task OpenEmail(CustomerEventSummary summary)
    {
        var parameters = new DialogParameters<EmailDialog>
        {
            { x => x.Customer, summary.Customer },
            { x => x.CurrentEvent, CurrentEvent },
            { x => x.BookedDays, summary.ParticipationDays },
            { x => x.HasSeasonPass, false } // TODO: Check actual season pass
        };
        
        var options = new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true };
        
        await DialogService.ShowAsync<EmailDialog>("Email senden", parameters, options);
    }
    
    private async Task OpenStatistics()
    {
        var parameters = new DialogParameters<StatisticsDialog>
        {
            { x => x.EventId, EventId }
        };
        
        var options = new DialogOptions { MaxWidth = MaxWidth.Large, FullWidth = true };
        
        await DialogService.ShowAsync<StatisticsDialog>("Statistiken", parameters, options);
    }
    
    private async Task OpenPrintDialog()
    {
        var parameters = new DialogParameters<PrintDialog>
        {
            { x => x.CurrentEvent, CurrentEvent }
        };
        
        var options = new DialogOptions { MaxWidth = MaxWidth.Large, FullWidth = true };
        
        await DialogService.ShowAsync<PrintDialog>("Drucken", parameters, options);
    }
    
    private async Task OpenStartNumbers()
    {
        var parameters = new DialogParameters<StartNumberDialog>
        {
            { x => x.CurrentEvent, CurrentEvent },
            { x => x.AvailableCustomers, AllCustomers }
        };
        
        var options = new DialogOptions { MaxWidth = MaxWidth.Large, FullWidth = true };
        
        await DialogService.ShowAsync<StartNumberDialog>("Startnummern", parameters, options);
    }
    
    private async Task OpenAddCustomerDialog()
    {
        // Get customers not yet registered for this event
        var registeredCustomerIds = CustomerSummaries.Select(cs => cs.CustomerId).ToHashSet();
        var availableCustomers = AllCustomers.Where(c => !registeredCustomerIds.Contains(c.Id)).ToList();
        
        var parameters = new DialogParameters<AddCustomerToEventDialog>
        {
            { x => x.CurrentEvent, CurrentEvent },
            { x => x.AvailableCustomers, availableCustomers },
            { x => x.DayPrices, DayPrices },
            { x => x.AvailableTransponders, Transponders }
        };
        
        var options = new DialogOptions 
        { 
            MaxWidth = MaxWidth.Medium, 
            FullWidth = true,
            CloseButton = true
        };
        
        var dialog = await DialogService.ShowAsync<AddCustomerToEventDialog>("Kunde zum Event hinzufügen", parameters, options);
        var result = await dialog.Result;
        
        if (!result!.Canceled)
        {
            await RefreshDataAsync();
        }
    }
    #endregion

    #region Helper Methods
    private bool HasValidWaiver(long customerId) => ValidWaiverCustomerIds.Contains(customerId);
    
    private string GetStartNumber(CustomerEventSummary summary)
    {
        return summary.PrimaryParticipation?.EventStartNumber ?? summary.Customer?.Startnumber ?? string.Empty;
    }
    
    private decimal GetDayPrice(DateTime day)
    {
        var price = DayPrices.FirstOrDefault(p => p.Date.Date == day.Date);
        return price?.StandardPrice ?? 220.00m;
    }
    
    private bool HasOpenPayments(CustomerEventSummary summary)
    {
        return summary.AllParticipations.Any(p => !p.IsPaid);
    }
    
    private decimal CalculateTotal(CustomerEventSummary summary)
    {
        return summary.ParticipationDays.Sum(day => 
            summary.AllParticipations.FirstOrDefault(p => p.ParticipationDate.Date == day.Date)?.SpecialPrice 
            ?? GetDayPrice(day));
    }
    
    private decimal CalculatePaid(CustomerEventSummary summary)
    {
        return summary.AllParticipations.Where(p => p.IsPaid).Sum(p => p.PaidAmount);
    }
    
    private decimal CalculateOpen(CustomerEventSummary summary)
    {
        return CalculateTotal(summary) - CalculatePaid(summary);
    }
    
    private string GetRowClass(CustomerEventSummary summary)
    {
        var status = summary.PrimaryParticipation?.Status;
        var isPaid = summary.AllParticipations.All(p => p.IsPaid);
        
        if (status == ParticipationStatus.Absent) return "status-absent";
        if (status == ParticipationStatus.Waitlist) return "status-waitlist";
        if (status == ParticipationStatus.AdditionalBike) return "status-additional";
        if (!isPaid) return "status-unpaid";
        
        return "status-registered";
    }
    
    private Color GetStatusColor(ParticipationStatus status)
    {
        return status switch
        {
            ParticipationStatus.Registered => Color.Success,
            ParticipationStatus.Waitlist => Color.Warning,
            ParticipationStatus.Absent => Color.Default,
            ParticipationStatus.AdditionalBike => Color.Info,
            _ => Color.Default
        };
    }
    
    private string GetStatusText(ParticipationStatus status)
    {
        return status switch
        {
            ParticipationStatus.Registered => "Angemeldet",
            ParticipationStatus.Waitlist => "Warteliste",
            ParticipationStatus.Absent => "Abwesend",
            ParticipationStatus.AdditionalBike => "Zusatzbike",
            _ => status.ToString()
        };
    }
    
    private Color GetGroupColor(string? groupName)
    {
        if (string.IsNullOrEmpty(groupName)) return Color.Default;
        
        return groupName.ToUpper() switch
        {
            var g when g.Contains("SCHNELL") => Color.Error,
            var g when g.Contains("MITTEL") => Color.Warning,
            var g when g.Contains("RACER") => Color.Info,
            var g when g.Contains("INSTRUKTOR") => Color.Primary,
            _ => Color.Success
        };
    }
    
    private Color GetOccupancyColor(double percent)
    {
        if (percent >= 90) return Color.Error;
        if (percent >= 75) return Color.Warning;
        if (percent >= 50) return Color.Success;
        return Color.Primary;
    }
    
    private Color GetOccupancyMudColor(double percent)
    {
        if (percent >= 90) return Color.Error;
        if (percent >= 75) return Color.Warning;
        if (percent >= 50) return Color.Success;
        return Color.Primary;
    }
    #endregion
}
