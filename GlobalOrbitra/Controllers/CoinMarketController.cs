using Microsoft.AspNetCore.Mvc;

namespace GlobalOrbitra.Controllers
{
    public class CoinMarketController : Controller
    {
        [HttpGet]
        public IActionResult CoinMarket(string symbol)
        {
            ViewBag.Symbol = symbol; // View'e geçiriyoruz
            return View();
        }
    }
}
