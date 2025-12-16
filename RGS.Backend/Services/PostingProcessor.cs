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

internal class PostingProcessor(ILogger<PostingProcessor> logger, CosmosClient cosmosClient, AzureOpenAIClient aiClient, IUserDataRepository userDataRepository)
{
  private const int LineLength = 85;
  private const int MaxLines = 25;

  private readonly ILogger<PostingProcessor> _logger = logger;
  private readonly CosmosClient _cosmosClient = cosmosClient;
  private readonly AzureOpenAIClient _aiClient = aiClient;
  private readonly IUserDataRepository _userDataRepository = userDataRepository;

  public async Task CatchUp()
  {
    _logger.LogInformation("Catching up...");
    var postings = _cosmosClient.GetContainer("Resumes", "UserData");
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
    var userDataContainer = _cosmosClient.GetContainer("Resumes", "UserData");
    var resumeData = await GenerateResumeDataAsync(posting);
    var coverLetter = await GenerateCoverLetterAsync(posting);
    await userDataContainer.UpsertItemAsync(resumeData);
    await userDataContainer.UpsertItemAsync(coverLetter);
    await userDataContainer.UpsertItemAsync(posting with { Status = PostingStatus.Ready });
  }

  public async Task<bool> RegenerateCoverLetterAsync(RegenerateCoverLetterModel model)
  {
    var posting = (await _userDataRepository.GetPostingAsync(model.PostingId)) ?? throw new ArgumentException("Posting not found");

    if (posting.ResumeData is null)
    {
      return false;
    }

    var coverLetter = await GenerateCoverLetterAsync(posting, model.AdditionalContext);

    return await _userDataRepository.SetCoverLetterAsync(posting.id, coverLetter);
  }

  private async Task<ResumeData> GenerateResumeDataAsync(JobPosting posting)
  {
    var sourceResumeData = await _userDataRepository.GetSourceResumeDataAsync() ?? throw new InvalidOperationException("Source resume data not found");

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

    var bullets = sourceResumeData.Jobs.SelectMany((j, jobid) => j.Bullets.Select((e, i) => new Bullet(i, jobid, e)));

    List<ChatMessage> messages = [
        new SystemChatMessage("You are a discerning technical recruiter helping the user construct their resume for a particular software developer position."),
            new UserChatMessage("This is the job description: " + posting.PostingData.PostingText),
            new UserChatMessage("These are my resume bullets: " + JsonSerializer.Serialize(bullets)),
            new UserChatMessage("Read the job description and assign a score between 0 and 10 to each bullet according to how appropriate it would be to appear on a resume for the job description. Return the scores associated by id."),
        ];
    ChatClient chatClient = _aiClient.GetChatClient("gpt-5-mini");
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

    return new ResumeData(
      posting.id,
      posting.UserId,
      sourceResumeData.Bio,
      sourceResumeData.Contact,
      sourceResumeData.Jobs.Select((job, jobid) => job with
      {
        Bullets = job.Bullets.Where((text, id) => pruned.Select(b => (b.id, b.jobid)).Contains((id, jobid))).ToArray()
      }).ToArray(),
      sourceResumeData.Projects,
      sourceResumeData.Education,
      sourceResumeData.Skills,
      sourceResumeData.Bookshelf,
      rankings);
  }

  private async Task<CoverLetter> GenerateCoverLetterAsync(JobPosting posting, string? additionalContext = null)
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
            new UserChatMessage("This is the information from my resume: " + JsonSerializer.Serialize(posting.ResumeData)),
            new UserChatMessage("This is the job description: " + posting.PostingData.PostingText),
            new UserChatMessage("Write a one page cover letter for the position, emphasizing why I am excited to take on the role and how my background makes me a good fit. Start with 'Dear Hiring Manager,' and end after the final paragraph without a signature. Instead of newline characters, separate paragraphs with a single pipe character: |."),
        ];

    if (additionalContext is not null)
    {
      messages.Add(new UserChatMessage("Here is some additional context to consider: " + additionalContext));
    }

    ChatClient chatClient = _aiClient.GetChatClient("gpt-5-mini");
    var response = await chatClient.CompleteChatAsync(messages, requestOptions);

    return new CoverLetter(posting.id, posting.UserId, response.Value.Content[0].Text);
  }
}
