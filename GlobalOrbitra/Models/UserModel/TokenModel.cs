
namespace GlobalOrbitra.Models.UserModel
{
    public class TokenModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; } = null!;
        public string ContractAddress { get; set; }
        public decimal Decimal { get; set; }
        public bool IsToken { get; set; }
        public int ChainId { get; set; }
        public  ChainModel Chain { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<UserWalletModel> UserWallets { get; set; } = new List<UserWalletModel>();
    }

}
