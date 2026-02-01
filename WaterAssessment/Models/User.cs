using System.ComponentModel.DataAnnotations;

namespace WaterAssessment.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        [MaxLength(50)] // نام کاربری معمولاً محدود است
        public string Username { get; set; }

        [Required]
        public string PasswordHash { get; set; } // هش پسورد یک رشته طولانی شامل کاراکترهای خاص است

        public string? FullName { get; set; }
        public string Role { get; set; } // مثلا "Admin" یا "User"
    }
}
