namespace Parking_web.Models
{
    public class TransaksionDetaj
    {
        public int TransaksionId { get; set; }
        public TransaksionParkimi TransaksionParkimi { get; set; }

        public int SherbimiId { get; set; }
        public Sherbimi Sherbimi { get; set; }

        //public int Quantity { get; set; }
        public decimal Cmimi { get; set; }
    }
}
