using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace DataImport.AzureFunctions;

public class TransformLoadInstanceOrchestration
{
    private readonly ILogger _logger;
    public TransformLoadInstanceOrchestration(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<TransformLoadInstanceOrchestration>();
    }

    [Function($"{nameof(TransformLoadInstance_RunOrchestrator)}")]
    public async Task TransformLoadInstance_RunOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context,
        CancellationToken cancellationToken)
    {

        try
        {
            _logger.LogInformation($"QueueTrigger {nameof(TransformLoadInstance_RunOrchestrator)} execution started at: {DateTime.Now}");

            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("A cancellation token was received, taking precautionary actions.");
                // Take precautions like noting how far along you are with processing the batch
                _logger.LogInformation("Precautionary activities complete.");
                //break;
            }

            string dataImportTransformLoadInstanceName = context.GetInput<string>();

            var toolsTask = await context.CallActivityAsync<TransformLoadToolResponse>(nameof(TransformLoadInstanceActivity.TransformLoadInstance_Activity), input: dataImportTransformLoadInstanceName);


            _logger.LogInformation($"Trigger {nameof(TransformLoadInstance_RunOrchestrator)} execution ended at: {DateTime.Now}");
        }
        catch (Exception exception)
        {
            _logger.LogError($"{exception}");
            _logger.LogInformation($"Trigger {nameof(TransformLoadInstance_RunOrchestrator)} execution ended at with exception: {DateTime.Now}");
        }
    }
}
