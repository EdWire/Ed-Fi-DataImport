// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common.ExtensionMethods;
using DataImport.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataImport.Web.Services.Swagger
{
    public class SwaggerMetadataFetcher : ISwaggerMetadataFetcher
    {
        private readonly IEnumerable<ISwaggerMetadataProcessor> _swaggerMetadataProcessors;
        private readonly ISwaggerWebClient _swaggerWebClient;

        public SwaggerMetadataFetcher(IEnumerable<ISwaggerMetadataProcessor> swaggerMetadataProcessors, ISwaggerWebClient swaggerWebClient)
        {
            _swaggerMetadataProcessors = swaggerMetadataProcessors;
            _swaggerWebClient = swaggerWebClient;
        }

        public async Task<IEnumerable<SwaggerResource>> GetMetadata(string apiUrl, string apiVersion)
        {
            var (resourcesSwaggerDocument, resourcesHandler) = await GetSwaggerDocument(apiUrl, apiVersion, ApiSection.Resources);
            var (descriptorsSwaggerDocument, descriptorsHandler) = await GetSwaggerDocument(apiUrl, apiVersion, ApiSection.Descriptors);

            var resources = await resourcesHandler.GetMetadata(resourcesSwaggerDocument, ApiSection.Resources);
            var descriptors = await descriptorsHandler.GetMetadata(descriptorsSwaggerDocument, ApiSection.Descriptors);

            return resources.Concat(descriptors);
        }

        public async Task<string> GetTokenUrl(string apiUrl, string apiVersion)
        {
            var (swaggerDocument, handler) = await GetSwaggerDocument(apiUrl, apiVersion, ApiSection.Resources);

            return handler.GetTokenUrl(swaggerDocument);
        }

        public async Task<string> GetAuthUrl(string apiUrl, string apiVersion)
        {
            var (swaggerDocument, handler) = await GetSwaggerDocument(apiUrl, apiVersion, ApiSection.Resources);

            return handler.GetAuthUrl(swaggerDocument);
        }

        public async Task<string> GetYearSpecificYear(string apiUrl)
        {
            try
            {
                var baseUrl = Common.Helpers.UrlUtility.RemoveAfterLastInstanceOf(apiUrl.Trim(), "/data/");

                var rawApis = await _swaggerWebClient.DownloadString(baseUrl);
                var response = JToken.Parse(rawApis);
                var isYearSpecific = response["apiMode"].ToString() == "Year Specific";
                var isInstanceYearSpecific = response["apiMode"].ToString() == "Instance Year Specific";

                string year;
                year = isYearSpecific ? new Uri(apiUrl).Segments.Last().Trim('/') : null;
                year = isInstanceYearSpecific ? new Uri(apiUrl).Segments.Last().Trim('/') : null;
                return year;
            }
            catch (Exception exception)
            {
                throw new OdsApiServerException(exception);
            }
        }

        public async Task<string> GetInstanceYearSpecificInstance(string apiUrl)
        {
            try
            {
                var baseUrl = Common.Helpers.UrlUtility.RemoveAfterLastInstanceOf(apiUrl.Trim(), "/data/");

                var rawApis = await _swaggerWebClient.DownloadString(baseUrl);
                var response = JToken.Parse(rawApis);
                var isInstanceYearSpecific = response["apiMode"].ToString() == "Instance Year Specific";

                return isInstanceYearSpecific ? new Uri(apiUrl).Segments.Reverse().Skip(1).Take(1).Single().Trim('/') : null;
            }
            catch (Exception exception)
            {
                throw new OdsApiServerException(exception);
            }
        }

        public async Task<string> GetInstanceYearSpecificYear(string apiUrl)
        {
            try
            {
                var baseUrl = Common.Helpers.UrlUtility.RemoveAfterLastInstanceOf(apiUrl.Trim(), "/data/");

                var rawApis = await _swaggerWebClient.DownloadString(baseUrl);
                var response = JToken.Parse(rawApis);
                var isInstanceYearSpecific = response["apiMode"].ToString() == "Instance Year Specific";

                return isInstanceYearSpecific ? new Uri(apiUrl).Segments.Last().Trim('/') : null;
            }
            catch (Exception exception)
            {
                throw new OdsApiServerException(exception);
            }
        }

        public async Task<string> InferOdsApiVersion(string apiUrl)
        {
            const string OdsV2Default = "2.5+";
            var (isOdsV3, apiVersion) = await IsOdsV3(apiUrl);
            if (isOdsV3)
                return apiVersion;
            var (isOdsV2, _) = await IsOdsV2(apiUrl);
            return isOdsV2 ? OdsV2Default : null;
        }

        private async Task<(bool isOdsV3, string apiVersion)> IsOdsV3(string apiUrl)
        {
            var baseUrl = Common.Helpers.UrlUtility.RemoveAfterLastInstanceOf(apiUrl.Trim(), "/data/");
            try
            {
                var rawApis = await _swaggerWebClient.DownloadString(baseUrl);
                var response = JToken.Parse(rawApis);
                var apiVersion = response["version"].ToString();
                return (true, apiVersion);
            }
            catch (Exception)
            {
                return (false, null);
            }
        }

        private async Task<(bool isOdsV2, string apiVersion)> IsOdsV2(string apiUrl)
        {
            var baseUrl = Common.Helpers.UrlUtility.RemoveAfterLastInstanceOf(apiUrl.Trim(), "/api/");
            baseUrl = $"{baseUrl}/metadata/resources/api-docs";
            try
            {
                var rawApis = await _swaggerWebClient.DownloadString(baseUrl);
                var response = JToken.Parse(rawApis);
                var apiVersion = response["apiVersion"]?.ToString();
                return (apiVersion == "2.0", apiVersion);
            }
            catch (Exception)
            {
                return (false, null);
            }
        }

        private async Task<(JObject swaggerDocument, ISwaggerMetadataProcessor swaggerMetadataProcessor)> GetSwaggerDocument(string apiUrl, string apiVersion, ApiSection apiSection)
        {
            var baseUrl = await GetSwaggerBaseDocumentUrl(apiUrl, apiVersion, apiSection);

            var rawApis = await _swaggerWebClient.DownloadString(baseUrl);

            JObject swaggerDocument;

            try
            {
                swaggerDocument = JObject.Parse(rawApis);
            }

            catch (Exception)
            {
                throw new Exception($"Swagger document at '{baseUrl}' could not be parsed");
            }

            var handler = _swaggerMetadataProcessors?.FirstOrDefault(p => p.CanHandle(swaggerDocument));
            if (handler == null)
                throw new NotSupportedException("No handler available to process Swagger document");
            return (swaggerDocument, handler);
        }

        protected async Task<string> GetSwaggerBaseDocumentUrl(string apiUrl, string apiVersion, ApiSection apiSection)
        {
            if (apiVersion.IsOdsV2())
            {
                var v2BaseUrl = Common.Helpers.UrlUtility.RemoveAfterLastInstanceOf(apiUrl.Trim(), "/api/");
                return $"{v2BaseUrl}/metadata/{apiSection.ToMetadataRoutePart()}/api-docs";
            }

            var baseUrl = Common.Helpers.UrlUtility.RemoveAfterLastInstanceOf(apiUrl.Trim(), "/data/");
            var year = await GetYearSpecificYear(apiUrl);

            var instanceYearSpecificInstance = await GetInstanceYearSpecificInstance(apiUrl);
            var instanceYearSpecificYear = await GetInstanceYearSpecificYear(apiUrl);

            string path;

            if (year is not null && instanceYearSpecificInstance is null)
            {
                path = $"data/v3/{year}";
            }
            else if (instanceYearSpecificInstance is not null && instanceYearSpecificYear is not null)
            {
                path = $"data/v3/{instanceYearSpecificInstance}/{instanceYearSpecificYear}";
            }
            else
            {
                path = "data/v3";
            }

            return $"{baseUrl}/metadata/{path}/{apiSection.ToMetadataRoutePart()}/swagger.json";
        }
    }
}
