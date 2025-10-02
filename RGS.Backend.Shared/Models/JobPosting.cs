using System.Text.Json.Serialization;

namespace RGS.Backend.Shared.Models;

public record JobPosting(string id, string PostingText, DateTime ImportedAt);

public record CompletedPosting(string id, string PostingText, DateTime ImportedAt, string ResumeUrl);