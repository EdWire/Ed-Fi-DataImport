using System;
using DataImport.Models;
using DataImport.Web.Areas.Instance.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace DataImport.Web.Areas.Instance.Modules
{
    public class DefaultJwtClaimBasedInstanceConnectionStringProvider : IDatabaseConnectionStringProvider
    {
        private readonly string _connectionString;
        private readonly HttpContext _httpContext;
        private readonly string _jwtInstanceIdKey;

        public DefaultJwtClaimBasedInstanceConnectionStringProvider(IConfiguration configuration, IHttpContextAccessor httpContentAccessor = null, IOptions<ConnectionStrings> options = null)
        {
            _connectionString = options?.Value?.DefaultConnection;
            _httpContext = httpContentAccessor?.HttpContext;
            _jwtInstanceIdKey = configuration["Instance:JwtInstanceIdKey"];
        }

        public string GetConnectionString()
        {
            if (_connectionString == default)
                throw new ConfigurationErrorsException($"{nameof(InstanceSqlDataImportDbContext)} was not configured and a default connection string was not provided via {nameof(IOptions<ConnectionStrings>)}.");
            if (_httpContext == default)
                throw new ConfigurationErrorsException($"{nameof(InstanceSqlDataImportDbContext)} was not configured and an http context was not provided via {nameof(IHttpContextAccessor)}.");

            var instanceId = _httpContext.GetJwtClaimBasedInstanceIdAsync(_jwtInstanceIdKey).Result;
            var instanceIdReplacementToken = GetReplacementToken(instanceId);

            var connectionStringBuilder = new SqlConnectionStringBuilder(_connectionString);

            // Override the Database Name, format if string coming in has a format replacement token,
            // otherwise use database name set in the Initial Catalog.
            connectionStringBuilder.InitialCatalog = IsFormatString(connectionStringBuilder.InitialCatalog)
                ? string.Format(connectionStringBuilder.InitialCatalog, instanceIdReplacementToken)
                : connectionStringBuilder.InitialCatalog;

            return connectionStringBuilder.ConnectionString;

            string GetReplacementToken(string instanceId)
            {
                //Convention: "DataImport" + instance id.
                if (string.IsNullOrEmpty(instanceId)) throw new InvalidOperationException("The instance-year-specific DataImport database name replacement token cannot be derived because the instance id was not set in the current context.");

                return $"DataImport_{instanceId}";
            }
        }

        public bool IsFormatString(string text) => text != null && text.Contains("{0}");
    }
}
