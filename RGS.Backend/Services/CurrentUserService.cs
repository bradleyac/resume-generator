using RGS.Backend.Middleware;

namespace RGS.Backend.Services;

internal interface ICurrentUserService
{
  string? GetCurrentUserId();
}

internal class CurrentUserService(FunctionContextAccessor functionContextAccessor) : ICurrentUserService
{
  private readonly FunctionContextAccessor _functionContextAccessor = functionContextAccessor;

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

internal class DevelopmentCurrentUserService : ICurrentUserService
{
  public string? GetCurrentUserId() => "13b1c25378654837956349833d60216e";
}