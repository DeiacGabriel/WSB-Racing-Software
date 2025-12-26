using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using WSB_Management.Data;
using WSB_Management.Models;

namespace WSB_Management.Components.Pages
{
    public partial class TransponderPage : IDisposable, IAsyncDisposable
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
        
        private Grid<Transponder>? grid;

        private Transponder? _selectedTransponder;
        public Transponder? SelectedTransponder
        {
            get => _selectedTransponder;
            set
            {
                if (_selectedTransponder != value && value is not null)
                {
                    _selectedTransponder = value;
                    SafeStateHasChanged();
                }
            }
        }

        private Transponder _newTransponder = new();
        public Transponder NewTransponder
        {
            get => _newTransponder;
            set
            {
                if (_newTransponder != value)
                {
                    _newTransponder = value;
                    SafeStateHasChanged();
                }
            }
        }
        
        private Transponder CurrentTransponder => SelectedTransponder ?? NewTransponder;
        public List<Transponder> transponders { get; set; } = new();
        private string? Message { get; set; }
        
        // Cache für bessere Performance
        private void InvalidateTranspondersCache()
        {
            transponders = new List<Transponder>(); // Cache leeren, wird bei nächstem Zugriff neu geladen
        }

        [Inject] public IDbContextFactory<WSBRacingDbContext> _contextFactory { get; set; } = default!;

        public async Task SaveTransponder()
        {
            if (_isDisposed) return;

            try
            {
                // Basic Validation
                if (string.IsNullOrWhiteSpace(CurrentTransponder?.Bezeichung))
                {
                    Message = "Bitte Bezeichnung eingeben.";
                    SafeStateHasChanged();
                    return;
                }

                var isNew = CurrentTransponder.Id == 0;
                
                await using var context = await _contextFactory.CreateDbContextAsync();
                if (isNew)
                {
                    context.Transponders.Add(CurrentTransponder);
                }
                else
                {
                    context.Transponders.Update(CurrentTransponder);
                }

                await context.SaveChangesAsync(_cts?.Token ?? CancellationToken.None);

                if (_isDisposed) return;

                Message = $"Gespeichert: {CurrentTransponder.Bezeichung}";
                InvalidateTranspondersCache(); // Cache invalidieren für bessere Performance

                if (_isDisposed) return;

                SelectedTransponder = null;
                NewTransponder = new Transponder();

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

        private async Task<GridDataProviderResult<Transponder>> TransponderDataProvider(GridDataProviderRequest<Transponder> request)
        {
            if (_isDisposed) return new GridDataProviderResult<Transponder> { Data = new List<Transponder>(), TotalCount = 0 };

            try
            {
                if (transponders is null || transponders.Count == 0)
                {
                    await using var context = await _contextFactory.CreateDbContextAsync();
                    transponders = await context.Transponders
                        .AsNoTracking()
                        .OrderBy(t => t.Id)
                        .ToListAsync(_cts?.Token ?? CancellationToken.None);
                }

                if (_isDisposed) return new GridDataProviderResult<Transponder> { Data = new List<Transponder>(), TotalCount = 0 };

                return await Task.FromResult(request.ApplyTo(transponders));
            }
            catch (ObjectDisposedException) 
            { 
                return new GridDataProviderResult<Transponder> { Data = new List<Transponder>(), TotalCount = 0 }; 
            }
            catch (TaskCanceledException) 
            { 
                return new GridDataProviderResult<Transponder> { Data = new List<Transponder>(), TotalCount = 0 }; 
            }
        }

        private void OnSelectedItemsChanged(IEnumerable<Transponder> selected)
        {
            var row = selected.FirstOrDefault();
            SelectedTransponder = row ?? new Transponder();
            SafeStateHasChanged();
        }

        public async Task DeleteTransponderAsync(long? transponderId)
        {
            if (_isDisposed) return;

            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                var transponder = await context.Transponders.FirstOrDefaultAsync(t => t.Id == transponderId, _cts?.Token ?? CancellationToken.None);
                if (transponder == null) return;

                context.Transponders.Remove(transponder);
                await context.SaveChangesAsync(_cts?.Token ?? CancellationToken.None);

                if (_isDisposed) return;

                InvalidateTranspondersCache(); // Cache invalidieren für bessere Performance
                
                if (_isDisposed) return;
                
                if (SelectedTransponder?.Id == transponderId) SelectedTransponder = null;
                NewTransponder = new Transponder();
                
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
    }
}
