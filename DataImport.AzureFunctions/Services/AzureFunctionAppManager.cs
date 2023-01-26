using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RestSharp;
using System.Text.Json;

namespace DataImport.AzureFunctions.Services;

public class AzureFunctionAppManager : BackgroundService
{
    private readonly ILogger<AzureFunctionAppManager> _logger;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;

    private readonly bool Enabled = false;
    private readonly string AzureFunctionAppMasterKey = "MASTER_KEY";
    private readonly int AzureFunctionAppTimerTicksCycleInSeconds = 60;
    private readonly int AzureFunctionAppIdleTicksCyclesMax = 2;
    private readonly string? WebsiteHostNameEnvForAzureFunctionApp;

    private int AzureFuncionAppIdleTicksCyclesCount = 0;
    private bool AzureFunctionAppIsIdle = false;
    private bool AzureFunctionAppIsInDrainMode = false;

    // NOTE: TotalTime after which AzureFunc App Host Drain will start is
    // NOTE: AzureFunctionAppTimerTicksCycleInSeconds * AzureFunctionAppIdleTicksCyclesMax = 30 * 2 = 60 seconds

    public AzureFunctionAppManager(IConfiguration configuration, IHostApplicationLifetime hostApplicationLifetime, ILogger<AzureFunctionAppManager> logger)
    {
        _hostApplicationLifetime = hostApplicationLifetime;
        _logger = logger;

        Enabled = !string.IsNullOrEmpty(configuration.GetValue<string>("AzureFunctionAppManager:Enabled"));
        AzureFunctionAppMasterKey = configuration.GetValue<string>("AzureFunctionAppManager:AzureFunctionAppMasterKey") ?? "MASTER_KEY";
        AzureFunctionAppTimerTicksCycleInSeconds = configuration.GetValue<int>("AzureFunctionAppManager:AzureFunctionAppTimerTicksCycleInSeconds");
        AzureFunctionAppIdleTicksCyclesMax = configuration.GetValue<int>("AzureFunctionAppManager:AzureFunctionAppIdleTicksCyclesMax");

        WebsiteHostNameEnvForAzureFunctionApp = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");
        if (string.IsNullOrWhiteSpace(WebsiteHostNameEnvForAzureFunctionApp)) throw new InvalidOperationException($"Environment variable WEBSITE_HOSTNAME not defined.");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!Enabled)
        {
            _logger.LogInformation($"\'Azure Function App\' AzureFunctionAppManager is disabled.");
            return;
        }

        var timer = new PeriodicTimer(TimeSpan.FromSeconds(AzureFunctionAppTimerTicksCycleInSeconds));

        var client = new RestClient($"http://{WebsiteHostNameEnvForAzureFunctionApp}");
        var requestDrainStatusGet = new RestRequest($"/admin/host/drain/status?code={AzureFunctionAppMasterKey}");
        var requestDrainStartPost = new RestRequest($"/admin/host/drain?code={AzureFunctionAppMasterKey}");
        var requestHostStateOffline = new RestRequest($"/admin/host/state?code={AzureFunctionAppMasterKey}")
                                            .AddBody(ScriptHostState.Offline);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            RestResponse? responseDrainStatusGet = await client.GetAsync(requestDrainStatusGet);
            if (responseDrainStatusGet is null || (!responseDrainStatusGet.IsSuccessful))
                throw new InvalidOperationException($"variable {nameof(responseDrainStatusGet)} is null.");
#pragma warning disable CS8604 // Possible null reference argument.
            DrainModeStatus? drainModeStatus = JsonSerializer.Deserialize<DrainModeStatus>(responseDrainStatusGet.Content);
#pragma warning restore CS8604 // Possible null reference argument.
            if (drainModeStatus is null) throw new InvalidOperationException($"variable {nameof(drainModeStatus)} is null.");

            _logger.LogDebug($"Local info: {nameof(AzureFunctionAppIsIdle)}:{AzureFunctionAppIsIdle} and {nameof(AzureFuncionAppIdleTicksCyclesCount)}: {AzureFuncionAppIdleTicksCyclesCount} and {nameof(drainModeStatus.State)}: {drainModeStatus.State} and {nameof(drainModeStatus.OutstandingInvocations)}: {drainModeStatus.OutstandingInvocations} and {nameof(drainModeStatus.OutstandingRetries)}: {drainModeStatus.OutstandingRetries}");

