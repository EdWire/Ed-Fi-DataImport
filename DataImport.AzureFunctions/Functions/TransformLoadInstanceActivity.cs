using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace DataImport.AzureFunctions;

public class TransformLoadInstanceActivity
{
    private readonly ILogger _logger;
    public TransformLoadInstanceActivity(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<TransformLoadInstanceQueue>();
    }


    [Function(nameof(TransformLoadInstance_Activity))]
    public async Task<TransformLoadToolResponse> TransformLoadInstance_Activity(
        [ActivityTrigger] string dataImportTransformLoadInstanceName,
        //[ActivityTrigger] TaskActivityContext context,
        CancellationToken cancellationToken
        )
    {
        try
        {
            _logger.LogInformation($"QueueTrigger {nameof(TransformLoadInstance_Activity)} execution started at: {DateTime.Now}");

            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("A cancellation token was received, taking precautionary actions.");
                // Take precautions like noting how far along you are with processing the batch
                _logger.LogInformation("Precautionary activities complete.");
                //break;
            }

            ////TODO: Remove code in prod
            //await Task.Delay(1000);

            Process process = Extensions.Extensions.GetTransformLoadProcess(dataImportTransformLoadInstanceName, _logger);

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string err = process.StandardError.ReadToEnd();
            await process.WaitForExitAsync(cancellationToken);

            _logger.LogInformation($"{output}");
            _logger.LogError($"{err}");

            TransformLoadToolResponse TransformLoadToolResponse = new TransformLoadToolResponse() { Response = "Successful run" };

            _logger.LogInformation($"QueueTrigger {nameof(TransformLoadInstance_Activity)} execution ended at: {DateTime.Now}");
            return TransformLoadToolResponse;
        }
        catch (Exception exception)
        {
            _logger.LogError($"{exception}");
            _logger.LogInformation($"QueueTrigger {nameof(TransformLoadInstance_Activity)} execution ended at with exception: {DateTime.Now}");

            TransformLoadToolResponse TransformLoadToolResponse = new TransformLoadToolResponse() { Response = "Failed run" };
            return TransformLoadToolResponse;
        }
    }
}
