using Parking_web.Models.DTO;

namespace Parking_web.Services.IServices
{
    public interface IUserService
    {
        Task<T?> GetAllAsync<T>();
        Task<T?> GetAsync<T>(int id);
        Task<T?> UpdateAsync<T>(UserUpdateDTO dto);
        Task<T?> ChangePasswordAsync<T>(ChangePasswordDTO dto);
        Task<T?> DeleteAsync<T>(int id);
    }
}
