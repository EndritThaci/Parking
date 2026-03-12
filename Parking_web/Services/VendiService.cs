using Parking_web.Models;
using Parking_web.Models.DTO;
using Parking_web.Services.IServices;

namespace Parking_web.Services
{
    public class VendiService : BaseService, IVendiService
    {
        private readonly string APIEndPoint = "/api/vendi";
        public VendiService(IHttpClientFactory httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(httpClient, httpContextAccessor)
        {
        }

        public Task<T?> CreateAsync<T>(VendiCreateDTO dto)
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

        public Task<T?> GetByOrgAsync<T>()
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.GET,
                Url = $"{APIEndPoint}/ByOrg",
            });
        }

        public Task<T?> GetByNjesiAsync<T>()
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.GET,
                Url = $"{APIEndPoint}/ByNjesi",
            });
        }

        public Task<T?> GetByLokacionAsync<T>(int? lokacioniId)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.GET,
                Url = $"{APIEndPoint}/{lokacioniId}/ByLokacion",
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

        public Task<T?> UpdateAsync<T>(VendiUpdateDTO dto)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.PUT,
                Data = dto,
                Url = $"{APIEndPoint}/{dto.VendiId}",
            });
        }
    }
}
