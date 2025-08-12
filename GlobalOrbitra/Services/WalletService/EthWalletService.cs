using GlobalOrbitra.Models.DTO.WalletDTO;
using Nethereum.Signer;

namespace GlobalOrbitra.Services.WalletService
{
    public class EthWalletService
    {
        public WalletDto EthCreateWallet()
        {
            var key = EthECKey.GenerateKey();
            var privateKey = key.GetPrivateKey();
            var address = key.GetPublicAddress();

            return new WalletDto
            {
                Address = address,
                PrivateKey = privateKey
            };
        }
    }
}
