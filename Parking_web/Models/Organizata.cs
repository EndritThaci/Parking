using System.ComponentModel.DataAnnotations;

namespace Parking_web.Models
{
    public class Organizata
    {
        [Key]
        public int BiznesId { get; set; }

        [Required]
        [StringLength(200)]
        public string EmriBiznesit { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string NumriUnikIdentifikues { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Adresa { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string NumriBiznesit { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string NumriFiskal { get; set; } = string.Empty;

        public int NumriPunetoreve { get; set; }

        [Required]
        public DateTime DataRegjistrimit { get; set; }

        [Required]
        [StringLength(100)]
        public string Komuna { get; set; } = string.Empty;

        [Phone]
        [StringLength(30)]
        public string Telefoni { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        public bool AllowCustomers { get; set; } = true;
        public bool QRScanner { get; set; } = false;
    }
}
