using Parking_web.Models.DTO;

namespace Parking_web.Services.IServices
{
    public interface IUserService
    {
        Task<T?> GetAllAsync<T>();
        Task<T?> GetAsync<T>(int id);
        Task<T?> GetUsersPaginationAsync<T>(int? orgId, string? search, string? role, bool active, int page = 1, int pageSize = 10);
        Task<T?> UpdateAsync<T>(UserUpdateDTO dto);
        Task<T?> ChangePasswordAsync<T>(ChangePasswordDTO dto);
        Task<T?> ActivateSuperAdminAsync<T>(int orgId);
        Task<T?> DeleteAsync<T>(int id);
    }
}
