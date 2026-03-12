using Parking_project.Models;

namespace Parking_project.Repository
{
    public interface IOrganizataRepository
    {
        Task<Organizata> CreateDefaultSherbimNjesiAsync(Organizata organizata);
    }

}
