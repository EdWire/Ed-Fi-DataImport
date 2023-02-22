using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DataImport.Web.Areas.EdGraph.Entities;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Newtonsoft.Json;
using RestSharp;

namespace DataImport.Web.Areas.EdGraph;

public static class Extensions
{
    //public static readonly List<string> AuthSchemes = new() { "Cookies", "oidc" };
    public static readonly string OidcAuthenticationScheme = "oidc";
    private const string AccessTokenName = "access_token";

    public static async Task<UserProfile> GetEdGraphUserProfileAsync(this HttpContext httpContext, string userProfileUri)
    {
        var token = await httpContext.GetTokenAsync(AccessTokenName);
        if (string.IsNullOrEmpty(token))
            throw new Exception($"Please verify that the login provider has been correctly configured. {AccessTokenName} was not provided via {nameof(IHttpContextAccessor)}.");

        var client = new RestClient(userProfileUri);
        var request = new RestRequest(Method.GET);
        request.AddHeader("Accept", "application/json");
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Authorization", $"Bearer {token}");
        IRestResponse response = await client.ExecuteAsync(request);
        if (!response.IsSuccessful)
            throw new Exception($"Please verify that the login provider has been correctly configured. StatusCode: {response.StatusCode} Content: {response.Content}");

        var userProfile = JsonConvert.DeserializeObject<UserProfile>(response.Content);
        if (userProfile is null)
            throw new Exception($"Please verify that the login provider has been correctly configured. User profile is not found.");

        return userProfile;
    }

    public static async Task ManualLogOut(this HttpContext httpContext, ILogger logger)//, string _instanceSwitchRedirectUri)
    {
        //var authProperties = httpContext.Features.Get<IAuthenticateResultFeature>()?.AuthenticateResult?.Properties;
        //if (authProperties is null) throw new Exception($"AuthenticationProperties was not provided via {nameof(HttpContext)}.");

        //authProperties.RedirectUri = _instanceSwitchRedirectUri;

        //httpContext.Session.Clear();
        //foreach (var cookie in httpContext.Request.Cookies.Keys)
        //{
        //    httpContext.Response.Cookies.Delete(cookie);
        //}
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await httpContext.SignOutAsync(OidcAuthenticationScheme);
        //httpContext.Response.Redirect(_instanceSwitchRedirectUri);
        //await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        //await httpContext.SignOutAsync(OidcAuthenticationScheme, authProperties);
        //await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        //httpContext.Session.Clear();


        //TODO: Keep until it feature stabilizes
        //await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        //await httpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme, authProperties);

        //await httpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);

        // inject the HttpContextAccessor to get "context"
        //await httpContext.SignOutAsync("Cookies");
        //var prop = new AuthenticationProperties()
        //{
        //    RedirectUri = _instanceSwitchRedirectUri
        //};
        // after signout this will redirect to your provided target
        //await httpContext.SignOutAsync("oidc");
        //httpContext.Response.Redirect(_instanceSwitchRedirectUri);

        //var allExpectedSchemes = (await _schemeProvider.GetAllSchemesAsync()).Where(sh => Extensions.AuthSchemes.Contains(sh.Name)).ToList();
        //var allNotExpectedSchemes = (await _schemeProvider.GetAllSchemesAsync()).Where(sh => !Extensions.AuthSchemes.Contains(sh.Name)).ToList();
        //foreach (var schemeItem in allExpectedSchemes.OrderBy(x => x.Name))
        //{
        //    //TODO: Change to Debug
        //    await httpContext.SignOutAsync(schemeItem.Name);
        //    _logger.LogInformation($"Expected Auth Scheme: {schemeItem.Name}");
        //}
        //foreach (var schemeItem in allNotExpectedSchemes.OrderBy(x => x.Name))
        //{
        //    _logger.LogWarning($"Not Expected Auth Scheme: {schemeItem.Name}");
        //}
    }

    public static async Task<bool> GetEdGraphUserIdSrvCheckSessionAsync(this HttpContext httpContext, ILogger logger, string userIdSrvCheckSessionUri)
    {
        CookieContainer cookies = new CookieContainer(); //this container saves cookies from responses and send them in requests
        //var handler = new HttpClientHandler
        //{
        //    CookieContainer = cookies
        //};

        foreach (var cookie in httpContext.Request.Cookies)
        {
            cookies.Add(new Cookie(cookie.Key, cookie.Value, "/", httpContext.Request.Host.Host));
        }

        //var clientHandler = new HttpClientHandler { CookieContainer = cookies };
        //using (var httpClient = new HttpClient(clientHandler)) { }


        var token = await httpContext.GetTokenAsync(AccessTokenName);
        if (string.IsNullOrEmpty(token))
            return false;

        var client = new RestClient(userIdSrvCheckSessionUri);

        client.CookieContainer = cookies;

        var request = new RestRequest(Method.GET);
        request.AddHeader("Accept", "application/json");
        request.AddHeader("Content-Type", "application/json");
        //request.AddHeader("Authorization", $"Bearer {token}");

        try
        {
            IRestResponse response = await client.ExecuteAsync(request);

            if (!response.IsSuccessful)
            {
                logger.LogWarning($"Please verify that the login provider has been correctly configured. StatusCode: {response.StatusCode} Content: {response.Content}");
                return false;
            }
            else
            {
                var resultContent = response.Content;
                var sessionValid = JsonConvert.DeserializeObject<bool>(response.Content);
                if (sessionValid) return true;
                else return false;
            }
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static async Task UpdateEdGraphTokensAsync(this HttpContext httpContext, ILogger logger, IHttpClientFactory httpClientFactory, RefreshTokenRequest refreshTokenRequest)
    {
        var authProperties = httpContext.Features.Get<IAuthenticateResultFeature>()?.AuthenticateResult?.Properties;
        if (authProperties is null) throw new Exception($"AuthenticationProperties was not provided via {nameof(HttpContext)}.");

        var tokens = authProperties.GetTokens().ToList();
        if (!tokens.Any()) throw new Exception("No tokens found in cookie properties. SaveTokens must be enabled for automatic token refresh.");

        var refreshToken = tokens.SingleOrDefault(t => t.Name == OpenIdConnectParameterNames.RefreshToken);
        if (refreshToken == null) throw new Exception("Please verify that the login provider has been correctly configured. No refresh token found in cookie properties. A refresh token must be requested and SaveTokens must be enabled.");

        refreshTokenRequest.RefreshToken = refreshToken.Value;
        var tokenClient = httpClientFactory.CreateClient();
        var tokenResponse = await tokenClient.RequestRefreshTokenAsync(refreshTokenRequest);
        if (tokenResponse.IsError) throw new Exception($"Please verify that the login provider has been correctly configured. Error refreshing token: {tokenResponse.Error}");

        authProperties.UpdateTokenValue("access_token", tokenResponse.AccessToken);
        authProperties.UpdateTokenValue("refresh_token", tokenResponse.RefreshToken);
        var newExpiresAt = DateTime.UtcNow + TimeSpan.FromSeconds(tokenResponse.ExpiresIn);
        authProperties.UpdateTokenValue("expires_at", newExpiresAt.ToString("o", CultureInfo.InvariantCulture));

        await httpContext.SignInAsync(httpContext.User, authProperties);
    }
}
