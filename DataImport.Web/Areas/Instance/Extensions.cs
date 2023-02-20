using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace DataImport.Web.Areas.Instance;

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

    public static string GetName(this IIdentity identity)
    {
        ClaimsIdentity claimsIdentity = identity as ClaimsIdentity;
        Claim claimName = claimsIdentity?.FindFirst(ClaimTypes.Name);
        Claim JwtClaimName = claimsIdentity?.FindFirst(JwtClaimTypes.Name);

        return claimName?.Value ?? JwtClaimName?.Value ?? string.Empty;
    }
}
