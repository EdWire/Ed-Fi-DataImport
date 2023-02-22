using System;
using System.Threading.Tasks;
using DataImport.Models;
using DataImport.Web.Areas.Instance.Models;
using DataImport.Web.Areas.Instance.Modules;
using Microsoft.AspNetCore.Http;

namespace DataImport.Web.Areas.Instance.Middleware
{
    public class InstanceSqlDataImportDbContextMiddleware
    {
        private readonly RequestDelegate _next;

        public InstanceSqlDataImportDbContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            //NOTE: Patch for hc
            if (context.Request.Path.HasValue && (context.Request.Path.Value.Contains("/hc") || context.Request.Path.Value.Contains("/liveness")))
            {
                await _next(context);
                return;
            }

            try
            {
                var instanceValidationProvider = (IInstanceValidationProvider) context.RequestServices.GetService(typeof(IInstanceValidationProvider));
                var validationResult = await instanceValidationProvider.ValidateAsync(context);
                if (validationResult != true)
                    return;
                //if (validationResult != true) throw new Exception($"Please verify that the login provider has been correctly configured. Error validating instance via {nameof(HttpContext)}.");

                var instanceSqlDataImportDbContext = (InstanceSqlDataImportDbContext) context.RequestServices.GetService(typeof(InstanceSqlDataImportDbContext));
                if (instanceSqlDataImportDbContext is null) throw new NotImplementedException($"{nameof(InstanceSqlDataImportDbContext)} was not configured and a default implementation was not provided via {nameof(DataImportDbContext)}.");

                //Note: Manual call to force call for implementation of virtual DbContext.OnConfiguring(DbContextOptionsBuilder options)
                var model = instanceSqlDataImportDbContext.Model;
                instanceSqlDataImportDbContext.Migration();

            }
            catch (Exception e)
            {
                throw new Exception($"Please verify that the login provider has been correctly configured. Error message: {e.Message}.");
            }

            await _next(context);
        }
    }
}
