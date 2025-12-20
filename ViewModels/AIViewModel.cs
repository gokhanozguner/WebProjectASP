using System.ComponentModel.DataAnnotations;

namespace GokhanOzgunerWEB.ViewModels
{
    public class AIRecommendationViewModel
    {
        [Display(Name = "Boy (cm)")]
        [Range(100, 250, ErrorMessage = "Boy 100-250 cm arasında olmalıdır")]
        public int? Boy { get; set; }

        [Display(Name = "Kilo (kg)")]
        [Range(30, 300, ErrorMessage = "Kilo 30-300 kg arasında olmalıdır")]
        public int? Kilo { get; set; }

        [Display(Name = "Yaş")]
        [Range(10, 100, ErrorMessage = "Yaş 10-100 arasında olmalıdır")]
        public int? Yas { get; set; }

        [Display(Name = "Cinsiyet")]
        public string? Cinsiyet { get; set; }

        [Display(Name = "Hedef")]
        public string? Hedef { get; set; }

        [Display(Name = "Vücut Fotoğrafı")]
        public IFormFile? Foto { get; set; }

        public string? Oneri { get; set; }

        // ... Boy, Kilo, Foto vs.)
        // Fotoğraf adresi:
        public string? GeneratedImageBase64 { get; set; }
    }
}