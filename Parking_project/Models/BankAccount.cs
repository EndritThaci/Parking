using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parking_project.Models
{
    public class BankAccount
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string AccountNumber { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public int BankId { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public Useri User { get; set; }

        [ForeignKey(nameof(BankId))]
        public Banka Bank { get; set; }
    }
}