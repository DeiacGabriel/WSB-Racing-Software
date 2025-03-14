using Microsoft.AspNetCore.Identity;

namespace WSB_Racing.Models
{
    public class Position:IdentityRole
    {
        public int PositionId { get; set; }
        public string PositionName { get; set; }
    }
}
