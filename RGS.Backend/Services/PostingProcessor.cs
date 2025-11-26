using System;
using System.Diagnostics;
using System.Text.Json;
using Azure.AI.OpenAI;
using Azure.AI.OpenAI.Chat;
using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Schema.Generation;
using OpenAI.Chat;
using RGS.Backend.Shared;
using RGS.Backend.Shared.Models;

namespace RGS.Backend.Services;

internal class PostingProcessor(ILogger<PostingProcessor> logger, CosmosClient cosmosClient, AzureOpenAIClient aiClient)
{
  private const int LineLength = 85;
  private const int MaxLines = 25;

  private readonly ILogger<PostingProcessor> _logger = logger;
  private readonly CosmosClient _cosmosClient = cosmosClient;
  private readonly AzureOpenAIClient _aiClient = aiClient;

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
    await postings.UpsertItemAsync(posting with { Status = PostingStatus.Ready });
  }

  public async Task RegenerateCoverLetterAsync(RegenerateCoverLetterModel model)
  {
    var postings = _cosmosClient.GetContainer("Resumes", "Postings");
    var resumeDataContainer = _cosmosClient.GetContainer("Resumes", "ResumeData");

    var postingResponse = await postings.ReadItemAsync<JobPosting>(model.PostingId, new PartitionKey(model.PostingId));

    if (postingResponse.StatusCode != System.Net.HttpStatusCode.OK || postingResponse.Resource is null)
    {
      throw new ArgumentException("Posting not found");
    }

    var posting = postingResponse.Resource;

    var resumeDataResponse = await resumeDataContainer.ReadItemAsync<ResumeData>(posting.id, new PartitionKey(posting.id));

    if (resumeDataResponse.StatusCode != System.Net.HttpStatusCode.OK || resumeDataResponse.Resource is null)
    {
      throw new ArgumentException("Resume data not found");
    }

    var resumeData = resumeDataResponse.Resource;

    var coverLetter = await GenerateCoverLetterAsync(posting, resumeData, _aiClient, model.AdditionalContext);
    resumeData = resumeData with { CoverLetter = coverLetter };
    await resumeDataContainer.UpsertItemAsync(resumeData);
  }

  private async Task<ResumeData> GenerateResumeDataAsync(JobPosting posting, Container resumeDataContainer, AzureOpenAIClient aiClient)
  {
    var masterResumeData = (await resumeDataContainer.ReadItemAsync<ResumeData>(posting.UserId, new PartitionKey(posting.UserId))).Resource;

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
    var response = await chatClient.CompleteChatAsync(messages, requestOptions);

    var rankings = JsonSerializer.Deserialize<Rankings>(response.Value.Content[0].Text) ?? throw new InvalidOperationException("Failed to deserialize rankings");
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

  private async Task<string> GenerateCoverLetterAsync(JobPosting posting, ResumeData resumeData, AzureOpenAIClient aiClient, string? additionalContext = null)
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
            new UserChatMessage("This is the information from my resume: " + JsonSerializer.Serialize(resumeData with { CoverLetter = null })),
            new UserChatMessage("This is the job description: " + posting.PostingText),
            new UserChatMessage("Write a one page cover letter for the position, emphasizing why I am excited to take on the role and how my background makes me a good fit. Start with 'Dear Hiring Manager,' and end after the final paragraph without a signature. Instead of newline characters, separate paragraphs with a single pipe character: |."),
        ];

    if (additionalContext is not null)
    {
      messages.Add(new UserChatMessage("Here is some additional context to consider: " + additionalContext));
    }

    ChatClient chatClient = aiClient.GetChatClient("gpt-5-mini");
    var response = await chatClient.CompleteChatAsync(messages, requestOptions);

    return response.Value.Content[0].Text;
  }
}
