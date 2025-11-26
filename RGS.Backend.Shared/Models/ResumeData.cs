using RGS.Backend.Shared.ViewModels;

namespace RGS.Backend.Shared.Models;

public record Job(string Title, string Company, string Location, string Start, string End, string[] Bullets)
{
  public JobModel Wrap() => new JobModel
  {
    Company = Company,
    Location = Location,
    Title = Title,
    Start = Start,
    End = End,
    Bullets = [.. Bullets],
  };
};

public record Project(string Name, string Description, string[] Technologies, string When)
{
  public ProjectModel Wrap() => new ProjectModel
  {
    Description = Description,
    Name = Name,
    Technologies = [.. Technologies],
    When = When
  };
};

public record Education(string Degree, string School, string Location, string Graduation)
{
  public EducationModel Wrap() => new EducationModel
  {
    Degree = Degree,
    Graduation = Graduation,
    Location = Location,
    School = School
  };
};

public record Contact(string Email, string Phone, string Github)
{
  public ContactModel Wrap() => new ContactModel
  {
    Email = Email,
    Github = Github,
    Phone = Phone
  };
};

public record SkillCategory(string Label, string[] Items)
{
  public SkillCategoryModel Wrap() => new SkillCategoryModel
  {
    Items = [.. Items],
    Label = Label
  };
};

public record Book(string Title, string Author)
{
  public BookModel Wrap() => new BookModel
  {
    Author = Author,
    Title = Title
  };
}

public record ResumeData(string id,
                         string UserId,
                         bool IsMaster,
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
                         string? CoverLetter = null)
{
  public ResumeDataModel Wrap() => new ResumeDataModel
  {
    id = id,
    IsMaster = IsMaster,
    Name = Name,
    Title = Title,
    About = About,
    StreetAddress = StreetAddress,
    City = City,
    State = State,
    Zip = Zip,
    Contact = Contact.Wrap(),
    Jobs = [.. Jobs.Select(j => j.Wrap())],
    Projects = [.. Projects.Select(p => p.Wrap())],
    Education = [.. Education.Select(e => e.Wrap())],
    Skills = [.. Skills.Select(s => s.Wrap())],
    Bookshelf = [.. Bookshelf.Select(b => b.Wrap())]
  };
}