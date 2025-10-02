using System.ComponentModel.DataAnnotations;

namespace GlobalOrbitra.Models.UserModel
{
    public class UserModel
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Username { get; set; } = null!;

        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string PasswordHash { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<UserWalletModel> Wallets { get; set; } = new List<UserWalletModel>();
    }
}
