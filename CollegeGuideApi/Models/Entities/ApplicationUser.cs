using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace CollegeGuideApi.Models.Entities
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string FName { get; set; }
        public string LName { get; set; }
        public string? ConfirmPassword { get; set; }
        public string UserType { get; set; }
        public bool IsExternalLogin { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
