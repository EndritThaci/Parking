using Parking_web.Models;
using Parking_web.Services.IServices;

namespace Parking_web.Services
{
    public class BankService : BaseService, IBankService
    {
        private readonly string APIEndPoint = "/api/bank";
        public BankService(IHttpClientFactory httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(httpClient, httpContextAccessor)
        {
        }

        public Task<T?> CreateAsync<T>(string name)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.POST,
                Data = name,
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

        public Task<T?> GetAllAsync<T>()
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.GET,
                Url = APIEndPoint,
            });
        }
    }
}
