using System.ComponentModel.DataAnnotations;

namespace TapAndGo.Api.Models
{
    public class RegisterDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public string Rol { get; set; } 
    }
}
