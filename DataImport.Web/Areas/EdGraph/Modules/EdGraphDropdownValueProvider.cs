using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataImport.Web.Areas.Instance;
using DataImport.Web.Areas.Instance.Entities;
using DataImport.Web.Areas.Instance.Modules;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DataImport.Web.Areas.EdGraph.Modules;

public class EdGraphDropdownValueProvider : IInstanceDropdownValueProvider
{
    private readonly ILogger<EdGraphDropdownValueProvider> _logger;
    private readonly string _jwtInstanceIdKey;
    private readonly string _userProfileUri;
    private readonly string _userProfileInstanceIdKey;
    private readonly string _instanceSwitchUri;
    private readonly string _instanceSwitchRedirectUri;
    private readonly string _instanceSwitchFailureRedirectUri;

    public EdGraphDropdownValueProvider(IConfiguration configuration, ILogger<EdGraphDropdownValueProvider> logger)
    {
        _logger = logger;
        _jwtInstanceIdKey = configuration["Instance:JwtInstanceIdKey"];
        _userProfileUri = configuration["EdGraph:Instance:UserProfileUri"];
        _userProfileInstanceIdKey = configuration["EdGraph:Instance:UserProfileInstanceIdKey"];
        _instanceSwitchUri = configuration["EdGraph:Instance:InstanceSwitchUri"];
        _instanceSwitchRedirectUri = configuration["EdGraph:Instance:InstanceSwitchRedirectUri"];
        _instanceSwitchFailureRedirectUri = configuration["EdGraph:Instance:InstanceSwitchFailureRedirectUri"];
    }

    public async Task<InstancesDropdown> GetDropdown(HttpContext httpContext)
    {
        try
        {
            var userProfile = await httpContext.GetEdGraphUserProfileAsync(_userProfileUri);
            var usrPrfInstanceId = userProfile.Preferences.Single(x => x.Code == _userProfileInstanceIdKey);

            return new InstancesDropdown
            {
                SelectedInstanceName = userProfile.Tenants.Single(tenant => tenant.TenantId == usrPrfInstanceId.Value).OrganizationName,
                Instances = userProfile.Tenants.Select(tenant => $"<a href=\"{_instanceSwitchUri}?tenantId={tenant.TenantId}&redirectUri={_instanceSwitchRedirectUri}&failureRedirectUri={_instanceSwitchFailureRedirectUri}&requestTime={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}\">{tenant.OrganizationName}</a>").ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Please verify that the login provider has been correctly configured. System Error: {ex.Message}");
            //throw new Exception($"Please verify that the login provider has been correctly configured. System Error: {ex.Message}");
            //PATCH: Fallback
            var jwtInstanceId = await httpContext.GetJwtClaimBasedInstanceIdAsync(_jwtInstanceIdKey);
            return new InstancesDropdown { SelectedInstanceName = jwtInstanceId, Instances = new List<string>() { jwtInstanceId } };
        }
    }
}
