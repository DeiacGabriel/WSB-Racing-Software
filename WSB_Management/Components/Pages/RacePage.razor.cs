using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using WSB_Management.Data;
using WSB_Management.Models;

namespace WSB_Management.Components.Pages;

// Vereinfachte, bereinigte Code-Behind Implementierung damit die RacePage wieder kompiliert und läuft
public partial class RacePage : ComponentBase
{
	#region DI / Parameter
	[Inject] public WSBRacingDbContext Db { get; set; } = default!;
	[Parameter] public string? EventName { get; set; }
	#endregion

	#region Zustandsfelder
	private Grid<CostumerEvent>? grid;
	private CostumerEvent? _selectedParticipation;
	private CostumerEvent _newParticipation = new();
	private Event? _currentEvent;
	private bool _initialLoadDone;
	private bool _isLoading = false;

	public List<CostumerEvent> _eventParticipations = new(); 
	public List<Customer> AllCustomers { get; set; } = new();
	public List<Gruppe> Gruppen { get; set; } = new();
	public List<Country> Countries { get; set; } = new();
	public List<Brand> Brands { get; set; } = new();
	public List<Transponder> Transponders { get; set; } = new();

	public string? Message { get; set; }
	#endregion

	#region Eigenschaften für Razor
	public Event? CurrentEvent
	{
		get => _currentEvent;
		private set
		{
			if (_currentEvent != value)
			{
				_currentEvent = value;
				StateHasChanged();
			}
		}
	}

	public CostumerEvent? SelectedParticipation
	{
		get => _selectedParticipation;
		set
		{
			if (_selectedParticipation != value)
			{
				_selectedParticipation = value;
				StateHasChanged();
			}
		}
	}

	public CostumerEvent NewParticipation
	{
		get => _newParticipation;
		set
		{
			if (_newParticipation != value)
			{
				_newParticipation = value;
				EnsureParticipationProperties(_newParticipation);
				StateHasChanged();
			}
		}
	}

	private CostumerEvent CurrentParticipation 
	{
		get
		{
			var participation = SelectedParticipation ?? NewParticipation;
			EnsureParticipationProperties(participation);
			return participation;
		}
	}

	// Statistiken
	public int TotalRegistered => _eventParticipations.Count(p => p.Status == ParticipationStatus.Registered);
	public int TotalWaitlist => _eventParticipations.Count(p => p.Status == ParticipationStatus.Waitlist);
	public int TotalAbsent => _eventParticipations.Count(p => p.Status == ParticipationStatus.Absent);
	public int TotalAdditionalBikes => _eventParticipations.Count(p => p.Status == ParticipationStatus.AdditionalBike);

	public Dictionary<DateTime, int> ParticipationsByDay
	{
		get
		{
			var dict = new Dictionary<DateTime, int>();
			if (CurrentEvent == null) return dict;
			foreach (var d in CurrentEvent.GetEventDays())
			{
				dict[d.Date] = _eventParticipations.Count(p => p.ParticipationDate.Date == d.Date && (p.Status == ParticipationStatus.Registered || p.Status == ParticipationStatus.AdditionalBike));
			}
			return dict;
		}
	}

	public int GetAdditionalBikesCountForDay(DateTime day) => _eventParticipations.Count(p => p.ParticipationDate.Date == day.Date && p.Status == ParticipationStatus.AdditionalBike);
	public int GetAbsentCountForDay(DateTime day) => _eventParticipations.Count(p => p.ParticipationDate.Date == day.Date && p.Status == ParticipationStatus.Absent);
	public int GetWaitlistCountForDay(DateTime day) => _eventParticipations.Count(p => p.ParticipationDate.Date == day.Date && p.Status == ParticipationStatus.Waitlist);
	#endregion

	#region Lifecycle
	protected override async Task OnInitializedAsync()
	{
		// Sicherstellen, dass NewParticipation korrekt initialisiert ist
		EnsureParticipationProperties(_newParticipation);
		await base.OnInitializedAsync();
	}

