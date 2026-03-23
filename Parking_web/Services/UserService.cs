using Parking_web.Models;
using Parking_web.Models.DTO;
using Parking_web.Services.IServices;

namespace Parking_web.Services
{
    public class UserService : BaseService, IUserService
    {
        private readonly string APIEndPoint = "/api/users";
        public UserService(IHttpClientFactory httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(httpClient, httpContextAccessor)
        {
        }

        public Task<T?> DeleteAsync<T>(int id)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.DELETE,
                Url = $"{APIEndPoint}/{id}",
            });
        }

        public Task<T?> GetAllAsync<T>()
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.GET,
                Url = APIEndPoint,
            });
        }

        public Task<T?> GetAsync<T>(int id)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.GET,
                Url = $"{APIEndPoint}/{id}",
            });
        }

        public Task<T?> UpdateAsync<T>(UserUpdateDTO dto)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.PUT,
                Data = dto,
                Url = $"{APIEndPoint}/{dto.UserId}",
            });
        }
        public Task<T?> ChangePasswordAsync<T>(ChangePasswordDTO dto)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.PUT,
                Data = dto,
                Url = $"{APIEndPoint}/Password",
            });
        }
    }
}
