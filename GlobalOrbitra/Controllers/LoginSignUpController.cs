using GlobalOrbitra.Db;
using GlobalOrbitra.Models.MailModel;
using GlobalOrbitra.Models.UserModel;
using GlobalOrbitra.Services.MailService;
using GlobalOrbitra.Services.WalletService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly GmailMailService _gmailMailService;


        public LoginSignUpController(AppDbContext appDbContext, EthWalletService ethereumWalletService, BscWalletService bscWalletService, SolWalletService solanaWalletService, TronWalletService tronWalletService, GmailMailService gmailMailService)
        {
            _appDbContext = appDbContext;
            _ethereumWalletService = ethereumWalletService;
            _bscWalletService = bscWalletService;
            _solanaWalletService = solanaWalletService;
            _tronWalletService = tronWalletService;
            _gmailMailService = gmailMailService;
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult SignUp()
        {
            return View();
        }

        public IActionResult VerifyIndex()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUpMethod(UserModel model)
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
            UserId = user.Id,
            TokenId = trxToken.Id,
            Network = "TRON",
            UpdatedAt = DateTime.UtcNow
        },
        new UserWalletModel
        {
            Address = ethereumWallet.Address,
            PrivateKey = ethereumWallet.PrivateKey,
            UserId = user.Id,
            TokenId = ethToken.Id,
            Network = "ETH",
            UpdatedAt = DateTime.UtcNow
        },
        new UserWalletModel
        {
            Address = bscWallet.Address,
            PrivateKey = bscWallet.PrivateKey,
            UserId = user.Id,
            TokenId = bscToken.Id,
            Network = "BSC",
            UpdatedAt = DateTime.UtcNow
        },
        new UserWalletModel
        {
            Address = solanaWallet.Address,
            PrivateKey = solanaWallet.PrivateKey,
            UserId = user.Id,
            TokenId = solToken.Id,
            Network = "SOL",
            UpdatedAt = DateTime.UtcNow
        }
    };


            _appDbContext.UserWalletModels.AddRange(wallets);
            _appDbContext.SaveChanges();


            var otp = OtpHelper.GenerateOtp();
            var salt = OtpHelper.GenerateSalt();
            var otpHash = OtpHelper.HashOtp(otp, salt);

            var otpRecord = new OtpCodeModel
            {
                Email = user.Email,
                OtpHash = otpHash,
                Salt = salt,
                Purpose = "registration",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                Consumed = false
            };

            _appDbContext.OtpCodes.Add(otpRecord);
            await _appDbContext.SaveChangesAsync();

            // Mail gönder
            await _gmailMailService.SendOtpAsync(user.Email, otp);

            // TempData ile email geçişi
            TempData["Email"] = user.Email;
            return RedirectToAction("VerifyIndex");
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

        [HttpGet]
        public IActionResult VerifyOtp()
        {
            // TempData'dan e-posta adresini alıyoruz (kayıttan gelen)
            ViewBag.Email = TempData["Email"];
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> VerifyOtpPost(string email, string otp)
        {
            email = email ?? TempData["Email"] as string;
            ViewBag.Email = email;

            Console.WriteLine($"[LOG] Gelen email: {email}, OTP: {otp}");

             var otpRecord = await _appDbContext.OtpCodes
        .Where(x => x.Email == email)
        .OrderByDescending(x => x.CreatedAt)
        .FirstOrDefaultAsync();


            if (otpRecord == null)
            {
                ViewBag.Error = "Kod geçersiz veya süresi dolmuş.";
                ViewBag.Email = email;
                Console.WriteLine("[LOG] OTP bulunamadı veya süresi dolmuş");
                return View("VerifyIndex");
            }

            var hashCheck = OtpHelper.HashOtp(otp, otpRecord.Salt);
            if (hashCheck != otpRecord.OtpHash)
            {
                ViewBag.Error = "Kod hatalı.";
                ViewBag.Email = email;
                Console.WriteLine("[LOG] OTP hatalı");
                return View("VerifyIndex");
            }

            otpRecord.Consumed = true;
            await _appDbContext.SaveChangesAsync();

            TempData["OtpVerified"] = true;
            Console.WriteLine("[LOG] OTP doğrulandı");
            return RedirectToAction("Login");
        }
    }
}


    
