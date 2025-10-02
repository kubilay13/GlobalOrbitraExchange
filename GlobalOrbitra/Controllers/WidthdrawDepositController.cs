using GlobalOrbitra.Models.UserModel;
using Microsoft.AspNetCore.Mvc;

namespace GlobalOrbitra.Controllers
{
    public class WidthdrawDepositController : Controller
    {
        public IActionResult Widthdraw()
        {
            var assets = new List<TokenModel>
            {
                //new TokenModel { Symbol = "BTC", Name = "Bitcoin", ImageUrl = "/images/btc.png" },
                //new TokenModel { Symbol = "ETH", Name = "Ethereum", ImageUrl = "/images/eth.png" },
                //new TokenModel { Symbol = "USDT", Name = "Tether", ImageUrl = "/images/usdt.png" }
            };

               return View(assets);
        }

        public IActionResult Deposit()
        {
            var assets = new List<TokenModel>
            {
                //new TokenModel { Symbol = "BTC", Name = "Bitcoin", ImageUrl = "/images/btc.png" },
                //new TokenModel { Symbol = "ETH", Name = "Ethereum", ImageUrl = "/images/eth.png" },
                //new TokenModel { Symbol = "USDT", Name = "Tether", ImageUrl = "/images/usdt.png" }
            };

            return View(assets);
        }

    }
}
