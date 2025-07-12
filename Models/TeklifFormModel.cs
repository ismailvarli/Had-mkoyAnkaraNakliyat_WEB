using System.ComponentModel.DataAnnotations;

namespace HadımkoyAnkaraNakliyat_WEB.Models
{
    public class TeklifFormModel
    {
        [Required(ErrorMessage = "Ad Soyad zorunlu.")]
        public string AdSoyad { get; set; }

        [Required(ErrorMessage = "Email zorunlu.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Telefon zorunlu.")]
        public string Telefon { get; set; }

        public string Tarih { get; set; }
        public string? Agirlik { get; set; }
        public string? AlinanSehir { get; set; }
        public string? TasinanSehir { get; set; }
    }
}
