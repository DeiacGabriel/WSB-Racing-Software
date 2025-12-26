using Microsoft.AspNetCore.Identity;

namespace WSB_Racing.Models
{
    public class Personal : IdentityUser
    {
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
