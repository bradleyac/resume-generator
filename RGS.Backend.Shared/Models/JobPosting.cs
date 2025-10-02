using System.Text.Json.Serialization;

namespace RGS.Backend.Shared.Models;

public record JobPosting([property: JsonPropertyName("id")] string Id, string PostingText, DateTime ImportedAt);

public record CompletedPosting([property: JsonPropertyName("id")] string Id, string PostingText, DateTime ImportedAt, string ResumeUrl);