	protected override async Task OnParametersSetAsync()
	{
		// Threading-Problem vermeiden - nur laden wenn nicht bereits geladen wird
		if (_isLoading) return;
		
		// Event wechseln / initial laden
		if (!_initialLoadDone || !string.Equals(CurrentEvent?.Name, EventName, StringComparison.OrdinalIgnoreCase))
		{
			_isLoading = true;
			try
			{
				await LoadAllDataAsync();
				_initialLoadDone = true;
			}
			finally
			{
				_isLoading = false;
			}
		}
	}
	#endregion

	#region Lade-Methoden
	private async Task LoadAllDataAsync()
	{
		try
		{
			// Stammdaten sequenziell laden um Threading-Probleme zu vermeiden
			Countries = await Db.Countries.AsNoTracking().ToListAsync();
			Brands = await Db.Brands.AsNoTracking().ToListAsync();
			Transponders = await Db.Transponders.AsNoTracking().ToListAsync();
			Gruppen = await Db.Gruppes.AsNoTracking().ToListAsync();
			
			AllCustomers = await Db.Customers
				.Include(c => c.Contact)
				.Include(c => c.Address).ThenInclude(a => a.Country)
				.Include(c => c.Bike).ThenInclude(b => b.Brand)
				.Include(c => c.Gruppe)
				.AsNoTracking()
				.ToListAsync();

			if (!string.IsNullOrWhiteSpace(EventName))
			{
				// Event in separatem Aufruf laden
				var eventEntity = await Db.Events
					.AsNoTracking()
					.FirstOrDefaultAsync(e => e.Name == EventName);
				
				CurrentEvent = eventEntity;
				
				// Participations separat laden nachdem Event gesetzt wurde
				if (eventEntity != null)
				{
					await LoadParticipationsAsync();
				}
			}

			EnsureParticipationProperties(NewParticipation);
		}
		catch (Exception ex)
		{
			Message = $"Fehler beim Laden: {ex.Message}";
		}
	}

	private async Task LoadParticipationsAsync()
	{
		if (CurrentEvent == null) 
		{ 
			_eventParticipations = new(); 
			return; 
		}
		
		try
		{
			_eventParticipations = await Db.CustomerEvents
				.Include(p => p.Customer).ThenInclude(c => c.Contact)
				.Include(p => p.Customer).ThenInclude(c => c.Address).ThenInclude(a => a.Country)
				.Include(p => p.Customer).ThenInclude(c => c.Bike).ThenInclude(b => b.Brand)
				.Include(p => p.Customer).ThenInclude(c => c.Gruppe)
				.Where(p => p.Event.Id == CurrentEvent.Id)
				.AsNoTracking()
				.OrderBy(p => p.ParticipationDate)
				.ThenBy(p => p.Customer.Contact.Surname)
				.ToListAsync();
				
			StateHasChanged();
		}
		catch (Exception ex)
		{
			Message = $"Fehler beim Laden der Teilnahmen: {ex.Message}";
			_eventParticipations = new();
		}
	}
	#endregion

	#region Participation Helpers
	private void EnsureParticipationProperties(CostumerEvent participation)
	{
		if (participation.Customer == null)
		{
			participation.Customer = new Customer 
			{ 
				Contact = new Contact(),
				Address = new Address(),
				Bike = new Bike()
			};
		}
		
		// Sicherstellen, dass Customer Properties nicht null sind
		participation.Customer.Contact ??= new Contact();
		participation.Customer.Address ??= new Address();
		participation.Customer.Bike ??= new Bike();
		participation.Customer.Bike.Brand ??= Brands.FirstOrDefault() ?? new Brand();
		
		if (participation.ParticipationDate == default)
		{
			participation.ParticipationDate = CurrentEvent?.Validfrom.Date ?? DateTime.Today;
		}
		if (participation.Event == null && CurrentEvent != null)
		{
			participation.Event = CurrentEvent;
		}
	}

	public bool IsParticipatingOnDay(DateTime eventDay)
		=> CurrentParticipation.Customer != null && _eventParticipations.Any(p => p.Customer.Id == CurrentParticipation.Customer.Id && p.ParticipationDate.Date == eventDay.Date);

