namespace GlobalOrbitra.Models.UserModel
{
    public class TransactionModel
    {
        public int Id { get; set; }

        // Kullanıcı ilişkisi
        public int UserId { get; set; }
        public UserModel User { get; set; } = null!;

        //  cüzdan adresi
        public string WalletAddress { get; set; } = null!;
        // Gönderen adres
        public string SenderAddress { get; set; } = null!;

        // Hangi token/coin
        public int? TokenId { get; set; }
        public TokenModel? Token { get; set; }

        // Miktar ve komisyon
        public decimal Amount { get; set; }
        public decimal Commission { get; set; }

        // İşlem tipi ve durumu
        public string Type { get; set; } = null!; // deposit | withdrawal
        public string Status { get; set; } = "pending"; // pending | completed | failed

        // Blockchain işlem kimliği
        public string? TxHash { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
