using Microsoft.AspNetCore.Mvc;

namespace GlobalOrbitra.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
