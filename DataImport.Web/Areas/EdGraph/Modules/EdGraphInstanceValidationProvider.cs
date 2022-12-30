using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DataImport.Web.Areas.Instance;
using DataImport.Web.Areas.Instance.Modules;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DataImport.Web.Areas.EdGraph.Modules;

public class EdGraphInstanceValidationProvider : IInstanceValidationProvider
{
    private readonly ILogger<EdGraphInstanceValidationProvider> _logger;
    private readonly IOptionsMonitor<OpenIdConnectOptions> _oidcOptions;
    private readonly IAuthenticationSchemeProvider _schemeProvider;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _jwtInstanceIdKey;
    private readonly string _userProfileUri;
    private readonly string _userProfileInstanceIdKey;
    private readonly string _instanceSwitchRedirectUri;

    public EdGraphInstanceValidationProvider(IConfiguration configuration,
        ILogger<EdGraphInstanceValidationProvider> logger,
        IOptionsMonitor<OpenIdConnectOptions> oidcOptions,
        IAuthenticationSchemeProvider schemeProvider,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _oidcOptions = oidcOptions;
        _schemeProvider = schemeProvider;
        _httpClientFactory = httpClientFactory;
        _jwtInstanceIdKey = configuration["Instance:JwtInstanceIdKey"];
        _userProfileUri = configuration["EdGraph:Instance:UserProfileUri"];
        _userProfileInstanceIdKey = configuration["EdGraph:Instance:UserProfileInstanceIdKey"];
        _instanceSwitchRedirectUri = configuration["EdGraph:Instance:InstanceSwitchRedirectUri"];
    }

    public async Task<bool> ValidateAsync(HttpContext httpContext)
    {
        var jwtInstanceId = await httpContext.GetJwtClaimBasedInstanceIdAsync(_jwtInstanceIdKey);
        var userProfile = await httpContext.GetEdGraphUserProfileAsync(_userProfileUri);

        var usrPrfInstanceId = userProfile.Preferences.SingleOrDefault(x => x.Code == _userProfileInstanceIdKey);
        if (usrPrfInstanceId is null) throw new Exception($"User profile InstanceId not found");


        if (jwtInstanceId != usrPrfInstanceId.Value)
        {
            try
            {
                var oidcOptions = _oidcOptions.Get((await _schemeProvider.GetDefaultChallengeSchemeAsync()).Name);
                var configuration = await oidcOptions.ConfigurationManager.GetConfigurationAsync(default(CancellationToken));
                var refreshTokenRequest = new RefreshTokenRequest { Address = configuration.TokenEndpoint, ClientId = oidcOptions.ClientId, ClientSecret = oidcOptions.ClientSecret };
                await httpContext.UpdateEdGraphTokensAsync(_logger, _httpClientFactory, refreshTokenRequest);
                httpContext.Response.Redirect(_instanceSwitchRedirectUri);  //PATCH: to make the app reload after correction
            }
            catch (Exception e)
            {
                throw new Exception($"Manual user access token refresh failed.{e}.");
            }
        }

        return true;
    }
}
