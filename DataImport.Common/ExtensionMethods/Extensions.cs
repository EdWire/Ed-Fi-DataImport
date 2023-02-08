using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace DataImport.Common.ExtensionMethods;

public static class Extensions
{
    private const string AccessTokenName = "access_token";

    public static async Task<string> GetJwtClaimBasedInstanceIdAsync(this HttpContext httpContext, string jwtInstanceIdKey)
    {
        var token = await httpContext.GetTokenAsync(AccessTokenName);
        if (string.IsNullOrEmpty(token))
            throw new Exception($"{AccessTokenName} was not provided via {nameof(IHttpContextAccessor)}.");

        var jsonToken = new JwtSecurityTokenHandler().ReadToken(token);
        var tokenS = jsonToken as JwtSecurityToken;

        var instanceId = tokenS.Claims
            .FirstOrDefault(x => x.Type == jwtInstanceIdKey)?
            .Value;

        if (string.IsNullOrEmpty(instanceId))
            throw new Exception($"InstanceId was not provided via {nameof(IHttpContextAccessor)}.");

        return instanceId;
    }
}
