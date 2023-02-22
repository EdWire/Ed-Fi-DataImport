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
    private readonly IAuthenticationSchemeProvider _schemeProvider;
    private readonly IdentitySettings _identitySettings;
    private readonly string _instanceSwitchRedirectUri;

    public OpenIdConnectController(IOptions<IdentitySettings> identitySettings,
        IConfiguration configuration,
        ILogger logger,
        IAuthenticationSchemeProvider schemeProvider)
    {
        _identitySettings = identitySettings.Value;
        _schemeProvider = schemeProvider;
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
        await HttpContext.ManualLogOut(_logger);
        return Ok();
        //var scheme = (await _schemeProvider.GetDefaultChallengeSchemeAsync()).Name;
        //await HttpContext.SignOutAsync(scheme);
        //return Redirect(_instanceSwitchRedirectUri);
    }
}
