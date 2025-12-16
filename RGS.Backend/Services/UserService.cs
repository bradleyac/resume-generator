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
  string? GetCurrentUserId();
  Task<User?> GetUserByApiKeyAsync(string apiKey);
  Task<User?> GetUserByIdAsync(string userId);
}

internal class UserService(CosmosClient cosmosClient, FunctionContextAccessor functionContextAccessor) : IUserService
{
  private readonly CosmosClient _cosmosClient = cosmosClient;
  private readonly FunctionContextAccessor _functionContextAccessor = functionContextAccessor;

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

internal class DevelopmentUserService : IUserService
{
  private static readonly string DefaultSourceResumeDataId = Guid.NewGuid().ToString();
  public string? GetCurrentUserId()
  {
    return "13b1c25378654837956349833d60216e";
  }

  public Task<User?> GetUserByApiKeyAsync(string apiKey) => Task.FromResult<User?>(new User("13b1c25378654837956349833d60216e", "13b1c25378654837956349833d60216e", apiKey, "andrew.charles.bradley@gmail.com", DefaultSourceResumeDataId));

  public Task<User?> GetUserByIdAsync(string userId) => Task.FromResult<User?>(new User(userId, userId, "13b1c25378654837956349833d60216e", "andrew.charles.bradley@gmail.com", DefaultSourceResumeDataId));
}