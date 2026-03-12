using Parking_web.Models;
using Parking_web.Models.DTO;
using Parking_web.Services.IServices;

namespace Parking_web.Services
{
    public class AuthService : BaseService, IAuthService
    {
        private readonly string APIEndPoint = "/api/auth";

        public AuthService(IHttpClientFactory httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(httpClient, httpContextAccessor)
        {
        }

        public Task<T?> LoginAsync<T>(LoginDTO login)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.POST,
                Data = login,
                Url = APIEndPoint + "/logIn",
            });
        }
        public Task<T?> RegisterAsync<T>(UserCreateDTO userCreateDTO)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.POST,
                Data = userCreateDTO,
                Url = APIEndPoint + "/signUp",
            });
        }
        public Task<T?> RegisterAdminAsync<T>(UserCreateDTO userCreateDTO)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.POST,
                Data = userCreateDTO,
                Url = APIEndPoint + "/signUp/Admin",
            });
        }

        public Task<T?> RegisterManagerAsync<T>(UserCreateDTO userCreateDTO)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.POST,
                Data = userCreateDTO,
                Url = APIEndPoint + "/signUp/Manager",
            });
        }
    }
}
