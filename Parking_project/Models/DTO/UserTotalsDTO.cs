namespace Parking_project.Models.DTO
{
    public class UserTotalsDTO
    {
        public UserReadDTO User { get; set; } = new();

        public TransactionTotalsDTO Total { get; set; } = new();
    }

    public class TransactionTotalsDTO
    {
        public int CountToday { get; set; }
        public decimal AmountToday { get; set; }
        public int CountWeek { get; set; }
        public decimal AmountWeek { get; set; }
        public int CountMonth { get; set; }
        public decimal AmountMonth { get; set; }
        public int CountYear { get; set; }
        public decimal AmountYear { get; set; }
        public int CountAll { get; set; }
        public decimal AmountAll { get; set; }
    }
}