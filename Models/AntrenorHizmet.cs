namespace GokhanOzgunerWEB.Models
{
    // Many-to-Many ilişki tablosu: Antrenör hangi hizmetleri verebilir
    public class AntrenorHizmet
    {
        public int Id { get; set; }

        public int AntrenorId { get; set; }

        public int HizmetId { get; set; }

        // Navigation Properties
        public virtual Antrenor? Antrenor { get; set; }
        public virtual Hizmet? Hizmet { get; set; }
    }
}