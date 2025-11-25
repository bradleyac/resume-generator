using System.ComponentModel.DataAnnotations;
using System.Runtime.Versioning;
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

public class UpdatePostingAddressModel
{
  [Required]
  public string PostingId { get; set; } = string.Empty;

  [Required]
  public string StreetAddress { get; set; } = string.Empty;

  [Required]
  public string City { get; set; } = string.Empty;

  [Required]
  public string State { get; set; } = string.Empty;

  [Required]
  public string Zip { get; set; } = string.Empty;
}

public class RegenerateCoverLetterModel
{
  [Required]
  public string PostingId { get; set; } = string.Empty;

  public string? AdditionalContext { get; set; }
}

public record JobPosting(string id, string UserId, string Link, string Company, string Title, string PostingText, DateTime ImportedAt, string? StreetAddress = null, string? City = null, string? State = null, string? Zip = null, string Status = PostingStatus.Pending);

public record PostingSummary(string id, string Link, string Company, string Title, DateTime ImportedAt, string Status);

public static class PostingStatus
{
  public static string[] ValidStatuses = new[] { Pending, Ready, Applied, Archived };
  public const string Pending = "Pending";
  public const string Ready = "Ready";
  public const string Applied = "Applied";
  public const string Archived = "Archived";
}

public record PostingStatusUpdate(string PostingId, string UserId, string NewStatus);
