using Parking_project.Models;
using Parking_project.Models.DTO;

namespace Parking_project.Services
{
    public interface IAuthService
    {
        Task<UserReadDTO> RegisterAsync(UserCreateDTO userCreate, string role);

        Task<LoginResponseDTO> LoginAsync(LoginDTO loginDTO);

        Task<string> ChangePassword(Useri user, string OldPassword, string NewPassword);

        Task<bool> IsEmailExistsAsync(string email);
    }
}
