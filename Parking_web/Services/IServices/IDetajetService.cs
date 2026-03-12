using Parking_web.Models.DTO;

namespace Parking_web.Services.IServices
{
    public interface IDetajetService
    {
        Task<T?> GetAllAsync<T>();
        Task<T?> GetAsync<T>(int id);
        Task<T?> GetByOrgAsync<T>();
        Task<T?> GetByNjesiAsync<T>();
        Task<T?> CreateAsync<T>(DetajetCreateDto dto);
        Task<T?> UpdateAsync<T>(int id,DetajetUpdateDto dto);
        Task<T?> DeleteAsync<T>(int id);
    }
}
