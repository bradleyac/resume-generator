using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using RGS.Backend.Middleware;
using RGS.Backend.Shared.Models;

namespace RGS.Backend.Services;

internal interface IUserService
{
  string? GetCurrentUserId();
  Task<UserModel?> GetUserByApiKeyAsync(string apiKey);
  Task<UserModel?> GetUserByIdAsync(string userId);
}

internal class UserService(CosmosClient cosmosClient, FunctionContextAccessor functionContextAccessor) : IUserService
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
                              .Where(u => u.id == userId)
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

internal class DevelopmentUserService : IUserService
{
  public string? GetCurrentUserId()
  {
    return "13b1c25378654837956349833d60216e";
  }

  public Task<UserModel?> GetUserByApiKeyAsync(string apiKey) => Task.FromResult<UserModel?>(new UserModel { id = "13b1c25378654837956349833d60216e", ApiKey = apiKey, UserDetails = "andrew.charles.bradley@gmail.com" });

  public Task<UserModel?> GetUserByIdAsync(string userId) => Task.FromResult<UserModel?>(new UserModel { id = userId, ApiKey = "13b1c25378654837956349833d60216e", UserDetails = "andrew.charles.bradley@gmail.com" });
}