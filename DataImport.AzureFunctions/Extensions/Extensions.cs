using Azure.Storage.Queues;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;

namespace DataImport.AzureFunctions.Extensions
{
    public static class Extensions
    {
        public const string TransformLoadFolder = "TransformLoadTool";
        public const string TransformLoadExe = "DataImport.Server.TransformLoad"; //linux single exe

        public static Process GetTransformLoadProcess(string dataImportTransformLoadInstanceName, ILogger _logger)
        {
            string? pathBase = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var toolPath = Path.Combine(pathBase, TransformLoadFolder);
            var toolExe = Path.Combine(toolPath, TransformLoadExe);

            ProcessStartInfo processStartInfo = new()
            {
                WorkingDirectory = toolPath,
                FileName = toolExe,//toolExe;   
                //Arguments = "";

                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            //var systemRootEnv = processStartInfo.EnvironmentVariables["SYSTEMROOT"];
            //NOTE: Patch needed to control various ENV from inheriting in local machine
            //NOTE: Breaking in K8
            //processStartInfo.Environment.Clear();
            //processStartInfo.EnvironmentVariables["SYSTEMROOT"] = systemRootEnv;

            processStartInfo.EnvironmentVariables["DOTNET_ENVIRONMENT"] =  Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
            processStartInfo.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            processStartInfo.EnvironmentVariables["AppSettings__DatabaseEngine"] = Environment.GetEnvironmentVariable("AppSettings__DatabaseEngine");

            processStartInfo.EnvironmentVariables["AppSettings__FileMode"] = Environment.GetEnvironmentVariable("AppSettings__FileMode");
            processStartInfo.EnvironmentVariables["AppSettings__ShareName"] = Environment.GetEnvironmentVariable("AppSettings__ShareName");

            processStartInfo.EnvironmentVariables["ConnectionStrings__storageConnection"] = Environment.GetEnvironmentVariable("ConnectionStrings__storageConnection");// "UseDevelopmentStorage=true";
            processStartInfo.EnvironmentVariables["ConnectionStrings__defaultConnection"] = DbExtensions.SubstituteDataImportInstance(dataImportTransformLoadInstanceName);

            var process = new Process()
            {
                StartInfo = processStartInfo
            };

            return process;
        }
    }
}

