using Microsoft.Extensions.Hosting;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using DataImport.AzureFunctions.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((ctx, serviceProvider) =>
    {
        serviceProvider.AddApplicationInsightsTelemetryWorkerService();
        serviceProvider.ConfigureFunctionsApplicationInsights();

        serviceProvider.Configure<LoggerFilterOptions>(options =>
        {
            var toRemove = options.Rules.FirstOrDefault(rule => rule.ProviderName
                == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");

            if (toRemove is not null)
            {
                options.Rules.Remove(toRemove);
            }
        });

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
