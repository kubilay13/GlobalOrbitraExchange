using Microsoft.AspNetCore.Mvc;

namespace GlobalOrbitra.Controllers
{
    public class LoginSignUpController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult SignUp()
        {
            return View();
        }
    }
}
