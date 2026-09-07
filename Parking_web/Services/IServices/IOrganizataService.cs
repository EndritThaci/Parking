using Parking_web.Models.DTO;

namespace Parking_web.Services.IServices
{
    public interface IOrganizataService
    {
        Task<T?> GetAllAsync<T>();
        Task<T?> GetForCustomersAsync<T>(int pageNumber = 1, int pageSize = 10);
        Task<T?> GetPaginationAsync<T>(string? search, int? userId, int pageNumber = 1, int pageSize = 10);
        Task<T?> GetAsync<T>(int id );
        Task<T?> CreateAsync<T>(OrgCreateDTO dto);
        Task<T?> UpdateAsync<T>(OrgUpdateDTO dto);
        Task<T?> DeleteAsync<T>(int id);
    }
}
