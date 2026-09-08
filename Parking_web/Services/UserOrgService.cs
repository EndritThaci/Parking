using Parking_web.Models;
using Parking_web.Services.IServices;

namespace Parking_web.Services
{
    public class UserOrgService : BaseService, IUserOrgService
    {
        private readonly string APIEndPoint = "/api/userOrg";
        public UserOrgService(IHttpClientFactory httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(httpClient, httpContextAccessor)
        {
        }

        public Task<T?> DeleteUserOrgAsync<T>(int userId)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.DELETE,
                Url = $"{APIEndPoint}/User/{userId}",
            });
        }

        public Task<T?> GetUserOrgByUserAsync<T>(int id)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.GET,
                Url = $"{APIEndPoint}/User/{id}",
            });
        }
    }
}
