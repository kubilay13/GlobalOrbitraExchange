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
        [HttpGet]
        public IActionResult ResetPassword()
        {
            if (TempData["OtpVerified"] == null)
                return RedirectToAction("Login");

            ViewBag.UserId = TempData["UserId"];
            TempData.Keep("UserId");
            TempData.Keep("OtpVerified");
            return View();
        }
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("MyCookieAuth");
            return RedirectToAction("Login", "LoginSignUp");
        }
        [HttpPost]
        public async Task<IActionResult> SignUpMethod(UserModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            bool UsernameExists = _appDbContext.UserModels.Any(u => u.Username == model.Username);
            bool UserEmailExists = _appDbContext.UserModels.Any(u => u.Email == model.Email);

            if (UsernameExists)
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
                 new Claim(ClaimTypes.Email, user.Email),
                 new Claim(ClaimTypes.Name, user.Username)
            };

            var identity = new ClaimsIdentity(claims, "login");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(principal);
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
            email ??= TempData["Email"]?.ToString();
            var userId = Convert.ToInt32(TempData["UserId"]);

            TempData.Keep("Email");
            TempData.Keep("UserId");

            var otpRecord = await _appDbContext.OtpCodes
                .Where(x => x.Email == email)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (otpRecord == null || otpRecord.ExpiresAt < DateTime.UtcNow)
            {
                ViewBag.Error = "Kod geçersiz veya süresi dolmuş.";
                return View("VerifyIndex");
            }

            if (OtpHelper.HashOtp(otp, otpRecord.Salt) != otpRecord.OtpHash)
            {
                ViewBag.Error = "Kod hatalı.";
                return View("VerifyIndex");
            }

            otpRecord.Consumed = true;
            await _appDbContext.SaveChangesAsync();

            // Şifre sıfırlama
            if (otpRecord.Purpose == "forgot_password")
            {
                TempData["OtpVerified"] = true;
                TempData["UserId"] = userId;
                return RedirectToAction("ResetPassword");
            }

            // Kayıt → cüzdan oluştur
            var user = await _appDbContext.UserModels.FindAsync(userId);

            var tokens = _appDbContext.TokenModels.ToList();

            var wallets = new List<UserWalletModel>
            {
                new() { Address = _walletService.TronWallet.Address, PrivateKey = _walletService.TronWallet.PrivateKey, UserId = user.Id, TokenId = tokens.First(x=>x.Symbol=="TRX").Id, Network = "TRON", UpdatedAt = DateTime.UtcNow },
                new() { Address = _walletService.EthWallet.Address, PrivateKey = _walletService.EthWallet.PrivateKey, UserId = user.Id, TokenId = tokens.First(x=>x.Symbol=="ETH").Id, Network = "ETH", UpdatedAt = DateTime.UtcNow },
                new() { Address = _walletService.BscWallet.Address, PrivateKey = _walletService.BscWallet.PrivateKey, UserId = user.Id, TokenId = tokens.First(x=>x.Symbol=="BSC").Id, Network = "BSC", UpdatedAt = DateTime.UtcNow },
                new() { Address = _walletService.SolWallet.Address, PrivateKey = _walletService.SolWallet.PrivateKey, UserId = user.Id, TokenId = tokens.First(x=>x.Symbol=="SOL").Id, Network = "SOL", UpdatedAt = DateTime.UtcNow },
                new() { Address = _walletService.BttcWallet.Address, PrivateKey = _walletService.BttcWallet.PrivateKey, UserId = user.Id, TokenId = tokens.First(x=>x.Symbol=="BTT").Id, Network = "BTTC", UpdatedAt = DateTime.UtcNow }
            };

            await _appDbContext.UserWalletModels.AddRangeAsync(wallets);
            await _appDbContext.SaveChangesAsync();

            TempData["OtpVerified"] = true;
            return RedirectToAction("Login");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPasswordPost(int userId, string NewPassword, string ConfirmPassword)
        {
            if (NewPassword != ConfirmPassword)
            {
                ModelState.AddModelError("", "Şifreler uyuşmuyor.");
                ViewBag.UserId = userId;
                return View("ResetPassword");
            }

            var user = await _appDbContext.UserModels.FindAsync(userId);
            if (user == null)
                return RedirectToAction("Login");

            user.PasswordHash = NewPassword;
            await _appDbContext.SaveChangesAsync();
            await _gmailMailService.SendResetPassword(user.Email, "Şifreniz başarıyla sıfırlandı.");
            return RedirectToAction("Login");
        }


        // POST: E-posta girildiğinde OTP gönder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(UserModel model)
        {
            var user = await _appDbContext.UserModels.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                ModelState.AddModelError("Email", "Bu e-posta kayıtlı değil.");
                return View();
            }

            var otp = OtpHelper.GenerateOtp();
            var salt = OtpHelper.GenerateSalt();
            var otpHash = OtpHelper.HashOtp(otp, salt);

            _appDbContext.OtpCodes.Add(new OtpCodeModel
            {
                Email = user.Email,
                OtpHash = otpHash,
                Salt = salt,
                Purpose = "forgot_password",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10)
            });

            await _appDbContext.SaveChangesAsync();
            await _gmailMailService.SendOtpAsync(user.Email, otp);

            TempData["Email"] = user.Email;
            TempData["UserId"] = user.Id;

            return RedirectToAction("VerifyIndex");
        }
    }
}


    
