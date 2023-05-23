using Azure.Storage.Queues;
using DataImport.AzureFunctions.Manager.Extensions;
using Microsoft.Azure.Functions.Worker;
//using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace DataImport.AzureFunctions.Manager;

public class TransformLoadTimerFunction
{
    private readonly ILogger _logger;
    public TransformLoadTimerFunction(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<TransformLoadTimerFunction>();
    }

    [Function("TransformLoad_TimerFunction")]
    public async Task Run(
        [TimerTrigger("%EdGraphTransformLoadTimerTrigger%", /*RunOnStartup = true,*/ UseMonitor = true)] TimerInfo timerInfo,
        FunctionContext executionContext,
        CancellationToken cancellationToken
        )
    {
        var storageConnectionTransformLoadQueue = Environment.GetEnvironmentVariable("ConnectionStrings__storageConnection");
        var dataImportTransformLoadQueueName = Environment.GetEnvironmentVariable("EdGraph__storageConnection__QueueName"); //"DataImport-TransformLoad-Queue"

        QueueClient queueClient = Extensions.Extensions.GetQueue(storageConnectionTransformLoadQueue, dataImportTransformLoadQueueName);

        //var FUNCTIONS_ENABLE_DRAIN_ON_APP_STOPPING = Environment.GetEnvironmentVariable("FUNCTIONS_ENABLE_DRAIN_ON_APP_STOPPING");

        _logger.LogInformation("Scan DbServer for DataImport instances");
        var dataImportDbs = DbExtensions.ScanDataImportDatabases();
        _logger.LogInformation($"Scan DbServer for DataImport instances found: {string.Join(", ", dataImportDbs)}");

        //_logger.LogInformation($"Delay start");
        //await Task.Delay(6000);
        //_logger.LogInformation($"Delay over");

        foreach (var dbName in dataImportDbs)
        {
            var isPendingAgentSchedules = DbExtensions.ScanDataImportPendingAgentSchedules(dbName);
            var isPendingFiles = DbExtensions.ScanDataImportPendingFiles(dbName);

            _logger.LogInformation($"For Db:{dbName}, isPendingAgentSchedules is: {isPendingAgentSchedules} and isPendingFiles: {isPendingFiles}");
            if (!isPendingFiles && !isPendingAgentSchedules) continue;

            await queueClient.SendMessageAsync($"{dbName}");
        }

        _logger.LogInformation($"TransformLoadTimerFunction Function Ran. Next timer schedule = {timerInfo.ScheduleStatus.Next}");
    }
}
