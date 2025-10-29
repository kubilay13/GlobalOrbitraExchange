using System.Net;
using System.Net.Mail;

namespace GlobalOrbitra.Services.MailService
{
    public class GmailMailService
    {
        private readonly string _smtpUser;       // Gmail hesabın
        private readonly string _smtpAppPassword; // Gmail App Password

        public GmailMailService(string smtpUser, string smtpAppPassword)
        {
            _smtpUser = smtpUser;
            _smtpAppPassword = smtpAppPassword;
        }

        public async Task<bool> SendOtpAsync(string toEmail, string otp)
        {
            try
            {
                var mail = new MailMessage();
                mail.To.Add(toEmail);
                mail.From = new MailAddress(_smtpUser);
                mail.Subject = "Doğrulama Kodunuz";
                mail.Body = $"Giriş/Kayıt doğrulama kodunuz: {otp}";
                mail.IsBodyHtml = false;

                using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential(_smtpUser, _smtpAppPassword);
                    smtp.EnableSsl = true;
                    await smtp.SendMailAsync(mail);
                }
                return true; // Başarılı
            }
            catch
            {
                return false; // Hata
            }
        }
        public async Task<bool> SendLoginwarning(string toEmail , string otp)
        { 
                var mail = new MailMessage();
                mail.To.Add(toEmail);
                mail.From = new MailAddress(_smtpUser);
                mail.Subject = "Hesabınızda Giriş Yapıldı!";
                mail.Body = $"Hesabınıza Giriş Yapıldı Eğer bu siz değilseniz, lütfen hesabınızı korumak için gerekli önlemleri alın";
                mail.IsBodyHtml = false;
                using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential(_smtpUser, _smtpAppPassword);
                    smtp.EnableSsl = true;
                    await smtp.SendMailAsync(mail);
                }
                return true; 
        }
    }
}
