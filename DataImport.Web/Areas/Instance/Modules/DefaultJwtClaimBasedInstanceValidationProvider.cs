using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace DataImport.Web.Areas.Instance.Modules;

public class DefaultJwtClaimBasedInstanceValidationProvider : IInstanceValidationProvider
{
    private readonly string _jwtInstanceIdKey;

    public DefaultJwtClaimBasedInstanceValidationProvider(IConfiguration configuration)
    {
        _jwtInstanceIdKey = configuration["Instance:JwtInstanceIdKey"];
    }

    public async Task<bool> ValidateAsync(HttpContext httpContext)
    {
        var jwtInstanceId = await httpContext.GetJwtClaimBasedInstanceIdAsync(_jwtInstanceIdKey);
        return !string.IsNullOrEmpty(jwtInstanceId);
    }
}
