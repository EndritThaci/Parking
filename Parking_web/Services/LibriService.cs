using Parking_web.Models;
using Parking_web.Models.DTO;
using Parking_web.Services.IServices;

namespace Parking_web.Services
{
    public class LibriService : BaseService, ILibriService
    {
        private readonly string APIEndPoint = "/api/LibriShitjes";
        public LibriService(IHttpClientFactory httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(httpClient, httpContextAccessor)
        {
        }

        public Task<T?> GetLibriShitjesAsync<T>(LibriShitjesCreateDTO dto)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.GET,
                Data = dto,
                Url = $"{APIEndPoint}?id={dto.id}&month={dto.month}&year={dto.year}&all={dto.all}&njesia={dto.njesia}",
            });
        }
    }
}
