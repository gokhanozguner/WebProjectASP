using GokhanOzgunerWEB.Models;
using System.ComponentModel.DataAnnotations;

namespace GokhanOzgunerWEB.Models
{
    public class Antrenor
    {
        public int Id { get; set; }

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

        public int SalonId { get; set; }

        // Navigation Properties
        public virtual Salon? Salon { get; set; }
        public virtual ICollection<AntrenorMusaitlik> Musaitlikler { get; set; } = new List<AntrenorMusaitlik>();
        public virtual ICollection<AntrenorHizmet> AntrenorHizmetleri { get; set; } = new List<AntrenorHizmet>();
        public virtual ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();
    }
}