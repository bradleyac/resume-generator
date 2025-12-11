using System.Drawing.Text;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using RGS.Backend.Shared.Models;

var connectionString = "my_connection_string";

var cosmosClient = new CosmosClient(connectionString);

var container = cosmosClient.GetContainer("Resumes", "ResumeData");

var iterator = container.GetItemLinqQueryable<ResumeDataOld>().ToFeedIterator();
while (iterator.HasMoreResults)
{
  var response = await iterator.ReadNextAsync();
  bool once = false;
  foreach (var item in response.Where(i => i?.Name != null))
  {
    if (once) return;
    Console.WriteLine(item.id);
    var newItem = new ResumeData(item.id, item.UserId, item.IsMaster, new Bio(item.Name, item.Title, item.About, item.StreetAddress ?? "74 E Charlton Rd", item.City, item.State, item.Zip ?? "01562"), item.Contact, item.Jobs, item.Projects, item.Education, item.Skills, item.Bookshelf, item.GeneratedRankings, item.CoverLetter);
    container.UpsertItemAsync(newItem, new PartitionKey(newItem.id)).GetAwaiter().GetResult();
  }
}

record ResumeDataOld(string id,
                         string UserId,
                         bool IsMaster,
                         string Name, string Title, string About, string StreetAddress, string City, string State, string Zip,
                         Contact Contact,
                         Job[] Jobs,
                         Project[] Projects,
                         Education[] Education,
                         SkillCategory[] Skills,
                         Book[] Bookshelf,
                         Rankings? GeneratedRankings = null,
                         string? CoverLetter = null);