using GlobalOrbitra.Models.UserModel;
using Microsoft.AspNetCore.Mvc;

namespace GlobalOrbitra.Controllers
{
    public class WidthdrawDepositController : Controller
    {
        public IActionResult Widthdraw()
        {
            var assets = new List<Asset>
        {
            new Asset { Symbol = "BTC", Name = "Bitcoin", ImageUrl = "/images/btc.png" },
            new Asset { Symbol = "ETH", Name = "Ethereum", ImageUrl = "/images/eth.png" },
            new Asset { Symbol = "USDT", Name = "Tether", ImageUrl = "/images/usdt.png" }
        };

            return View(assets);
        }
    }
}
