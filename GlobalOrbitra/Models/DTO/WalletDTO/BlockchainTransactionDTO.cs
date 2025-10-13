namespace GlobalOrbitra.Models.DTO.WalletDTO
{
    public class BlockchainTransactionDto
    {
        public string TxHash { get; set; } = null!;
        public string From { get; set; } = null!;
        public string To { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Network { get; set; } = null!;
        public DateTime Timestamp { get; set; }
        public string TokenSymbol { get; set; } = null!;
        //public string? TokenContract { get; set; }
    }
}
