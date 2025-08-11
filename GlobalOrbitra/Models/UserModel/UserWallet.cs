using System.ComponentModel.DataAnnotations;

namespace GlobalOrbitra.Models.UserModel
{
    public class UserWallet
    {
        public int Id { get; set; }

        [Required]
        public string Address { get; set; } = null!;

        public decimal Balance { get; set; } = 0;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

}
