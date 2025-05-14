using Microsoft.AspNetCore.Identity;

namespace Finance_Literacy_App_Web.Models
{
    public class User : IdentityUser
    {
        public string Role { get; set; } = "user";
        public bool IsActive { get; set; } = true;
        public int? GroupId { get; set; }
        public Group Group { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}