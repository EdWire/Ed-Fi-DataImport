using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DataImport.Web.Areas.EdGraph.Entities;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Newtonsoft.Json;
using RestSharp;

namespace DataImport.Web.Areas.EdGraph;

public static class Extensions
{
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

    public static async Task UpdateEdGraphTokensAsync(this HttpContext httpContext, ILogger logger, IHttpClientFactory httpClientFactory, RefreshTokenRequest refreshTokenRequest)
    {
        var authProperties = httpContext.Features.Get<IAuthenticateResultFeature>()?.AuthenticateResult?.Properties;
        if (authProperties is null) throw new Exception($"AuthenticationProperties was not provided via {nameof(IHttpContextAccessor)}.");

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
