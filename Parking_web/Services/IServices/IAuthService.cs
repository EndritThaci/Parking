using Parking_web.Models.DTO;

namespace Parking_web.Services.IServices
{
    public interface IAuthService
    {
        Task<T?> LoginAsync<T>(LoginDTO login);
        Task<T?> RegisterAsync<T>(UserCreateDTO userCreateDTO);
        Task<T?> RegisterAdminAsync<T>(UserCreateDTO userCreateDTO);
        Task<T?> RegisterManagerAsync<T>(UserCreateDTO userCreateDTO);
    }
}
