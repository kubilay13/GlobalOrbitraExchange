using Nethereum.Signer;

namespace GlobalOrbitra.Services.WalletService
{
    public class BscWalletService
    {
        public async Task<string> BscCreatedWallet()
        {
            var key = EthECKey.GenerateKey();
            var privateKey = key.GetPrivateKey();
            var address = key.GetPublicAddress();
            return $"Bsc CREATED WALLET {address}";
        }
    }
}
