namespace Parking_project.Models.DTO
{
    public class CreditCardCreateDto
    {
        public int UserId { get; set; }
        //public string StripeCustomerId { get; set; }
        public string StripePaymentMethodId { get; set; }

        //public string Brand { get; set; }
        //public string Last4 { get; set; }
        //public int ExpMonth { get; set; }
        //public int ExpYear { get; set; }
    }
}
