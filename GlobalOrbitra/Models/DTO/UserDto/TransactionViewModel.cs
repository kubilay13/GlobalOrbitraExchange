namespace GlobalOrbitra.Models.DTO.UserDto
{
    public class TransactionViewModel
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public string Asset { get; set; }
        public decimal Amount { get; set; }
        public decimal Fee { get; set; }
        public string Status { get; set; }
        public string TxHash { get; set; }
        public string FromTo { get; set; }
        public string WalletAddress { get; set; }
        public string SenderAddress { get; set; }
    }
}
