using System.ComponentModel.DataAnnotations;

namespace GlobalOrbitra.Models.MailModel
{
    public class OtpCodeModel
    {
        public int Id { get; set; }  

        [Required, EmailAddress]
        public string Email { get; set; } = null!; // Kullanıcının .onion mail adresi

        [Required]
        public string OtpHash { get; set; } = null!; // 6 haneli kodun hash'i

        [Required]
        public string Salt { get; set; } = null!; // Hash için salt değeri

        [Required]
        public string Purpose { get; set; } = null!; // "registration" veya "login"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Oluşturulma zamanı
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(10); // 10 dk geçerli
        public bool Consumed { get; set; } = false; // Kod kullanıldı mı
    }
}
