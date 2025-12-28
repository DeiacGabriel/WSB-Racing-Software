using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using WSB_Management.Models;
using WSB_Management.Services;

namespace WSB_Management.Components.Pages;

public partial class CountryPage : IDisposable
{
    private bool _isDisposed;
    private CancellationTokenSource? _cts = new();

    [Inject] public MasterDataService Service { get; set; } = default!;
    [Inject] public ISnackbar Snackbar { get; set; } = default!;
    [Inject] public IWebHostEnvironment Environment { get; set; } = default!;

    private List<Country> Countries { get; set; } = new();
    private Country? SelectedCountry { get; set; }
    private bool IsNew => SelectedCountry?.Id == 0;
    private string? Message { get; set; }
    private string? SearchText { get; set; }
    private bool IsDragOver { get; set; }

    private IEnumerable<Country> FilteredCountries => string.IsNullOrWhiteSpace(SearchText)
        ? Countries
        : Countries.Where(c => 
            (c.Longtxt?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
            (c.Shorttxt?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        if (_isDisposed) return;
        try
        {
            Countries = await Service.GetCountriesAsync(_cts?.Token ?? CancellationToken.None);
        }
        catch (OperationCanceledException) { }
    }

    private void Select(Country country)
    {
        SelectedCountry = country;
        Message = null;
    }

    private void AddNew()
    {
        SelectedCountry = new Country();
        Message = null;
    }

    private void CloseDetail()
    {
        SelectedCountry = null;
        Message = null;
    }

    private async Task OnFileSelected(InputFileChangeEventArgs e)
    {
        if (SelectedCountry == null) return;

        var file = e.File;
        if (file.ContentType != "image/png")
        {
            Message = "Bitte nur PNG-Dateien hochladen.";
            return;
        }

        try
        {
            var fileName = $"{SelectedCountry.Shorttxt?.ToLower() ?? "flag"}.png";
            var path = Path.Combine(Environment.WebRootPath, "flags", fileName);
            
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            await using var stream = file.OpenReadStream();
            await using var fileStream = new FileStream(path, FileMode.Create);
            await stream.CopyToAsync(fileStream);

            SelectedCountry.FlagPath = $"/flags/{fileName}";
            Snackbar.Add("Flagge hochgeladen!", Severity.Success);
        }
        catch (Exception ex)
        {
            Message = $"Fehler beim Upload: {ex.Message}";
        }
    }

    private async Task Save()
    {
        if (_isDisposed || SelectedCountry == null) return;

        if (string.IsNullOrWhiteSpace(SelectedCountry.Shorttxt))
        {
            Message = "Bitte Kürzel eingeben.";
            return;
        }

        try
        {
            await Service.SaveCountryAsync(SelectedCountry, _cts?.Token ?? CancellationToken.None);
            Snackbar.Add("Land gespeichert!", Severity.Success);
            Message = "Erfolgreich gespeichert!";
            await LoadDataAsync();
            
            if (IsNew) CloseDetail();
        }
        catch (Exception ex)
        {
            Message = $"Fehler: {ex.Message}";
            Snackbar.Add($"Fehler: {ex.Message}", Severity.Error);
        }
    }

    private async Task Delete(long id)
    {
        if (_isDisposed) return;

        try
        {
            await Service.DeleteCountryAsync(id, _cts?.Token ?? CancellationToken.None);
            Snackbar.Add("Land gelöscht!", Severity.Warning);
            if (SelectedCountry?.Id == id) CloseDetail();
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Fehler: {ex.Message}", Severity.Error);
        }
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
