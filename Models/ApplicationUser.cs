using Microsoft.AspNetCore.Identity;

namespace GokhanOzgunerWEB.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? AdSoyad { get; set; }
        public DateTime KayitTarihi { get; set; } = DateTime.UtcNow;
        public string? VucutTipi { get; set; }
        public decimal? Boy { get; set; }
        public decimal? Kilo { get; set; }

        // Navigation Properties
        public virtual ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();
    }
}