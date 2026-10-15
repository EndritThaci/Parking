using Parking_web.Models.DTO;

namespace Parking_web.Services.IServices
{
    public interface ITransaksionService
    {
        Task<T?> GetAsync<T>(int id);
        Task<T?> GetAsync<T>(int pageNumber = 1, int pageSize = 10, int njesiaId = -1, string? status = null);
        Task<T?> GetByNjesiAsync<T>(int? njesia, string? search, DateTime? dateFrom, DateTime? dateTo, string? status, int page = 1, int pageSize = 10);
        Task<T?> GetByUserAsync<T>(int pageNumber = 1, int pageSize = 10, int njesiaId = 0, string? status = null);
        Task<T?> GetPendingAsync<T>(int? userId = null, int? njesiaId = null);
        Task<T?> GetReservedAsync<T>(int? userId = null, int? njesiaId = null);
        Task<T?> GetPriceAsync<T>(int id);
        Task<T?> CreateAsync<T>(TransaksionetCreateDto dto, string? Status = null);
        Task<T?> UpdateAsync<T>(int id,TransaksionUpdateDto dto);
        Task<T?> PayAsync<T>(int id, int? cardId);
        Task<T?> EnterAsync<T>(int id);
        Task<T?> DeleteAsync<T>(int id);
    }
}
