using Microsoft.Extensions.Hosting;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using DataImport.AzureFunctions.Manager.Services;
using System.Net;
using DurableFunctionsMonitor.DotNetBackend;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(builder =>
    {
        // Is added by package "Microsoft.Azure.Functions.Worker.ApplicationInsights".
        // Documented here because it is still preview: https://github.com/Azure/azure-functions-dotnet-worker/pull/944#issue-1282987627
        builder
            .AddApplicationInsights()
            .AddApplicationInsightsLogger();
    })
    .ConfigureServices((ctx, serviceProvider) =>
    {
        // You will need extra configuration because above will only log per default Warning (default AI configuration) and above because of following line:
        // https://github.com/microsoft/ApplicationInsights-dotnet/blob/main/NETCORE/src/Shared/Extensions/ApplicationInsightsExtensions.cs#L427
        // This is documented here:
        // https://github.com/microsoft/ApplicationInsights-dotnet/issues/2610#issuecomment-1316672650
        // So remove the default logger rule (warning and above). This will result that the default will be Information.
        serviceProvider.Configure<LoggerFilterOptions>(options =>
        {
            var toRemove = options.Rules.FirstOrDefault(rule => rule.ProviderName
                == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");

            if (toRemove is not null)
            {
                options.Rules.Remove(toRemove);
            }
        });

        // Azure Durable Monitor
        //DfmSettings DfmSettings = new DfmSettings() { };
        DfmEndpoint.Setup(new DfmSettings { DisableAuthentication = true });
        //DfmEndpoint.Setup();

        // Setup DI
        serviceProvider.AddSingleton<AzureFunctionAppManager>();
        serviceProvider.AddHostedService(provider => provider.GetRequiredService<AzureFunctionAppManager>());
    })
    .ConfigureAppConfiguration((hostContext, config) =>
    {
        // Add appsettings.json configuration so we can set logging in configuration.
        // Add in example a file called appsettings.json to the root and set the properties to:
        // Build Action: Content
        // Copy to Output Directory: Copy if newer
        //
        // Content:
        // {
        //   "Logging": {
        //     "LogLevel": {
        //       "Default": "Error" // Change this to ie Trace for more logging
        //     }
        //   }
        // }
        config
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .Build();
    })
    .ConfigureLogging((hostingContext, logging) =>
    {
        // Make sure the configuration of the appsettings.json file is picked up.
        logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));

        logging.AddSimpleConsole(o =>
        {
            o.ColorBehavior = LoggerColorBehavior.Enabled;
            o.SingleLine = true;
            o.IncludeScopes = true;
        });
    })
    .Build();

host.Run();

//for logging detailes
// https://github.com/Azure/azure-functions-dotnet-worker/issues/1182
//for host.json logging with AI
//https://learn.microsoft.com/en-us/azure/azure-functions/configure-monitoring?tabs=v2#configure-log-levels