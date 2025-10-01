namespace RGS.Backend.Shared.Models;

public record JobPosting(string id, string PostingText);

public record CompletedPosting(string id, string PostingText, string ResumeUrl);