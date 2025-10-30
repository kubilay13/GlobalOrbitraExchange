namespace GlobalOrbitra.Models.DTO.UserDto
{
    public class WalletViewModel
    {
        public string Network { get; set; }
        public string TokenSymbol { get; set; }
        public string Address { get; set; }
        public decimal Balance { get; set; }
        public string IconUrl { get; set; }
        public string LogoUrl { get; set; } = "";
    }
}
