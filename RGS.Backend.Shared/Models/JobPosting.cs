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

public record PostingDetails(string Link, string Company, string Title, string PostingText, string? StreetAddress = null, string? City = null, string? State = null, string? Zip = null);

public record Notes(string PostingId, string UserId, string Text);

public record JobPosting(string id, string UserId, DateTime ImportedAt, PostingDetails PostingData, ResumeData? ResumeData = null, CoverLetter? CoverLetter = null, Notes? Notes = null, string Status = PostingStatus.Pending) : UserDataRecord(id, UserId);

public record PostingSummary(string id, string Link, string Company, string Title, DateTime ImportedAt, string Status);

public static class PostingStatus
{
  public static string[] ValidStatuses = new[] { Pending, Ready, Applied, Replied, Archived };
  public const string Pending = "Pending";
  public const string Ready = "Ready";
  public const string Applied = "Applied";
  public const string Replied = "Replied";
  public const string Archived = "Archived";
}

public record PostingStatusUpdate(string PostingId, string NewStatus);

// Polymorphic base type for user data records stored in Cosmos DB.
// Use with care, because you have to write the base type when updating/inserting.
// TODO: Find a better way to do polymorphic serialization with CosmosDB.
[JsonPolymorphic]
[JsonDerivedType(typeof(JobPosting), "JobPosting")]
[JsonDerivedType(typeof(User), "User")]
[JsonDerivedType(typeof(SourceResumeData), "SourceResumeData")]
public record UserDataRecord(string id, string UserId);