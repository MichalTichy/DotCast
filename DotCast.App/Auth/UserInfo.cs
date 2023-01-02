using System.ComponentModel.DataAnnotations;

namespace DotCast.App.Auth
{
    public class UserInfo
    {
        [Required]
        public string Username { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;

        public bool IsAdmin { get; set; }
    }
}