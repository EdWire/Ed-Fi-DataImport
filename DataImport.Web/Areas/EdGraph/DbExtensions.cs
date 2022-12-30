using System;
using DataImport.Models;
using Microsoft.EntityFrameworkCore;

namespace DataImport.Web.Areas.EdGraph;

public static class DbExtensions
{
    public const string DbServerEdition = "Azure";

    public static void MoveDbToAzureElasticPool(this DataImportDbContext dbContext, string masterConnectionString, string instanceDatabaseName, string elasticPoolName)
    {
        // Checks if database is on elastic pool
        var command = $@"SELECT  Count(*)    
                            FROM sys.databases d   
                            JOIN sys.database_service_objectives slo    
                            ON d.database_id = slo.database_id
                            where service_objective = 'ElasticPool'
                            and name = '{instanceDatabaseName}';";

        var elasticPoolCheck = Instance.Models.DbExtensions.ExecuteCommandOnMaster(command, true, masterConnectionString);

        if (Convert.ToInt32(elasticPoolCheck) == 0)
        {
            var dbMoveCommand = $"ALTER DATABASE [{instanceDatabaseName}] MODIFY ( SERVICE_OBJECTIVE = ELASTIC_POOL (name = [{elasticPoolName}] ));";

            var dbMoveResult = dbContext.Database.ExecuteSqlRaw(dbMoveCommand);
            //TODO: test what is correct result
        }
    }
}
