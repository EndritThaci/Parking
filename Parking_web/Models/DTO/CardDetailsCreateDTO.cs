namespace Parking_web.Models.DTO
{
    public class CardDetailsCreateDTO
    {
        public string CardName { get; set; }
        public string CardNumber { get; set; }
        public DateOnly ExpirationDate { get; set; }
        public int BankAcountId { get; set; }
    }
}
