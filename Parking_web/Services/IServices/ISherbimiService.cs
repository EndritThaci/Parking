using Parking_web.Models.DTO;

namespace Parking_web.Services.IServices
{
    public interface ISherbimiService
    {
        Task<T?> GetAllAsync<T>();
        Task<T?> GetAsync<T>(int id);
        Task<T?> GetByOrgAsync<T>();
        Task<T?> CreateAsync<T>(SherbimiCreateDTO dto);
        Task<T?> CreateParkingAsync<T>(SherbimParkingDTO dto);
        Task<T?> UpdateAsync<T>(SherbimiUpdateDTO dto);
        Task<T?> DeleteAsync<T>(int id);
    }
}
