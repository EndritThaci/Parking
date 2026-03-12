using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Parking_project.Models
{
    public class Useri
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Emri { get; set; }

        [Required]
        [StringLength(100)]
        public string Mbiemri { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; }

        [Required]
        [StringLength(255)]
        public string Passwordi { get; set; }

        
        [MaxLength(50)]
        public string Role { get; set; } = "Customer";

        public int? BiznesId { get; set; }
        public int? NjesiaId { get; set; }


        [ForeignKey(nameof(BiznesId))]
        public Organizata? Organizata { get; set; }

        [ForeignKey(nameof(NjesiaId))]
        public NjesiOrg? Njesi { get; set; }
    }
}
