using Microsoft.AspNetCore.Http;

namespace RGS.Backend;

public static class Extensions
{
  public static string Get(this IFormCollection? @this, string key) =>
    (@this is not null && @this.TryGetValue(key, out var value) && value is var val
    ? val.FirstOrDefault() : null) ?? throw new ArgumentException($"{key} missing");
}