using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Runtime.ConstrainedExecution;
using System.Text.Json;
using Azure.AI.OpenAI;
using Azure.AI.OpenAI.Chat;
using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Newtonsoft.Json.Schema.Generation;
using OpenAI.Chat;
using RGS.Backend.Shared.Models;

namespace RGS.Backend;

public class PendingPostingsTrigger
{
    private const string PageUrl = "https://happy-mushroom-0344c0c0f.2.azurestaticapps.net/resume";
    private readonly ILogger<PendingPostingsTrigger> _logger;
    private readonly CosmosClient _cosmosClient;
    private readonly AzureOpenAIClient _aiClient;

    public PendingPostingsTrigger(ILogger<PendingPostingsTrigger> logger, CosmosClient cosmosClient, AzureOpenAIClient aiClient)
    {
        _logger = logger;
        _cosmosClient = cosmosClient;
        _aiClient = aiClient;
    }

    [Function("PendingPostingsTrigger")]
    public async Task Run([CosmosDBTrigger(
        databaseName: "Resumes",
        containerName: "PendingPostings",
        Connection = "CosmosDBConnectionString",
        LeaseContainerName = "leases",
        CreateLeaseContainerIfNotExists = true)] IReadOnlyList<JobPosting> input)
    {
        var pendingPostings = _cosmosClient.GetContainer("Resumes", "PendingPostings");
        var resumeDataContainer = _cosmosClient.GetContainer("Resumes", "ResumeData");
        var completedPostings = _cosmosClient.GetContainer("Resumes", "CompletedPostings");

        foreach (var posting in input)
        {
            await GenerateResumeDataAsync(posting, resumeDataContainer, _aiClient);
            using var stream = await GeneratePDFStreamAsync(posting.id);
            var resumeUrl = await SaveToBlobStorageAsync(stream);
            await completedPostings.UpsertItemAsync(new CompletedPosting(posting.id, posting.Link, posting.Company, posting.Title, posting.PostingText, posting.ImportedAt, resumeUrl));
            await pendingPostings.DeleteItemAsync<JobPosting>(posting.id, partitionKey: new PartitionKey(posting.id));
        }
    }

    private async Task GenerateResumeDataAsync(JobPosting posting, Container resumeDataContainer, AzureOpenAIClient aiClient)
    {
        var masterResumeData = (await resumeDataContainer.ReadItemAsync<ResumeData>("master", new PartitionKey("master"))).Resource;

        JSchemaGenerator generator = new JSchemaGenerator();
        var jsonSchema = generator.Generate(typeof(Rankings)).ToString();

        var requestOptions = new ChatCompletionOptions()
        {
            MaxOutputTokenCount = 10000,
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat("rankings", BinaryData.FromString(jsonSchema))
        };

#pragma warning disable AOAI001
        requestOptions.SetNewMaxCompletionTokensPropertyEnabled(true);
#pragma warning restore AOAI001

        var bullets = masterResumeData.Jobs.SelectMany((j, jobid) => j.Bullets.Select((e, i) => new Bullet(i, jobid, e)));

        List<ChatMessage> messages = [
            new SystemChatMessage("You are a helpful assistant."),
            new UserChatMessage("This is the job description: " + posting.PostingText),
            new UserChatMessage("These are my resume bullets: " + System.Text.Json.JsonSerializer.Serialize(bullets)),
            new UserChatMessage("Read the job description and assign a score between 0 and 10 to each bullet according to how appropriate it would be to appear on a resume for the job description. Return the scores associated by id."),
        ];
        ChatClient chatClient = aiClient.GetChatClient("gpt-5-mini");
        var response = chatClient.CompleteChat(messages, requestOptions);

        var rankings = JsonSerializer.Deserialize<Rankings>(response.Value.Content[0].Text);

        var bestOfEach = rankings.wts.GroupBy(wt => wt.jobid).SelectMany(g => g.OrderByDescending(wt => wt.wt).Take(4));

        Ranking[] toInclude = [.. bestOfEach, .. rankings.wts.Except(bestOfEach).OrderByDescending(wt => wt.wt).Take(8)];

        var newResumeData = masterResumeData with
        {
            id = posting.id,
            Jobs = masterResumeData.Jobs.Select((j, jobid) => j with
            {
                Bullets = j.Bullets.Where((b, i) => toInclude.Select(b => (b.id, b.jobid)).Contains((i, jobid))).ToArray()
            }).ToArray()
        };

        await resumeDataContainer.UpsertItemAsync(newResumeData);
    }

    private static async Task<Stream> GeneratePDFStreamAsync(string postingId)
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync();
        var page = await browser.NewPageAsync();

        await page.GotoAsync($"{PageUrl}/{postingId}", new() { WaitUntil = WaitUntilState.NetworkIdle });
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