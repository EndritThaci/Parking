using Parking_web.Models.DTO;

namespace Parking_web.Services.IServices
{
    public interface ILokacioniService
    {
        Task<T?> GetAllAsync<T>();
        Task<T?> GetAsync<T>(int id);
        Task<T?> GetByOrgAsync<T>();
        Task<T?> GetByNjesiAsync<T>(int njesiId);
        Task<T?> CreateAsync<T>(LokacioniCreateDTO dto);
        Task<T?> UpdateAsync<T>(LokacioniUpdateDTO dto);
        Task<T?> DeleteAsync<T>(int id);
    }
}
