using System.Threading.Tasks;
using DataImport.Web.Areas.Instance.Entities;
using Microsoft.AspNetCore.Http;

namespace DataImport.Web.Areas.Instance.Modules;

public interface IInstanceDropdownValueProvider
{
    Task<InstancesDropdown> GetDropdown(HttpContext httpContext);
}
