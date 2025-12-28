using Microsoft.AspNetCore.Components;
using MudBlazor;
using WSB_Management.Models;
using WSB_Management.Services;

namespace WSB_Management.Components.Pages;

public partial class CustomerPage : IDisposable
{
    private bool _isDisposed;
    private CancellationTokenSource? _cts = new();

    [Inject] public MasterDataService MasterDataService { get; set; } = default!;
    [Inject] public ISnackbar Snackbar { get; set; } = default!;

    // Data
    private List<Customer> Customers { get; set; } = new();
    private List<Country> Countries { get; set; } = new();
    private List<Gruppe> Gruppen { get; set; } = new();
    private List<Transponder> Transponders { get; set; } = new();
    private List<BikeType> BikeTypes { get; set; } = new();

    // Selection & Editing
    private Customer? SelectedCustomer { get; set; }
    private bool IsNewCustomer => SelectedCustomer?.Id == 0;
    private string? Message { get; set; }

    // Filters
    private string? SearchText { get; set; }
    private string? GroupFilter { get; set; }

    // Computed
    private IEnumerable<Customer> FilteredCustomers
    {
        get
        {
            var query = Customers.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.ToLower();
                query = query.Where(c =>
                    (c.Contact?.Surname?.ToLower().Contains(search) ?? false) ||
                    (c.Contact?.Firstname?.ToLower().Contains(search) ?? false) ||
                    (c.Mail?.ToLower().Contains(search) ?? false) ||
                    (c.Startnumber?.ToLower().Contains(search) ?? false));
            }

            if (!string.IsNullOrWhiteSpace(GroupFilter))
            {
                query = query.Where(c => c.Gruppe?.Name == GroupFilter);
            }

            return query.OrderBy(c => c.Contact?.Surname);
        }
    }

    // Date pickers
    private DateTime? BirthDatePicker
    {
        get => SelectedCustomer?.Birthdate == default ? null : SelectedCustomer?.Birthdate;
        set { if (SelectedCustomer != null && value.HasValue) SelectedCustomer.Birthdate = value.Value; }
    }

    private DateTime? LastGuthabenAddPicker
    {
        get => SelectedCustomer?.LastGuthabenAdd == default ? null : SelectedCustomer?.LastGuthabenAdd;
        set { if (SelectedCustomer != null && value.HasValue) SelectedCustomer.LastGuthabenAdd = value.Value; }
    }

    private DateTime? ValidFromPicker
    {
        get => SelectedCustomer?.Validfrom == default ? null : SelectedCustomer?.Validfrom;
        set { if (SelectedCustomer != null && value.HasValue) SelectedCustomer.Validfrom = value.Value; }
    }

    private DateTime? LetzteBuchungPicker
    {
        get => SelectedCustomer?.letzteBuchung == default ? null : SelectedCustomer?.letzteBuchung;
        set { if (SelectedCustomer != null && value.HasValue) SelectedCustomer.letzteBuchung = value.Value; }
    }

    private DateTime? LetzterEinkaufPicker
    {
        get => SelectedCustomer?.letzterEinkauf == default ? null : SelectedCustomer?.letzterEinkauf;
        set { if (SelectedCustomer != null && value.HasValue) SelectedCustomer.letzterEinkauf = value.Value; }
    }

    // Best Time
    private string? BestTimeString
    {
        get
        {
            if (SelectedCustomer?.BestTime == null) return null;
            var ts = SelectedCustomer.BestTime.Value;
            return $"{ts.Minutes}:{ts.Seconds:D2},{ts.Milliseconds / 10:D2}";
        }
        set
        {
            if (SelectedCustomer == null) return;
            if (TryParseTime(value, out var ts))
                SelectedCustomer.BestTime = ts;
            else
                SelectedCustomer.BestTime = null;
        }
    }

    private static bool TryParseTime(string? input, out TimeSpan ts)
    {
        ts = TimeSpan.Zero;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var pattern = @"^(\d+):(\d{2}),(\d{1,2})$";
        var match = System.Text.RegularExpressions.Regex.Match(input, pattern);
        if (!match.Success) return false;

        var minutes = int.Parse(match.Groups[1].Value);
        var seconds = int.Parse(match.Groups[2].Value);
        var hundredths = match.Groups[3].Value;
        if (hundredths.Length == 1) hundredths += "0";

        ts = new TimeSpan(0, 0, minutes, seconds, int.Parse(hundredths) * 10);
        return true;
    }

    // BikeType
    private BikeType? SelectedBikeType
    {
        get => SelectedCustomer?.Bike?.BikeType;
        set
        {
            if (SelectedCustomer == null) return;
            EnsureBike();
            SelectedCustomer.Bike!.BikeType = value;
            SelectedCustomer.Bike.BikeTypeId = value?.Id ?? 0;
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        if (_isDisposed) return;
        var ct = _cts?.Token ?? CancellationToken.None;

        try
        {
            Customers = await MasterDataService.GetCustomersAsync(ct);
            Countries = await MasterDataService.GetCountriesAsync(ct);
            Gruppen = await MasterDataService.GetGruppenAsync(ct);
            Transponders = await MasterDataService.GetTranspondersAsync(ct);
            BikeTypes = await MasterDataService.GetBikeTypesAsync(ct);
        }
        catch (OperationCanceledException) { }
    }

    private void SelectCustomer(Customer customer)
    {
        SelectedCustomer = customer;
        EnsureSubObjects();
        Message = null;
    }

    private void AddNewCustomer()
    {
        SelectedCustomer = new Customer
        {
            Validfrom = DateTime.UtcNow,
            Birthdate = DateTime.UtcNow.AddYears(-30)
        };
        EnsureSubObjects();
        Message = null;
    }

    private void CloseDetail()
    {
        SelectedCustomer = null;
        Message = null;
    }

    private void EnsureSubObjects()
    {
        if (SelectedCustomer == null) return;
        SelectedCustomer.Contact ??= new Contact();
        SelectedCustomer.NotfallContact ??= new Contact();
        SelectedCustomer.Address ??= new Address();
        EnsureBike();
    }

    private void EnsureBike()
    {
        if (SelectedCustomer == null) return;
        SelectedCustomer.Bike ??= new Bike();
    }

    private async Task SaveCustomer()
    {
        if (_isDisposed || SelectedCustomer == null) return;

        try
        {
            // Validation
            if (string.IsNullOrWhiteSpace(SelectedCustomer.Contact?.Firstname) &&
                string.IsNullOrWhiteSpace(SelectedCustomer.Contact?.Surname))
            {
                Message = "Bitte Vor- oder Nachname eingeben.";
                return;
            }

            // References are already set via the object properties
            // The service will handle the actual saving

            await MasterDataService.SaveCustomerAsync(SelectedCustomer, _cts?.Token ?? CancellationToken.None);
            
            Snackbar.Add("Kunde gespeichert!", Severity.Success);
            Message = "Erfolgreich gespeichert!";
            
            await LoadDataAsync();
            
            // Bei neuem Kunden: In Liste finden
            if (IsNewCustomer && SelectedCustomer.Id > 0)
            {
                var saved = Customers.FirstOrDefault(c => c.Id == SelectedCustomer.Id);
                if (saved != null) SelectCustomer(saved);
            }
        }
        catch (Exception ex)
        {
            Message = $"Fehler: {ex.Message}";
            Snackbar.Add($"Fehler beim Speichern: {ex.Message}", Severity.Error);
        }
    }

    private async Task DeleteCustomer(long id)
    {
        if (_isDisposed) return;

        try
        {
            await MasterDataService.DeleteCustomerAsync(id, _cts?.Token ?? CancellationToken.None);
            Snackbar.Add("Kunde gelöscht!", Severity.Warning);
            
            if (SelectedCustomer?.Id == id)
                CloseDetail();
                
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Fehler beim Löschen: {ex.Message}", Severity.Error);
        }
    }

    private Color GetGroupColor(string? groupName)
    {
        return groupName switch
        {
            "A" or "Schnell" => Color.Error,
            "B" or "Mittel" => Color.Warning,
            "C" or "Langsam" => Color.Success,
            _ => Color.Default
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
