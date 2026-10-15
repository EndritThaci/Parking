namespace Parking_web.Services.IServices
{
    public interface IUserOrgService
    {
        Task<T?> GetUserOrgByUserAsync<T>(int id);
        Task<T?> CreateUserOrgByNjesiAsync<T>(int userId, int njesiaId);
        Task<T?> UpdateAllUserOrgsAsync<T>(bool add);
        Task<T?> DeleteUserOrgAsync<T>(int userId);
    }
}
