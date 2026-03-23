using Parking_web.Models;
using Parking_web.Models.DTO;
using Parking_web.Services.IServices;

namespace Parking_web.Services
{
    public class CardDetailsService : BaseService, ICardDetailsService
    {
        private readonly string APIEndPoint = "/api/cardDetails";
        public CardDetailsService(IHttpClientFactory httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(httpClient, httpContextAccessor)
        {
        }

        public Task<T?> CreateAsync<T>(CardDetailsCreateDTO dto)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.POST,
                Data = dto,
                Url = APIEndPoint,
            });
        }

        public Task<T?> CreateAccountAsync<T>(CardAcountCreateDTO dto)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.POST,
                Data = dto,
                Url = $"{APIEndPoint}/Account",
            });
        }

        public Task<T?> GetByUserAsync<T>()
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.GET,
                Url = $"{APIEndPoint}/ByUser",
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

        public Task<T?> PayAsync<T>(int id, decimal amount)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.PUT,
                Data = amount,
                Url = $"{APIEndPoint}/Pay/{id}",
            });
        }
    }
}
