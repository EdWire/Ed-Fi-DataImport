using DataImport.Models;
using DataImport.Web.Areas.Instance.Modules;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DataImport.Web.Areas.EdGraph.Modules;

public class EdGraphInstancePostMigrationProcessingProvider : IInstancePostMigrationProcessingProvider
{
    private readonly string _elasticPoolName;

    public EdGraphInstancePostMigrationProcessingProvider(IConfiguration configuration)
    {
        _elasticPoolName = configuration["EdGraph:AzureSQL:ElasticPoolName"];
    }

    public void PostMigrationProcessing(DataImportDbContext dbContext, string instanceConnectionString)
    {
        //NOTE: Change database to master
        //NOTE: security profile in connection string should have access to master
        var connectionStringBuilder = new SqlConnectionStringBuilder(instanceConnectionString);
        var instanceDbNameDataImport = connectionStringBuilder.InitialCatalog;
        connectionStringBuilder.InitialCatalog = Instance.Models.DbExtensions.DbNameMaster;
        var masterConnectionString = connectionStringBuilder.ConnectionString;

        //NOTE: determine if the server is azure
        var dbVersion = dbContext.DatabaseVersion.FirstAsync().Result.VersionString;

        if (!dbVersion.Contains(DbExtensions.DbServerEdition)) return;

        dbContext.MoveDbToAzureElasticPool(masterConnectionString, instanceDbNameDataImport, _elasticPoolName);
    }
}
