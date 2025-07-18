using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using WSB_Management.Data;
using WSB_Management.Migrations;
using WSB_Management.Models;
namespace WSB_Management.Components.Pages
{
    public partial class Customer
    {
        // Kunden Details
        private string SelectedFirma { get; set; }
        private string Vorname { get; set; }
        private string Nachname { get; set; }
        private string Strasse { get; set; }
        private string PlzOrt { get; set; }
        private string SelectedLand { get; set; } = "AT";
        private DateTime? Geburtsdatum { get; set; }
        private int Alter { get; set; }
        private string Email { get; set; }
        private string Telefon { get; set; }
        private string NotfallEmail { get; set; }
        private string NotfallTelefon { get; set; }
        private string UID { get; set; }
        private string Sponsor { get; set; }
        private bool Tc5k { get; set; }
        private bool Teamchef { get; set; }
        private string SelectedTeam { get; set; }
        private bool EndCup { get; set; }
        private bool EndCupTeamchef { get; set; }
        private string EndCupTeam { get; set; }
        private decimal? Guthaben { get; set; }
        private DateTime? GuthabenDatum { get; set; }
        private decimal? GuthabenBetrag { get; set; }
        private string GuthabenBemerkung { get; set; }
        private decimal? Preisgeld { get; set; }
        private int? Gratisfahrer { get; set; }
        private int? Schurke { get; set; }
        private bool VerzichtOK { get; set; }
        private string SelectedGruppe { get; set; }

        // Bike
        private string SelectedMarke { get; set; }
        private string Type { get; set; }
        private string Klasse { get; set; }
        private DateTime? KundeSeit { get; set; }
        private int? Baujahr { get; set; }
        private string Youngtimer { get; set; }
        private DateTime? LetzteBuchung { get; set; }
        private string StartNummer { get; set; }
        private string Tran { get; set; }
        private string TranID { get; set; }
        private DateTime? LetzterEinkauf { get; set; }
        private string S8S { get; set; }
        private string Speeddays { get; set; }

        // Jahreswertung
        private bool WsbCup { get; set; }
        private bool SuzukiChallenge { get; set; }
        private bool LadiesCup { get; set; }
        private bool BmwBoxerCup { get; set; }
        private bool Stock1000Cup { get; set; }
        private bool DucatiChallenge { get; set; }
        private bool BmsS1000Cup { get; set; }
        private bool YoungtimerCup { get; set; }
        private bool Lizenz { get; set; }
        private bool Jahreskarte { get; set; }

        // Laptime Referenzen
        private string BRN { get; set; }
        private string MUG { get; set; }
        private string CRE { get; set; }
        private string PAN { get; set; }
        private string HUN { get; set; }
        private string RBR { get; set; }
        private string LAU { get; set; }
        private string RIJ { get; set; }
        private string MST { get; set; }
        private string SLO { get; set; }
        
        // Filter
        public string f_Nachname { get; set; }
        public string f_Vorname { get; set; }
        public string f_StartNr { get; set; }
        public string f_Email { get; set; }
        public DateTime? f_Geb { get; set; }
        public DateTime? f_GebBis { get; set; }
        public ObservableCollection<Models.Customer> Customers { get; set; } = new ObservableCollection<Models.Customer>();
        private readonly WSBRacingDbContext _context;
        public Customer(WSBRacingDbContext context)
        {
            _context = context;
            LoadCustomersAsync();
        }
        public void LoadCustomersAsync()
        {
            var customerList = _context.Customers.ToList();
            Customers = new ObservableCollection<Models.Customer>(customerList);
        }
        public void AddCustomer()
        {
            try
            {
                var newCustomer = new Models.Customer
                {
                    Firstname = Vorname,
                    Surname = Nachname,
                    Mail = Email,
                    Phonenumber = Telefon,
                    Birthdate = Geburtsdatum ?? DateTime.MinValue,
                    Validfrom = GuthabenDatum ?? DateTime.MinValue,
                    Startnumber = StartNummer,
                    Newsletter = false // Default value, can be changed 
                };
                _context.Customers.Add(newCustomer);
                _context.SaveChanges();
                LoadCustomersAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Hinzufügen des Kunden: {ex.Message}");
            }
        }
        public void UpdateCustomer(Models.Customer customer)
        {
            try
            {
                var existingCustomer = _context.Customers.Find(customer.Id);

                if (existingCustomer is null)
                {
                    Console.WriteLine("Kunde nicht gefunden.");
                    return;
                }

                existingCustomer.Firstname = string.IsNullOrWhiteSpace(Vorname) ? existingCustomer.Firstname : Vorname.Trim();
                existingCustomer.Surname = string.IsNullOrWhiteSpace(Nachname) ? existingCustomer.Surname : Nachname.Trim();
                existingCustomer.Mail = string.IsNullOrWhiteSpace(Email) ? existingCustomer.Mail : Email.Trim();
                existingCustomer.Phonenumber = string.IsNullOrWhiteSpace(Telefon) ? existingCustomer.Phonenumber : Telefon.Trim();
                existingCustomer.Birthdate = Geburtsdatum.HasValue ? Geburtsdatum.Value : existingCustomer.Birthdate;
                existingCustomer.Validfrom = GuthabenDatum ?? existingCustomer.Validfrom;
                existingCustomer.Startnumber = string.IsNullOrWhiteSpace(StartNummer) ? existingCustomer.Startnumber : StartNummer.Trim();

                _context.SaveChanges();
                LoadCustomersAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Aktualisieren des Kunden (ID: {customer.Id}): {ex.Message}");
            }
        }
        public void DeleteCustomer(long customerId)
        {
            try
            {
                var customer = _context.Customers.Find(customerId);
                if (customer != null)
                {
                    _context.Customers.Remove(customer);
                    _context.SaveChanges();
                    LoadCustomersAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Löschen des Kunden: {ex.Message}");
            }
        }
        public void FilterCustomers()
        {
            var filteredCustomers = _context.Customers
                .Where(c =>
                    (string.IsNullOrEmpty(f_Nachname) || c.Surname.Contains(f_Nachname)) &&
                    (string.IsNullOrEmpty(f_Vorname) || c.Firstname.Contains(f_Vorname)) &&
                    (string.IsNullOrEmpty(f_StartNr) || (c.Startnumber != null && c.Startnumber.Contains(f_StartNr))) &&
                    (string.IsNullOrEmpty(f_Email) || (c.Mail != null && c.Mail.Contains(f_Email))) &&
                    (!f_Geb.HasValue || c.Birthdate >= f_Geb.Value) &&
                    (!f_GebBis.HasValue || c.Birthdate <= f_GebBis.Value)
                )
                .ToList();
            Customers = new ObservableCollection<Models.Customer>(filteredCustomers);
        }
        public void ClearFilter()
        {
            f_Nachname = string.Empty;
            f_Vorname = string.Empty;
            f_StartNr = string.Empty;
            f_Email = string.Empty;
            f_Geb = null;
            f_GebBis = null;
            LoadCustomersAsync();
        }
    }
}
