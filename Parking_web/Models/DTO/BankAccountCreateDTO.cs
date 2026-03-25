namespace Parking_web.Models.DTO
{
    public class BankAccountCreateDTO
    {
        public string AccountNumber { get; set; }
        public decimal Amount { get; set; }
        public int BankId { get; set; }
    }
}
