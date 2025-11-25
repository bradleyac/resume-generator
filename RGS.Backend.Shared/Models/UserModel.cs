using System.Text.Json.Serialization;

namespace RGS.Backend.Shared.Models;

public class UserModel
{
  [JsonPropertyName("id")]
  public required string UserId { get; set; }
  public required string UserDetails { get; set; }
  public required string ApiKey { get; set; }
}