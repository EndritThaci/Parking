namespace Parking_web.Models.DTO
{
    public class CardAcountCreateDTO
    {
        public string AccountNumber { get; set; }
        public decimal Amount { get; set; }
        public int BankId { get; set; }
        public string CardNumber { get; set; }
        public DateOnly ExpirationDate { get; set; }
    }
}
