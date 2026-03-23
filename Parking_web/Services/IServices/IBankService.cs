namespace Parking_web.Services.IServices
{
    public interface IBankService
    {
        Task<T?> GetAllAsync<T>();
        Task<T?> GetAsync<T>(int id);
        Task<T?> CreateAsync<T>(string name);
    }
}
