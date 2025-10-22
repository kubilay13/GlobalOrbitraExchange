using GlobalOrbitra.Db;
using Nethereum.Web3;

namespace GlobalOrbitra.Services.WalletService.WalletListenerService
{
    public class EthWalletListenerService
    {
        private readonly string _infuraSepoliaUrl = "https://sepolia.infura.io/v3/URL"; // Sepolia test ağı URL'si
        private readonly string _infuraMainnetUrl = "https://mainnet.infura.io/v3/URL"; // Mainnet URL'si
        public EthWalletListenerService()
        {
            
        }


        // ETH Cüzdan bakiyelerini alır
        public async Task GetEthWalletBalances(string address)
        {
            var web3url = new Web3("https://sepolia.infura.io/v3/3fcb68529b9e4288a4eb599f266bbb50"); // Sepolia test ağı
            var weiEthBalance = await web3url.Eth.GetBalance.SendRequestAsync("0x31c1fe443E54d007FD1c8c5E7ae7C2356b374616"); // Adresin ETH bakiyesini alır
            var ethBalance = Web3.Convert.FromWei(weiEthBalance.Value);
            Console.WriteLine($"ETH BALANCE {ethBalance}");

        }
    }
}
