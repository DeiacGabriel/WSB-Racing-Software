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

        [Inject] public WSBRacingDbContext _context { get; set; } = default!;

        public async Task SaveGruppe()
        {
            if (_isDisposed) return;

            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(CurrentGruppe?.Name))
                {
                    Message = "Bitte Gruppenname eingeben.";
                    SafeStateHasChanged();
                    return;
                }

                var isNew = CurrentGruppe.Id == 0;
                
                if (isNew)
                {
                    // Neue Gruppe erstellen
                    var newGruppe = new Gruppe
                    {
                        Name = CurrentGruppe.Name,
                        MaxTimelap = CurrentGruppe.MaxTimelap
                    };
                    _context.Gruppes.Add(newGruppe);
                }
                else
                {
                    // Existierende Gruppe aktualisieren
                    var existingGruppe = await _context.Gruppes
                        .FirstOrDefaultAsync(g => g.Id == CurrentGruppe.Id, _cts?.Token ?? CancellationToken.None);
                        
                    if (existingGruppe == null)
                    {
                        Message = "❌ Gruppe nicht gefunden.";
                        SafeStateHasChanged();
                        return;
                    }
                    
                    // Eigenschaften aktualisieren
                    existingGruppe.Name = CurrentGruppe.Name;
                    existingGruppe.MaxTimelap = CurrentGruppe.MaxTimelap;
                    
                    _context.Gruppes.Update(existingGruppe);
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
            catch (DbUpdateException ex)
            {
                Message = $"❌ Datenbankfehler beim Speichern: {ex.GetBaseException().Message}";
                SafeStateHasChanged();
            }
            catch (Exception ex)
            {
                Message = $"❌ Fehler beim Speichern: {ex.Message}";
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
                var gruppe = await _context.Gruppes
                    .FirstOrDefaultAsync(g => g.Id == gruppeId, _cts?.Token ?? CancellationToken.None);
                    
                if (gruppe == null) 
                {
                    Message = "❌ Gruppe nicht gefunden.";
                    SafeStateHasChanged();
                    return;
                }

                // Prüfen ob Kunden dieser Gruppe zugeordnet sind
                var customersInGroup = await _context.Customers
                    .Where(c => c.Gruppe != null && c.Gruppe.Id == gruppeId)
                    .CountAsync(_cts?.Token ?? CancellationToken.None);

                if (customersInGroup > 0)
                {
                    Message = $"❌ Gruppe kann nicht gelöscht werden. {customersInGroup} Kunden sind noch dieser Gruppe zugeordnet.";
                    SafeStateHasChanged();
                    return;
                }

                _context.Gruppes.Remove(gruppe);
                await _context.SaveChangesAsync(_cts?.Token ?? CancellationToken.None);

                if (_isDisposed) return;

                Message = $"✅ Gruppe '{gruppe.Name}' erfolgreich gelöscht.";
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
            catch (DbUpdateException ex)
            {
                Message = $"❌ Datenbankfehler beim Löschen: {ex.GetBaseException().Message}";
                SafeStateHasChanged();
            }
            catch (Exception ex)
            {
                Message = $"❌ Fehler beim Löschen: {ex.Message}";
                SafeStateHasChanged();
            }
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
                        // Gruppe korrekt zuweisen - sicherstellen dass die Gruppe getrackt ist
                        var trackedGroup = await _context.Gruppes
                            .FirstOrDefaultAsync(g => g.Id == targetGroup.Id, _cts?.Token ?? CancellationToken.None);
                            
                        if (trackedGroup != null)
                        {
                            customer.Gruppe = trackedGroup;
                            _context.Customers.Update(customer);
                            assignedCount++;
                        }
                    }
                }

                if (assignedCount > 0)
                {
                    await _context.SaveChangesAsync(_cts?.Token ?? CancellationToken.None);
                    AutoAssignMessage = $"✅ Erfolgreich {assignedCount} Kunden in neue Gruppen eingeteilt.";
                }
                else
                {
                    AutoAssignMessage = "ℹ️ Alle Kunden waren bereits in den richtigen Gruppen.";
                }

                SafeStateHasChanged();
            }
            catch (ObjectDisposedException) { return; }
            catch (InvalidOperationException) { return; }
            catch (TaskCanceledException) { return; }
            catch (DbUpdateException ex)
            {
                AutoAssignMessage = $"❌ Datenbankfehler bei der automatischen Einteilung: {ex.GetBaseException().Message}";
                SafeStateHasChanged();
            }
            catch (Exception ex)
            {
                AutoAssignMessage = $"❌ Fehler bei der automatischen Einteilung: {ex.Message}";
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
