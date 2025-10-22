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

            // 1️⃣ Kullanıcı oluştur (henüz wallet yok)
            var user = new UserModel
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = model.PasswordHash,
                CreatedAt = DateTime.UtcNow
            };
            _appDbContext.UserModels.Add(user);
            await _appDbContext.SaveChangesAsync();

            // 2️⃣ OTP oluştur
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

            // 3️⃣ Mail gönder
            await _gmailMailService.SendOtpAsync(user.Email, otp);

            TempData["Email"] = user.Email;
            TempData["UserId"] = user.Id; // UserId geçişi için
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
            var userId = Convert.ToInt32(TempData["UserId"]);
            ViewBag.Email = email;

            var otpRecord = await _appDbContext.OtpCodes.Where(x => x.Email == email).OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync();

            if (otpRecord == null || otpRecord.ExpiresAt < DateTime.UtcNow)
            {
                ViewBag.Error = "Kod geçersiz veya süresi dolmuş.";
                return View("VerifyIndex");
            }

            var hashCheck = OtpHelper.HashOtp(otp, otpRecord.Salt);
            if (hashCheck != otpRecord.OtpHash)
            {
                ViewBag.Error = "Kod hatalı.";
                return View("VerifyIndex");
            }

            // ✅ OTP doğrulandı
            otpRecord.Consumed = true;
            await _appDbContext.SaveChangesAsync();

            // ✅ Şimdi cüzdanları oluştur
            var user = await _appDbContext.UserModels.FindAsync(userId);

            var trxToken = _appDbContext.TokenModels.First(t => t.Name == "TRX");
            var ethToken = _appDbContext.TokenModels.First(t => t.Name == "ETH");
            var bscToken = _appDbContext.TokenModels.First(t => t.Name == "BSC");
            var solToken = _appDbContext.TokenModels.First(t => t.Name == "SOL");

            var tronWallet = _tronWalletService.TronCreateWallet();
            var ethWallet = _ethereumWalletService.EthCreateWallet();
            var bscWallet = _bscWalletService.BscCreatedWallet();
            var solWallet = _solanaWalletService.SolCreatedWallet();

            var wallets = new List<UserWalletModel>
            {
                  new() { Address = tronWallet.Address, PrivateKey = tronWallet.PrivateKey, UserId = user.Id, TokenId = trxToken.Id, Network = "TRON", UpdatedAt = DateTime.UtcNow },
                  new() { Address = ethWallet.Address, PrivateKey = ethWallet.PrivateKey, UserId = user.Id, TokenId = ethToken.Id, Network = "ETH", UpdatedAt = DateTime.UtcNow },
                  new() { Address = bscWallet.Address, PrivateKey = bscWallet.PrivateKey, UserId = user.Id, TokenId = bscToken.Id, Network = "BSC", UpdatedAt = DateTime.UtcNow },
                  new() { Address = solWallet.Address, PrivateKey = solWallet.PrivateKey, UserId = user.Id, TokenId = solToken.Id, Network = "SOL", UpdatedAt = DateTime.UtcNow }
            };

            _appDbContext.UserWalletModels.AddRange(wallets);
            await _appDbContext.SaveChangesAsync();

            TempData["OtpVerified"] = true;
            return RedirectToAction("Login");
        }
    }
}


    
