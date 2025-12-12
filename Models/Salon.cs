using System.ComponentModel.DataAnnotations;

namespace GokhanOzgunerWEB.Models
{
    public class Salon
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Salon adı zorunludur")]
        [StringLength(100)]
        public string Ad { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Adres { get; set; }

        [Required]
        public TimeSpan AcilisSaati { get; set; }

        [Required]
        public TimeSpan KapanisSaati { get; set; }

        [StringLength(20)]
        public string? Telefon { get; set; }

        public bool Aktif { get; set; } = true;

        // Navigation Properties
        public virtual ICollection<Antrenor> Antrenorler { get; set; } = new List<Antrenor>();
        public virtual ICollection<Hizmet> Hizmetler { get; set; } = new List<Hizmet>();
    }
}