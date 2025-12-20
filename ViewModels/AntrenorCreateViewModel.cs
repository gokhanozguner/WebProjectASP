using System.ComponentModel.DataAnnotations;

namespace GokhanOzgunerWEB.ViewModels
{
    public class AntrenorCreateViewModel
    {
        [Required(ErrorMessage = "Ad Soyad zorunludur")]
        [StringLength(100)]
        public string AdSoyad { get; set; } = string.Empty;

        [StringLength(500)]
        public string? UzmanlikAlanlari { get; set; }

        [StringLength(20)]
        public string? Telefon { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public bool Aktif { get; set; } = true;

        [Required]
        public int SalonId { get; set; }

        // Müsaitlik bilgileri
        public List<MusaitlikViewModel> Musaitlikler { get; set; } = new List<MusaitlikViewModel>();
    }

    public class MusaitlikViewModel
    {
        public DayOfWeek Gun { get; set; }
        public string GunAdi { get; set; } = string.Empty;
        public bool Secildi { get; set; }
        public TimeSpan BaslangicSaati { get; set; } = new TimeSpan(9, 0, 0);  // 09:00
        public TimeSpan BitisSaati { get; set; } = new TimeSpan(18, 0, 0);     // 18:00
    }
}