using Parking_web.Models;
using Parking_web.Models.DTO;
using Parking_web.Services.IServices;
using System.Drawing.Printing;

namespace Parking_web.Services
{
    public class TransaksionService : BaseService, ITransaksionService
    {
        private readonly string APIEndPoint = "/api/transaksionetParkimit";
        public TransaksionService(IHttpClientFactory httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(httpClient, httpContextAccessor)
        {
        }

        public Task<T?> CreateAsync<T>(TransaksionetCreateDto dto)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.POST,
                Data = dto,
                Url = APIEndPoint,
            });
        }

        public Task<T?> GetAsync<T>(int pageNumber = 1, int pageSize = 10, int njesia = -1)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.GET,
                Url = $"{APIEndPoint}?pageNumber={pageNumber}&pageSize={pageSize}&njesiaId={njesia}",
            });
        }

        public Task<T?> GetByNjesiAsync<T>(int? njesia, string? search, DateTime? dateFrom, DateTime? dateTo, string? status, int page = 1, int pageSize = 10)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.GET,
                Url = $"{APIEndPoint}/ByNjesi?njesia={njesia}&search={search}&dateFrom={dateFrom}&dateTo={dateTo}&status={status}&page={page}&pageSize={pageSize}",
            });
        }

        public Task<T?> GetByUserAsync<T>(int pageNumber = 1, int pageSize = 10, int njesiaId = 0)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.GET,
                Url = $"{APIEndPoint}/ByUser?pageNumber={pageNumber}&pageSize={pageSize}&njesiaId={njesiaId}",
            });
        }

        public Task<T?> GetPendingAsync<T>(int? userId = null, int? njesiaId = null)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.GET,
                Url = $"{APIEndPoint}/Pending?userId={userId}&njesiaId={njesiaId}",
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

        public Task<T?> GetPriceAsync<T>(int id)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.GET,
                Url = $"{APIEndPoint}/{id}/Price",
            });
        }


        public Task<T?> UpdateAsync<T>(int id,TransaksionUpdateDto dto)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.PUT,
                Data = dto,
                Url = $"{APIEndPoint}/{id}",
            });
        }

        public Task<T?> PayAsync<T>(int id, int? cardId)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.PUT,
                Url = $"{APIEndPoint}/{id}/Pay?cardId={cardId}",
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
    }
}
