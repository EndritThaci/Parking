using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parking_web.Models.DTO
{
    public class TransaksionUpdateDto
    {
        public List<int>? SherbimiId { get; set; }
    }
}
