using System.ComponentModel;
using System.Globalization;

namespace WSB_Management.Models
{
    public class Team
    {
        public long Id { get; set; }
        public string Name { get; set; } = "";
        public List<Customer>? Members { get; set; }
        public Customer? TeamChef { get; set; }
    }
}
