using System.Net;
using Grpc.Core;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using RGS.Backend.Middleware;
using RGS.Backend.Shared.Models;
using User = RGS.Backend.Shared.Models.User;

namespace RGS.Backend.Services;

internal interface IUserService
{
  Task<User?> GetUserByApiKeyAsync(string apiKey);
  Task<User?> GetUserByIdAsync(string userId);
}

internal class UserService(CosmosClient cosmosClient) : IUserService
{
  private readonly CosmosClient _cosmosClient = cosmosClient;

  public async Task<User?> GetUserByApiKeyAsync(string apiKey)
  {
    var usersContainer = _cosmosClient.GetContainer("Resumes", "UserData");

    var query = usersContainer.GetItemLinqQueryable<User>()
                              .Where(u => u.ApiKey == apiKey)
                              .Take(1)
                              .ToFeedIterator();

    var results = await query.ReadNextAsync();
    return results.FirstOrDefault();
  }

  public async Task<User?> GetUserByIdAsync(string userId)
  {
    var usersContainer = _cosmosClient.GetContainer("Resumes", "UserData");

    var result = await usersContainer.ReadItemAsync<User>(userId, new PartitionKey(userId));

    return result.StatusCode switch
    {
      HttpStatusCode.OK => result.Resource,
      _ => null,
    };
  }
}