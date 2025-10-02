using System.ComponentModel.DataAnnotations;

namespace GlobalOrbitra.Models.UserModel
{
    public class ChainModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!; // Ethereum, Tron, Bitcoin

        [Required]
        public string Symbol { get; set; } = null!; // ETH, TRX, BTC

        public string ChainType { get; set; } = null!; // EVM, UTXO, Solana vb.

        public string RpcUrl { get; set; } = null!;

        public bool IsActive { get; set; } = true;

        public ICollection<TokenModel> TokenModels { get; set; } = new List<TokenModel>();
    }
}
