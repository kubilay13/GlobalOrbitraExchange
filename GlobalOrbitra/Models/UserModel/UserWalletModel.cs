using System.ComponentModel.DataAnnotations;

namespace GlobalOrbitra.Models.UserModel
{
    public class UserWalletModel
    {
        public int Id { get; set; }

        [Required]
        public string Address { get; set; } = null!;

        public string? PrivateKey { get; set; } // saklamak istersen kullan, istemezsen null

        public decimal Balance { get; set; } = 0;

        public int UserId { get; set; }
        public UserModel User { get; set; } = null!;
        public string Network { get; set; } = null!;
        public int TokenId { get; set; }
        public TokenModel Token { get; set; } = null!;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
