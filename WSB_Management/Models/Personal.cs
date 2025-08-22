using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using System.Text;

namespace WSB_Management.Models
{
    public class Personal : IdentityUser<int>
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Position { get; set; }
        public string Description { get; set; }
    }
}
