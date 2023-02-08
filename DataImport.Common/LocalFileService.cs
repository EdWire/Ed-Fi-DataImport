// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common.ExtensionMethods;
using DataImport.Common.Helpers;
using DataImport.Models;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using File = DataImport.Models.File;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Configuration;

namespace DataImport.Common
{
    public class LocalFileService : IFileService
    {
        private readonly ILogger<LocalFileService> _logger;
        private readonly IFileSettings _fileSettings;
        private readonly IFileHelper _fileHelper;

        private readonly IConfiguration _configuration;
        private readonly HttpContext _httpContext;
        private readonly string _jwtInstanceIdKey;
        public LocalFileService(ILogger<LocalFileService> logger, IFileSettings fileSettings, IFileHelper fileHelper, IConfiguration configuration, IHttpContextAccessor httpContentAccessor = null)
        {
            _logger = logger;
            _fileSettings = fileSettings;
            _fileHelper = fileHelper;

            _configuration = configuration;
            _httpContext = httpContentAccessor?.HttpContext;
            _jwtInstanceIdKey = configuration["Instance:JwtInstanceIdKey"];
        }

        public async Task Upload(string fileName, Stream fileStream, Agent agent)
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

            var uploadPath = Path.Combine(_fileSettings.ShareName, agent.GetDirectory(rootDataImportDirectory));

            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var fullFilePath = Path.Combine(uploadPath, $"{Guid.NewGuid()}-{fileName}");

            using (var file = System.IO.File.Create(fullFilePath))
            {
                fileStream.Seek(0, SeekOrigin.Begin);
                await fileStream.CopyToAsync(file);
            }

            var recordCount = fileStream.TotalLines(fileName.IsCsvFile());

            _fileHelper.LogFile(fileName, agent.Id, UrlUtility.ConvertLocalPathToUri(fullFilePath), FileStatus.Uploaded, recordCount);

            _logger.LogInformation("File '{file}' was uploaded to '{path}' for Agent '{name}' (Id: {id}).", fileName, uploadPath, agent.Name, agent.Id);
        }

        public virtual async Task Transfer(Stream stream, string file, Agent agent)
        {
            var shortFileName = file.Substring(file.LastIndexOf('/') + 1);

            try
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

                var localPath = Path.Combine(_fileSettings.ShareName, agent.GetDirectory(rootDataImportDirectory));
                var localFilePath = Path.Combine(localPath, $"{Guid.NewGuid()}-{shortFileName}");
                var localFileUri = new Uri(localFilePath);

                if (!Directory.Exists(localPath))
                    Directory.CreateDirectory(localPath);

                using (var fileStream = System.IO.File.Create(localFilePath))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(fileStream);
                }

                stream.Seek(0, SeekOrigin.Begin);
                var recordCount = stream.TotalLines(file.IsCsvFile());

                await _fileHelper.LogFileAsync(shortFileName, agent.Id, localFileUri.AbsoluteUri, FileStatus.Uploaded, recordCount);
                _logger.LogInformation("Successfully transferred file {file} to {path} by agent ID: {id}", shortFileName, localFileUri.AbsoluteUri, agent.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in TransferFile for file: {file} on site: {path}", file, agent.Url);
                await _fileHelper.LogFileAsync(shortFileName, agent.Id, "", FileStatus.ErrorUploaded, 0);
            }
        }

        public Task<string> Download(File file)
        {
            var tempFileFullPath = Path.Combine(
                Path.GetTempPath(),
                Guid.NewGuid() + Path.GetExtension(file.FileName));

            var localPath = new Uri(file.Url).LocalPath;
            var localFileInfo = new FileInfo(localPath);
            System.IO.File.Copy(localFileInfo.FullName, tempFileFullPath, true);

            return Task.FromResult(tempFileFullPath);
        }

        public Task Delete(File file)
        {
            System.IO.File.Delete(new Uri(file.Url).LocalPath);
            return CleanAgentDirectoryIfEmpty(file);
        }

        private static Task CleanAgentDirectoryIfEmpty(File file)
        {
            var agentDirectoryPath = Directory.GetParent(new Uri(file.Url).LocalPath).ToString();
            if (!Directory.GetFiles(agentDirectoryPath).Any())
                Directory.Delete(agentDirectoryPath);
            return Task.CompletedTask;
        }

        public Task<string> GetRowProcessorScript(string name)
        {
            return GetScriptContent("RowProcessors", name);
        }

        public Task<string> GetFileGeneratorScript(string name)
        {
            return GetScriptContent("FileGenerators", name);
        }

        private async Task<string> GetScriptContent(string scriptFolder, string name)
        {
            var filePath = Path.Combine(_fileSettings.ShareName, "DataImport", scriptFolder, name);
            return await System.IO.File.ReadAllTextAsync(filePath);
        }
    }
}
