using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace DataImport.AzureFunctions;

public class TransformLoadInstanceQueue
{
    private readonly ILogger _logger;
    public TransformLoadInstanceQueue(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<TransformLoadInstanceQueue>();
    }

    [Function($"{nameof(TransformLoadInstance_QueueFunction)}")]
    public async Task TransformLoadInstance_QueueFunction(
        [QueueTrigger("%EdGraphStorageConnectionQueueName%", Connection = "ConnectionStringsStorageConnection")]
         string dataImportTransformLoadInstanceName,
        [DurableClient] DurableTaskClient client,
        //[DurableClient] DurableClientContext starter,
        FunctionContext executionContext,
        CancellationToken cancellationToken
        )
    {
        try
        {
            _logger.LogInformation($"{nameof(QueueTriggerAttribute)} {nameof(TransformLoadInstance_QueueFunction)} execution started at: {DateTime.Now}");

            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("A cancellation token was received, taking precautionary actions.");
                // Take precautions like noting how far along you are with processing the batch
                _logger.LogInformation("Precautionary activities complete.");
                //break;
            }

            //await Task.Delay(1000);

            var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(TransformLoadInstanceOrchestration.TransformLoadInstance_RunOrchestrator), input: dataImportTransformLoadInstanceName);
            //Process process = Extensions.Extensions.GetTransformLoadProcess(dataImportTransformLoadInstanceName, _logger);

            //process.Start();
            //string output = process.StandardOutput.ReadToEnd();
            //string err = process.StandardError.ReadToEnd();
            //await process.WaitForExitAsync(cancellationToken);

            //_logger.LogInformation($"{output}");
            //_logger.LogError($"{err}");

            _logger.LogInformation($"{nameof(QueueTriggerAttribute)} {nameof(TransformLoadInstance_QueueFunction)} execution ended at: {DateTime.Now}");
        }
        catch (Exception exception)
        {
            _logger.LogError($"{exception}");
            _logger.LogInformation($"{nameof(QueueTriggerAttribute)} {nameof(TransformLoadInstance_QueueFunction)} execution ended at with exception: {DateTime.Now}");
        }
        //_logger.LogInformation($" TransformLoad Delay start");
        //await Task.Delay(6000);
        //_logger.LogInformation($" TransformLoad Delay over");
    }
}
