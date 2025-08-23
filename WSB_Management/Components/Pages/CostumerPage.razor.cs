using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.EntityFrameworkCore;
using WSB_Management.Data;
using WSB_Management.Models;

namespace WSB_Management.Components.Pages
{
    public partial class CostumerPage : IDisposable, IAsyncDisposable
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
        
        private Grid<Customer>? grid;
        private Customer? _selectedCustomer;
        public Customer? SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (_selectedCustomer != value && value is not null)
                {
                    _selectedCustomer = value;
                    // Cup-States für den ausgewählten Customer initialisieren
                    InitCupStateFromDb();
                    SafeStateHasChanged();
                }
            }
        }
        
        private Customer _newCustomer = new Customer();
        public Customer NewCustomer
        {
            get => _newCustomer;
            set
            {
                if (_newCustomer != value)
                {
                    _newCustomer = value;
                    EnsureCustomerProperties(_newCustomer);
                    SafeStateHasChanged();
                }
            }
        }
        private Customer CurrentCustomer 
        {
            get 
            {
                var customer = SelectedCustomer ?? NewCustomer;
                EnsureCustomerProperties(customer);
                return customer;
            }
        }
        
        // Cache für bessere Performance
        private void InvalidateCustomersCache()
        {
            customers = new List<Customer>(); // Cache leeren, wird bei nächstem Zugriff neu geladen
        }
        private string PlzOrt
        {
            get => $"{CurrentCustomer?.Address?.Zip ?? ""} {CurrentCustomer?.Address?.City ?? ""}".Trim();
            set
            {
                EnsureAddress();
                var input = (value ?? string.Empty).Trim();
                var parts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0 && int.TryParse(parts[0], out _))
                {
                    CurrentCustomer.Address.Zip = parts[0];
                    CurrentCustomer.Address.City = parts.Length > 1 ? parts[1] : ""; 
                }
                else
                {
                    CurrentCustomer.Address.Zip = "";
                    CurrentCustomer.Address.City = input;
                }
                SafeStateHasChanged();
            }
        }
        
        // Für die BestTime-Eingabe
        private string _bestTimeInput = string.Empty;
        public string BestTimeInput
        {
            get => _bestTimeInput;
            set
            {
                _bestTimeInput = value;
                // Versuche die Eingabe zu parsen und in CurrentCustomer.BestTime zu setzen
                if (TryParseTimeString(value, out TimeSpan timeSpan))
                {
                    CurrentCustomer.BestTime = timeSpan;
                }
                else
                {
                    CurrentCustomer.BestTime = null;
                }
                SafeStateHasChanged();
            }
        }
        
        private bool TryParseTimeString(string input, out TimeSpan timeSpan)
        {
            timeSpan = TimeSpan.Zero;
            if (string.IsNullOrWhiteSpace(input)) return false;
            
            // Pattern: mm:ss,f oder mm:ss,ff (z.B. "2:15,5" oder "2:15,50")
            var pattern = @"^([0-5]?[0-9]):([0-5][0-9]),([0-9]{1,2})$";
            var match = System.Text.RegularExpressions.Regex.Match(input, pattern);
            
            if (match.Success)
            {
                var minutes = int.Parse(match.Groups[1].Value);
                var seconds = int.Parse(match.Groups[2].Value);
                var hundredthsStr = match.Groups[3].Value;
                
                // Wenn nur eine Stelle, mit 0 auffüllen (z.B. "3" -> "30")
                if (hundredthsStr.Length == 1)
                    hundredthsStr += "0";
                    
                var hundredths = int.Parse(hundredthsStr);
                
                timeSpan = new TimeSpan(0, 0, minutes, seconds, hundredths * 10);
                return true;
            }
            return false;
        }
        
        public List<Country> countries { get; set; } = new List<Country>();
        public List<Transponder> transponders { get; set; } = new List<Transponder>();
        private string NotfallContact 
        {             
            get => $"{CurrentCustomer?.NotfallContact?.Firstname ?? ""} {CurrentCustomer?.NotfallContact?.Surname ?? ""}".Trim();
            set
            {
                EnsureNotfallContact();
                var input = (value ?? string.Empty).Trim();
                var parts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0)
                {
                    CurrentCustomer.NotfallContact.Firstname = parts[0];
                    CurrentCustomer.NotfallContact.Surname = parts.Length > 1 ? parts[1] : "";
                }
                else
                {
                    CurrentCustomer.NotfallContact.Firstname = "";
                    CurrentCustomer.NotfallContact.Surname = input;
                }
                SafeStateHasChanged();
            }
        }
        // Falggen
        private string SelectedFlagPath => CurrentCustomer?.Address?.Country?.FlagPath ?? "";

        // Cup State
        private Cup? Tc5kCup;
        private Cup? EndCup;
        private List<Team> AllTeams = new();

        // TC5K State
        private bool Tc5kParticipates;
        private bool Tc5kIsTeamChef;
        private Team? Tc5kTeam;

        // END State
        private bool EndParticipates;
        private bool EndIsTeamChef;
        private Team? EndTeam;

        public List<Gruppe> gruppen { get; set; } = new List<Gruppe>();
        public List<Brand> brands { get; set; } = new List<Brand>();
        public List<Cup> cups { get; set; } = new List<Cup>();

        // Tracks
        string Pattern = "^([0-5]?[0-9]):([0-5][0-9]),[0-9]{2}$";

        public class TrackRef
        {
            public string Code { get; set; } = "";
            public string? Value { get; set; }
        }

        List<TrackRef> Tracks = new()
        {
            new(){ Code="BRN"}, new(){ Code="MUG"},
            new(){ Code="CRE"}, new(){ Code="PAN"},
            new(){ Code="HUN"}, new(){ Code="RBR"},
            new(){ Code="LAU"}, new(){ Code="RIJ"},
            new(){ Code="MST"}, new(){ Code="SLO"},
        };


        public List<Customer> customers { get; set; } = new List<Customer>();
        private readonly WSBRacingDbContext _context;
        public CostumerPage(WSBRacingDbContext context)
        {
            _context = context;
        }
        public async Task SaveCustomerAsync()
        {
            if (_isDisposed) return;

            try
            {
                // Basic Validation
                if (string.IsNullOrWhiteSpace(CurrentCustomer?.Contact?.Firstname) && 
                    string.IsNullOrWhiteSpace(CurrentCustomer?.Contact?.Surname))
                {
                    // Optional: Add validation message
                    return;
                }

                // Brand-Referenz korrigieren falls nötig
                await ValidateAndFixBrandReference();

                // Country-Referenz korrigieren falls nötig
                ValidateAndFixCountryReference();

                // Gruppe-Referenz korrigieren falls nötig
                ValidateAndFixGruppeReference();

                // Transponder-Referenz korrigieren falls nötig
                ValidateAndFixTransponderReference();

                // Bike-Entität für Entity Framework vorbereiten
                PrepareBikeForSaving();

                var isNew = CurrentCustomer.Id == 0;
                
                // Schritt 1: Customer ohne Team-Referenzen speichern
                if (isNew)
                {
                    CurrentCustomer.Validfrom = DateTime.UtcNow;
                    _context.Customers.Add(CurrentCustomer);
                }
                else
                {
                    // Prüfen ob die Entität bereits getrackt wird
                    var tracked = _context.Customers.Local.FirstOrDefault(c => c.Id == CurrentCustomer.Id);
                    if (tracked != null)
                    {
                        // Vorhandene getrackte Entität aktualisieren
                        _context.Entry(tracked).CurrentValues.SetValues(CurrentCustomer);
                    }
                    else
                    {
                        // Entität anhängen und als geändert markieren
                        _context.Customers.Attach(CurrentCustomer);
                        _context.Entry(CurrentCustomer).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    }
                }

                // Customer zuerst speichern, um ID zu erhalten
                await _context.SaveChangesAsync(_cts?.Token ?? CancellationToken.None);

                if (_isDisposed) return;

                // Schritt 2: Team-Zuordnungen aktualisieren
                await UpdateTeamAssignments();

                if (_isDisposed) return;

                // Cache invalidieren für Grid-Refresh
                InvalidateCustomersCache();

                SelectedCustomer = null;
                NewCustomer = new Customer();

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
            catch (Exception)
            {
                SafeStateHasChanged();
            }
        }

        private void PrepareBikeForSaving()
        {
            if (CurrentCustomer?.Bike == null) return;

            // Wenn das Bike keine Brand hat, entferne es komplett
            if (CurrentCustomer.Bike.Brand == null)
            {
                CurrentCustomer.Bike = null!;
                return;
            }

            // Sicherstellen, dass die Brand korrekt getrackt ist
            if (CurrentCustomer.Bike.Brand.Id > 0)
            {
                var trackedBrand = _context.Brands.Local.FirstOrDefault(b => b.Id == CurrentCustomer.Bike.Brand.Id);
                if (trackedBrand != null)
                {
                    CurrentCustomer.Bike.Brand = trackedBrand;
                }
                else
                {
                    // Brand als Unchanged markieren, da sie bereits in der DB existiert
                    _context.Entry(CurrentCustomer.Bike.Brand).State = EntityState.Unchanged;
                }
            }

            // Customers-Liste für Bike initialisieren falls nötig
            CurrentCustomer.Bike.Customers ??= new List<Customer>();
        }

        private async Task ValidateAndFixBrandReference()
        {
            if (CurrentCustomer?.Bike == null) return;

            try
            {
                // Wenn keine Brand ausgewählt wurde, erstelle kein Bike
                if (CurrentCustomer.Bike.Brand == null || CurrentCustomer.Bike.Brand.Id == 0)
                {
                    // Wenn das Bike leer ist (keine anderen Werte), entferne es komplett
                    if (string.IsNullOrWhiteSpace(CurrentCustomer.Bike.Type) && 
                        string.IsNullOrWhiteSpace(CurrentCustomer.Bike.Ccm) && 
                        CurrentCustomer.Bike.Year == 0)
                    {
                        // Setze Bike auf null, damit es nicht gespeichert wird
                        CurrentCustomer.Bike = null!;
                        return;
                    }
                    
                    // Andernfalls verwende eine Default-Brand
                    var defaultBrand = await GetOrCreateDefaultBrand();
                    CurrentCustomer.Bike.Brand = defaultBrand;
                    return;
                }

                // Prüfen ob die ausgewählte Brand gültig ist
                var existingBrand = brands.FirstOrDefault(b => b.Id == CurrentCustomer.Bike.Brand.Id);
                if (existingBrand == null)
                {
                    // Nochmal in der Datenbank suchen
                    existingBrand = await _context.Brands
                        .FirstOrDefaultAsync(b => b.Id == CurrentCustomer.Bike.Brand.Id, _cts?.Token ?? CancellationToken.None);
                }

                if (existingBrand == null)
                {
                    // Falls Brand nicht existiert, verwende Default-Brand
                    var defaultBrand = await GetOrCreateDefaultBrand();
                    CurrentCustomer.Bike.Brand = defaultBrand;
                }
                else
                {
                    // Verwende die gültige Brand
                    CurrentCustomer.Bike.Brand = existingBrand;
                }
            }
            catch (Exception)
            {
                // Im Fehlerfall Default-Brand verwenden oder Bike entfernen
                try
                {
                    var defaultBrand = await GetOrCreateDefaultBrand();
                    CurrentCustomer.Bike.Brand = defaultBrand;
                }
                catch
                {
                    // Als letzte Option, Bike entfernen
                    CurrentCustomer.Bike = null!;
                }
            }
        }

        private void ValidateAndFixCountryReference()
        {
            if (CurrentCustomer?.Address?.Country == null) return;

            try
            {
                // Prüfen ob das ausgewählte Land gültig ist
                var existingCountry = countries.FirstOrDefault(c => c.Id == CurrentCustomer.Address.Country.Id);
                if (existingCountry != null)
                {
                    // Verwende die gültige Country-Referenz aus der lokalen Liste
                    CurrentCustomer.Address.Country = existingCountry;
                    
                    // Sicherstellen, dass Entity Framework die Country als Unchanged markiert
                    var trackedCountry = _context.Countries.Local.FirstOrDefault(c => c.Id == existingCountry.Id);
                    if (trackedCountry != null)
                    {
                        CurrentCustomer.Address.Country = trackedCountry;
                    }
                    else
                    {
                        _context.Entry(CurrentCustomer.Address.Country).State = EntityState.Unchanged;
                    }
                }
                else
                {
                    // Falls Country nicht in der lokalen Liste ist, aus DB laden
                    var dbCountry = _context.Countries.FirstOrDefault(c => c.Id == CurrentCustomer.Address.Country.Id);
                    if (dbCountry != null)
                    {
                        CurrentCustomer.Address.Country = dbCountry;
                    }
                    else
                    {
                        // Falls Country gar nicht existiert, entfernen
                        CurrentCustomer.Address.Country = null!;
                    }
                }
            }
            catch (Exception)
            {
                // Im Fehlerfall Country-Referenz entfernen
                CurrentCustomer.Address.Country = null!;
            }
        }

        private void ValidateAndFixGruppeReference()
        {
            if (CurrentCustomer?.Gruppe == null) return;

            try
            {
                // Prüfen ob die ausgewählte Gruppe gültig ist
                var existingGruppe = gruppen.FirstOrDefault(g => g.Id == CurrentCustomer.Gruppe.Id);
                if (existingGruppe != null)
                {
                    // Verwende die gültige Gruppe-Referenz aus der lokalen Liste
                    CurrentCustomer.Gruppe = existingGruppe;
                    
                    // Sicherstellen, dass Entity Framework die Gruppe als Unchanged markiert
                    var trackedGruppe = _context.Gruppes.Local.FirstOrDefault(g => g.Id == existingGruppe.Id);
                    if (trackedGruppe != null)
                    {
                        CurrentCustomer.Gruppe = trackedGruppe;
                    }
                    else
                    {
                        _context.Entry(CurrentCustomer.Gruppe).State = EntityState.Unchanged;
                    }
                }
                else
                {
                    // Falls Gruppe nicht in der lokalen Liste ist, aus DB laden
                    var dbGruppe = _context.Gruppes.FirstOrDefault(g => g.Id == CurrentCustomer.Gruppe.Id);
                    if (dbGruppe != null)
                    {
                        CurrentCustomer.Gruppe = dbGruppe;
                    }
                    else
                    {
                        // Falls Gruppe gar nicht existiert, entfernen
                        CurrentCustomer.Gruppe = null!;
                    }
                }
            }
            catch (Exception)
            {
                // Im Fehlerfall Gruppe-Referenz entfernen
                CurrentCustomer.Gruppe = null!;
            }
        }

        private void ValidateAndFixTransponderReference()
        {
            if (CurrentCustomer?.Transponder == null) return;

            try
            {
                // Prüfen ob der ausgewählte Transponder gültig ist
                var existingTransponder = transponders.FirstOrDefault(t => t.Id == CurrentCustomer.Transponder.Id);
                if (existingTransponder != null)
                {
                    // Verwende die gültige Transponder-Referenz aus der lokalen Liste
                    CurrentCustomer.Transponder = existingTransponder;
                    
                    // Sicherstellen, dass Entity Framework den Transponder als Unchanged markiert
                    var trackedTransponder = _context.Transponders.Local.FirstOrDefault(t => t.Id == existingTransponder.Id);
                    if (trackedTransponder != null)
                    {
                        CurrentCustomer.Transponder = trackedTransponder;
                    }
                    else
                    {
                        _context.Entry(CurrentCustomer.Transponder).State = EntityState.Unchanged;
                    }
                }
                else
                {
                    // Falls Transponder nicht in der lokalen Liste ist, aus DB laden
                    var dbTransponder = _context.Transponders.FirstOrDefault(t => t.Id == CurrentCustomer.Transponder.Id);
                    if (dbTransponder != null)
                    {
                        CurrentCustomer.Transponder = dbTransponder;
                    }
                    else
                    {
                        // Falls Transponder gar nicht existiert, entfernen
                        CurrentCustomer.Transponder = null!;
                    }
                }
            }
            catch (Exception)
            {
                // Im Fehlerfall Transponder-Referenz entfernen
                CurrentCustomer.Transponder = null!;
            }
        }

        private async Task<Brand> GetOrCreateDefaultBrand()
        {
            const string defaultBrandName = "Unbekannt";
            
            var defaultBrand = await _context.Brands
                .FirstOrDefaultAsync(b => b.Name == defaultBrandName, _cts?.Token ?? CancellationToken.None);

            if (defaultBrand == null)
            {
                defaultBrand = new Brand { Name = defaultBrandName };
                _context.Brands.Add(defaultBrand);
                await _context.SaveChangesAsync(_cts?.Token ?? CancellationToken.None);
                
                // Zur lokalen Liste hinzufügen
                if (!brands.Any(b => b.Name == defaultBrandName))
                    brands.Add(defaultBrand);
            }

            return defaultBrand;
        }

        private async Task UpdateTeamAssignments()
        {
            if (CurrentCustomer == null) return;

            // TC5K Team-Zuordnung
            if (Tc5kParticipates && Tc5kTeam != null && Tc5kCup != null)
            {
                // Wenn das Team eine ID von 0 hat, ist es neu und muss erstellt werden
                if (Tc5kTeam.Id == 0)
                {
                    var createdTeam = await FindOrCreateTeamAsync(Tc5kTeam.Name, Tc5kCup);
                    Tc5kTeam = createdTeam; // Referenz auf das korrekte Team setzen
                }
                
                // Sicherstellen, dass das Team zu dem Cup gehört
                Tc5kCup.CupTeams ??= new List<Team>();
                if (!Tc5kCup.CupTeams.Any(t => t.Id == Tc5kTeam.Id))
                    Tc5kCup.CupTeams.Add(Tc5kTeam);
                
                EnsureMember(Tc5kTeam, CurrentCustomer);
                
                if (Tc5kIsTeamChef)
                    Tc5kTeam.TeamChef = CurrentCustomer;
            }
            else if (!Tc5kParticipates && Tc5kCup != null)
            {
                RemoveCustomerFromCup(Tc5kCup, CurrentCustomer);
            }

            // END Team-Zuordnung
            if (EndParticipates && EndTeam != null && EndCup != null)
            {
                // Wenn das Team eine ID von 0 hat, ist es neu und muss erstellt werden
                if (EndTeam.Id == 0)
                {
                    var createdTeam = await FindOrCreateTeamAsync(EndTeam.Name, EndCup);
                    EndTeam = createdTeam; // Referenz auf das korrekte Team setzen
                }
                
                // Sicherstellen, dass das Team zu dem Cup gehört
                EndCup.CupTeams ??= new List<Team>();
                if (!EndCup.CupTeams.Any(t => t.Id == EndTeam.Id))
                    EndCup.CupTeams.Add(EndTeam);
                
                EnsureMember(EndTeam, CurrentCustomer);
                
                if (EndIsTeamChef)
                    EndTeam.TeamChef = CurrentCustomer;
            }
            else if (!EndParticipates && EndCup != null)
            {
                RemoveCustomerFromCup(EndCup, CurrentCustomer);
            }

            // Team-Änderungen speichern
            await _context.SaveChangesAsync(_cts?.Token ?? CancellationToken.None);
        }
        public async Task DeleteCustomer(long? customerId)
        {
            if (_isDisposed) return;

            try
            {
                var customer = _context.Customers
                    .Include(c => c.Address)
                    .Include(c => c.Contact)
                    .FirstOrDefault(c => c.Id == customerId);

                if (customer == null) return;

                var cups = await _context.Cups
                    .Include(c => c.CupTeams).ThenInclude(t => t.Members)
                    .Include(c => c.CupTeams).ThenInclude(t => t.TeamChef)
                    .ToListAsync(_cts?.Token ?? CancellationToken.None);

                if (_isDisposed) return;

                foreach (var cup in cups)
                {
                    if (cup.CupTeams == null) continue;

                    foreach (var team in cup.CupTeams)
                    {
                        if (team.TeamChef != null && team.TeamChef.Id == customerId)
                            team.TeamChef = null;

                        if (team.Members != null)
                        {
                            var index = team.Members.FindIndex(m => m.Id == customerId);
                            if (index >= 0) team.Members.RemoveAt(index);
                        }
                    }
                }

                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync(_cts?.Token ?? CancellationToken.None);

                if (_isDisposed) return;

                // Cache invalidieren für Grid-Refresh
                InvalidateCustomersCache();

                if (SelectedCustomer?.Id == customerId) SelectedCustomer = null;
                NewCustomer = new Customer();

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
        private async Task<GridDataProviderResult<Customer>> CustomerDataProvider(GridDataProviderRequest<Customer> request)
        {
            if (_isDisposed) return new GridDataProviderResult<Customer> { Data = new List<Customer>(), TotalCount = 0 };

            try
            {
                if (customers is null || customers.Count==0)
                {
                    customers = await _context.Customers
                        .Include(c => c.Address).ThenInclude(a => a.Country)
                        .Include(c => c.Gruppe)
                        .Include(c => c.Contact)
                        .Include(c => c.NotfallContact)
                        .Include(c => c.Transponder)
                        .Include(c => c.Bike).ThenInclude(b => b.Brand)
                        .AsNoTracking()
                        .OrderBy(c => c.Contact!.Surname)
                        .ToListAsync(_cts?.Token ?? CancellationToken.None);
                }

                if (_isDisposed) return new GridDataProviderResult<Customer> { Data = new List<Customer>(), TotalCount = 0 };

                return await Task.FromResult(request.ApplyTo(customers));
            }
            catch (ObjectDisposedException) 
            { 
                return new GridDataProviderResult<Customer> { Data = new List<Customer>(), TotalCount = 0 }; 
            }
            catch (TaskCanceledException) 
            { 
                return new GridDataProviderResult<Customer> { Data = new List<Customer>(), TotalCount = 0 }; 
            }
        }
        private void OnSelectedItemsChanged(IEnumerable<Customer> selected)
        {
            var row = selected.FirstOrDefault();
            SelectedCustomer = row ?? new Customer();
            SafeStateHasChanged();
        }
        private void EnsureAddress()
        {
            if (CurrentCustomer.Address == null)
                CurrentCustomer.Address = new Address();
        }
        
        private void EnsureNotfallContact()
        {
            if (CurrentCustomer.NotfallContact == null)
                CurrentCustomer.NotfallContact = new Contact();
        }
        private void EnsureCustomerProperties(Customer customer)
        {
            if (customer == null) return;
            
            customer.Address ??= new Address();
            customer.Contact ??= new Contact();
            customer.NotfallContact ??= new Contact();
            
            // Bike nur initialisieren wenn noch nicht vorhanden
            // Die Brand wird später in ValidateAndFixBrandReference behandelt
            if (customer.Bike == null)
            {
                customer.Bike = new Bike
                {
                    Type = "",
                    Ccm = "",
                    Year = 0,
                    Brand = null!, // Wird später in ValidateAndFixBrandReference gesetzt
                    Customers = new List<Customer>()
                };
            }
        }
        protected override async Task OnInitializedAsync()
        {
            if (_isDisposed) return;

            try
            {
                countries = await _context.Countries
                    .AsNoTracking()
                    .OrderBy(c => c.Shorttxt)
                    .ToListAsync(_cts?.Token ?? CancellationToken.None);

                transponders = await _context.Transponders
                    .AsNoTracking()
                    .OrderBy(c => c.Id)
                    .ToListAsync(_cts?.Token ?? CancellationToken.None);

                if (_isDisposed) return;

                gruppen = await _context.Gruppes
                    .AsNoTracking()
                    .ToListAsync(_cts?.Token ?? CancellationToken.None);

                if (_isDisposed) return;

                brands = await _context.Brands
                    .AsNoTracking()
                    .OrderBy(b => b.Name)
                    .ToListAsync(_cts?.Token ?? CancellationToken.None);

                if (_isDisposed) return;

                cups = await _context.Cups
                    .Include(c => c.CupTeams)
                        .ThenInclude(t => t.TeamChef)
                    .Include(c => c.CupTeams)
                        .ThenInclude(t => t.Members)
                    .Where(c => c.Name != "TC5K" && c.Name != "END Cup")
                    .OrderBy(c => c.Name)
                    .ToListAsync(_cts?.Token ?? CancellationToken.None);


                if (_isDisposed) return;

                Tc5kCup = await _context.Cups
                    .Include(c => c.CupTeams).ThenInclude(t => t.Members)
                    .Include(c => c.CupTeams).ThenInclude(t => t.TeamChef)
                    .FirstOrDefaultAsync(c => c.Name == "TC5K", _cts?.Token ?? CancellationToken.None);

                if (_isDisposed) return;

                EndCup = await _context.Cups
                    .Include(c => c.CupTeams).ThenInclude(t => t.Members)
                    .Include(c => c.CupTeams).ThenInclude(t => t.TeamChef)
                    .FirstOrDefaultAsync(c => c.Name == "END Cup", _cts?.Token ?? CancellationToken.None);

                if (_isDisposed) return;

                AllTeams = await _context.Teams
                    .Include(t => t.Members)
                    .Include(t => t.TeamChef)
                    .OrderBy(t => t.Name)
                    .ToListAsync(_cts?.Token ?? CancellationToken.None);

                if (_isDisposed) return;

                if (Tc5kCup == null)
                {
                    Tc5kCup = new Cup { Name = "TC5K", CupTeams = new List<Team>() };
                    _context.Cups.Add(Tc5kCup);
                    await _context.SaveChangesAsync(_cts?.Token ?? CancellationToken.None);
                }
                
                if (_isDisposed) return;
                
                if (EndCup == null)
                {
                    EndCup = new Cup { Name = "END Cup", CupTeams = new List<Team>() };
                    _context.Cups.Add(EndCup);
                    await _context.SaveChangesAsync(_cts?.Token ?? CancellationToken.None);
                }
                
                if (_isDisposed) return;
                
                InitCupStateFromDb();
            }
            catch (ObjectDisposedException) { return; }
            catch (InvalidOperationException) { return; }
            catch (TaskCanceledException) { return; }
            catch (Exception) { return; }
        }

        private void InitCupStateFromDb()
        {
            if (CurrentCustomer == null || CurrentCustomer.Id == 0) return;

            // BestTime Input initialisieren
            if (CurrentCustomer.BestTime.HasValue)
            {
                var time = CurrentCustomer.BestTime.Value;
                _bestTimeInput = $"{(int)time.TotalMinutes}:{time.Seconds:D2},{time.Milliseconds / 10:D2}";
            }
            else
            {
                _bestTimeInput = string.Empty;
            }

            // TC5K - Null-Check für Tc5kCup
            if (Tc5kCup?.CupTeams != null)
            {
                var t1 = Tc5kCup.CupTeams.FirstOrDefault(t => t.Members?.Any(m => m.Id == CurrentCustomer.Id) == true);
                if (t1 != null)
                {
                    Tc5kParticipates = true;
                    Tc5kTeam = t1;
                    Tc5kIsTeamChef = (t1.TeamChef?.Id == CurrentCustomer.Id);
                }
                else
                {
                    Tc5kParticipates = false;
                    Tc5kTeam = null;
                    Tc5kIsTeamChef = false;
                }
            }
            else
            {
                Tc5kParticipates = false;
                Tc5kTeam = null;
                Tc5kIsTeamChef = false;
            }

            // END - Null-Check für EndCup
            if (EndCup?.CupTeams != null)
            {
                var t2 = EndCup.CupTeams.FirstOrDefault(t => t.Members?.Any(m => m.Id == CurrentCustomer.Id) == true);
                if (t2 != null)
                {
                    EndParticipates = true;
                    EndTeam = t2;
                    EndIsTeamChef = (t2.TeamChef?.Id == CurrentCustomer.Id);
                }
                else
                {
                    EndParticipates = false;
                    EndTeam = null;
                    EndIsTeamChef = false;
                }
            }
            else
            {
                EndParticipates = false;
                EndTeam = null;
                EndIsTeamChef = false;
            }
        }
        // ---------- TC5K Events ----------
        private async Task ToggleParticipatesTc5k(bool value)
        {
            if (_isDisposed) return;

            try
            {
                Tc5kParticipates = value;

                if (!value && Tc5kCup != null)
                {
                    RemoveCustomerFromCup(Tc5kCup, CurrentCustomer);
                    Tc5kTeam = null;
                    Tc5kIsTeamChef = false;
                    
                    // Nur speichern wenn Customer bereits existiert
                    if (CurrentCustomer.Id > 0)
                        await _context.SaveChangesAsync(_cts?.Token ?? CancellationToken.None);
                }
                // Team-Auswahl erfolgt direkt über die InputSelectTeam Komponente

                SafeStateHasChanged();
            }
            catch (ObjectDisposedException) { return; }
            catch (TaskCanceledException) { return; }
            catch (Exception) { return; }
        }

        private void ToggleTeamchefTc5k(bool value)
        {
            if (_isDisposed || CurrentCustomer == null) return;

            try
            {
                Tc5kIsTeamChef = value;
                
                // Team-Chef Status wird beim Speichern verarbeitet
                // Keine direkte Datenbankänderung hier
                
                SafeStateHasChanged();
            }
            catch (ObjectDisposedException) { return; }
            catch (TaskCanceledException) { return; }
            catch (Exception) { return; }
        }

        // ---------- END Events ----------
        private async Task ToggleParticipatesEnd(bool value)
        {
            if (_isDisposed) return;

            try
            {
                EndParticipates = value;

                if (!value && EndCup != null)
                {
                    RemoveCustomerFromCup(EndCup, CurrentCustomer);
                    EndTeam = null;
                    EndIsTeamChef = false;
                    
                    // Nur speichern wenn Customer bereits existiert
                    if (CurrentCustomer.Id > 0)
                        await _context.SaveChangesAsync(_cts?.Token ?? CancellationToken.None);
                }
                // Team-Auswahl erfolgt direkt über die InputSelectTeam Komponente

                SafeStateHasChanged();
            }
            catch (ObjectDisposedException) { return; }
            catch (TaskCanceledException) { return; }
            catch (Exception) { return; }
        }

        private void ToggleTeamchefEnd(bool value)
        {
            if (_isDisposed || CurrentCustomer == null) return;

            try
            {
                EndIsTeamChef = value;
                
                // Team-Chef Status wird beim Speichern verarbeitet
                // Keine direkte Datenbankänderung hier
                
                SafeStateHasChanged();
            }
            catch (ObjectDisposedException) { return; }
            catch (TaskCanceledException) { return; }
            catch (Exception) { return; }
        }

        private async Task<Team> FindOrCreateTeamAsync(string name, Cup cup)
        {
            if (string.IsNullOrWhiteSpace(name)) 
                throw new ArgumentException("Team name cannot be empty", nameof(name));

            // Zuerst in der lokalen Liste suchen
            var team = AllTeams.FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            
            if (team == null)
            {
                // In der Datenbank nach existierendem Team suchen
                team = await _context.Teams
                    .Include(t => t.Members)
                    .FirstOrDefaultAsync(t => t.Name == name, _cts?.Token ?? CancellationToken.None);
                
                if (team == null)
                {
                    // Neues Team erstellen
                    team = new Team { Name = name, Members = new List<Customer>() };
                    _context.Teams.Add(team);
                    
                    // Team muss erst gespeichert werden, um ID zu bekommen
                    await _context.SaveChangesAsync(_cts?.Token ?? CancellationToken.None);
                }
                
                // Team zur lokalen Liste hinzufügen
                AllTeams.Add(team);
            }

            // Sicherstellen, dass Cup Teams initialisiert ist
            cup.CupTeams ??= new List<Team>();
            
            // Team zum Cup hinzufügen, falls noch nicht vorhanden
            if (!cup.CupTeams.Any(t => t.Id == team.Id))
                cup.CupTeams.Add(team);

            return team;
        }

        private Team FindOrCreateTeam(string name, Cup cup)
        {
            if (string.IsNullOrWhiteSpace(name)) 
                throw new ArgumentException("Team name cannot be empty", nameof(name));

            var team = AllTeams.FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (team == null)
            {
                team = new Team { Name = name, Members = new List<Customer>() };
                AllTeams.Add(team);
                _context.Teams.Add(team);
            }

            // Sicherstellen, dass Cup Teams initialisiert ist
            cup.CupTeams ??= new List<Team>();
            
            // Team zum Cup hinzufügen, falls noch nicht vorhanden
            if (!cup.CupTeams.Any(t => t.Id == team.Id || (t.Id == 0 && t.Name.Equals(team.Name, StringComparison.OrdinalIgnoreCase))))
                cup.CupTeams.Add(team);

            return team;
        }

        private static void EnsureMember(Team team, Customer cust)
        {
            if (team == null || cust == null) return;

            team.Members ??= new List<Customer>();
            if (!team.Members.Any(m => m.Id == cust.Id || (cust.Id == 0 && m == cust)))
                team.Members.Add(cust);
        }

        private static void RemoveCustomerFromCup(Cup cup, Customer cust)
        {
            if (cup.CupTeams == null || cust == null) return;

            foreach (var t in cup.CupTeams)
            {
                // Member entfernen (sowohl mit ID als auch mit Object-Referenz für neue Customers)
                if (t.Members != null)
                {
                    var removedCount = t.Members.RemoveAll(m => 
                        (cust.Id > 0 && m.Id == cust.Id) || 
                        (cust.Id == 0 && ReferenceEquals(m, cust)));
                    
                    // Teamchef entfernen falls nötig
                    if (removedCount > 0 && t.TeamChef != null && 
                        ((cust.Id > 0 && t.TeamChef.Id == cust.Id) || 
                         (cust.Id == 0 && ReferenceEquals(t.TeamChef, cust))))
                    {
                        t.TeamChef = null;
                    }
                }
            }
        }
    }
}
