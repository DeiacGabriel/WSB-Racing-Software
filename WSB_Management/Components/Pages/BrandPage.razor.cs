using Microsoft.AspNetCore.Components;
using MudBlazor;
using WSB_Management.Models;
using WSB_Management.Services;

namespace WSB_Management.Components.Pages;

public partial class BrandPage : IDisposable
{
    private bool _isDisposed;
    private CancellationTokenSource? _cts = new();

    [Inject] public MasterDataService Service { get; set; } = default!;
    [Inject] public ISnackbar Snackbar { get; set; } = default!;

    private List<Brand> Brands { get; set; } = new();
    private Brand? SelectedBrand { get; set; }
    private bool IsNew => SelectedBrand?.Id == 0;
    private string? Message { get; set; }
    private string? SearchText { get; set; }

    private IEnumerable<Brand> FilteredBrands => string.IsNullOrWhiteSpace(SearchText)
        ? Brands
        : Brands.Where(b => b.Name?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false);

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        if (_isDisposed) return;
        try
        {
            Brands = await Service.GetBrandsAsync(_cts?.Token ?? CancellationToken.None);
        }
        catch (OperationCanceledException) { }
    }

    private void Select(Brand brand)
    {
        SelectedBrand = brand;
        Message = null;
    }

    private void AddNew()
    {
        SelectedBrand = new Brand();
        Message = null;
    }

    private void CloseDetail()
    {
        SelectedBrand = null;
        Message = null;
    }

    private async Task Save()
    {
        if (_isDisposed || SelectedBrand == null) return;

        if (string.IsNullOrWhiteSpace(SelectedBrand.Name))
        {
            Message = "Bitte Namen eingeben.";
            return;
        }

        try
        {
            await Service.SaveBrandAsync(SelectedBrand, _cts?.Token ?? CancellationToken.None);
            Snackbar.Add("Marke gespeichert!", Severity.Success);
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
            await Service.DeleteBrandAsync(id, _cts?.Token ?? CancellationToken.None);
            Snackbar.Add("Marke gelöscht!", Severity.Warning);
            if (SelectedBrand?.Id == id) CloseDetail();
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
