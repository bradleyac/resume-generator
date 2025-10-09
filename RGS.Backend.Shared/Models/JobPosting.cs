using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RGS.Backend.Shared.Models;

public class NewPostingModel
{
  [Required]
  public string Link { get; set; } = string.Empty;

  [Required]
  public string Company { get; set; } = string.Empty;

  [Required]
  public string Title { get; set; } = string.Empty;

  [Required]
  public string PostingText { get; set; } = string.Empty;
}

public record JobPosting(string id, string Link, string Company, string Title, string PostingText, DateTime ImportedAt);

public record CompletedPosting(string id, string Link, string Company, string Title, string PostingText, DateTime ImportedAt, string ResumeUrl, string Status = PostingStatus.Pending);
public record PostingSummary(string id, string Link, string Company, string Title, DateTime ImportedAt, string Status);

public static class PostingStatus
{
  public const string Pending = "Pending";
  public const string Applied = "Applied";
  public const string Archived = "Archived";
}

public record PostingStatusUpdate(string PostingId, string NewStatus);