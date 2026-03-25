using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parking_web.Models
{
    public class CardDetails
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string CardNumber { get; set; }

        [Required]
        public DateOnly ExpirationDate { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int BankAcountId { get; set; }

        [ForeignKey(nameof(UserId))]
        public Useri User { get; set; }

        [ForeignKey(nameof(BankAcountId))]
        public BankAccount BankAccount { get; set; }

    }
}
