namespace Parking_web.Services.IServices
{
    public interface IUserOrgService
    {
        Task<T?> GetUserOrgByUserAsync<T>(int id);
        Task<T?> DeleteUserOrgAsync<T>(int userId);
    }
}
