using GlobalOrbitra.Db;
using GlobalOrbitra.Models.UserModel;
using GlobalOrbitra.Services.WalletService;
using Microsoft.AspNetCore.Mvc;

namespace GlobalOrbitra.Controllers
{
    public class LoginSignUpController : Controller
    {
        private readonly AppDbContext _appDbContext;
        private readonly TronWalletService _tronWalletService;
        private readonly EthWalletService _ethereumWalletService;
        private readonly BscWalletService _bscWalletService;
        private readonly SolWalletService _solanaWalletService;


        public LoginSignUpController(AppDbContext appDbContext, EthWalletService ethereumWalletService, BscWalletService bscWalletService, SolWalletService solanaWalletService,TronWalletService tronWalletService)
        {
            _appDbContext = appDbContext;
            _ethereumWalletService = ethereumWalletService;
            _bscWalletService = bscWalletService;
            _solanaWalletService = solanaWalletService;
            _tronWalletService = tronWalletService;
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SignUpMethod(User model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    Username = model.Username,
                    Email = model.Email,
                    PasswordHash = model.PasswordHash,
                    CreatedAt = DateTime.UtcNow,

                };

                _appDbContext.Users.Add(user);
                _appDbContext.SaveChanges();

                var tronWallet = _tronWalletService.TronCreateWallet();
                var ethereumWallet = _ethereumWalletService.EthCreateWallet();
                var bscWallet = _bscWalletService.BscCreatedWallet();
                var solanaWallet = _solanaWalletService.SolCreatedWallet();

                var wallets = new List<UserWallet>
                {
                   new UserWallet
                   {
                       Address = tronWallet.Address,
                       PrivateKey = tronWallet.PrivateKey,
                       Network = "TRON",
                       Balance = 0,
                       UserId = user.Id,
                       UpdatedAt = DateTime.UtcNow
                   },
                   new UserWallet
                   {
                       Address = ethereumWallet.Address,
                       PrivateKey = ethereumWallet.PrivateKey,
                       Network = "ETH",
                       Balance = 0,
                       UserId = user.Id,
                       UpdatedAt = DateTime.UtcNow
                   },
                   new UserWallet
                   {
                       Address = bscWallet.Address,
                       PrivateKey = bscWallet.PrivateKey,
                       Network = "BSC",
                       Balance = 0,
                       UserId = user.Id,
                       UpdatedAt = DateTime.UtcNow
                   },
                   new UserWallet
                   {
                       Address = solanaWallet.Address,
                       PrivateKey = solanaWallet.PrivateKey,
                       Network = "SOL",
                       Balance = 0,
                       UserId = user.Id,
                       UpdatedAt = DateTime.UtcNow
                   },
                };

                _appDbContext.UserWallets.AddRange(wallets);
                _appDbContext.SaveChanges();
                return RedirectToAction("Login", "LoginSignUp");

            }
            return Ok("Kullanıcı ve cüzdan oluşturuldu");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LoginUp(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Email ve şifre gerekli.");
                return View();
            }

            var user = _appDbContext.Users
                .FirstOrDefault(u => u.Email == email && u.PasswordHash == password);

            if (user == null)
            {
                ModelState.AddModelError("", "Email veya şifre yanlış.");
                return View();
            }

            // Giriş başarılı - session veya cookie ayarlanabilir
            HttpContext.Session.SetInt32("UserId", user.Id);

            return RedirectToAction("Dashboard", "Dashboard");
        }
    }
}
