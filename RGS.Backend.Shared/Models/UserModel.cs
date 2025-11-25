using System.Text.Json.Serialization;

namespace RGS.Backend.Shared.Models;

public class UserModel
{
  public required string id { get; set; }
  public required string UserDetails { get; set; }
  public required string ApiKey { get; set; }
}