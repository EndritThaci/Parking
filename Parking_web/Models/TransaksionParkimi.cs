using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parking_web.Models
{
    public class TransaksionParkimi
    {
        [Key]
        public int TransaksioniId { get; set; }

        [Required]
        public DateTime KohaHyrjes { get; set; }
        public DateTime? KohaDaljes { get; set; }

        [Required]
        [StringLength(50)]
        public string Statusi { get; set; }

        [Required]
        public int VendiParkimitId { get; set; }
        
        [ForeignKey(nameof(VendiParkimitId))]
        public Vendi Vendi { get; set; }
        
        [Required]
        public int NjesiaId { get; set; }
        
        [ForeignKey(nameof(NjesiaId))]
        public NjesiOrg Njesia { get; set; }
        
        [Required]
        public int CilsimiId { get; set; }

        [ForeignKey(nameof(CilsimiId))]
        public CilsimetParkimit Cilsimet{ get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public Useri User { get; set; }

    }
}
