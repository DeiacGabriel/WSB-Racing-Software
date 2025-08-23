using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using WSB_Management.Data;
using WSB_Management.Models;

namespace WSB_Management.Components.Pages
{
    public partial class GroupPage : IDisposable, IAsyncDisposable
    {
        private bool _isDisposed;
        private CancellationTokenSource? _cts = new();
        
        public void Dispose()
        {
            _isDisposed = true;
            try { _cts?.Cancel(); } catch { }
            _cts?.Dispose();
            _cts = null;
            GC.SuppressFinalize(this);
        }
        
        public ValueTask DisposeAsync()
        {
            Dispose();
            return ValueTask.CompletedTask;
        }
        
        private void SafeStateHasChanged()
        {
            if (_isDisposed) return;
            try
            {
                if (!_isDisposed)
                    StateHasChanged();
            }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
        }
        
        private Grid<Gruppe>? grid;

        private Gruppe? _selectedGruppe;
        public Gruppe? SelectedGruppe
        {
            get => _selectedGruppe;
            set
            {
                if (_selectedGruppe != value && value is not null)
                {
                    _selectedGruppe = value;
                    SafeStateHasChanged();
                }
            }
        }

        private Gruppe _newGruppe = new();
        public Gruppe NewGruppe
        {
            get => _newGruppe;
            set
            {
                if (_newGruppe != value)
                {
                    _newGruppe = value;
                    SafeStateHasChanged();
                }
            }
        }
        
        private Gruppe CurrentGruppe => SelectedGruppe ?? NewGruppe;
        public List<Gruppe> gruppen { get; set; } = new();
        private string? Message { get; set; }
        private string? AutoAssignMessage { get; set; }
        
        // Für die Zeit-Eingabe mit TimeInput
        private TimeOnly? _maxTime;
        public TimeOnly? MaxTime
        {
            get => _maxTime;
            set
            {
                _maxTime = value;
                // TimeOnly zu TimeSpan konvertieren
                if (value.HasValue)
                {
                    var time = value.Value;
                    // TimeSpan aus Stunden, Minuten, Sekunden erstellen weil input nur ab Stunden geht
                    CurrentGruppe.MaxTimelap = new TimeSpan(0, 0, time.Hour, time.Minute, time.Second);
                }
                else
                {
                    CurrentGruppe.MaxTimelap = null;
                }
                SafeStateHasChanged();
            }
        }
        
        // Cache für bessere Performance
        private void InvalidateGruppenCache()
        {
            gruppen = new List<Gruppe>(); // Cache leeren, wird bei nächstem Zugriff neu geladen
        }

        private readonly WSBRacingDbContext _context;
        public GroupPage(WSBRacingDbContext context)
        {
            _context = context;
        }

        public async Task SaveGruppe()
        {
            if (_isDisposed) return;

            try
            {
                // Basic Validation
                if (string.IsNullOrWhiteSpace(CurrentGruppe?.Name))
                {
                    Message = "Bitte Gruppenname eingeben.";
                    SafeStateHasChanged();
                    return;
                }

                var isNew = CurrentGruppe.Id == 0;
                
                if (isNew)
                {
                    _context.Gruppes.Add(CurrentGruppe);
                }
                else
                {
                    // Prüfen ob die Entität bereits getrackt wird
                    var tracked = _context.Gruppes.Local.FirstOrDefault(g => g.Id == CurrentGruppe.Id);
                    if (tracked != null)
                    {
                        // Vorhandene getrackte Entität aktualisieren
                        _context.Entry(tracked).CurrentValues.SetValues(CurrentGruppe);
                    }
                    else
                    {
                        // Entität anhängen und als geändert markieren
                        _context.Gruppes.Attach(CurrentGruppe);
                        _context.Entry(CurrentGruppe).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    }
                }

                await _context.SaveChangesAsync(_cts?.Token ?? CancellationToken.None);

                if (_isDisposed) return;

                Message = $"Gespeichert: {CurrentGruppe.Name}";
                InvalidateGruppenCache(); // Cache invalidieren für bessere Performance

                if (_isDisposed) return;

                SelectedGruppe = null;
                NewGruppe = new Gruppe();
                _maxTime = null; // MaxTime auch zurücksetzen

                if (!_isDisposed && grid is not null)
                {
                    try
                    {
                        await grid.RefreshDataAsync();
                    }
                    catch (ObjectDisposedException) { return; }
                    catch (InvalidOperationException) { return; }
                    catch (TaskCanceledException) { return; }
                    catch (Exception) { return; }
                }

                SafeStateHasChanged();
            }
            catch (ObjectDisposedException) { return; }
            catch (InvalidOperationException) { return; }
            catch (TaskCanceledException) { return; }
            catch (Exception ex)
            {
                Message = $"Fehler beim Speichern: {ex.Message}";
                SafeStateHasChanged();
            }
        }

