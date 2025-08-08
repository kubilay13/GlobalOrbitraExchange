using Binance.Net.Clients;
using GlobalOrbitra.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GlobalOrbitra.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly BinanceSocketClient _binanceSocketClient;
        public HomeController(ILogger<HomeController> logger, BinanceSocketClient binanceSocketClient)
        {
            _binanceSocketClient = binanceSocketClient;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Errortest()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult About()
        {
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