	public void ToggleDayParticipation(DateTime eventDay, bool isParticipating)
	{
		if (CurrentParticipation.Customer == null || CurrentEvent == null) return;
		if (isParticipating)
		{
			if (!_eventParticipations.Any(p => p.Customer.Id == CurrentParticipation.Customer.Id && p.ParticipationDate.Date == eventDay.Date))
			{
				_eventParticipations.Add(new CostumerEvent
				{
					Customer = CurrentParticipation.Customer,
					Event = CurrentEvent,
					ParticipationDate = eventDay.Date,
					Status = ParticipationStatus.Registered
				});
			}
		}
		else
		{
			_eventParticipations.RemoveAll(p => p.Customer.Id == CurrentParticipation.Customer.Id && p.ParticipationDate.Date == eventDay.Date);
		}
	}
	#endregion

	#region Grid Data Provider
	private Task<GridDataProviderResult<CostumerEvent>> ParticipationDataProvider(GridDataProviderRequest<CostumerEvent> req)
	{
		// Einfache Implementierung ohne Paging/Sorting (Grid ist ohne Paging konfiguriert)
		IEnumerable<CostumerEvent> data = _eventParticipations;

		// Filter (einfach: erster Filter auf Nachname / Startnummer)
		var filter = req.Filters?.FirstOrDefault()?.Value?.ToString();
		if (!string.IsNullOrWhiteSpace(filter))
		{
			data = data.Where(p => (p.Customer.Contact.Surname + " " + p.Customer.Contact.Firstname).Contains(filter, StringComparison.OrdinalIgnoreCase)
								 || (p.Customer.Startnumber ?? "").Contains(filter, StringComparison.OrdinalIgnoreCase));
		}

		return Task.FromResult(new GridDataProviderResult<CostumerEvent>
		{
			Data = data.ToList(),
			TotalCount = data.Count()
		});
	}

	private void OnSelectedItemsChanged(IEnumerable<CostumerEvent> selected)
	{
		SelectedParticipation = selected.FirstOrDefault();
	}
	#endregion

	#region CRUD Teilnahme
	public async Task SaveParticipationAsync()
	{
		if (CurrentParticipation.Customer == null || CurrentEvent == null) return;

		try
		{
			// Existiert Kunde im Kontext? (falls aus DropDown gewählt)
			if (CurrentParticipation.Customer.Id > 0)
			{
				var tracked = await Db.Customers.FindAsync(CurrentParticipation.Customer.Id);
				if (tracked != null) CurrentParticipation.Customer = tracked;
			}

			// Event sicherstellen
			CurrentParticipation.Event = CurrentEvent;

			if (CurrentParticipation.Id == 0)
			{
				Db.CustomerEvents.Add(CurrentParticipation);
			}
			else
			{
				Db.CustomerEvents.Update(CurrentParticipation);
			}

			await Db.SaveChangesAsync();
			Message = "Teilnahme gespeichert";
			SelectedParticipation = null;
			NewParticipation = new CostumerEvent();
			EnsureParticipationProperties(NewParticipation);
			await LoadParticipationsAsync();
		}
		catch (DbUpdateException ex)
		{
			Message = $"Datenbankfehler: {ex.GetBaseException().Message}";
		}
		catch (Exception ex)
		{
			Message = $"Fehler: {ex.Message}";
		}
	}

	public async Task DeleteParticipationAsync(long id)
	{
		try
		{
			var entity = await Db.CustomerEvents.FindAsync(id);
			if (entity != null)
			{
				Db.CustomerEvents.Remove(entity);
				await Db.SaveChangesAsync();
				Message = "Teilnahme gelöscht";
				await LoadParticipationsAsync();
			}
		}
		catch (Exception ex)
		{
			Message = $"Fehler beim Löschen: {ex.Message}";
		}
	}
	#endregion

	#region Helper Methods
	/// <summary>
	/// Sicherstellt, dass Customer und andere wichtige Properties nicht null sind
	/// </summary>
	private void EnsureCustomerProperties(Customer customer)
	{
		customer.Contact ??= new Contact();
		customer.Address ??= new Address();
		customer.Bike ??= new Bike();
		customer.Bike.Brand ??= Brands.FirstOrDefault() ?? new Brand();
	}
	#endregion
}
