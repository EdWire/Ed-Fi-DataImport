// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common.ExtensionMethods;
using DataImport.Common.Helpers;
using DataImport.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.File;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using File = DataImport.Models.File;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace DataImport.Common
{
    public class AzureFileService : IFileService
    {
        private readonly ILogger<AzureFileService> _logger;
        private readonly IFileSettings _azureFileSettings;
        private readonly ConnectionStrings _connectionStrings;
        private readonly IFileHelper _fileHelper;

        private readonly IConfiguration _configuration;
        private readonly HttpContext _httpContext;
        private readonly string _jwtInstanceIdKey;

        public AzureFileService(ILogger<AzureFileService> logger, IOptions<ConnectionStrings> connectionStringsOptions, IFileSettings azureFileSettings, IFileHelper fileHelper, IConfiguration configuration, IHttpContextAccessor httpContentAccessor = null)
        {
            _logger = logger;
            _connectionStrings = connectionStringsOptions.Value;
            _azureFileSettings = azureFileSettings;
            _fileHelper = fileHelper;

            _configuration = configuration;
            _httpContext = httpContentAccessor?.HttpContext;
            _jwtInstanceIdKey = configuration["Instance:JwtInstanceIdKey"];
        }

        public async Task Upload(string fileName, Stream fileStream, Agent agent)
        {
            var fileShare = GetFileShare();

            if (await fileShare.ExistsAsync())
            {
                //NOTE: Patch for tenant in azure file service
                string rootDataImportDirectory = "DataImport";
                if (_configuration["AppSettings:Mode"] == "InstanceYearSpecific")
                {
                    if (_httpContext == default) throw new DataImport.Models.ConfigurationErrorsException($"{nameof(AzureFileService)} was not configured and an http context was not provided via {nameof(IHttpContextAccessor)}.");
                    var instanceId = _httpContext.GetJwtClaimBasedInstanceIdAsync(_jwtInstanceIdKey).Result;
                    if (string.IsNullOrEmpty(instanceId)) throw new InvalidOperationException("The instance-year-specific DataImport database name replacement token cannot be derived because the instance id was not set in the current context.");

                    rootDataImportDirectory = $"DataImport_{instanceId}";
                }

                var fileDirectoryRoot = fileShare.GetRootDirectoryReference();
                var fileAgentDirectory = fileDirectoryRoot.GetDirectoryReference(agent.GetDirectory(rootDataImportDirectory));

                await EnsureDataImportDirectoryExists(fileDirectoryRoot, rootDataImportDirectory);
                await fileAgentDirectory.CreateIfNotExistsAsync();
                var cloudFile = fileAgentDirectory.GetFileReference($"{Guid.NewGuid()}-{fileName}");
                await cloudFile.UploadFromStreamAsync(fileStream);
                var recordCount = fileStream.TotalLines(fileName.IsCsvFile());

                _fileHelper.LogFile(fileName, agent.Id, cloudFile.StorageUri.PrimaryUri.ToString(), FileStatus.Uploaded, recordCount);
                _logger.LogInformation("File '{file}' was uploaded to '{uri}' for Agent '{name}' (Id: {id}).", fileName, cloudFile.StorageUri.PrimaryUri, agent.Name, agent.Id);
            }
            else
            {
                var message = $"The file share '{fileShare}' does not exist.";
                _logger.LogError(message);
                throw new Exception(message);
            }
        }

        public async Task Transfer(Stream stream, string file, Agent agent)
        {
            var shortFileName = file.Substring(file.LastIndexOf('/') + 1);

            var fileShare = GetFileShare();

            if (!await fileShare.ExistsAsync())
                _logger.LogError("Azure file share does not exist.");
            else
            {
                try
                {
                    //NOTE: Patch for tenant in azure file service
                    string rootDataImportDirectory = "DataImport";
                    if (_configuration["AppSettings:Mode"] == "InstanceYearSpecific")
                    {
                        if (_httpContext == default) throw new ConfigurationErrorsException($"{nameof(AzureFileService)} was not configured and an http context was not provided via {nameof(IHttpContextAccessor)}.");
                        var instanceId = _httpContext.GetJwtClaimBasedInstanceIdAsync(_jwtInstanceIdKey).Result;
                        if (string.IsNullOrEmpty(instanceId)) throw new InvalidOperationException("The instance-year-specific DataImport database name replacement token cannot be derived because the instance id was not set in the current context.");

                        rootDataImportDirectory = $"DataImport_{instanceId}";
                    }

                    var fileDirectoryRoot = fileShare.GetRootDirectoryReference();
                    var fileAgentDirectory = fileDirectoryRoot.GetDirectoryReference(agent.GetDirectory(rootDataImportDirectory));

                    await EnsureDataImportDirectoryExists(fileDirectoryRoot, rootDataImportDirectory);
                    await fileAgentDirectory.CreateIfNotExistsAsync();
                    var cloudFile = fileAgentDirectory.GetFileReference($"{Guid.NewGuid()}-{shortFileName}");
                    stream.Seek(0, SeekOrigin.Begin);
                    await cloudFile.UploadFromStreamAsync(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    var recordCount = stream.TotalLines(file.IsCsvFile());

                    await _fileHelper.LogFileAsync(shortFileName, agent.Id, cloudFile.StorageUri.PrimaryUri.ToString(), FileStatus.Uploaded, recordCount);
                    _logger.LogInformation("Successfully transferred file {file} to {uri} by agent ID: {agent}", shortFileName, cloudFile.StorageUri.PrimaryUri, agent.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in TransferFile for file: {file} on site: ", agent.Url);
                    await _fileHelper.LogFileAsync(shortFileName, agent.Id, "", FileStatus.ErrorUploaded, 0);
                }
            }
        }

        public async Task<string> Download(File file)
        {
            var tempFileFullPath = Path.Combine(
                Path.GetTempPath(),
                Guid.NewGuid() + Path.GetExtension(file.FileName));

            var cloudFile = GetCloudFile(file);
            await cloudFile.DownloadToFileAsync(tempFileFullPath, FileMode.Create);

            return tempFileFullPath;
        }

        public async Task Delete(File file)
        {
            var cloudFile = GetCloudFile(file);
            await cloudFile.DeleteAsync();
            await CleanAgentDirectoryIfEmpty(cloudFile.Parent);
        }

        private CloudFileShare GetFileShare()
        {
            var storageAccount = GetStorageAccount();
            var fileClient = storageAccount.CreateCloudFileClient();
            return fileClient.GetShareReference(_azureFileSettings.ShareName);
        }

        private CloudFile GetCloudFile(File file)
        {
            var fileUri = new Uri(file.Url);
            var storageAccount = GetStorageAccount();
            storageAccount.CreateCloudFileClient();
            return new CloudFile(fileUri, storageAccount.Credentials);
        }

        private CloudStorageAccount GetStorageAccount()
        {
            var azureFileConnectionString = _connectionStrings.StorageConnection;
            return CloudStorageAccount.Parse(azureFileConnectionString);
        }

        private static async Task EnsureDataImportDirectoryExists(CloudFileDirectory fileDirectoryRoot, string dataImportDirectory)
        {
            var fileAgentDirectory = fileDirectoryRoot.GetDirectoryReference(dataImportDirectory);
            await fileAgentDirectory.CreateIfNotExistsAsync();
        }

        private static async Task CleanAgentDirectoryIfEmpty(CloudFileDirectory agentDirectory)
        {
            if (!(await ListFilesAndDirectories(agentDirectory)).Any())
                await agentDirectory.DeleteAsync();
        }

        private static async Task<IEnumerable<IListFileItem>> ListFilesAndDirectories(CloudFileDirectory directory)
        {
            FileContinuationToken token = null;
            var listResultItems = new List<IListFileItem>();
            do
            {
                FileResultSegment resultSegment = await directory.ListFilesAndDirectoriesSegmentedAsync(token);
                token = resultSegment.ContinuationToken;

                foreach (IListFileItem listResultItem in resultSegment.Results)
                {
                    listResultItems.Add(listResultItem);
                }
            }
            while (token != null);

            return listResultItems;
        }

        public async Task<string> GetRowProcessorScript(string name)
        {
            return await GetScriptContent("RowProcessors", name);
        }

        public async Task<string> GetFileGeneratorScript(string name)
        {
            return await GetScriptContent("FileGenerators", name);
        }

        private async Task<string> GetScriptContent(string scriptFolder, string name)
        {
            var directory = GetFileShare()
                .GetRootDirectoryReference()
                .GetDirectoryReference(Path.Combine("DataImport", scriptFolder));

            var filesAndDirectories = await ListFilesAndDirectories(directory);

            return await filesAndDirectories
                .OfType<CloudFile>()
                .Single(x => x.Name == name)
                .DownloadTextAsync();
        }
    }
}
