using System;
using System.Diagnostics;
using System.Text.Json;
using Azure.AI.OpenAI;
using Azure.AI.OpenAI.Chat;
using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Newtonsoft.Json.Schema.Generation;
using OpenAI.Chat;
using RGS.Backend.Shared;
using RGS.Backend.Shared.Models;

namespace container.Services;

public class PostingProcessor(ILogger<PostingProcessor> logger, CosmosClient cosmosClient, AzureOpenAIClient aiClient)
{
  private const int LineLength = 85;
  private const int MaxLines = 25;
  private static readonly string PageUrl;
  private static readonly string ResumeDatabaseUrl;
  private readonly ILogger<PostingProcessor> _logger = logger;
  private readonly CosmosClient _cosmosClient = cosmosClient;
  private readonly AzureOpenAIClient _aiClient = aiClient;

  static PostingProcessor()
  {
    // TODO: Do this at startup and inject config instead of using static constructor.
    //       Also find any other places this happens and fix those. 
    var swaHostUrl = Environment.GetEnvironmentVariable("SWA_HOST");
    PageUrl = $"{swaHostUrl}" ?? throw new ArgumentException("SWA_HOST not configured");
    ResumeDatabaseUrl = Environment.GetEnvironmentVariable("RESUME_BLOB_CONTAINER_URL") ?? throw new ArgumentException("RESUME_BLOB_CONTAINER_URL not configured");
  }

  public async Task CatchUp()
  {
    _logger.LogInformation("Catching up...");
    var postings = _cosmosClient.GetContainer("Resumes", "Postings");
    var pendingPostings = postings.GetItemLinqQueryable<JobPosting>().Where(p => p.Status == PostingStatus.Pending).ToFeedIterator();

    int processedCount = 0;

    while (pendingPostings.HasMoreResults)
    {
      foreach (var posting in await pendingPostings.ReadNextAsync())
      {
        _logger.LogInformation($"Processing posting {posting.id}");
        await ProcessPendingPosting(posting);
        ++processedCount;
      }
    }

    _logger.LogInformation($"Catch up complete! Processed {processedCount} postings.");
  }

  public async Task ProcessPendingPosting(JobPosting posting)
  {
    var postings = _cosmosClient.GetContainer("Resumes", "Postings");
    var resumeDataContainer = _cosmosClient.GetContainer("Resumes", "ResumeData");
    var resumeData = await GenerateResumeDataAsync(posting, resumeDataContainer, _aiClient);
    var coverLetter = await GenerateCoverLetterAsync(posting, resumeData, _aiClient);
    resumeData = resumeData with { CoverLetter = coverLetter };
    await resumeDataContainer.UpsertItemAsync(resumeData);
    using var resumeStream = await GeneratePDFStreamAsync(GetResumeUrl(posting.id));
    var resumeUrl = await SaveToBlobStorageAsync(resumeStream, "resumes", $"{posting.id}.pdf");
    using var coverLetterStream = await GeneratePDFStreamAsync(GetCoverLetterUrl(posting.id));
    var coverLetterUrl = await SaveToBlobStorageAsync(coverLetterStream, "coverletters", $"{posting.id}.pdf");
    await postings.UpsertItemAsync(posting with { ResumeUrl = resumeUrl, CoverLetterUrl = coverLetterUrl, Status = PostingStatus.Ready });
  }

