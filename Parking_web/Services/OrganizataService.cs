using Parking_web.Models;
using Parking_web.Models.DTO;
using Parking_web.Services.IServices;

namespace Parking_web.Services
{
    public class OrganizataService : BaseService ,IOrganizataService
    {
        private readonly string APIEndPoint = "/api/organizata";

        public OrganizataService(IHttpClientFactory httpClient,IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(httpClient,httpContextAccessor)
        {
        }

        public Task<T?> CreateAsync<T>(OrgCreateDTO dto)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.POST,
                Data = dto,
                Url = APIEndPoint,
            });
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

        public Task<T?> GetPaginationAsync<T>(string? search, int? userId, bool onlyAvailable = false, int pageNumber = 1, int pageSize = 10)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.GET,
                Url = $"{APIEndPoint}/Pagination?search={search}&userId={userId}&onlyAvailable={onlyAvailable}&pageNumber={pageNumber}&pageSize={pageSize}",
            });
        }

        public Task<T?> GetForCustomersAsync<T>(string? search, int pageNumber = 1, int pageSize = 10)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.GET,
                Url = $"{APIEndPoint}/ForCustomers?search={search}&pageNumber={pageNumber}&pageSize={pageSize}",
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

        public Task<T?> UpdateAsync<T>(OrgUpdateDTO dto)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.PUT,
                Data = dto,
                Url = $"{APIEndPoint}/{dto.BiznesId}",
            });
        }
    }
}
