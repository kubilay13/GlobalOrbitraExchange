using System.ComponentModel.DataAnnotations;

namespace GlobalOrbitra.Models.UserModel
{
    public class User
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Username { get; set; } = null!;

        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string PasswordHash { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<UserWallet> Wallets { get; set; } = new List<UserWallet>();
    }
}
