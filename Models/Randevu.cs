using GokhanOzgunerWEB.Models;
using System.ComponentModel.DataAnnotations;

namespace GokhanOzgunerWEB.Models
{
    public enum RandevuDurum
    {
        Beklemede = 0,
        Onaylandi = 1,
        Reddedildi = 2,
        Tamamlandi = 3,
        IptalEdildi = 4
    }

    public class Randevu
    {
        public int Id { get; set; }

        [Required]
        public string UyeId { get; set; } = string.Empty;

        [Required]
        public int AntrenorId { get; set; }

        [Required]
        public int HizmetId { get; set; }

        [Required]
        public DateTime RandevuTarihi { get; set; }

        [Required]
        public TimeSpan BaslangicSaati { get; set; }

        [Required]
        public TimeSpan BitisSaati { get; set; }

        public RandevuDurum Durum { get; set; } = RandevuDurum.Beklemede;

        [StringLength(500)]
        public string? Notlar { get; set; }

        public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual ApplicationUser? Uye { get; set; }
        public virtual Antrenor? Antrenor { get; set; }
        public virtual Hizmet? Hizmet { get; set; }
    }
}