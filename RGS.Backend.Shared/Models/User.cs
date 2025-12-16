using System.Text.Json.Serialization;

namespace RGS.Backend.Shared.Models;

public record User(string id, string UserId, string UserDetails, string ApiKey, string SourceResumeDataId) : UserDataRecord(id, UserId);