﻿using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using WSB_Management.Data;
using WSB_Management.Models;

namespace WSB_Management.Components.Pages
{
    public partial class BrandPage : IDisposable, IAsyncDisposable
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
        
        private Grid<Brand>? grid;

        private Brand? _selectedBrand;
        public Brand? SelectedBrand
        {
            get => _selectedBrand;
            set
            {
                if (_selectedBrand != value && value is not null)
                {
                    _selectedBrand = value;
                    SafeStateHasChanged();
                }
            }
        }

        private Brand _newBrand = new();
        public Brand NewBrand
        {
            get => _newBrand;
            set
            {
                if (_newBrand != value)
                {
                    _newBrand = value;
                    SafeStateHasChanged();
                }
            }
        }
        
        private Brand CurrentBrand => SelectedBrand ?? NewBrand;
        public List<Brand> brands { get; set; } = new();
        private string? Message { get; set; }
        
        // Cache für bessere Performance
        private void InvalidateBrandsCache()
        {
            brands = new List<Brand>(); // Cache leeren, wird bei nächstem Zugriff neu geladen
        }

        [Inject] public WSBRacingDbContext _context { get; set; } = default!;

        public async Task SaveBrand()
        {
            if (_isDisposed) return;

            try
            {
                // Basic Validation
                if (string.IsNullOrWhiteSpace(CurrentBrand?.Name))
                {
                    Message = "Bitte Markenname eingeben.";
                    SafeStateHasChanged();
                    return;
                }

                var isNew = CurrentBrand.Id == 0;
                
                if (isNew)
                {
                    _context.Brands.Add(CurrentBrand);
                }
                else
                {
                    // Prüfen ob die Entität bereits getrackt wird
                    var tracked = _context.Brands.Local.FirstOrDefault(b => b.Id == CurrentBrand.Id);
                    if (tracked != null)
                    {
                        // Vorhandene getrackte Entität aktualisieren
                        _context.Entry(tracked).CurrentValues.SetValues(CurrentBrand);
                    }
                    else
                    {
                        // Entität anhängen und als geändert markieren
                        _context.Brands.Attach(CurrentBrand);
                        _context.Entry(CurrentBrand).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    }
                }

                await _context.SaveChangesAsync(_cts?.Token ?? CancellationToken.None);

                if (_isDisposed) return;

                Message = $"Gespeichert: {CurrentBrand.Name}";
                InvalidateBrandsCache(); // Cache invalidieren für bessere Performance

                if (_isDisposed) return;

                SelectedBrand = null;
                NewBrand = new Brand();

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

        private async Task<GridDataProviderResult<Brand>> BrandDataProvider(GridDataProviderRequest<Brand> request)
        {
            if (_isDisposed) return new GridDataProviderResult<Brand> { Data = new List<Brand>(), TotalCount = 0 };

            try
            {
                if (brands is null || brands.Count == 0)
                {
                    brands = await _context.Brands
                        .AsNoTracking()
                        .OrderBy(b => b.Id)
                        .ToListAsync(_cts?.Token ?? CancellationToken.None);
                }

                if (_isDisposed) return new GridDataProviderResult<Brand> { Data = new List<Brand>(), TotalCount = 0 };

                return await Task.FromResult(request.ApplyTo(brands));
            }
            catch (ObjectDisposedException) 
            { 
                return new GridDataProviderResult<Brand> { Data = new List<Brand>(), TotalCount = 0 }; 
            }
            catch (TaskCanceledException) 
            { 
                return new GridDataProviderResult<Brand> { Data = new List<Brand>(), TotalCount = 0 }; 
            }
        }

        private void OnSelectedItemsChanged(IEnumerable<Brand> selected)
        {
            var row = selected.FirstOrDefault();
            SelectedBrand = row ?? new Brand();
            SafeStateHasChanged();
        }

        public async Task DeleteBrandAsync(long? brandId)
        {
            if (_isDisposed) return;

            try
            {
                var brand = _context.Brands.FirstOrDefault(b => b.Id == brandId);
                if (brand == null) return;

                _context.Brands.Remove(brand);
                await _context.SaveChangesAsync(_cts?.Token ?? CancellationToken.None);

                if (_isDisposed) return;

                InvalidateBrandsCache(); // Cache invalidieren für bessere Performance
                
                if (_isDisposed) return;
                
                if (SelectedBrand?.Id == brandId) SelectedBrand = null;
                NewBrand = new Brand();
                
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