  private async Task<ResumeData> GenerateResumeDataAsync(JobPosting posting, Container resumeDataContainer, AzureOpenAIClient aiClient)
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
        new SystemChatMessage("You are a discerning technical recruiter helping the user construct their resume for a particular software developer position."),
            new UserChatMessage("This is the job description: " + posting.PostingText),
            new UserChatMessage("These are my resume bullets: " + JsonSerializer.Serialize(bullets)),
            new UserChatMessage("Read the job description and assign a score between 0 and 10 to each bullet according to how appropriate it would be to appear on a resume for the job description. Return the scores associated by id."),
        ];
    ChatClient chatClient = aiClient.GetChatClient("gpt-5-mini");
    var response = chatClient.CompleteChat(messages, requestOptions);

    var rankings = JsonSerializer.Deserialize<Rankings>(response.Value.Content[0].Text);
    var idToLengthWeightMap = bullets.ToDictionary(b => (b.id, b.jobid), b => b.bulletText.Length / LineLength + 1);

    var bestOfEach = rankings.wts.GroupBy(wt => wt.jobid).SelectMany(g => g.OrderByDescending(wt => wt.wt).Take(4));

    Ranking[] toInclude = [.. bestOfEach, .. rankings.wts.Except(bestOfEach).OrderByDescending(wt => wt.wt)];

    var pruned = toInclude
        .Zip(toInclude.Select(bullet => idToLengthWeightMap[(bullet.id, bullet.jobid)])
            .Scan((a, b) => a + b))
        .TakeWhile(rankingAndLines => rankingAndLines.Second <= MaxLines)
        .Select(rankingAndLines => rankingAndLines.First)
        .ToArray();

    return masterResumeData with
    {
      id = posting.id,
      Jobs = masterResumeData.Jobs.Select((job, jobid) => job with
      {
        Bullets = job.Bullets.Where((text, id) => pruned.Select(b => (b.id, b.jobid)).Contains((id, jobid))).ToArray()
      }).ToArray(),
      GeneratedRankings = rankings
    };
  }

  private async Task<string> GenerateCoverLetterAsync(JobPosting posting, ResumeData resumeData, AzureOpenAIClient aiClient)
  {
    var requestOptions = new ChatCompletionOptions()
    {
      MaxOutputTokenCount = 10000,
    };

#pragma warning disable AOAI001
    requestOptions.SetNewMaxCompletionTokensPropertyEnabled(true);
#pragma warning restore AOAI001

    List<ChatMessage> messages = [
        new SystemChatMessage("You are a discerning technical recruiter helping the user construct write a cover letter for a particular software developer position."),
            new UserChatMessage("This is the information from my resume: " + JsonSerializer.Serialize(resumeData)),
            new UserChatMessage("This is the job description: " + posting.PostingText),
            new UserChatMessage("Write a one page cover letter for the position, emphasizing why I am excited to take on the role and how my background makes me a good fit. Start with 'Dear Hiring Manager,' and end after the final paragraph without a signature. Separate paragraphs with a single '|' character."),
        ];
    ChatClient chatClient = aiClient.GetChatClient("gpt-5-mini");
    var response = chatClient.CompleteChat(messages, requestOptions);

    return response.Value.Content[0].Text;
  }

  private static string GetResumeUrl(string postingId) => $"{PageUrl}/resume/{postingId}";
  private static string GetCoverLetterUrl(string postingId) => $"{PageUrl}/cover/{postingId}";

  private static async Task<Stream> GeneratePDFStreamAsync(string fullUrl)
  {
    using var playwright = await Playwright.CreateAsync();
    await using var browser = await playwright.Chromium.LaunchAsync();
    var page = await browser.NewPageAsync();
    await page.GotoAsync(fullUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });
    await page.WaitForSelectorAsync(".loaded", new() { State = WaitForSelectorState.Visible });
    return new MemoryStream(await page.PdfAsync(new PagePdfOptions { PrintBackground = true }));
  }

  private static async Task<string> SaveToBlobStorageAsync(Stream pdfStream, string containerName, string blobName)
  {
    var connectionString = Environment.GetEnvironmentVariable("RESUME_BLOB_CONTAINER_CONNECTION_STRING");
    var client = new BlobContainerClient(connectionString, containerName);
    _ = await client.UploadBlobAsync(blobName, pdfStream);
    return $"{ResumeDatabaseUrl}/{containerName}/{blobName}";
  }
}
