using GlobalOrbitra.Models.DTO.WalletDTO;
using TronNet;

namespace GlobalOrbitra.Services.WalletService
{
    public class TronWalletService
    {
        public WalletDto TronCreateWallet()
        {
            var key = TronECKey.GenerateKey(TronNetwork.MainNet);
            var address = key.GetPublicAddress();
            var privatekey = key.GetPrivateKey();

            return new WalletDto
            {
                Address = address,
                PrivateKey = privatekey
            };
        }
    }
}
