using Parking_web.Models;
using Parking_web.Models.DTO;

namespace Parking_web.Services.IServices
{
    public interface IBaseService
    {
        ApiResponse<object> ResponseModel { get; set; } 
        Task<T> SendAsync<T>(ApiRequest apiRequest);
    }
}
