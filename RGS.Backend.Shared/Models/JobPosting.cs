using System.Text.Json.Serialization;

namespace RGS.Backend.Shared.Models;

public record JobPosting(string id, string Link, string Company, string Title, string PostingText, DateTime ImportedAt);

public record CompletedPosting(string id, string Link, string Company, string Title, string PostingText, DateTime ImportedAt, string ResumeUrl);