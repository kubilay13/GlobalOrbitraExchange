using GlobalOrbitra.Models.DTO.WalletDTO;
using Solnet.Wallet;
using Solnet.Wallet.Utilities;

namespace GlobalOrbitra.Services.WalletService
{
    public class SolWalletService
    {
        public WalletDto SolCreatedWallet()
        {
            var account = new Account();

            var address = account.PublicKey;

            var privateKeyBytes = account.PrivateKey;

            // Base58 encoder örneği:
            var encoder = new Base58Encoder();
            var privateKeyBase58 = encoder.EncodeData(privateKeyBytes);

            return new WalletDto
            {
                Address = address,
                PrivateKey = privateKeyBytes
            };
        }
    }
}
