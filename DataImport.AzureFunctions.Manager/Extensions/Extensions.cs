using Azure.Storage.Queues;

using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataImport.AzureFunctions.Manager.Extensions
{
    public static class Extensions
    {
        public const string TransformLoadFolder = "TransformLoadTool";
        public const string TransformLoadExe = "DataImport.Server.TransformLoad"; //linux single exe

        public static QueueClient GetQueue(string? storageConnectionTransformLoadQueue, string? dataImportTransformLoadQueueName)
        {

            QueueClient queueClient = new QueueClient(storageConnectionTransformLoadQueue, dataImportTransformLoadQueueName, new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 });
            // Instantiate a QueueClient to create and interact with the queue        
            queueClient.CreateIfNotExists();
            return queueClient;
        }
    }
}

