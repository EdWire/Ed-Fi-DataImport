
namespace DataImport.AzureFunctions.Manager.Services
{
    public enum DrainModeState
    {
        /// <summary>
        /// Drain mode is disabled
        /// </summary>
        Disabled,

        /// <summary>
        /// Drain mode is enabled and there are active invocations or retries
        /// </summary>
        InProgress,

        /// <summary>
        /// Drain mode is enabled and there are no active invocations or retries
        /// </summary>
        Completed
    }
}
