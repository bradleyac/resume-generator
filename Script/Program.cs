using System.Drawing.Text;
using System.Text.Json;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using RGS.Backend.Shared.Models;

var connectionString = "";

var cosmosClient = new CosmosClient(connectionString, new CosmosClientOptions { Serializer = new RGS.Backend.CosmosSystemTextJsonSerializer() });

var userData = cosmosClient.GetContainer("Resumes", "UserData");
var postings = cosmosClient.GetContainer("Resumes", "Postings");
var resumeData = cosmosClient.GetContainer("Resumes", "ResumeData");
var users = cosmosClient.GetContainer("Resumes", "Users");

const string masterResumeId = "13b1c25378654837956349833d60216e";
var masterResumeData = await resumeData.ReadItemAsync<ResumeDataOld>(masterResumeId, new PartitionKey(masterResumeId));
var newSourceResumeData = new SourceResumeData(Guid.NewGuid().ToString(), masterResumeData.Resource.UserId, new Bio(masterResumeData.Resource.Name, masterResumeData.Resource.Title, masterResumeData.Resource.About, masterResumeData.Resource.StreetAddress, masterResumeData.Resource.City, masterResumeData.Resource.State, masterResumeData.Resource.Zip), masterResumeData.Resource.Contact, masterResumeData.Resource.Jobs, masterResumeData.Resource.Projects, masterResumeData.Resource.Education, masterResumeData.Resource.Skills, masterResumeData.Resource.Bookshelf);

await userData.UpsertItemAsync<UserDataRecord>(newSourceResumeData, new PartitionKey(newSourceResumeData.UserId));

var iterator = postings.GetItemLinqQueryable<JobPostingOld>().ToFeedIterator();

while (iterator.HasMoreResults)
{
  var response = await iterator.ReadNextAsync();
  bool once = false;
  foreach (var item in response.Where(i => i?.UserId != null))
  {
    if (once) return;
    Console.WriteLine(item.id);
    var oldResumeData = (await resumeData.ReadItemAsync<ResumeDataOld>(item.id, new PartitionKey(item.id))).Resource;
    var oldCoverLetter = oldResumeData.CoverLetter;
    var oldUser = (await users.ReadItemAsync<OldUser>(item.UserId, new PartitionKey(item.UserId))).Resource;
    var newPosting = new JobPosting(item.id, item.UserId, item.ImportedAt, new PostingDetails(item.Link, item.Company, item.Title, item.PostingText, item.StreetAddress, item.City, item.State, item.Zip), oldResumeData == null ? null : new ResumeData(item.id, oldResumeData.UserId, new Bio(oldResumeData.Name, oldResumeData.Title, oldResumeData.About, oldResumeData.StreetAddress, oldResumeData.City, oldResumeData.State, oldResumeData.Zip), oldResumeData.Contact, oldResumeData.Jobs, oldResumeData.Projects, oldResumeData.Education, oldResumeData.Skills, oldResumeData.Bookshelf, oldResumeData.GeneratedRankings), oldCoverLetter is null ? null : new CoverLetter(item.id, item.UserId, oldCoverLetter), item.Status.ToString());
    await userData.UpsertItemAsync<UserDataRecord>(new RGS.Backend.Shared.Models.User(oldUser.id, oldUser.id, oldUser.UserDetails, oldUser.ApiKey, newSourceResumeData.id), new PartitionKey(oldUser.id));
    await userData.UpsertItemAsync<UserDataRecord>(newPosting, new PartitionKey(newPosting.UserId));
    // Console.WriteLine(JsonSerializer.Serialize<UserDataRecord>(newPosting));
  }
}

public record JobPostingOld(string id, string UserId, string Link, string Company, string Title, string PostingText, DateTime ImportedAt, string? StreetAddress = null, string? City = null, string? State = null, string? Zip = null, string Status = PostingStatus.Pending);
public record ResumeDataOld(string id, string UserId, bool IsMaster, string Name, string Title, string About, string StreetAddress, string City, string State, string Zip, Contact Contact, Job[] Jobs, Project[] Projects, Education[] Education, SkillCategory[] Skills, Book[] Bookshelf, Rankings? GeneratedRankings = null, string? CoverLetter = null);
public record OldUser(string id, string UserDetails, string ApiKey);