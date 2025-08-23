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
                    SafeStateHasChanged();
                }
            }
        }
        private Customer CurrentCustomer => SelectedCustomer ?? NewCustomer;
        
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
        private string Tc5kTeamQuery = "";
        private List<Team> Tc5kTeamSuggestions = new List<Team>();

        // END State
        private bool EndParticipates;
        private bool EndIsTeamChef;
        private Team? EndTeam;
        private string EndTeamQuery = "";
        private List<Team> EndTeamSuggestions = new List<Team>();

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

                var isNew = CurrentCustomer.Id == 0;
                
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

                await _context.SaveChangesAsync(_cts?.Token ?? CancellationToken.None);

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
                if (customers is null)
                {
                    customers = await _context.Customers
                        .Include(c => c.Address).ThenInclude(a => a.Country)
                        .Include(c => c.Gruppe)
                        .Include(c => c.Contact)
                        .Include(c => c.NotfallContact)
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
            SelectedCustomer = row;
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

                cups = await _context.Cups.Include(c => c.CupTeams)
                    .ThenInclude(t => t.TeamChef)
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
            // TC5K
            var t1 = Tc5kCup!.CupTeams?.FirstOrDefault(t => t.Members?.Any(m => m.Id == CurrentCustomer.Id) == true);
            if (t1 != null)
            {
                Tc5kParticipates = true;
                Tc5kTeam = t1;
                Tc5kTeamQuery = t1.Name;
                Tc5kIsTeamChef = (t1.TeamChef?.Id == CurrentCustomer.Id);
            }
            Tc5kTeamSuggestions = SuggestTeams("");

            // END
            var t2 = EndCup!.CupTeams?.FirstOrDefault(t => t.Members?.Any(m => m.Id == CurrentCustomer.Id) == true);
            if (t2 != null)
            {
                EndParticipates = true;
                EndTeam = t2;
                EndTeamQuery = t2.Name;
                EndIsTeamChef = (t2.TeamChef?.Id == CurrentCustomer.Id);
            }
            EndTeamSuggestions = SuggestTeams("");
        }
        // ---------- TC5K Events ----------
        private async Task ToggleParticipatesTc5k(bool value)
        {
            if (_isDisposed) return;

            try
            {
                Tc5kParticipates = value;

                if (!value)
                {
                    RemoveCustomerFromCup(Tc5kCup!, CurrentCustomer);
                    Tc5kTeam = null;
                    Tc5kTeamQuery = "";
                    Tc5kIsTeamChef = false;
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(Tc5kTeamQuery))
                    {
                        Tc5kTeam = FindOrCreateTeam(Tc5kTeamQuery.Trim(), Tc5kCup!);
                        EnsureMember(Tc5kTeam, CurrentCustomer);
                    }
                }
                
                await _context.SaveChangesAsync(_cts?.Token ?? CancellationToken.None);
                SafeStateHasChanged();
            }
            catch (ObjectDisposedException) { return; }
            catch (TaskCanceledException) { return; }
            catch (Exception) { return; }
        }

        private async Task ToggleTeamchefTc5k(bool value)
        {
            Tc5kIsTeamChef = value;

            if (Tc5kTeam != null && Tc5kParticipates)
            {
                EnsureMember(Tc5kTeam, CurrentCustomer);
                if (!value)
                    if (Tc5kTeam.TeamChef != null && Tc5kTeam.TeamChef.Id == CurrentCustomer.Id)
                        Tc5kTeam.TeamChef = null!;
                else
                    Tc5kTeam.TeamChef = CurrentCustomer;
            }

            await _context.SaveChangesAsync();
        }

        private void OnTeamQueryChangedTc5k(string? term)
        {
            Tc5kTeamQuery = (term ?? "").Trim();
            Tc5kTeamSuggestions = SuggestTeams(Tc5kTeamQuery);
        }

        private async Task OnTeamChosenOrTypedTc5k()
        {
            if (!Tc5kParticipates) Tc5kParticipates = true;

            var name = Tc5kTeamQuery.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                Tc5kTeam = null;
                Tc5kIsTeamChef = false;
                await _context.SaveChangesAsync();
                return;
            }

            Tc5kTeam = FindOrCreateTeam(name, Tc5kCup!);
            EnsureMember(Tc5kTeam, CurrentCustomer);
            if (Tc5kIsTeamChef) Tc5kTeam.TeamChef = CurrentCustomer;

            await _context.SaveChangesAsync();
        }

        // ---------- END Events ----------
        private async Task ToggleParticipatesEnd(bool value)
        {
            if (_isDisposed) return;

            try
            {
                EndParticipates = value;

                if (!value)
                {
                    RemoveCustomerFromCup(EndCup!, CurrentCustomer);
                    EndTeam = null;
                    EndTeamQuery = "";
                    EndIsTeamChef = false;
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(EndTeamQuery))
                    {
                        EndTeam = FindOrCreateTeam(EndTeamQuery.Trim(), EndCup!);
                        EnsureMember(EndTeam, CurrentCustomer);
                    }
                }

                await _context.SaveChangesAsync(_cts?.Token ?? CancellationToken.None);
                SafeStateHasChanged();
            }
            catch (ObjectDisposedException) { return; }
            catch (TaskCanceledException) { return; }
            catch (Exception) { return; }
        }

        private async Task ToggleTeamchefEnd(bool value)
        {
            EndIsTeamChef = value;

            if (EndTeam != null && EndParticipates)
            {
                EnsureMember(EndTeam, CurrentCustomer);
                if (!value)
                    if (EndTeam.TeamChef != null && EndTeam.TeamChef.Id == CurrentCustomer.Id)
                        EndTeam.TeamChef = null!;
                    else
                        EndTeam.TeamChef = CurrentCustomer;
                EndTeam.TeamChef = value ? CurrentCustomer : (EndTeam.TeamChef?.Id == CurrentCustomer.Id ? null : EndTeam.TeamChef);
            }

            await _context.SaveChangesAsync();
        }

        private void OnTeamQueryChangedEnd(string? term)
        {
            EndTeamQuery = (term ?? "").Trim();
            EndTeamSuggestions = SuggestTeams(EndTeamQuery);
        }

        private async Task OnTeamChosenOrTypedEnd()
        {
            try
            {
                if (!EndParticipates) EndParticipates = true;

                var name = EndTeamQuery?.Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    EndTeam = null;
                    EndIsTeamChef = false;
                    await _context.SaveChangesAsync();
                    return;
                }

                EndTeam = FindOrCreateTeam(name, EndCup!);
                EnsureMember(EndTeam, CurrentCustomer);
                if (EndIsTeamChef) EndTeam.TeamChef = CurrentCustomer;

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }


        private List<Team> SuggestTeams(string? term)
        {
            term = (term ?? "").Trim();
            var q = AllTeams.AsEnumerable();
            if (!string.IsNullOrEmpty(term))
                q = q.Where(t => t.Name.Contains(term, StringComparison.OrdinalIgnoreCase));
            return q.OrderBy(t => t.Name).Take(10).ToList();
        }

        private Team FindOrCreateTeam(string name, Cup cup)
        {
            var team = AllTeams.FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (team == null)
            {
                team = new Team { Name = name, Members = new List<Customer>() };
                AllTeams.Add(team);
                _context.Teams.Add(team);
            }

            cup.CupTeams ??= new List<Team>();
            if (!cup.CupTeams.Any(t => t.Id == team.Id || t.Name.Equals(team.Name, StringComparison.OrdinalIgnoreCase)))
                cup.CupTeams.Add(team);

            return team;
        }

        private static void EnsureMember(Team team, Customer cust)
        {
            team.Members ??= new List<Customer>();
            if (!team.Members.Any(m => m.Id == cust.Id))
                team.Members.Add(cust);
        }

        private static void RemoveCustomerFromCup(Cup cup, Customer cust)
        {
            if (cup.CupTeams == null) return;

            foreach (var t in cup.CupTeams)
            {
                if (t.Members?.RemoveAll(m => m.Id == cust.Id) > 0 && t.TeamChef?.Id == cust.Id)
                    t.TeamChef = null;
            }
        }
    }
}
