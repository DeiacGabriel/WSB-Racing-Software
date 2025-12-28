using Microsoft.AspNetCore.Components;
using MudBlazor;
using WSB_Management.Models;
using WSB_Management.Services;

namespace WSB_Management.Components.Pages;

public partial class TypePage : IDisposable
{
    private bool _isDisposed;
    private CancellationTokenSource? _cts = new();

    [Inject] public MasterDataService Service { get; set; } = default!;
    [Inject] public ISnackbar Snackbar { get; set; } = default!;

    // Data
    private List<Klasse> Klassen { get; set; } = new();
    private List<BikeType> BikeTypes { get; set; } = new();
    private List<Brand> Brands { get; set; } = new();

    // Selection
    private Klasse? SelectedKlasse { get; set; }
    private BikeType? SelectedBikeType { get; set; }
    private string? SearchText { get; set; }

    // BikeType helpers
    private Brand? SelectedBrand
    {
        get => SelectedBikeType?.Brand ?? Brands.FirstOrDefault(b => b.Id == SelectedBikeType?.BrandId);
        set
        {
            if (SelectedBikeType != null)
            {
                SelectedBikeType.Brand = value;
                SelectedBikeType.BrandId = value?.Id ?? 0;
            }
        }
    }

    private Klasse? SelectedBikeTypeKlasse
    {
        get => SelectedBikeType?.Klasse ?? Klassen.FirstOrDefault(k => k.Id == SelectedBikeType?.KlasseId);
        set
        {
            if (SelectedBikeType != null)
            {
                SelectedBikeType.Klasse = value;
                SelectedBikeType.KlasseId = value?.Id ?? 0;
            }
        }
    }

    private IEnumerable<BikeType> FilteredBikeTypes => string.IsNullOrWhiteSpace(SearchText)
        ? BikeTypes
        : BikeTypes.Where(bt =>
            (bt.Name?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
            (bt.Brand?.Name?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
            (bt.Klasse?.Bezeichnung?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));

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
            Klassen = await Service.GetKlassenAsync(ct);
            BikeTypes = await Service.GetBikeTypesAsync(ct);
            Brands = await Service.GetBrandsAsync(ct);
        }
        catch (OperationCanceledException) { }
    }

    #region Klasse

    private void SelectKlasse(Klasse klasse)
    {
        SelectedKlasse = klasse;
    }

    private void AddNewKlasse()
    {
        SelectedKlasse = new Klasse();
    }

    private async Task SaveKlasse()
    {
        if (_isDisposed || SelectedKlasse == null) return;

        if (string.IsNullOrWhiteSpace(SelectedKlasse.Bezeichnung))
        {
            Snackbar.Add("Bitte Bezeichnung eingeben.", Severity.Warning);
            return;
        }

        try
        {
            await Service.SaveKlasseAsync(SelectedKlasse, _cts?.Token ?? CancellationToken.None);
            Snackbar.Add("Klasse gespeichert!", Severity.Success);
            await LoadDataAsync();
            SelectedKlasse = null;
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Fehler: {ex.Message}", Severity.Error);
        }
    }

    private async Task DeleteKlasse(long id)
    {
        if (_isDisposed) return;

        try
        {
            await Service.DeleteKlasseAsync(id, _cts?.Token ?? CancellationToken.None);
            Snackbar.Add("Klasse gelöscht!", Severity.Warning);
            if (SelectedKlasse?.Id == id) SelectedKlasse = null;
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Fehler: {ex.Message}", Severity.Error);
        }
    }

    #endregion

    #region BikeType

    private void SelectBikeType(BikeType bikeType)
    {
        SelectedBikeType = bikeType;
    }

    private void AddNewBikeType()
    {
        SelectedBikeType = new BikeType();
    }

    private async Task SaveBikeType()
    {
        if (_isDisposed || SelectedBikeType == null) return;

        if (string.IsNullOrWhiteSpace(SelectedBikeType.Name))
        {
            Snackbar.Add("Bitte Typ-Namen eingeben.", Severity.Warning);
            return;
        }

        if (SelectedBikeType.BrandId == 0)
        {
            Snackbar.Add("Bitte Marke auswählen.", Severity.Warning);
            return;
        }

        try
        {
            await Service.SaveBikeTypeAsync(SelectedBikeType, _cts?.Token ?? CancellationToken.None);
            Snackbar.Add("Motorrad Typ gespeichert!", Severity.Success);
            await LoadDataAsync();
            SelectedBikeType = null;
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Fehler: {ex.Message}", Severity.Error);
        }
    }

    private async Task DeleteBikeType(long id)
    {
        if (_isDisposed) return;

        try
        {
            await Service.DeleteBikeTypeAsync(id, _cts?.Token ?? CancellationToken.None);
            Snackbar.Add("Motorrad Typ gelöscht!", Severity.Warning);
            if (SelectedBikeType?.Id == id) SelectedBikeType = null;
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Fehler: {ex.Message}", Severity.Error);
        }
    }

    #endregion

    public void Dispose()
    {
        _isDisposed = true;
        try { _cts?.Cancel(); } catch { }
        _cts?.Dispose();
        _cts = null;
        GC.SuppressFinalize(this);
    }
}
