using GlobalOrbitra.Db;
using GlobalOrbitra.Models.MailModel;
using GlobalOrbitra.Services.MailService;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;

namespace GlobalOrbitra.Controllers
{
    public class AuthController : Controller
    {

        private readonly AppDbContext _context;
        private readonly GmailMailService _gmailMailService;

        public AuthController(AppDbContext context, GmailMailService mailService)
        {
            _context = context;
            _gmailMailService = mailService;
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOtp(string mail, string otp, string purpose)
        {
            var record = _context.OtpCodes
                .Where(x => x.Email == mail && x.Purpose == purpose && !x.Consumed)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefault();

            if (record == null) return BadRequest("Kod yok.");
            if (record.ExpiresAt < DateTime.UtcNow) return BadRequest("Süresi dolmuş.");

            var hash = OtpHelper.HashOtp(otp, record.Salt);
            if (hash != record.OtpHash) return BadRequest("Kod yanlış.");

            record.Consumed = true;
            await _context.SaveChangesAsync();

            return Ok("Doğrulama başarılı.");
        }


        public IActionResult Index()
        {
            return View();
        }
    }
}
