using System.Security.Cryptography;
using System.Text;

namespace GlobalOrbitra.Services.MailService
{
    public class OtpHelper
    {
        public static string GenerateOtp()
        {
            var n = RandomNumberGenerator.GetInt32(0, 1000000);
            return n.ToString("D6");
        }

        public static string GenerateSalt()
        {
            var bytes = RandomNumberGenerator.GetBytes(16);
            return Convert.ToBase64String(bytes);
        }

        public static string HashOtp(string otp, string salt)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(salt));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(otp));
            return Convert.ToBase64String(hash);
        }
    }
}
