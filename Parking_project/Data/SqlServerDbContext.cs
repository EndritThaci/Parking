using Microsoft.EntityFrameworkCore;

namespace Parking_project.Data
{
    public class SqlServerDbContext : AplicationDbContext
    {
        public SqlServerDbContext( DbContextOptions<SqlServerDbContext> options) : base(options)
        {
        }
    }

    public class PostgreSqlDbContext : AplicationDbContext
    {
        public PostgreSqlDbContext( DbContextOptions<PostgreSqlDbContext> options) : base(options)
        {
        }
    }
}