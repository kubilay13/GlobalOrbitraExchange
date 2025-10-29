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
        private readonly GmailMailService _gmailMailService;
        private readonly WalletService _walletService;


        public LoginSignUpController(AppDbContext appDbContext,WalletService walletService, GmailMailService gmailMailService)
        {
            _walletService = walletService;
            _appDbContext = appDbContext;
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
                return View(model);

            bool UsernameExists = _appDbContext.UserModels.Any(u => u.Username == model.Username);
            bool UserEmailExists = _appDbContext.UserModels.Any(u=>u.Username==model.Username);

            if(UsernameExists)
            {
                ModelState.AddModelError("Username","Bu kullanıcı adı zaten alınmış.");
            }

            if (UserEmailExists)
            {
                ModelState.AddModelError("Email", "Bu e-posta zaten kayıtlı.");
            }

            if (UsernameExists || UserEmailExists)
            {
                return View("SignUp", model); 
            }
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
        public async Task<IActionResult> Login(string email, string password)
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
            await _gmailMailService.SendLoginwarning(user.Email, "Hesabınıza giriş yapıldı.");

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

            var otpRecord = await _appDbContext.OtpCodes
                .Where(x => x.Email == email)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

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

            // ✅ Şimdi kullanıcıyı bul
            var user = await _appDbContext.UserModels.FindAsync(userId);
            if (user == null)
            {
                ViewBag.Error = "Kullanıcı bulunamadı.";
                return View("VerifyIndex");
            }

            // ✅ Tokenları DB’den al (Seed data’da bunlar)
            var trxToken = _appDbContext.TokenModels.First(t => t.Symbol == "TRX");
            var ethToken = _appDbContext.TokenModels.First(t => t.Symbol == "ETH");
            var bscToken = _appDbContext.TokenModels.First(t => t.Symbol == "BSC");
            var solToken = _appDbContext.TokenModels.First(t => t.Symbol == "SOL");
            var bttcToken = _appDbContext.TokenModels.First(t => t.Symbol == "BTT");


            if (trxToken == null || ethToken == null || bscToken == null || solToken == null || bttcToken== null)
            {
                ViewBag.Error = "Token verileri eksik. Lütfen sistem yöneticisine başvurun.";
                return View("VerifyIndex");
            }

            // ✅ Cüzdan oluşturma
            var tronWallet = _walletService.TronWallet;
            var ethWallet = _walletService.EthWallet;
            var bscWallet = _walletService.BscWallet;
            var solWallet = _walletService.SolWallet;
            var bttcwallet = _walletService.BttcWallet;

            if (tronWallet == null || ethWallet == null || bscWallet == null || solWallet == null)
            {
                ViewBag.Error = "Cüzdan oluşturulamadı. Lütfen tekrar deneyin.";
                return View("VerifyIndex");
            }

            // ✅ Kullanıcıya ait cüzdan kayıtlarını ekle
            var wallets = new List<UserWalletModel>
            {
                new() { Address = tronWallet.Address, PrivateKey = tronWallet.PrivateKey, UserId = user.Id, TokenId = trxToken.Id, Network = "TRON", UpdatedAt = DateTime.UtcNow },
                new() { Address = ethWallet.Address, PrivateKey = ethWallet.PrivateKey, UserId = user.Id, TokenId = ethToken.Id, Network = "ETH", UpdatedAt = DateTime.UtcNow },
                new() { Address = bscWallet.Address, PrivateKey = bscWallet.PrivateKey, UserId = user.Id, TokenId = bscToken.Id, Network = "BSC", UpdatedAt = DateTime.UtcNow },
                new() { Address = solWallet.Address, PrivateKey = solWallet.PrivateKey, UserId = user.Id, TokenId = solToken.Id, Network = "SOL", UpdatedAt = DateTime.UtcNow },
                new() { Address = bttcwallet.Address, PrivateKey = bttcwallet.PrivateKey, UserId = user.Id, TokenId = bttcToken.Id, Network = "BTTC", UpdatedAt = DateTime.UtcNow }
            };

            await _appDbContext.UserWalletModels.AddRangeAsync(wallets);
            await _appDbContext.SaveChangesAsync();

            TempData["OtpVerified"] = true;
            return RedirectToAction("Login");
        }
    }
}


    
