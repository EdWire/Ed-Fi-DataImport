using Microsoft.Data.SqlClient;
using System.Data;

namespace DataImport.AzureFunctions.Manager.Extensions
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


        public static object ExecuteCommandOnMaster(string command, string masterConnectionString, string executeType)
        {
            var sqlConnBuilder = new SqlConnectionStringBuilder(masterConnectionString);
            //sqlConnBuilder.ConnectTimeout = connectionTimeout;
            var sqlConn = new SqlConnection(masterConnectionString);
            sqlConn.Open();
            var sqlCommand = new SqlCommand(command, sqlConn);
            //sqlCommand.CommandTimeout = connectionTimeout;
            object result;

            if (executeType.Equals(ExecuteTypeEnum.Scalar))
            {
                result = sqlCommand.ExecuteScalar();
            }
            else if (executeType.Equals(ExecuteTypeEnum.Reader))
            {
                var dt = new DataTable();
                var reader = sqlCommand.ExecuteReader();

                dt.Load(reader);
                result = dt;
            }
            else
                result = sqlCommand.ExecuteNonQuery();

            sqlConn.Close();
            sqlConn.Dispose();

            return result;
        }


        public static List<object> ExecuteReaderCommandOnMaster(string commandStr, string masterConnectionString)
        {
            var columnData = new List<object>();
            var sqlConn = new SqlConnection(masterConnectionString);
            try
            {
                sqlConn.Open();
                using SqlCommand command = new SqlCommand(commandStr, sqlConn);
                using SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    columnData.Add(reader.GetString(0));
                }
            }
            finally
            {
                sqlConn.Close();
                sqlConn.Dispose();
            }

            return columnData;
        }

        //SELECT @@VERSION as VersionString

        public static List<string> ScanDataImportDatabases()
        {
            var databaseEngine = Environment.GetEnvironmentVariable("AppSettings__DatabaseEngine");
            if (!DatabaseEngineEnum.Parse(databaseEngine).Equals(DatabaseEngineEnum.SqlServer))
                throw new NotImplementedException();

            var defaultConnectionSqlServer = Environment.GetEnvironmentVariable("ConnectionStrings__defaultConnection");
            var EdGraphAzureSqlElasticPoolName = Environment.GetEnvironmentVariable("EdGraph__AzureSQL__ElasticPoolName");

            var connectionStringBuilder = new SqlConnectionStringBuilder(defaultConnectionSqlServer);
            var instanceDbNameDataImport = connectionStringBuilder.InitialCatalog;
            connectionStringBuilder.InitialCatalog = DbExtensions.DbNameMaster;
            var masterConnectionString = connectionStringBuilder.ConnectionString;

            //var commandDbServerVersion = "SELECT @@VERSION";
            //var dbServerVersion = ExecuteCommandOnMaster(commandDbServerVersion, masterConnectionString, ExecuteTypeEnum.Scalar);
            //Azure db server query
            //if (dbServerVersion.Contains(DbExtensions.DbServerEdition))

            var commandScanDataImportDatabases = $@"SELECT  name
                                                    FROM sys.databases
                                                    WHERE name LIKE '{DataImportDbNamePrefix}%';";

            var dataImportDatabases = ExecuteReaderCommandOnMaster(commandScanDataImportDatabases, masterConnectionString)
                                                .Where(y => y is not null)
                                                .Select(x => (string) x)
                                                .ToList();

            return dataImportDatabases;
        }


        public static bool ScanDataImportPendingFiles(string dataImportDbName)
        {
            var databaseEngine = Environment.GetEnvironmentVariable("AppSettings__DatabaseEngine");
            if (!DatabaseEngineEnum.Parse(databaseEngine).Equals(DatabaseEngineEnum.SqlServer))
                throw new NotImplementedException();

            var defaultConnectionSqlServer = Environment.GetEnvironmentVariable("ConnectionStrings__defaultConnection");
            var EdGraphAzureSqlElasticPoolName = Environment.GetEnvironmentVariable("EdGraph__AzureSQL__ElasticPoolName");

            var connectionStringBuilder = new SqlConnectionStringBuilder(defaultConnectionSqlServer);
            connectionStringBuilder.InitialCatalog = dataImportDbName;
            var masterConnectionString = connectionStringBuilder.ConnectionString;

            var commandAgentFilesPending =
                $@"SELECT Count(*)
                        FROM [Agents] AS [agent]
                        WHERE
                        (([agent].[Enabled] = CAST(1 AS bit))
                        AND ([agent].[Archived] = CAST(0 AS bit)))
                        AND EXISTS (    
                                SELECT 1
                                FROM [Files] AS [files]
                                WHERE ([agent].[Id] = [files].[AgentId]) AND [files].[Status] IN (7, 8))";

            var filePending = ExecuteCommandOnMaster(commandAgentFilesPending, masterConnectionString, ExecuteTypeEnum.Scalar);

            var filePendingExists = Convert.ToInt32(filePending) != 0;
            return filePendingExists;
        }

        public static bool ScanDataImportPendingAgentSchedules(string dataImportDbName)
        {
            var databaseEngine = Environment.GetEnvironmentVariable("AppSettings__DatabaseEngine");
            if (!DatabaseEngineEnum.Parse(databaseEngine).Equals(DatabaseEngineEnum.SqlServer))
                throw new NotImplementedException();

            var defaultConnectionSqlServer = Environment.GetEnvironmentVariable("ConnectionStrings__defaultConnection");
            var EdGraphAzureSqlElasticPoolName = Environment.GetEnvironmentVariable("EdGraph__AzureSQL__ElasticPoolName");

            var connectionStringBuilder = new SqlConnectionStringBuilder(defaultConnectionSqlServer);
            connectionStringBuilder.InitialCatalog = dataImportDbName;
            var masterConnectionString = connectionStringBuilder.ConnectionString;

            var commandAgentSchedulesPending =
                $@"	SELECT 
	                  [a0].[Day],
	                  [a0].[Hour],
	                  [a0].[Minute],
	                  [a].[LastExecuted]
	                FROM 
	                  [Agents] AS [a] 
	                  LEFT JOIN [AgentSchedules] AS [a0] ON [a].[Id] = [a0].[AgentId] 
	                WHERE 
	                  (
		                [a].[Enabled] = CAST(1 AS bit) 
		                AND [a].[Archived] = CAST(0 AS bit)
	                  ) 
	                  AND [a].[AgentTypeCode] IN (N'SFTP', N'FTPS') 
	                ORDER BY 
	                  CASE WHEN [a].[RunOrder] IS NULL THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END, 
	                  [a].[RunOrder], 
	                  [a].[Id],
	                  [a0].[Day],
	                  [a0].[Hour],
	                  [a0].[Minute]";

            var agentSchedules = (DataTable) ExecuteCommandOnMaster(commandAgentSchedulesPending, masterConnectionString, ExecuteTypeEnum.Reader);

            var shouldRun = false;

            DateTimeOffset? nowDate = DateTimeOffset.Now;
            var nowDay = (int) nowDate.Value.DayOfWeek;
            var nowHour = nowDate.Value.Hour;
            var nowMinute = nowDate.Value.Minute;

            if (agentSchedules.Rows.Count > 0)
            {
                //while (agentSchedules.Read())
                foreach (DataRow row in agentSchedules.Rows)
                {
                    //TODO: Properly get column numbers
                    var scheduleDay = row.Field<int>(0);
                    var scheduleHour = row.Field<int>(1);
                    var scheduleMinute = row.Field<int>(2);

                    DateTimeOffset? agentLastExecuted = null;
                    if(!row.IsNull(3))
                        agentLastExecuted = row.Field<DateTimeOffset>(3);

                    var scheduleDateTime = DateTime.Parse(nowDate.Value.Date.ToShortDateString() + " " + scheduleHour + ":" + scheduleMinute);
                    scheduleDateTime = scheduleDateTime.AddDays(-((int) nowDate.Value.DayOfWeek - scheduleDay));

                    if (!agentLastExecuted.HasValue || scheduleDateTime > agentLastExecuted)
                    {
                        if (scheduleDay <= nowDay)
                        {
                            if (scheduleHour < nowHour)
                                shouldRun = true;
                            else if (scheduleHour == nowHour && scheduleMinute <= nowMinute)
                                shouldRun = true;
                        }
                    }
                }
            }

            return shouldRun;
        }

    }
    public static class ExecuteTypeEnum
    {
        public const string Scalar = "Scalar";
        public const string NonQuery = "NonQuery";
        public const string Reader = "Reader";
    }

    public static class DatabaseEngineEnum
    {
        public const string SqlServer = "SqlServer";
        public const string PostgreSql = "PostgreSql";

        public static string Parse(string value)
        {
            if (value.Equals(SqlServer, StringComparison.InvariantCultureIgnoreCase))
            {
                return SqlServer;
            }

            if (value.Equals(PostgreSql, StringComparison.InvariantCultureIgnoreCase))
            {
                return PostgreSql;
            }

            throw new NotSupportedException("Not supported DatabaseEngine \"" + value + "\". Supported engines: SqlServer, and PostgreSql.");
        }
    }
}

