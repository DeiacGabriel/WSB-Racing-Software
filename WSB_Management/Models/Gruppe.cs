using System.ComponentModel;

namespace WSB_Management.Models
{
    public class Gruppe
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public TimeSpan? MaxTimelap { get; set; } // Maximale Zeit für diese Gruppe
    }
}