        private async Task<GridDataProviderResult<Gruppe>> GruppeDataProvider(GridDataProviderRequest<Gruppe> request)
        {
            if (_isDisposed) return new GridDataProviderResult<Gruppe> { Data = new List<Gruppe>(), TotalCount = 0 };

            try
            {
                if (gruppen is null || gruppen.Count == 0)
                {
                    gruppen = await _context.Gruppes
                        .AsNoTracking()
                        .OrderBy(g => g.Id)
                        .ToListAsync(_cts?.Token ?? CancellationToken.None);
                }

                if (_isDisposed) return new GridDataProviderResult<Gruppe> { Data = new List<Gruppe>(), TotalCount = 0 };

                return await Task.FromResult(request.ApplyTo(gruppen));
            }
            catch (ObjectDisposedException) 
            { 
                return new GridDataProviderResult<Gruppe> { Data = new List<Gruppe>(), TotalCount = 0 }; 
            }
            catch (TaskCanceledException) 
            { 
                return new GridDataProviderResult<Gruppe> { Data = new List<Gruppe>(), TotalCount = 0 }; 
            }
        }

        private void OnSelectedItemsChanged(IEnumerable<Gruppe> selected)
        {
            var row = selected.FirstOrDefault();
            SelectedGruppe = row ?? new Gruppe();
            
            // MaxTime aktualisieren wenn eine Gruppe ausgewählt wird
            if (SelectedGruppe?.MaxTimelap.HasValue == true)
            {
                var timeSpan = SelectedGruppe.MaxTimelap.Value;
                // TimeSpan zu TimeOnly konvertieren
                _maxTime = new TimeOnly(timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
            }
            else
            {
                _maxTime = null;
            }
            
            SafeStateHasChanged();
        }

        public async Task DeleteGruppeAsync(long? gruppeId)
        {
            if (_isDisposed) return;

            try
            {
                var gruppe = _context.Gruppes.FirstOrDefault(g => g.Id == gruppeId);
                if (gruppe == null) return;

                _context.Gruppes.Remove(gruppe);
                await _context.SaveChangesAsync(_cts?.Token ?? CancellationToken.None);

                if (_isDisposed) return;

                InvalidateGruppenCache(); // Cache invalidieren für bessere Performance
                
                if (_isDisposed) return;
                
                if (SelectedGruppe?.Id == gruppeId) SelectedGruppe = null;
                NewGruppe = new Gruppe();
                _maxTime = null; // MaxTime auch zurücksetzen
                
                if (!_isDisposed && grid is not null)
                {
                    try
                    {
                        await grid.RefreshDataAsync();
                    }
                    catch (ObjectDisposedException) { return; }
                    catch (InvalidOperationException) { return; }
                    catch (TaskCanceledException) { return; }
                    catch (Exception) { return; }
                }

                SafeStateHasChanged();
            }
            catch (ObjectDisposedException) { return; }
            catch (InvalidOperationException) { return; }
            catch (TaskCanceledException) { return; }
            catch (Exception) { return; }
        }
        
        public async Task AutoAssignCustomersToGroups()
        {
            if (_isDisposed) return;

            try
            {
                AutoAssignMessage = "Starte automatische Gruppeneinteilung...";
                SafeStateHasChanged();
                
                // Alle Kunden mit bester Zeit laden
                var customers = await _context.Customers
                    .Include(c => c.Gruppe)
                    .Where(c => c.BestTime.HasValue)
                    .OrderBy(c => c.BestTime)
                    .ToListAsync(_cts?.Token ?? CancellationToken.None);

                if (_isDisposed) return;

                // Alle Gruppen mit MaxTimelap laden, nach Zeit sortiert
                var sortedGroups = await _context.Gruppes
                    .Where(g => g.MaxTimelap.HasValue)
                    .OrderBy(g => g.MaxTimelap)
                    .ToListAsync(_cts?.Token ?? CancellationToken.None);

                if (_isDisposed) return;
                
                if (!sortedGroups.Any())
                {
                    AutoAssignMessage = "Keine Gruppen mit definierten Zeitlimits gefunden.";
                    SafeStateHasChanged();
                    return;
                }

                int assignedCount = 0;

                foreach (var customer in customers)
                {
                    if (_isDisposed) return;
                    
                    if (!customer.BestTime.HasValue) continue;
                    
                    // Finde die passende Gruppe für diese Zeit
                    var targetGroup = FindBestGroupForTime(customer.BestTime.Value, sortedGroups);
                    
                    if (targetGroup != null && (customer.Gruppe == null || customer.Gruppe.Id != targetGroup.Id))
                    {
                        customer.Gruppe = targetGroup;
                        _context.Entry(customer).Property(c => c.Gruppe).IsModified = true;
                        assignedCount++;
                    }
                }

                if (assignedCount > 0)
                {
                    await _context.SaveChangesAsync(_cts?.Token ?? CancellationToken.None);
                    AutoAssignMessage = $"Erfolgreich {assignedCount} Kunden in neue Gruppen eingeteilt.";
                }
                else
                {
                    AutoAssignMessage = "Alle Kunden waren bereits in den richtigen Gruppen.";
                }

                SafeStateHasChanged();
            }
            catch (ObjectDisposedException) { return; }
            catch (InvalidOperationException) { return; }
            catch (TaskCanceledException) { return; }
            catch (Exception ex)
            {
                AutoAssignMessage = $"Fehler bei der automatischen Einteilung: {ex.Message}";
                SafeStateHasChanged();
            }
        }
        
        private Gruppe? FindBestGroupForTime(TimeSpan customerTime, List<Gruppe> sortedGroups)
        {
            // Finde die erste Gruppe, deren MaxTimelap >= der Kundenzeit ist
            return sortedGroups.FirstOrDefault(g => g.MaxTimelap.HasValue && customerTime <= g.MaxTimelap.Value);
        }
    }
}
