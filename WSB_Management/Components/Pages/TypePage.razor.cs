using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using WSB_Management.Data;
using WSB_Management.Models;

namespace WSB_Management.Components.Pages
{
    public partial class TypePage : IDisposable, IAsyncDisposable
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
        
        [Inject] public IDbContextFactory<WSBRacingDbContext> _contextFactory { get; set; } = default!;
        
        // Klasse Grid & Properties
        private Grid<Klasse>? klasseGrid;
        private Klasse? _selectedKlasse;
        public Klasse? SelectedKlasse
        {
            get => _selectedKlasse;
            set
            {
                if (_selectedKlasse != value && value is not null)
                {
                    _selectedKlasse = value;
                    SafeStateHasChanged();
                }
            }
        }

        private Klasse _newKlasse = new();
        public Klasse NewKlasse
        {
            get => _newKlasse;
            set
            {
                if (_newKlasse != value)
                {
                    _newKlasse = value;
                    SafeStateHasChanged();
                }
            }
        }
        
        private Klasse CurrentKlasse => SelectedKlasse ?? NewKlasse;
        public List<Klasse> klassen { get; set; } = new();
        private string? KlasseMessage { get; set; }
        
        // BikeType Grid & Properties
        private Grid<BikeType>? bikeTypeGrid;
        private BikeType? _selectedBikeType;
        public BikeType? SelectedBikeType
        {
            get => _selectedBikeType;
            set
            {
                if (_selectedBikeType != value && value is not null)
                {
                    _selectedBikeType = value;
                    CurrentBikeTypeBrand = value.Brand;
                    CurrentBikeTypeKlasseId = value.KlasseId;
                    SafeStateHasChanged();
                }
            }
        }

        private BikeType _newBikeType = new();
        public BikeType NewBikeType
        {
            get => _newBikeType;
            set
            {
                if (_newBikeType != value)
                {
                    _newBikeType = value;
                    SafeStateHasChanged();
                }
            }
        }
        
        private BikeType CurrentBikeType => SelectedBikeType ?? NewBikeType;
        public List<BikeType> bikeTypes { get; set; } = new();
        private string? BikeTypeMessage { get; set; }
        
        // Helper properties for BikeType
        private Brand? _currentBikeTypeBrand;
        public Brand? CurrentBikeTypeBrand
        {
            get => _currentBikeTypeBrand;
            set
            {
                _currentBikeTypeBrand = value;
                if (value != null)
                {
                    CurrentBikeType.BrandId = value.Id;
                    CurrentBikeType.Brand = value;
                }
            }
        }
        
        private long _currentBikeTypeKlasseId;
        public long CurrentBikeTypeKlasseId
        {
            get => _currentBikeTypeKlasseId;
            set
            {
                _currentBikeTypeKlasseId = value;
                CurrentBikeType.KlasseId = value;
                var klasse = klassen.FirstOrDefault(k => k.Id == value);
                if (klasse != null)
                {
                    CurrentBikeType.Klasse = klasse;
                }
            }
        }
        
        public List<Brand> brands { get; set; } = new();
        
