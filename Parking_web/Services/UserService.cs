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
        
        public Task<T?> GetTotalsAsync<T>(int id, int? orgId)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.GET,
                Url = $"{APIEndPoint}/{id}/Totals?orgId={orgId}",
            });
        }

        public Task<T?> GetUsersPaginationAsync<T>(int? orgId, string? search, string? role, bool active, int page = 1, int pageSize = 10)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.GET,
                Url = $"{APIEndPoint}/Pagination?orgId={orgId}&search={search}&role={role}&active={active}&page={page}&pageSize={pageSize}",
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

        public Task<T?> ActivateSuperAdminAsync<T>(int orgId)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.PUT,
                Url = $"{APIEndPoint}/{orgId}/ActivateSuperAdmin",
            });
        }
    }
}
