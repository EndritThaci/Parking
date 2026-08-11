using Parking_web.Models.DTO;

namespace Parking_web.Services.IServices
{
    public interface ICreditCardService
    {
        Task<T?> GetByUserAsync<T>();
        Task<T?> GetAsync<T>(int id);
        Task<T?> DeleteAsync<T>(int id);
        Task<T?> CreateAsync<T>(CreditCardCreateDto dto);
        Task<T?> PayAsync<T>(PayRequestDto dto);
    }
}
