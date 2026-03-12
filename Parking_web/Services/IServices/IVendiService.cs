using Parking_web.Models.DTO;

namespace Parking_web.Services.IServices
{
    public interface IVendiService
    {
        Task<T?> GetAllAsync<T>();
        Task<T?> GetAsync<T>(int id);
        Task<T?> GetByOrgAsync<T>();
        Task<T?> GetByNjesiAsync<T>();
        Task<T?> GetByLokacionAsync<T>(int? lokacioniId);
        Task<T?> CreateAsync<T>(VendiCreateDTO dto);
        Task<T?> UpdateAsync<T>(VendiUpdateDTO dto);
        Task<T?> DeleteAsync<T>(int id);
    }
}
