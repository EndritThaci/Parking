namespace Parking_project.Models
{
    public class CreditCard
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string StripeCustomerId { get; set; } = "";
        public string StripePaymentMethodId { get; set; } = "";
        public string Brand { get; set; } = "";     // Visa, MasterCard
        public string Last4 { get; set; } = "";     // 4242
        public int ExpMonth { get; set; }
        public int ExpYear { get; set; }
    }
}
