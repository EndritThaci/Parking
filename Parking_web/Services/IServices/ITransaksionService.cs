using Parking_web.Models.DTO;

namespace Parking_web.Services.IServices
{
    public interface ITransaksionService
    {
        Task<T?> GetAsync<T>(int id);
        Task<T?> GetAsync<T>(int pageNumber = 1, int pageSize = 10, int njesiaId = -1);
        Task<T?> GetByNjesiAsync<T>();
        Task<T?> GetByUserAsync<T>(int pageNumber = 1, int pageSize = 10, int njesiaId = 0);
        Task<T?> GetPendingAsync<T>(int? userId = null, int? njesiaId = null);
        Task<T?> GetPriceAsync<T>(int id);
        Task<T?> CreateAsync<T>(TransaksionetCreateDto dto);
        Task<T?> UpdateAsync<T>(int id,TransaksionUpdateDto dto);
        Task<T?> PayAsync<T>(int id, int? cardId);
        Task<T?> DeleteAsync<T>(int id);
    }
}
