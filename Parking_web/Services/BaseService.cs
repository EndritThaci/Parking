using Parking_web.Models;
using Parking_web.Models.DTO;
using Parking_web.Services.IServices;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Parking_web.Services
{
    public class BaseService : IBaseService
    {
        public IHttpClientFactory HttpClient { get; set; }
        private readonly IHttpContextAccessor _httpContextAccessor;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };
        public IHttpClientFactory _httpClient { get; set; }
        public ApiResponse<object> ResponseModel { get; set; }

        public BaseService(IHttpClientFactory httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            this.ResponseModel = new();
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<T?> SendAsync<T>(ApiRequest apiRequest)
        {
            try
            {
                var client = _httpClient.CreateClient("OrganizataAPI");
                var message = new HttpRequestMessage
                {
                    RequestUri = new Uri(apiRequest.Url!, uriKind:UriKind.RelativeOrAbsolute),
                    Method = GetHttpMethod(apiRequest.ApiType),
                };

                var token = _httpContextAccessor.HttpContext?.Session?.GetString(SD.SessionToken);
                if(!string.IsNullOrEmpty(token))
                {
                   message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                if (apiRequest.Data != null)
                {
                    message.Content = JsonContent.Create(apiRequest.Data, options: JsonOptions);
                }

                var apiResponse = await client.SendAsync(message);

                return await apiResponse.Content.ReadFromJsonAsync<T>(JsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected Error: {ex.Message}");
                return default!;
            }
        }   

        private static HttpMethod GetHttpMethod(SD.ApiType apiType)
        {
            return apiType switch
            {
                SD.ApiType.POST => HttpMethod.Post,
                SD.ApiType.PUT => HttpMethod.Put,
                SD.ApiType.DELETE => HttpMethod.Delete,
                _ => HttpMethod.Get,
            };
        }
    }
}
