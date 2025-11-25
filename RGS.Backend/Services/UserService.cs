using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using RGS.Backend.Middleware;
using RGS.Backend.Shared.Models;

namespace RGS.Backend.Services;

internal class UserService(CosmosClient cosmosClient, FunctionContextAccessor functionContextAccessor)
{
  private readonly CosmosClient _cosmosClient = cosmosClient;
  private readonly FunctionContextAccessor _functionContextAccessor = functionContextAccessor;

  public async Task<UserModel?> GetUserByApiKeyAsync(string apiKey)
  {
    var usersContainer = _cosmosClient.GetContainer("Resumes", "Users");

    var query = usersContainer.GetItemLinqQueryable<UserModel>()
                              .Where(u => u.ApiKey == apiKey)
                              .Take(1)
                              .ToFeedIterator();

    var results = await query.ReadNextAsync();
    return results.FirstOrDefault();
  }

  public async Task<UserModel?> GetUserByIdAsync(string userId)
  {
    var usersContainer = _cosmosClient.GetContainer("Resumes", "Users");

    var query = usersContainer.GetItemLinqQueryable<UserModel>()
                              .Where(u => u.UserId == userId)
                              .Take(1)
                              .ToFeedIterator();

    var results = await query.ReadNextAsync();
    return results.FirstOrDefault();
  }

  public string? GetCurrentUserId()
  {
    if (_functionContextAccessor.Current?.Items.TryGetValue("User", out var userObj) ?? false)
    {
      return (userObj as EasyAuthUser)?.UserId;
    }
    else
    {
      return null;
    }
  }
}