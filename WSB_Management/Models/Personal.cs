using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using System.Text;

namespace WSB_Management.Models
{
    public class Personal : IdentityUser<int>
    {
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
