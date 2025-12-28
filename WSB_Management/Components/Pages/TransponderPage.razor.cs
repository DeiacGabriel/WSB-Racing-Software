using Microsoft.AspNetCore.Components;
using MudBlazor;
using WSB_Management.Models;
using WSB_Management.Services;

namespace WSB_Management.Components.Pages;

public partial class TransponderPage : IDisposable
{
    private bool _isDisposed;
    private CancellationTokenSource? _cts = new();

    [Inject] public MasterDataService Service { get; set; } = default!;
    [Inject] public ISnackbar Snackbar { get; set; } = default!;

    private List<Transponder> Transponders { get; set; } = new();
    private Transponder? SelectedTransponder { get; set; }
    private bool IsNew => SelectedTransponder?.Id == 0;
    private string? Message { get; set; }
    private string? SearchText { get; set; }

    private IEnumerable<Transponder> FilteredTransponders => string.IsNullOrWhiteSpace(SearchText)
        ? Transponders
        : Transponders.Where(t =>
            (t.Bezeichung?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
            (t.Number?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        if (_isDisposed) return;
        try
        {
            Transponders = await Service.GetTranspondersAsync(_cts?.Token ?? CancellationToken.None);
        }
        catch (OperationCanceledException) { }
    }

    private void Select(Transponder transponder)
    {
        SelectedTransponder = transponder;
        Message = null;
    }

    private void AddNew()
    {
        SelectedTransponder = new Transponder();
        Message = null;
    }

    private void CloseDetail()
    {
        SelectedTransponder = null;
        Message = null;
    }

    private async Task Save()
    {
        if (_isDisposed || SelectedTransponder == null) return;

        if (string.IsNullOrWhiteSpace(SelectedTransponder.Bezeichung) && 
            string.IsNullOrWhiteSpace(SelectedTransponder.Number))
        {
            Message = "Bitte Bezeichnung oder Nummer eingeben.";
            return;
        }

        try
        {
            await Service.SaveTransponderAsync(SelectedTransponder, _cts?.Token ?? CancellationToken.None);
            Snackbar.Add("Transponder gespeichert!", Severity.Success);
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
            await Service.DeleteTransponderAsync(id, _cts?.Token ?? CancellationToken.None);
            Snackbar.Add("Transponder gelöscht!", Severity.Warning);
            if (SelectedTransponder?.Id == id) CloseDetail();
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
