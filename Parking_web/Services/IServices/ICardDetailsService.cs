using Parking_web.Models.DTO;

namespace Parking_web.Services.IServices
{
    public interface ICardDetailsService
    {
        Task<T?> GetByUserAsync<T>();
        Task<T?> GetAsync<T>(int id);
        Task<T?> PayAsync<T>(int id, decimal amount);
        Task<T?> CreateAsync<T>(CardDetailsCreateDTO dto);
        Task<T?> CreateAccountAsync<T>(CardAcountCreateDTO dto);
    }
}
