using System.Collections.Generic;
using System.Threading.Tasks;
using DataImport.Web.Areas.Instance.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DataImport.Web.Areas.Instance.Modules;

public class DefaultJwtClaimBasedInstanceDropdownValueProvider : IInstanceDropdownValueProvider
{
    private readonly string _jwtInstanceIdKey;

    public DefaultJwtClaimBasedInstanceDropdownValueProvider(IConfiguration configuration, ILogger<DefaultJwtClaimBasedInstanceDropdownValueProvider> logger)
    {
        _jwtInstanceIdKey = configuration["Instance:JwtInstanceIdKey"];
    }

    public async Task<InstancesDropdown> GetDropdown(HttpContext httpContext)
    {
        string jwtInstanceId = await httpContext.GetJwtClaimBasedInstanceIdAsync(_jwtInstanceIdKey);
        return new InstancesDropdown { SelectedInstanceName = jwtInstanceId, Instances = new List<string>() { jwtInstanceId } };
    }
}
