using System;
using DataImport.Models.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DataImport.Web.Areas.Instance.Models.Design
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/ef/core/cli/dbcontext-creation?tabs=dotnet-core-cli#from-a-design-time-factory
    /// Add-Migration InitialMigration -Args '"Data Source=(local);Initial Catalog=EdFi_DataImport;Trusted_Connection=True" "SqlServer"'
    /// </summary>

    public class DesignTimeInstanceSqlDataImportDbContextFactory : DesignTimeDataImportDbContextFactoryBase, IDesignTimeDbContextFactory<InstanceSqlDataImportDbContext>
    {
        public InstanceSqlDataImportDbContext CreateDbContext(string[] args)
        {
            Validate(args);

            var dbType = DatabaseType(args);

            if (dbType.StartsWith("SqlServer", StringComparison.InvariantCultureIgnoreCase))
            {
                var optionsBuilder = new DbContextOptionsBuilder<InstanceSqlDataImportDbContext>();
                optionsBuilder.UseSqlServer(args[0]);
                return new InstanceSqlDataImportDbContext(Logger(), optionsBuilder.Options);
            }
            else
            {
                throw new Exception($"Unsupported provider: {dbType}.");
            }
        }
    }
}
