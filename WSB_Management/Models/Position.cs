using Microsoft.AspNetCore.Identity;

namespace WSB_Management.Models
{
    public class Position : IdentityRole<int>
    {
        public int PositionId { get; set; }
        public string PositionName { get; set; } = string.Empty;
    }
}
