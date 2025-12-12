using GokhanOzgunerWEB.Models;
using System.ComponentModel.DataAnnotations;

namespace GokhanOzgunerWEB.Models
{
    public class Hizmet
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Hizmet adı zorunludur")]
        [StringLength(100)]
        public string Ad { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Aciklama { get; set; }

        [Required(ErrorMessage = "Süre zorunludur")]
        [Range(15, 240, ErrorMessage = "Süre 15-240 dakika arasında olmalıdır")]
        public int Sure { get; set; } // Dakika cinsinden

        [Required(ErrorMessage = "Ücret zorunludur")]
        [Range(0, 10000, ErrorMessage = "Ücret 0-10000 TL arasında olmalıdır")]
        public decimal Ucret { get; set; }

        public bool Aktif { get; set; } = true;

        public int SalonId { get; set; }

        // Navigation Properties
        public virtual Salon? Salon { get; set; }
        public virtual ICollection<AntrenorHizmet> AntrenorHizmetleri { get; set; } = new List<AntrenorHizmet>();
        public virtual ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();
    }
}