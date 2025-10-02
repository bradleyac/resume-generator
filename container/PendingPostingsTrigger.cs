using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Runtime.ConstrainedExecution;
using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using RGS.Backend.Shared.Models;

namespace RGS.Functions;

public class PendingPostingsTrigger
{
    private const string PageUrl = "https://happy-mushroom-0344c0c0f.2.azurestaticapps.net/resume";
    private readonly ILogger<PendingPostingsTrigger> _logger;

    public PendingPostingsTrigger(ILogger<PendingPostingsTrigger> logger)
    {
        _logger = logger;
    }

    [Function("PendingPostingsTrigger")]
    public async Task Run([CosmosDBTrigger(
        databaseName: "Resumes",
        containerName: "PendingPostings",
        Connection = "CosmosDBConnectionString",
        LeaseContainerName = "leases",
        CreateLeaseContainerIfNotExists = true)] IReadOnlyList<JobPosting> input)
    {
        var connectionString = Environment.GetEnvironmentVariable("CosmosDBConnectionString");
        CosmosClient client = new(connectionString);
        var pendingPostings = client.GetContainer("Resumes", "PendingPostings");
        var completedPostings = client.GetContainer("Resumes", "CompletedPostings");

        foreach (var posting in input)
        {
            using var stream = await GeneratePDFStreamAsync();
            var resumeUrl = await SaveToBlobStorageAsync(stream);
            await completedPostings.UpsertItemAsync(new CompletedPosting(posting.id, posting.PostingText, posting.ImportedAt, resumeUrl));
            await pendingPostings.DeleteItemAsync<JobPosting>(posting.id, partitionKey: new PartitionKey(posting.id));
        }
    }

    private static async Task<Stream> GeneratePDFStreamAsync()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync();
        var page = await browser.NewPageAsync();

        await page.GotoAsync(PageUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });
        return new MemoryStream(await page.PdfAsync(new PagePdfOptions { PrintBackground = true }));
    }

    private static async Task<string> SaveToBlobStorageAsync(Stream pdfStream)
    {
        var connectionString = Environment.GetEnvironmentVariable("RESUME_BLOB_CONTAINER_CONNECTION_STRING");
        var client = new BlobContainerClient(connectionString, "resumes");
        var blobName = $"resume-{Guid.NewGuid()}";
        _ = await client.UploadBlobAsync(blobName, pdfStream);
        return $"https://resumeartifacts.blob.core.windows.net/resumes/{blobName}";
    }
}