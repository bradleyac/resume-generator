// using System;
// using System.Collections.Generic;
// using Microsoft.Azure.Functions.Worker;
// using Microsoft.Extensions.Logging;

// namespace RGS.Functions;

// public class PendingPostingsTrigger
// {
//     private readonly ILogger<PendingPostingsTrigger> _logger;

//     public PendingPostingsTrigger(ILogger<PendingPostingsTrigger> logger)
//     {
//         _logger = logger;
//     }

//     [Function("PendingPostingsTrigger")]
//     public void Run([CosmosDBTrigger(
//         databaseName: "Resumes",
//         containerName: "PendingPostings",
//         Connection = "CosmosDBConnectionString",
//         LeaseContainerName = "leases",
//         CreateLeaseContainerIfNotExists = true)] IReadOnlyList<MyDocument> input)
//     {
//         Microsoft.Azure.Cosmos.CosmosClient client = new(accountEndpoint: "https://resume-generation-system.documents.azure.com:443/", tokenCredential: new DefaultAzureCredential());
//         var pendingPostings = client.GetContainer("Resumes", "PendingPostings");

//         if (input != null && input.Count > 0)
//         {
//             _logger.LogInformation("Documents modified: " + input.Count);
//             _logger.LogInformation("First document Id: " + input[0].id);
//         }
//     }
// }

// public class MyDocument
// {
//     public string id { get; set; }

//     public string Text { get; set; }

//     public int Number { get; set; }

//     public bool Boolean { get; set; }
// }