        protected override async Task OnInitializedAsync()
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            if (_isDisposed) return;

            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                klassen = await context.Klasses.OrderBy(k => k.Bezeichnung).ToListAsync(_cts?.Token ?? CancellationToken.None);
                brands = await context.Brands.OrderBy(b => b.Name).ToListAsync(_cts?.Token ?? CancellationToken.None);
                bikeTypes = await context.BikeTypes
                    .Include(bt => bt.Brand)
                    .Include(bt => bt.Klasse)
                    .OrderBy(bt => bt.Brand!.Name)
                    .ThenBy(bt => bt.Name)
                    .ToListAsync(_cts?.Token ?? CancellationToken.None);
            }
            catch (OperationCanceledException) { }
            catch (ObjectDisposedException) { }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading data: {ex.Message}");
            }
        }

        // Klasse DataProvider
        private async Task<GridDataProviderResult<Klasse>> KlasseDataProvider(GridDataProviderRequest<Klasse> request)
        {
            if (_isDisposed) return new GridDataProviderResult<Klasse> { Data = null, TotalCount = 0 };

            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.Klasses.AsQueryable();

                // Filtering can be added if needed in future

                if (request.Sorting?.Any() == true)
                {
                    var sorting = request.Sorting.First();
                    query = sorting.SortDirection == SortDirection.Ascending
                        ? query.OrderBy(k => k.Bezeichnung)
                        : query.OrderByDescending(k => k.Bezeichnung);
                }
                else
                {
                    query = query.OrderBy(k => k.Bezeichnung);
                }

                var totalCount = await query.CountAsync(_cts?.Token ?? CancellationToken.None);
                var data = await query.ToListAsync(_cts?.Token ?? CancellationToken.None);

                return new GridDataProviderResult<Klasse> { Data = data, TotalCount = totalCount };
            }
            catch (OperationCanceledException)
            {
                return new GridDataProviderResult<Klasse> { Data = null, TotalCount = 0 };
            }
            catch (ObjectDisposedException)
            {
                return new GridDataProviderResult<Klasse> { Data = null, TotalCount = 0 };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in KlasseDataProvider: {ex.Message}");
                return new GridDataProviderResult<Klasse> { Data = null, TotalCount = 0 };
            }
        }

        // BikeType DataProvider
        private async Task<GridDataProviderResult<BikeType>> BikeTypeDataProvider(GridDataProviderRequest<BikeType> request)
        {
            if (_isDisposed) return new GridDataProviderResult<BikeType> { Data = null, TotalCount = 0 };

            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.BikeTypes
                    .Include(bt => bt.Brand)
                    .Include(bt => bt.Klasse)
                    .AsQueryable();

                // Filtering can be added if needed in future

                if (request.Sorting?.Any() == true)
                {
                    var sorting = request.Sorting.First();
                    query = sorting.SortDirection == SortDirection.Ascending
                        ? query.OrderBy(bt => bt.Brand!.Name).ThenBy(bt => bt.Name)
                        : query.OrderByDescending(bt => bt.Brand!.Name).ThenByDescending(bt => bt.Name);
                }
                else
                {
                    query = query.OrderBy(bt => bt.Brand!.Name).ThenBy(bt => bt.Name);
                }

                var totalCount = await query.CountAsync(_cts?.Token ?? CancellationToken.None);
                var data = await query.ToListAsync(_cts?.Token ?? CancellationToken.None);

                return new GridDataProviderResult<BikeType> { Data = data, TotalCount = totalCount };
            }
            catch (OperationCanceledException)
            {
                return new GridDataProviderResult<BikeType> { Data = null, TotalCount = 0 };
            }
            catch (ObjectDisposedException)
            {
                return new GridDataProviderResult<BikeType> { Data = null, TotalCount = 0 };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in BikeTypeDataProvider: {ex.Message}");
                return new GridDataProviderResult<BikeType> { Data = null, TotalCount = 0 };
            }
        }

        // Klasse Methods
        private async Task OnKlasseSelectedItemsChanged(HashSet<Klasse> selectedItems)
        {
            var selected = selectedItems?.FirstOrDefault();
            if (selected != null)
            {
                SelectedKlasse = selected;
            }
        }

        private void AddNewKlasse()
        {
            NewKlasse = new Klasse();
            SelectedKlasse = null;
            KlasseMessage = null;
            SafeStateHasChanged();
        }

        public async Task SaveKlasseAsync()
        {
            if (_isDisposed) return;

            try
            {
                if (string.IsNullOrWhiteSpace(CurrentKlasse?.Bezeichnung))
                {
                    KlasseMessage = "Bitte Bezeichnung eingeben.";
                    SafeStateHasChanged();
                    return;
                }

                var isNew = CurrentKlasse.Id == 0;
                
                await using var context = await _contextFactory.CreateDbContextAsync();
                if (isNew)
                {
                    context.Klasses.Add(CurrentKlasse);
                }
                else
                {
                    context.Klasses.Update(CurrentKlasse);
                }

                await context.SaveChangesAsync(_cts?.Token ?? CancellationToken.None);
                
                KlasseMessage = isNew ? "Klasse erfolgreich erstellt." : "Klasse erfolgreich aktualisiert.";
                
                await LoadDataAsync();
                await klasseGrid!.RefreshDataAsync();
                
                AddNewKlasse();
            }
            catch (OperationCanceledException) { }
            catch (ObjectDisposedException) { }
            catch (Exception ex)
            {
                KlasseMessage = $"Fehler beim Speichern: {ex.Message}";
                Console.WriteLine($"Error saving Klasse: {ex.Message}");
            }
            
            SafeStateHasChanged();
        }

        public async Task DeleteKlasseAsync(long id)
        {
            if (_isDisposed) return;

            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                var klasse = await context.Klasses.FindAsync(id);
                if (klasse != null)
                {
                    // Check if Klasse is used in BikeTypes
                    var hasReferences = await context.BikeTypes.AnyAsync(bt => bt.KlasseId == id);
                    if (hasReferences)
                    {
                        KlasseMessage = "Klasse kann nicht gelöscht werden, da sie in Motorrad-Typen verwendet wird.";
                        SafeStateHasChanged();
                        return;
                    }

                    context.Klasses.Remove(klasse);
                    await context.SaveChangesAsync();
                    
                    KlasseMessage = "Klasse erfolgreich gelöscht.";
                    
                    await LoadDataAsync();
                    await klasseGrid!.RefreshDataAsync();
                    
                    if (SelectedKlasse?.Id == id)
                    {
                        AddNewKlasse();
                    }
                }
            }
            catch (Exception ex)
            {
                KlasseMessage = $"Fehler beim Löschen: {ex.Message}";
                Console.WriteLine($"Error deleting Klasse: {ex.Message}");
            }
            
            SafeStateHasChanged();
        }

        // BikeType Methods
        private async Task OnBikeTypeSelectedItemsChanged(HashSet<BikeType> selectedItems)
        {
            var selected = selectedItems?.FirstOrDefault();
            if (selected != null)
            {
                SelectedBikeType = selected;
            }
        }

        private void AddNewBikeType()
        {
            NewBikeType = new BikeType();
            SelectedBikeType = null;
            CurrentBikeTypeBrand = null;
            CurrentBikeTypeKlasseId = 0;
            BikeTypeMessage = null;
            SafeStateHasChanged();
        }

        public async Task SaveBikeTypeAsync()
        {
            if (_isDisposed) return;

            try
            {
                if (string.IsNullOrWhiteSpace(CurrentBikeType?.Name))
                {
                    BikeTypeMessage = "Bitte Typ Name eingeben.";
                    SafeStateHasChanged();
                    return;
                }

                if (CurrentBikeType.BrandId == 0)
                {
                    BikeTypeMessage = "Bitte Marke auswählen.";
                    SafeStateHasChanged();
                    return;
                }

                if (CurrentBikeType.KlasseId == 0)
                {
                    BikeTypeMessage = "Bitte Klasse auswählen.";
                    SafeStateHasChanged();
                    return;
                }

                var isNew = CurrentBikeType.Id == 0;
                
                await using var context = await _contextFactory.CreateDbContextAsync();
                
                // Nur BikeType speichern, nicht Brand/Klasse (die existieren bereits)
                var bikeType = new BikeType
                {
                    Id = CurrentBikeType.Id,
                    Name = CurrentBikeType.Name,
                    BrandId = CurrentBikeType.BrandId,
                    KlasseId = CurrentBikeType.KlasseId
                };
                
                if (isNew)
                {
                    context.BikeTypes.Add(bikeType);
                }
                else
                {
                    context.BikeTypes.Update(bikeType);
                }

                await context.SaveChangesAsync(_cts?.Token ?? CancellationToken.None);
                
                BikeTypeMessage = isNew ? "Motorrad-Typ erfolgreich erstellt." : "Motorrad-Typ erfolgreich aktualisiert.";
                
                await LoadDataAsync();
                await bikeTypeGrid!.RefreshDataAsync();
                
                AddNewBikeType();
            }
            catch (Exception ex)
            {
                BikeTypeMessage = $"Fehler beim Speichern: {ex.Message}";
                Console.WriteLine($"Error saving BikeType: {ex.Message}");
            }
            
            SafeStateHasChanged();
        }

        public async Task DeleteBikeTypeAsync(long id)
        {
            if (_isDisposed) return;

            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                var bikeType = await context.BikeTypes.FindAsync(id);
                if (bikeType != null)
                {
                    // Check if BikeType is used in Bikes
                    var hasReferences = await context.Bikes.AnyAsync(b => b.BikeTypeId == id);
                    if (hasReferences)
                    {
                        BikeTypeMessage = "Motorrad-Typ kann nicht gelöscht werden, da er in Bikes verwendet wird.";
                        SafeStateHasChanged();
                        return;
                    }

                    context.BikeTypes.Remove(bikeType);
                    await context.SaveChangesAsync();
                    
                    BikeTypeMessage = "Motorrad-Typ erfolgreich gelöscht.";
                    
                    await LoadDataAsync();
                    await bikeTypeGrid!.RefreshDataAsync();
                    
                    if (SelectedBikeType?.Id == id)
                    {
                        AddNewBikeType();
                    }
                }
            }
            catch (Exception ex)
            {
                BikeTypeMessage = $"Fehler beim Löschen: {ex.Message}";
                Console.WriteLine($"Error deleting BikeType: {ex.Message}");
            }
            
            SafeStateHasChanged();
        }
    }
}
