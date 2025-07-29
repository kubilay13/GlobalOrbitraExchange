using Microsoft.AspNetCore.Mvc;

namespace GlobalOrbitra.Controllers
{
    public class CoinMarketController : Controller
    {
        public IActionResult CoinMarket()
        {
            return View();
        }
    }
}
