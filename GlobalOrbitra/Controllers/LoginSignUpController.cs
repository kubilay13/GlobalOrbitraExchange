using GlobalOrbitra.Db;
using GlobalOrbitra.Models.UserModel;
using GlobalOrbitra.Services.WalletService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        public IActionResult SignUpMethod(UserModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest("Model geçersiz");

            // 1️⃣ Kullanıcı oluştur
            var user = new UserModel
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = model.PasswordHash,
                CreatedAt = DateTime.UtcNow
            };
            _appDbContext.UserModels.Add(user);
            _appDbContext.SaveChanges();

            // 2️⃣ Token ID’lerini DB’den al (örn: TRX, ETH, BSC, SOL)
            var trxToken = _appDbContext.TokenModels.First(t => t.Name == "TRX");
            var ethToken = _appDbContext.TokenModels.First(t => t.Name == "ETH");
            var bscToken = _appDbContext.TokenModels.First(t => t.Name == "BSC");
            var solToken = _appDbContext.TokenModels.First(t => t.Name == "SOL");

            // 3️⃣ Cüzdan oluştur (servislerden)
            var tronWallet = _tronWalletService.TronCreateWallet();
            var ethereumWallet = _ethereumWalletService.EthCreateWallet();
            var bscWallet = _bscWalletService.BscCreatedWallet();
            var solanaWallet = _solanaWalletService.SolCreatedWallet();

            // 4️⃣ UserWallet ekle
            var wallets = new List<UserWalletModel>
    {
        new UserWalletModel
        {
            Address = tronWallet.Address,
            PrivateKey = tronWallet.PrivateKey,
            Balance = 0,
            UserId = user.Id,
            TokenId = trxToken.Id,
            UpdatedAt = DateTime.UtcNow
        },
        new UserWalletModel
        {
            Address = ethereumWallet.Address,
            PrivateKey = ethereumWallet.PrivateKey,
            Balance = 0,
            UserId = user.Id,
            TokenId = ethToken.Id,
            UpdatedAt = DateTime.UtcNow
        },
        new UserWalletModel
        {
            Address = bscWallet.Address,
            PrivateKey = bscWallet.PrivateKey,
            Balance = 0,
            UserId = user.Id,
            TokenId = bscToken.Id,
            UpdatedAt = DateTime.UtcNow
        },
        new UserWalletModel
        {
            Address = solanaWallet.Address,
            PrivateKey = solanaWallet.PrivateKey,
            Balance = 0,
            UserId = user.Id,
            TokenId = solToken.Id,
            UpdatedAt = DateTime.UtcNow
        }
    };

         
            _appDbContext.UserWalletModels.AddRange(wallets);
            _appDbContext.SaveChanges();

            return RedirectToAction("Login", "LoginSignUp");
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Email ve şifre gerekli.");
                return View();
            }

            var user = _appDbContext.UserModels
                .FirstOrDefault(u => u.Email == email && u.PasswordHash == password);

            if (user == null)
            {
                ModelState.AddModelError("", "Email veya şifre yanlış.");
                return View();
            }

            var claims = new List<Claim>
            {
               new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
               new Claim(ClaimTypes.Email, user.Email)
            };

            var identity = new ClaimsIdentity(claims, "MyCookieAuth");
            var principal = new ClaimsPrincipal(identity);

            HttpContext.SignInAsync("MyCookieAuth", principal);

            return RedirectToAction("Dashboard", "Dashboard");
        }
    }
}
