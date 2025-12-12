using System.ComponentModel.DataAnnotations;

namespace GokhanOzgunerWEB.Models
{
    public class AntrenorMusaitlik
    {
        public int Id { get; set; }

        public int AntrenorId { get; set; }

        [Required]
        public DayOfWeek Gun { get; set; }

        [Required]
        public TimeSpan BaslangicSaati { get; set; }

        [Required]
        public TimeSpan BitisSaati { get; set; }

        public bool Aktif { get; set; } = true;

        // Navigation Property
        public virtual Antrenor? Antrenor { get; set; }
    }
}