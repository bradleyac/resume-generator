using RGS.Backend.Shared.ViewModels;

namespace RGS.Backend.Shared.Models;

public interface IWrappable<TWrapped>
{
  TWrapped Wrap();
}

public interface IUnwrappable<TUnwrapped>
{
  TUnwrapped Unwrap();
}

public record Job(string Title, string Company, string Location, string Start, string End, string[] Bullets) : IWrappable<JobModel>
{
  public JobModel Wrap() => new JobModel
  {
    Company = Company,
    Location = Location,
    Title = Title,
    Start = Start,
    End = End,
    Bullets = [.. Bullets.Select(b => new BindableString { Value = b })],
  };
};

public record Project(string Name, string Description, string[] Technologies, string When) : IWrappable<ProjectModel>
{
  public ProjectModel Wrap() => new ProjectModel
  {
    Description = Description,
    Name = Name,
    Technologies = [.. Technologies.Select(t => new BindableString { Value = t })],
    When = When
  };
};

public record Education(string Degree, string School, string Location, string Graduation) : IWrappable<EducationModel>
{
  public EducationModel Wrap() => new EducationModel
  {
    Degree = Degree,
    Graduation = Graduation,
    Location = Location,
    School = School
  };
};

public record Contact(string Email, string Phone, string Github) : IWrappable<ContactModel>
{
  public ContactModel Wrap() => new ContactModel
  {
    Email = Email,
    Github = Github,
    Phone = Phone
  };
};

public record SkillCategory(string Label, string[] Items) : IWrappable<SkillCategoryModel>
{
  public SkillCategoryModel Wrap() => new SkillCategoryModel
  {
    Items = [.. Items.Select(i => new BindableString { Value = i })],
    Label = Label
  };
};

public record Book(string Title, string Author) : IWrappable<BookModel>
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
                         string? CoverLetter = null) : IWrappable<ResumeDataModel>
{
  public ResumeDataModel Wrap() => new ResumeDataModel
  {
    id = id,
    UserId = UserId,
    IsMaster = IsMaster,
    Name = Name,
    Title = Title,
    About = About,
    StreetAddress = StreetAddress,
    City = City,
    State = State,
    Zip = Zip,
    Contact = Contact,
    Jobs = Jobs.ToList(),
    Projects = Projects.ToList(),
    Education = Education.ToList(),
    Skills = Skills.ToList(),
    Bookshelf = Bookshelf.ToList(),
  };
}