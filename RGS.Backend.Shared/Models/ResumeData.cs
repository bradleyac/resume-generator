namespace RGS.Backend.Shared.Models;

public record Job(string Title, string Company, string Location, string Start, string End, string[] Bullets);

public record Project(string Name, string Description, string[] Technologies, string When);

public record Education(string Degree, string School, string Location, string Graduation);

public record Contact(string Email, string Phone, string Github);

public record SkillCategory(string Label, string[] Items);

public record Book(string Title, string Author);

public record ResumeData(string id,
                         string UserId,
                         string Name,
                         string Title,
                         string About,
                         string StreetAddress,
                         string City,
                         string State,
                         string Zip,
                         Contact Contact,
                         Job[] Jobs,
                         Project[] Projects,
                         Education[] Education,
                         SkillCategory[] Skills,
                         Book[] Bookshelf,
                         Rankings? GeneratedRankings = null,
                         string? CoverLetter = null);

public class ResumeDataModel
{
  public required string id { get; set; }
  public required string UserId { get; set; }
  public required string Name { get; set; }
  public required string Title { get; set; }
  public required string About { get; set; }
  public required string StreetAddress { get; set; }
  public required string City { get; set; }
  public required string State { get; set; }
  public required string Zip { get; set; }
  public required Contact Contact { get; set; }
  public required Job[] Jobs { get; set; }
  public required Project[] Projects { get; set; }
  public required Education[] Education { get; set; }
  public required SkillCategory[] Skills { get; set; }
  public required Book[] Bookshelf { get; set; }
}