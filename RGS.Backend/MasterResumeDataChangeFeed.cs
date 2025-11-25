using System;
using System.Collections.Generic;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RGS.Backend.Shared.Models;

namespace RGS.Backend;

internal class MasterResumeDataChangeFeed(ILogger<MasterResumeDataChangeFeed> logger, CosmosClient cosmosClient)
{
    private readonly ILogger<MasterResumeDataChangeFeed> _logger = logger;
    private readonly CosmosClient _cosmosClient = cosmosClient;

    [Function("MasterResumeDataChangeFeed")]
    public void Run([CosmosDBTrigger(
        databaseName: "Resumes",
        containerName: "ResumeData",
        Connection = "CosmosDBConnectionString",
        LeaseContainerName = "leases",
        CreateLeaseContainerIfNotExists = true)] IReadOnlyList<ResumeData> input)
    {
        var masterUpdates = input.Where(rd => rd.IsMaster).Select(rd => rd with { id = Guid.NewGuid().ToString() }).ToList();
        if (masterUpdates.Count > 0)
        {
            var resumeDataContainer = _cosmosClient.GetContainer("Resumes", "ResumeDataVersions");
            foreach (var update in masterUpdates)
            {
                _logger.LogInformation("Creating new resume data version with id {id}", update.id);
                resumeDataContainer.CreateItemAsync(update, new PartitionKey(update.id));
            }
        }
    }
}