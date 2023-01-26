using Microsoft.Data.SqlClient;

namespace DataImport.AzureFunctions.Extensions
{
    public static class DbExtensions
    {
        public const string DbServerEdition = "Azure";
        public const string DbNameMaster = "master";
        public const string DataImportDbNamePrefix = "EdFi_DataImport_";

        public static string? SubstituteDataImportInstance(string dataImportTransformLoadInstanceName)
        {
            var defaultConnectionSqlServer = Environment.GetEnvironmentVariable("ConnectionStrings__defaultConnection");
            var connectionStringBuilder = new SqlConnectionStringBuilder(defaultConnectionSqlServer);
            connectionStringBuilder.InitialCatalog = dataImportTransformLoadInstanceName;
            var instanceConnectionString = connectionStringBuilder.ConnectionString;

            return instanceConnectionString; 
        }
    }
}

