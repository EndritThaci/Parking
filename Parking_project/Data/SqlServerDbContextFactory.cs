using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;


// Përdoren Për Migrime 
namespace Parking_project.Data
{
    public class SqlServerDbContextFactory : IDesignTimeDbContextFactory<SqlServerDbContext>
    {
        public SqlServerDbContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile(
                    $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json",
                    optional: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<SqlServerDbContext>();

            optionsBuilder.UseSqlServer(
                connectionString,
                options =>
                {
                    options.MigrationsAssembly(typeof(SqlServerDbContext).Assembly.FullName);
                });

            return new SqlServerDbContext(optionsBuilder.Options);
        }
    }
    

    public class PostgreSqlDbContextFactory : IDesignTimeDbContextFactory<PostgreSqlDbContext>
    {
        public PostgreSqlDbContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile(
                    $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json",
                    optional: true)
                .Build();

            var connectionString = configuration.GetConnectionString("PostgreSQLConnection");

            var optionsBuilder = new DbContextOptionsBuilder<PostgreSqlDbContext>();

            optionsBuilder.UseNpgsql(
                connectionString,
                options =>
                {
                    options.MigrationsAssembly(typeof(PostgreSqlDbContext).Assembly.FullName);
                });

            return new PostgreSqlDbContext(optionsBuilder.Options);
        }
    }
}
