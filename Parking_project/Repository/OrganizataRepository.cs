using Parking_project.Data;
using Parking_project.Models;

namespace Parking_project.Repository
{
    public class OrganizataRepository : IOrganizataRepository
    {
        private readonly AplicationDbContext _db;

        public OrganizataRepository(AplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Organizata> CreateDefaultSherbimNjesiAsync(Organizata organizata)
        {

            //await _db.Organizata.AddAsync(organizata);
            //await _db.SaveChangesAsync();

            var defaultSherbim1 = new Sherbimi
            {
                Emri = "Parking",
                Cmimi = 0,
                Organizata = organizata
            };
            var defaultSherbim2 = new Sherbimi
            {
                Emri = "Auto Larje",
                Cmimi = 1,
                Organizata = organizata
            };
            await _db.Sherbimi.AddAsync(defaultSherbim1);
            await _db.Sherbimi.AddAsync(defaultSherbim2);


            var defaultNjesia = new NjesiOrg
            {
                Emri = organizata.EmriBiznesit,
                BiznesId = organizata.BiznesId,
                Kodi = organizata.NumriUnikIdentifikues,
                Adresa = organizata.Adresa,
            };

            await _db.NjesiOrg.AddAsync(defaultNjesia);
            await _db.SaveChangesAsync();

            return organizata;
        }
    }

}
