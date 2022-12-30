using System;
using Microsoft.Data.SqlClient;

namespace DataImport.Web.Areas.Instance.Models;

public static class DbExtensions
{
    public const string DbNameMaster = "master";

    public static object ExecuteCommandOnMaster(string command, bool isQuery, string masterConnectionString)
    {
        var sqlConnBuilder = new SqlConnectionStringBuilder(masterConnectionString);
        //sqlConnBuilder.ConnectTimeout = connectionTimeout;
        var sqlConn = new SqlConnection(masterConnectionString);
        sqlConn.Open();
        var sqlCommand = new SqlCommand(command, sqlConn);
        //sqlCommand.CommandTimeout = connectionTimeout;
        object result;
        if (isQuery)
            result = sqlCommand.ExecuteScalar();
        else
            result = sqlCommand.ExecuteNonQuery();

        sqlConn.Close();
        sqlConn.Dispose();

        return result;
    }

    public static bool CheckDbExists(this  string instanceConnectionString)
    {

        //NOTE: Change database to master
        //NOTE: security profile in connection string should have access to master
        var connectionStringBuilder = new SqlConnectionStringBuilder(instanceConnectionString);
        var instanceDbNameDataImport = connectionStringBuilder.InitialCatalog;
        connectionStringBuilder.InitialCatalog = DbExtensions.DbNameMaster;
        var masterConnectionString = connectionStringBuilder.ConnectionString;

        // Checks if database is on elastic pool
        var command = $@"SELECT  Count(*)
                            FROM sys.databases
                            where name = '{instanceDbNameDataImport}';";

        var dbCheck = ExecuteCommandOnMaster(command, true, masterConnectionString);

        var dbExists = Convert.ToInt32(dbCheck) != 0;
        return dbExists;
    }
}
