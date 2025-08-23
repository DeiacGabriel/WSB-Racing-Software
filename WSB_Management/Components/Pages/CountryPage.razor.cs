using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using WSB_Management.Data;
using WSB_Management.Models;

namespace WSB_Management.Components.Pages
{
    public partial class CountryPage : IDisposable, IAsyncDisposable
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
            catch (ObjectDisposedException) { /* Component disposed */ }
            catch (InvalidOperationException) { /* Renderer disposed */ }
        }
        private Grid<Country>? grid;

        private Country? _selectedCountry;
        public Country? SelectedCountry
        {
            get => _selectedCountry;
            set
            {
                if (_selectedCountry != value && value is not null)
                {
                    _selectedCountry = value;
                    SafeStateHasChanged();
                }
            }
        }


        private Country _newCountry = new();
        public Country NewCountry
        {
            get => _newCountry;
            set
            {
                if (_newCountry != value)
                {
                    _newCountry = value;
                    StateHasChanged();
                }
            }
        }

        private Country CurrentCountry => SelectedCountry ?? NewCountry;
        public List<Country> countries { get; set; } = new();

        [Inject] private IWebHostEnvironment Env { get; set; } = default!;
        private string UploadSubFolder { get; set; } = "flags";
        private string? _previousFlagPath;

        private bool IsDragOver { get; set; }
        private string? Message { get; set; }
        private const long MaxFileSize = 20 * 1024 * 1024;

        private string SelectedFlagPath => CurrentCountry?.FlagPath ?? "";

        private readonly WSBRacingDbContext _context;
        public CountryPage(WSBRacingDbContext context)
        {
            _context = context;
        }
        public async Task SaveCountry()
        {
            if (_isDisposed) return;

            try
            {
                var isNew = CurrentCountry.Id == 0;
                
                if (isNew) 
                    _context.Countries.Add(CurrentCountry);
                else 
                {
                    var tracked = _context.Countries.Local.FirstOrDefault(c => c.Id == CurrentCountry.Id);
                    if (tracked != null)
                        _context.Entry(tracked).CurrentValues.SetValues(CurrentCountry);
                    else
                    {
                        _context.Countries.Attach(CurrentCountry);
                        _context.Entry(CurrentCountry).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    }
                }

                await _context.SaveChangesAsync(_cts?.Token ?? CancellationToken.None);

                if (_isDisposed) return;

                Message = $"Gespeichert: {CurrentCountry.FlagPath}";
                countries = await _context.Countries.AsNoTracking().ToListAsync(_cts?.Token ?? CancellationToken.None);

                if (_isDisposed) return;

                SelectedCountry = null;
                NewCountry = new Country();

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

        private async Task<GridDataProviderResult<Country>> CountryDataProvider(GridDataProviderRequest<Country> request)
        {
            if (countries is null || countries.Count == 0)
                countries = await _context.Countries.AsNoTracking().ToListAsync();
            return await Task.FromResult(request.ApplyTo(countries));
        }

        private void OnSelectedItemsChanged(IEnumerable<Country> selected)
        {
            var row = selected.FirstOrDefault();
            SelectedCountry = row ?? new Country();
            _previousFlagPath = SelectedCountry?.FlagPath;
            StateHasChanged();
        }

        private async Task OnPngSelected(InputFileChangeEventArgs e)
        {
            var file = e.File;
            if (file is null) return;

            var ext = Path.GetExtension(file!.Name).ToLowerInvariant();
            if (ext != ".png" || !string.Equals(file.ContentType, "image/png", StringComparison.OrdinalIgnoreCase))
            {
                Message = "Nur PNG erlaubt.";
                StateHasChanged();
                return;
            }

            if (string.IsNullOrWhiteSpace(CurrentCountry.FlagPath))
                CurrentCountry.UpdateFlagPath();

            if (string.IsNullOrWhiteSpace(CurrentCountry.FlagPath))
            {
                Message = "Kein Zielpfad. Bitte Shorttxt/Longtxt setzen.";
                StateHasChanged();
                return;
            }

            var newPhys = PhysicalFromWebPath(Env, CurrentCountry.FlagPath);
            Directory.CreateDirectory(Path.GetDirectoryName(newPhys)!);

            if (!string.IsNullOrWhiteSpace(_previousFlagPath) &&
                !string.Equals(_previousFlagPath, CurrentCountry.FlagPath, StringComparison.OrdinalIgnoreCase))
            {
                var oldPhys = PhysicalFromWebPath(Env, _previousFlagPath);
                if (File.Exists(oldPhys))
                    TryDeleteFile(oldPhys);
            }

            if (File.Exists(newPhys))
                TryDeleteFile(newPhys);

            await using var read = file.OpenReadStream(MaxFileSize);
            await using var write = File.Create(newPhys);
            await read.CopyToAsync(write);

            _previousFlagPath = CurrentCountry.FlagPath;

            Message = $"Datei: {CurrentCountry.FlagPath}";
            StateHasChanged();
        }

        public async Task DeleteCountryAsync(long? countryId)
        {
            if (_isDisposed) return;

            try
            {
                var country = _context.Countries.FirstOrDefault(c => c.Id == countryId);
                if (country == null) return;

                if (!string.IsNullOrWhiteSpace(country.FlagPath))
                {
                    var phys = PhysicalFromWebPath(Env, country.FlagPath);
                    if (File.Exists(phys)) TryDeleteFile(phys);
                }

                countries.Remove(country);
                _context.Countries.Remove(country);
                await _context.SaveChangesAsync(_cts?.Token ?? CancellationToken.None);

                if (_isDisposed) return;

                countries = await _context.Countries.AsNoTracking().ToListAsync(_cts?.Token ?? CancellationToken.None);
                
                if (_isDisposed) return;
                
                if (SelectedCountry?.Id == countryId) SelectedCountry = null;
                NewCountry = new Country();
                
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

        private static void TryDeleteFile(string path)
        {
            try { File.Delete(path); }
            catch { }
        }

        private static string PhysicalFromWebPath(IWebHostEnvironment env, string webPath)
        {
            var rel = (webPath ?? "").Trim().Replace('\\', '/');
            if (rel.StartsWith("/")) rel = rel[1..];
            return Path.Combine(env.WebRootPath, rel);
        }
    }
}
