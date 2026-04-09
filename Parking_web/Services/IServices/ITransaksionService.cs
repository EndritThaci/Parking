using Parking_web.Models.DTO;

namespace Parking_web.Services.IServices
{
    public interface ITransaksionService
    {
        Task<T?> GetAsync<T>(int id);
        Task<T?> GetByOrgAsync<T>();
        Task<T?> GetByNjesiAsync<T>();
        Task<T?> GetByUserAsync<T>();
        Task<T?> GetPriceAsync<T>(int id);
        Task<T?> CreateAsync<T>(TransaksionetCreateDto dto);
        Task<T?> UpdateAsync<T>(int id,TransaksionUpdateDto dto);
        Task<T?> PayAsync<T>(int id);
    }
}
