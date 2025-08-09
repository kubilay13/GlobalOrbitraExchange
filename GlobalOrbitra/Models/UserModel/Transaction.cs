namespace GlobalOrbitra.Models.UserModel
{
    public class Transaction
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public string WalletAddress { get; set; } = null!;
        public decimal Amount { get; set; }
        public decimal Commusion { get; set; }
        public string Type { get; set; } = null!; // deposit / withdrawal
        public string Status { get; set; } = "pending"; // pending, completed, failed
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
