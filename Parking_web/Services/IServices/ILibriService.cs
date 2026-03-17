using Parking_web.Models.DTO;

namespace Parking_web.Services.IServices
{
    public interface ILibriService
    {
        Task<T?> GetLibriShitjesAsync<T>(LibriShitjesCreateDTO dto);
    }
}
