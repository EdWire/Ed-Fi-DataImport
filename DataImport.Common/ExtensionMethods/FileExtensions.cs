// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using CsvHelper;
using CsvHelper.Configuration;
using DataImport.Models;
using Microsoft.Extensions.Options;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Configuration = CsvHelper.Configuration.CsvConfiguration;

namespace DataImport.Common.ExtensionMethods
{
    public static class FileExtensions
    {
        private static IOptions<ConnectionStrings> ConnectionStringsOptions => _connectionStringsOptions;
        private static IOptions<ConnectionStrings> _connectionStringsOptions;

        public static void SetConnectionStringsOptions(IOptions<ConnectionStrings> options)
        {
            if (_connectionStringsOptions == default(IOptions<ConnectionStrings>))
                _connectionStringsOptions = options;
            else
            {
                // this is to make sure that the global state isn't changed while running.
                throw new NotSupportedException("ConnectionStringOptions may not be set more than once.");
            }
        }

        public static int TotalLines(this Stream stream, bool isCsv)
        {
            stream.Seek(0, SeekOrigin.Begin);
            var totalLines = 0;
            using (var r = new StreamReader(stream))
            {
                if (isCsv)
                {
                    var configuration = new Configuration(CultureInfo.InvariantCulture)
                    {
                        IgnoreBlankLines = true,
                        TrimOptions = TrimOptions.Trim,
                        ShouldSkipRecord = record => record.Record.All(string.IsNullOrWhiteSpace)
                    };
                    using (var csv = new CsvReader(r, configuration))
                    {
                        while (csv.Read())
                        {
                            var record = csv.GetRecord<object>();
                            if (record != null)
                                totalLines++;
                        }
                    }
                }
                else
                {
                    while (r.ReadLine() != null) { totalLines++; }
                }

                return totalLines;
            }
        }

        public static bool IsCsvFile(this string fileName) =>
            string.Equals(Path.GetExtension(fileName), ".csv", StringComparison.OrdinalIgnoreCase);

        public static string GetDirectory(this Agent agent, string dataImportDirectory) => Path.Combine(dataImportDirectory, $"Agent-{agent.Id}");
    }
}
