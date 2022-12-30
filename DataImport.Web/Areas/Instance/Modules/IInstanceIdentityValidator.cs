using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace DataImport.Web.Areas.Instance.Modules;

public interface IInstanceValidationProvider
{
    Task<bool> ValidateAsync(HttpContext httpContext);
}
