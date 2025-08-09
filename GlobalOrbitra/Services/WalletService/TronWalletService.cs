using TronNet;

namespace GlobalOrbitra.Services.WalletService
{
    public class TronWalletService
    {
        public async Task<string>TronCreateWallet()
        {
            var key = TronECKey.GenerateKey(TronNetwork.MainNet);
            var address = key.GetPublicAddress();
            var privatekey = key.GetPrivateKey();

            return $"TRON CREATED WALLET {address}";
        }
    }
}