            if (AzureFunctionAppIsInDrainMode == true &&
                    (
                        (drainModeStatus.State.Equals(DrainModeState.Completed))
                        || (drainModeStatus.OutstandingInvocations == 0)
                        || (drainModeStatus.OutstandingRetries == 0)
                    )
                )
            {
                _logger.LogInformation($"Conditions met for \'Azure Function App\' shutdown.");

                _logger.LogInformation($"Trying to put \'Azure Function App\' {nameof(ScriptHostState)}:{ScriptHostState.Offline}.");
                try
                {
                    var responsePost = await client.PutAsync(requestHostStateOffline);

                    _logger.LogInformation($"\'Azure Function App\' in {nameof(ScriptHostState)}:{ScriptHostState.Offline} successfully");
                }
                catch (Exception e)
                {
                    _logger.LogDebug($"{e}");
                }

                // Exit The Process, thus completing K8 Job
                //Environment.ExitCode = (int) 0;
                //_hostApplicationLifetime.StopApplication();
                //Environment.Exit((int) 0);
                break;
            }

            if (AzureFunctionAppIsInDrainMode == true &&
                    (
                        (!drainModeStatus.State.Equals(DrainModeState.Completed))
                        || (drainModeStatus.OutstandingInvocations != 0)
                        || (drainModeStatus.OutstandingRetries != 0)
                    )
                )
            {
                _logger.LogDebug($"Conditions not met for \'Azure Function App\' shutdown.");
                continue;
            }

            // Check if AzFuncHost is not idle any more
            if (AzureFunctionAppIsIdle == true &&
                    (
                        (!drainModeStatus.State.Equals(DrainModeState.Disabled))
                        || (drainModeStatus.OutstandingInvocations != 0)
                        || (drainModeStatus.OutstandingRetries != 0)
                    )
                )
            {
                AzureFunctionAppIsIdle = false;
                _logger.LogInformation($"Conditions not met, {nameof(AzureFunctionAppIsIdle)} set to :{AzureFunctionAppIsIdle}");
                continue;
            }

            // Check if Drain Can be started
            if (AzureFunctionAppIsIdle == true && drainModeStatus.State == DrainModeState.Disabled && drainModeStatus.OutstandingInvocations <= 0 && drainModeStatus.OutstandingRetries <= 0)
            {
                if (AzureFuncionAppIdleTicksCyclesCount == AzureFunctionAppIdleTicksCyclesMax)
                {
                    _logger.LogInformation($"Conditions met. Trying to put \'Azure Function App\' in DrainMode.");
                    // Try to put Azure Function App to Drain Mode
                    var responsePost = await client.PostAsync(requestDrainStartPost);
                    if (responsePost.IsSuccessful)
                    {
                        AzureFunctionAppIsInDrainMode = true;
                        _logger.LogInformation($"\'Azure Function App\' in DrainMode successfully");
                        continue;
                    }
                    else
                    {
                        //TODO: handle null in a non-exception way for (drainModeStatus)
                        _logger.LogError($"\'Azure Function App\' in DrainMode failure");
                        continue;
                    }
                }
                else
                {
                    _logger.LogDebug($"Conditions not met. Only increment for counter {nameof(AzureFuncionAppIdleTicksCyclesCount)}");
                    AzureFuncionAppIdleTicksCyclesCount += 1;
                    continue;
                }
            }

            if (AzureFunctionAppIsIdle == false && drainModeStatus.State == DrainModeState.Disabled && drainModeStatus.OutstandingInvocations <= 0 && drainModeStatus.OutstandingRetries <= 0)
            {
                AzureFunctionAppIsIdle = true;
                _logger.LogInformation($"{nameof(AzureFunctionAppIsIdle)} set to :{AzureFunctionAppIsIdle}");
                continue;
            }

            _logger.LogWarning($"{nameof(AzureFunctionAppManager)} end of cycle. Should not reach here.");
        }
    }
}
