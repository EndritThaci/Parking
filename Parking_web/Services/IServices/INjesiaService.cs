using Parking_web.Models.DTO;

namespace Parking_web.Services.IServices
{
    public interface INjesiaService
    {
        Task<T?> GetAllAsync<T>();
        Task<T?> GetAsync<T>(int id);
        Task<T?> GetByOrgAsync<T>();
        Task<T?> CreateAsync<T>(NjesiOrgDto dto);
        Task<T?> UpdateAsync<T>(NjesiUpdateDto dto);
        Task<T?> DeleteAsync<T>(int id);
    }
}
