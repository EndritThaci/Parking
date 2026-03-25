namespace Parking_project.Models.DTO
{
    public class CardDetailsCreateDTO
    {
        public string CardNumber { get; set; }
        public DateOnly ExpirationDate { get; set; }
        public int BankAcountId { get; set; }
    }
}
