using System.ComponentModel;
using System.Globalization;
using WSB_Management.Converter;

namespace WSB_Management.Models
{
    [TypeConverter(typeof(TeamTypeConverter))]
    public class Team
    {
        public long Id { get; set; }
        public string Name { get; set; } = "";
        public List<Customer>? Members { get; set; }
        public Customer? TeamChef { get; set; }
    }
}
