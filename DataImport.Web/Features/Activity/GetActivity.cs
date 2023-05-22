// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using AutoMapper;
using DataImport.Common;
using DataImport.Models;
using DataImport.Web.Features.Shared;
using DataImport.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DataImport.Web.Features.Activity
{
    public class GetActivity
    {
        public class ViewModel : IApiServerListViewModel
        {
            public HealthModel Health { get; set; }
            public FileModel[] Files { get; set; }
            public List<SelectListItem> ApiServers { get; set; }
            public int? ApiServerId { get; set; }
            public bool HasRecentFiles { get; set; }
        }

        public class HealthModel
        {
            public HealthModel(string message, bool warning = false)
            {
                Message = message;
                Warning = warning;
            }

            public string Message { get; set; }
            public bool Warning { get; set; }
        }

        public class FileModel
        {
            public string AgentName { get; set; }
            public string FileName { get; set; }
            public DateTimeOffset? CreateDate { get; set; }
            public int? Rows { get; set; }
            public FileStatus Status { get; set; }
            public string ApiConnection { get; set; }
        }

        public class Query : IRequest<ViewModel>, IApiServerSpecificRequest
        {
            public int? ApiServerId { get; set; }
            public int? ApiVersionId { get; set; }
            public int? BrowserDateTimeOffSet { get; set; }
        }

        public class QueryHandler : RequestHandler<Query, ViewModel>
        {
            private readonly DataImportDbContext _database;
            private readonly IMapper _mapper;
            private readonly IClock _clock;

            public QueryHandler(DataImportDbContext database, IMapper mapper, IClock clock)
            {
                _database = database;
                _mapper = mapper;
                _clock = clock;
            }

            protected override ViewModel Handle(Query request) =>
                new ViewModel
                {
                    Health = JobHealth(request),
                    Files = Files(request.ApiServerId),
                    HasRecentFiles = HasRecentFiles()
                };

            private HealthModel JobHealth(Query request = null)
            {
                var jobStatus = _database.EnsureSingle<JobStatus>();

                const string Job = "The Transform / Load process";

                if (jobStatus.Started == null)
                    return Warn($"{Job} has not yet executed.");

                var duration = (_clock.Now - jobStatus.Started.Value).ToReadableDuration();

                if (jobStatus.Completed == null)
                    return Ok($"{Job} has been running for {duration}.");

                duration = (jobStatus.Completed.Value - jobStatus.Started.Value).ToReadableDuration();

                //NOTE: Test Code
                //var job = jobStatus.Started.Value;
                //var joboffset = jobStatus.Started.Value.ToOffset(TimeSpan.FromMinutes(Convert.ToDouble(request.BrowserDateTimeOffSet)));

                //Note: Patch for browser based offset
                if (request is not null && request.BrowserDateTimeOffSet.HasValue)
                {
                    var browserOffset = TimeSpan.FromMinutes(Convert.ToDouble(request.BrowserDateTimeOffSet));
                    var utcDateTime = jobStatus.Started.Value.UtcDateTime; //Change Server Time to UTC Time
                    var utcDateTimeOffSet = new DateTimeOffset(utcDateTime, TimeSpan.Zero);
                    var browserDateTimeOffSet = utcDateTimeOffSet.ToOffset(browserOffset);//Change UTC Time to Browser Time

                    jobStatus.Started = browserDateTimeOffSet;
                }

                return Ok($"{Job} started at {Time(jobStatus.Started)} and ran for {duration}.");
            }

            private FileModel[] Files(int? apiServerId)
            {
                return _database.Files
                    .Include(x => x.Agent)
                    .ThenInclude(x => x.ApiServer)
                    .OrderByDescending(x => x.CreateDate)
                    .Where(x => apiServerId.HasValue && x.Agent.ApiServerId == apiServerId.Value || !apiServerId.HasValue)
                    .Where(GetRecentActivityFilterExpression())
                    .ToList()
                    .Select(_mapper.Map<FileModel>)
                    .ToArray();
            }

            private bool HasRecentFiles()
            {
                return _database.Files.Any(GetRecentActivityFilterExpression());
            }

            private Expression<Func<File, bool>> GetRecentActivityFilterExpression()
            {
                var weekAgo = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(7));
                return (x) =>
#pragma warning disable 618
                    x.Status != FileStatus.Deleted &&
                    x.Status != FileStatus.Canceled &&
                    (x.Status != FileStatus.Loaded || x.CreateDate >= weekAgo);
#pragma warning restore 618
            }

            private static HealthModel Ok(string message)
                => new HealthModel(message);

            private static HealthModel Warn(string message)
                => new HealthModel(message, warning: true);

            private static string Time(DateTimeOffset? dateTimeOffset)
                => dateTimeOffset?.ToString("yyyy-MM-dd hh:mm tt");
        }
    }
}
