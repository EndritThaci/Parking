namespace Parking_project.Models.DTO
{
    public class LibriShitjesCreateDTO
    {
        public int? id { get; set; }
        public DateOnly? day { get; set; }
        public int? month { get; set; }
        public int? year { get; set; }
        public bool? all { get; set; }
        public int? njesia { get; set; }
    }
}