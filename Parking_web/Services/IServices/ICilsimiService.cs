using Parking_web.Models.DTO;

namespace Parking_web.Services.IServices
{
    public interface ICilsimiService
    {
        Task<T?> GetAllAsync<T>();
        Task<T?> GetAsync<T>(int id);
        Task<T?> GetByOrgAsync<T>();
        Task<T?> GetByNjesiAsync<T>(int njsiaId);
        Task<T?> ActivateAsync<T>(int dto);
        Task<T?> CreateAsync<T>(CilsimetCreateDto dto);
        Task<T?> UpdateAsync<T>(CilsimetUpdateDto dto);
        Task<T?> DeleteAsync<T>(int id);
    }
}
