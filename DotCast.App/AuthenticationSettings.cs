using System.ComponentModel.DataAnnotations;

namespace DotCast.App
{
    public class AuthenticationSettings
    {
        public ICollection<UserInfo> Users { get; set; } = null!;
    }

    public class UserInfo
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        public bool IsAdmin { get; set; }
    }
}