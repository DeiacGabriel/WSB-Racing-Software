using Microsoft.AspNetCore.Components;
using MudBlazor;
using WSB_Management.Models;
using WSB_Management.Services;

namespace WSB_Management.Components.Pages;

public partial class GroupPage : IDisposable
{
    private bool _isDisposed;
    private CancellationTokenSource? _cts = new();

    [Inject] public MasterDataService Service { get; set; } = default!;
    [Inject] public ISnackbar Snackbar { get; set; } = default!;

    private List<Gruppe> Gruppen { get; set; } = new();
    private Gruppe? SelectedGruppe { get; set; }
    private bool IsNew => SelectedGruppe?.Id == 0;
    private string? Message { get; set; }
    private string? AutoAssignMessage { get; set; }

    private string? MaxTimeString
    {
        get
        {
            if (SelectedGruppe?.MaxTimelap == null) return null;
            var ts = SelectedGruppe.MaxTimelap.Value;
            return $"{ts.Minutes}:{ts.Seconds:D2},{ts.Milliseconds / 10:D2}";
        }
        set
        {
            if (SelectedGruppe == null) return;
            if (TryParseTime(value, out var ts))
                SelectedGruppe.MaxTimelap = ts;
            else
                SelectedGruppe.MaxTimelap = null;
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

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        if (_isDisposed) return;
        try
        {
            Gruppen = await Service.GetGruppenAsync(_cts?.Token ?? CancellationToken.None);
        }
        catch (OperationCanceledException) { }
    }

    private void Select(Gruppe gruppe)
    {
        SelectedGruppe = gruppe;
        Message = null;
    }

    private void AddNew()
    {
        SelectedGruppe = new Gruppe();
        Message = null;
    }

    private void CloseDetail()
    {
        SelectedGruppe = null;
        Message = null;
    }

    private async Task Save()
    {
        if (_isDisposed || SelectedGruppe == null) return;

        if (string.IsNullOrWhiteSpace(SelectedGruppe.Name))
        {
            Message = "Bitte Namen eingeben.";
            return;
        }

        try
        {
            await Service.SaveGruppeAsync(SelectedGruppe, _cts?.Token ?? CancellationToken.None);
            Snackbar.Add("Gruppe gespeichert!", Severity.Success);
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
            await Service.DeleteGruppeAsync(id, _cts?.Token ?? CancellationToken.None);
            Snackbar.Add("Gruppe gelöscht!", Severity.Warning);
            if (SelectedGruppe?.Id == id) CloseDetail();
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Fehler: {ex.Message}", Severity.Error);
        }
    }

    private async Task AutoAssign()
    {
        if (_isDisposed) return;

        try
        {
            var count = await Service.AutoAssignCustomersToGroupsAsync(_cts?.Token ?? CancellationToken.None);
            AutoAssignMessage = $"{count} Kunden wurden automatisch ihren Gruppen zugewiesen.";
            Snackbar.Add(AutoAssignMessage, Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Fehler bei der Auto-Zuweisung: {ex.Message}", Severity.Error);
        }
    }

    private Color GetGroupColor(string? name)
    {
        return name switch
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
