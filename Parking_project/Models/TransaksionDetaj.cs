using Parking_project.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parking_project.Models
{
    public class TransaksionDetaj
    {
        public int TransaksionId { get; set; }
        public TransaksionParkimi TransaksionParkimi { get; set; }

        public int SherbimiId { get; set; }
        public Sherbimi Sherbimi { get; set; }

        //public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Cmimi { get; set; }
    }
}
