using DataImport.Models;
using DataImport.Web.Areas.Instance.Modules;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DataImport.Web.Areas.Instance.Models
{
    public class InstanceSqlDataImportDbContext : DataImportDbContext
    {
        private readonly int ConmmandTimeoutInSeconds = 600;
        private readonly IConfiguration Configuration;
        private readonly IDatabaseConnectionStringProvider DatabaseConnectionStringProvider;
        private readonly IInstancePostMigrationProcessingProvider InstancePostMigrationProcessingProvider;
        private string InstanceConnectionString;

        public InstanceSqlDataImportDbContext(ILogger logger, DbContextOptions<InstanceSqlDataImportDbContext> dbOptions, IOptions<ConnectionStrings> options = null)
            : base(logger, dbOptions, options)
        {
            DatabaseVersionSql = "SELECT @@VERSION as VersionString";
        }

        public InstanceSqlDataImportDbContext(
            ILogger logger,
            DbContextOptions<InstanceSqlDataImportDbContext> dbOptions,
            IOptions<ConnectionStrings> options,
            IConfiguration configuration,
            IDatabaseConnectionStringProvider databaseConnectionStringProvider,
            IInstancePostMigrationProcessingProvider instancePostMigrationProcessingProvider
        ) : base(logger, dbOptions, options)
        {
            Configuration = configuration;
            DatabaseConnectionStringProvider = databaseConnectionStringProvider;
            InstancePostMigrationProcessingProvider = instancePostMigrationProcessingProvider;
            DatabaseVersionSql = "SELECT @@VERSION as VersionString";
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (!options.IsConfigured)
            {
                if (DatabaseConnectionStringProvider is not null)
                {
                    InstanceConnectionString = DatabaseConnectionStringProvider.GetConnectionString();
                    options.UseSqlServer(InstanceConnectionString,
                        opts => opts.CommandTimeout(ConmmandTimeoutInSeconds));
                }
                else
                {
                    if (ConnectionString == default)
                        throw new ConfigurationErrorsException($"{nameof(InstanceSqlDataImportDbContext)} was not configured and a default connection string was not provided via {nameof(IOptions<ConnectionStrings>)}.");

                    options.UseSqlServer(ConnectionString);
                }
            }
        }
        public void Migration()
        {
            if (InstanceConnectionString is null) return;
            if (InstanceConnectionString.CheckDbExists()) return;

            base.Database.Migrate();
            InstancePostMigrationProcessingProvider.PostMigrationProcessing(this, InstanceConnectionString);
        }
    }
}
