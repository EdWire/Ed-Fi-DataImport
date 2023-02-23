// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Threading.Tasks;
using DataImport.Web.Areas.EdGraph;
using DataImport.Web.Helpers;
using DataImport.Web.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DataImport.Web.Features.OpenIdConnect;

public class OpenIdConnectController : BaseController
{
    private readonly ILogger _logger;
    private readonly IdentitySettings _identitySettings;
    private readonly string _instanceSwitchRedirectUri;

    public OpenIdConnectController(IOptions<IdentitySettings> identitySettings,
        IConfiguration configuration,
        ILogger logger,
        IAuthenticationSchemeProvider schemeProvider)
    {
        _identitySettings = identitySettings.Value;
        _logger = logger;
        _instanceSwitchRedirectUri = configuration["EdGraph:Instance:InstanceSwitchRedirectUri"];
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string returnUrl)
    {
        returnUrl ??= Url.Content("~/");

        try
        {
            if (HttpContext.User.Identity != null && !HttpContext.User.Identity.IsAuthenticated)
            {
                return new ChallengeResult(
                    _identitySettings.OpenIdSettings.AuthenticationScheme, new AuthenticationProperties
                    {
                        RedirectUri = returnUrl
                    });
            }
        }
        catch (Exception exception)
        {
            throw new Exception(
                $"Please verify that the login provider has been correctly configured. System Error: {exception.Message}");
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Manage()
    {
        if (string.IsNullOrWhiteSpace(_identitySettings.OpenIdSettings.UserProfileUri))
            throw new Exception("User self-management URL is not configured. Please contact your administrator.");

        return Redirect(_identitySettings.OpenIdSettings.UserProfileUri);
    }

    [HttpPost]
    [AllowAnonymous]
    public Task<IActionResult> LogOut()
    {
        return Task.FromResult<IActionResult>(new SignOutResult(new[]
            {CookieAuthenticationDefaults.AuthenticationScheme, _identitySettings.OpenIdSettings.AuthenticationScheme}));
    }

    [HttpGet]
    public async Task<IActionResult> SwitchTenantFailed()
    {
        var idSrvMasterSessionCheckIsPresent = HttpContext.Request.Cookies.ContainsKey(Extensions.IdSrvMasterSessionCheck);
        if (idSrvMasterSessionCheckIsPresent)
        {
            var co = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddMinutes(-1),
                MaxAge = TimeSpan.FromMinutes(-1),
                IsEssential = true
            };

            HttpContext.Response.Cookies.Delete(Extensions.IdSrvMasterSessionCheck, co);
        }

        await HttpContext.ManualLogOut(_logger);
        return Redirect(_instanceSwitchRedirectUri);
        //var scheme = (await _schemeProvider.GetDefaultChallengeSchemeAsync()).Name;
        //await HttpContext.SignOutAsync(scheme);
        //return Redirect(_instanceSwitchRedirectUri);
    }
}
