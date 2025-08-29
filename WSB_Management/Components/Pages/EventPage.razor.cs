using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using WSB_Management.Data;
using WSB_Management.Models;
using WSB_Management.Services;

namespace WSB_Management.Components.Pages
{
    public partial class EventPage : IDisposable, IAsyncDisposable
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
        
        private Grid<Event>? grid;

        private Event? _selectedEvent;
        public Event? SelectedEvent
        {
            get => _selectedEvent;
            set
            {
                if (_selectedEvent != value && value is not null)
                {
                    _selectedEvent = value;
                    SafeStateHasChanged();
                }
            }
        }

        private Event _newEvent = new();
        public Event NewEvent
        {
            get => _newEvent;
            set
            {
                if (_newEvent != value)
                {
                    _newEvent = value;
                    SafeStateHasChanged();
                }
            }
        }
        
        private Event CurrentEvent => SelectedEvent ?? NewEvent;
        public List<Event> events { get; set; } = new();
        private string? Message { get; set; }
        
        // Cache für bessere Performance
        private void InvalidateEventsCache()
        {
            events = new List<Event>(); // Cache leeren, wird bei nächstem Zugriff neu geladen
        }

        [Inject] public WSBRacingDbContext _context { get; set; } = default!;
        [Inject] public EventService EventService { get; set; } = default!;

        public async Task SaveEvent()
        {
            if (_isDisposed) return;

            try
            {
                // Basic Validation
                if (string.IsNullOrWhiteSpace(CurrentEvent?.Name))
                {
                    Message = "Bitte Eventname eingeben.";
                    SafeStateHasChanged();
                    return;
                }

                var isNew = CurrentEvent.Id == 0;
                
                if (isNew)
                {
                    _context.Events.Add(CurrentEvent);
                }
                else
                {
                    // Prüfen ob die Entität bereits getrackt wird
                    var tracked = _context.Events.Local.FirstOrDefault(e => e.Id == CurrentEvent.Id);
                    if (tracked != null)
                    {
                        // Vorhandene getrackte Entität aktualisieren
                        _context.Entry(tracked).CurrentValues.SetValues(CurrentEvent);
                    }
                    else
                    {
                        // Entität anhängen und als geändert markieren
                        _context.Events.Attach(CurrentEvent);
                        _context.Entry(CurrentEvent).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    }
                }

                await _context.SaveChangesAsync(_cts?.Token ?? CancellationToken.None);

                if (_isDisposed) return;

                Message = $"Gespeichert: {CurrentEvent.Name}";
                InvalidateEventsCache(); // Cache invalidieren für bessere Performance
                
                // NavMenu über Event-Änderung benachrichtigen
                EventService.NotifyEventsChanged();

                if (_isDisposed) return;

                SelectedEvent = null;
                NewEvent = new Event();

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

        private async Task<GridDataProviderResult<Event>> EventDataProvider(GridDataProviderRequest<Event> request)
        {
            if (_isDisposed) return new GridDataProviderResult<Event> { Data = new List<Event>(), TotalCount = 0 };

            try
            {
                if (events is null || events.Count == 0)
                {
                    events = await _context.Events
                        .AsNoTracking()
                        .OrderBy(e => e.Id)
                        .ToListAsync(_cts?.Token ?? CancellationToken.None);
                }

                if (_isDisposed) return new GridDataProviderResult<Event> { Data = new List<Event>(), TotalCount = 0 };

                return await Task.FromResult(request.ApplyTo(events));
            }
            catch (ObjectDisposedException) 
            { 
                return new GridDataProviderResult<Event> { Data = new List<Event>(), TotalCount = 0 }; 
            }
            catch (TaskCanceledException) 
            { 
                return new GridDataProviderResult<Event> { Data = new List<Event>(), TotalCount = 0 }; 
            }
        }

        private void OnSelectedItemsChanged(IEnumerable<Event> selected)
        {
            var row = selected.FirstOrDefault();
            SelectedEvent = row ?? new Event();
            SafeStateHasChanged();
        }

        public async Task DeleteEventAsync(long? eventId)
        {
            if (_isDisposed) return;

            try
            {
                var eventItem = _context.Events.FirstOrDefault(e => e.Id == eventId);
                if (eventItem == null) return;

                _context.Events.Remove(eventItem);
                await _context.SaveChangesAsync(_cts?.Token ?? CancellationToken.None);

                if (_isDisposed) return;

                InvalidateEventsCache(); // Cache invalidieren für bessere Performance
                
                // NavMenu über Event-Änderung benachrichtigen
                EventService.NotifyEventsChanged();
                
                if (_isDisposed) return;
                
                if (SelectedEvent?.Id == eventId) SelectedEvent = null;
                NewEvent = new Event();
                
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
