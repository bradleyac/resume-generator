using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using RGS.Backend.Shared.Models;

namespace RGS.Backend.Shared.ViewModels;

public class JobModel : IUnwrappable<Job>
{
  [Required]
  public string? Title { get; set; }
  [Required]
  public string? Company { get; set; }
  [Required]
  public string? Location { get; set; }
  [Required]
  public string? Start { get; set; }
  [Required]
  public string? End { get; set; }
  [Required]
  public List<BindableString> Bullets { get; set; } = [];

  public Job Unwrap()
  {
    Validator.ValidateObject(this, new ValidationContext(this));
    return new Job(Title!, Company!, Location!, Start!, End!, [.. Bullets.Select(b => b.Value!)]);
  }
}

public class ProjectModel : IUnwrappable<Project>
{
  [Required]
  public string? Name { get; set; }
  [Required]
  public string? Description { get; set; }
  [Required]
  public List<BindableString> Technologies { get; set; } = [];
  [Required]
  public string? When { get; set; }
  public Project Unwrap()
  {
    Validator.ValidateObject(this, new ValidationContext(this));
    return new Project(Name!, Description!, [.. Technologies.Select(t => t.Value!)], When!);
  }
}

public class EducationModel : IUnwrappable<Education>
{
  [Required]
  public string? Degree { get; set; }
  [Required]
  public string? School { get; set; }
  [Required]
  public string? Location { get; set; }
  [Required]
  public string? Graduation { get; set; }
  public Education Unwrap()
  {
    Validator.ValidateObject(this, new ValidationContext(this));
    return new Education(Degree!, School!, Location!, Graduation!);
  }
}

public class ContactModel : IUnwrappable<Contact>
{
  [Required]
  public string? Email { get; set; }
  [Required]
  public string? Phone { get; set; }
  [Required]
  public string? Github { get; set; }
  public Contact Unwrap()
  {
    Validator.ValidateObject(this, new ValidationContext(this));
    return new Contact(Email!, Phone!, Github!);
  }
}

public class SkillCategoryModel : IUnwrappable<SkillCategory>
{
  [Required]
  public string? Label { get; set; }
  [Required]
  public List<BindableString> Items { get; set; } = [];
  public SkillCategory Unwrap()
  {
    Validator.ValidateObject(this, new ValidationContext(this));
    return new SkillCategory(Label!, [.. Items.Select(i => i.Value!)]);
  }
}

public class BookModel : IUnwrappable<Book>
{
  [Required]
  public string? Title { get; set; }
  [Required]
  public string? Author { get; set; }
  public Book Unwrap()
  {
    Validator.ValidateObject(this, new ValidationContext(this));
    return new Book(Title!, Author!);
  }
}

public class ResumeDataModel : IUnwrappable<ResumeData>
{
  [Required]
  public string? id { get; set; }
  [Required]
  public string? UserId { get; set; }
  [Required]
  public bool IsMaster { get; set; }
  [Required]
  public string? Name { get; set; }
  [Required]
  public string? Title { get; set; }
  [Required]
  public string? About { get; set; }
  [Required]
  public string? StreetAddress { get; set; }
  [Required]
  public string? City { get; set; }
  [Required]
  public string? State { get; set; }
  [Required]
  public string? Zip { get; set; }
  [Required]
  public Contact? Contact { get; set; }
  [Required]
  public List<Job> Jobs { get; set; } = [];
  [Required]
  public List<Project> Projects { get; set; } = [];
  [Required]
  public List<Education> Education { get; set; } = [];
  [Required]
  public List<SkillCategory> Skills { get; set; } = [];
  [Required]
  public List<Book> Bookshelf { get; set; } = [];
  public ResumeData Unwrap()
  {
    Validator.ValidateObject(this, new ValidationContext(this));
    return new ResumeData
    (
      id!,
      UserId!,
      IsMaster,
      Name!,
      Title!,
      About!,
      StreetAddress!,
      City!,
      State!,
      Zip!,
      Contact!,
      Jobs.ToArray(),
      Projects.ToArray(),
      Education.ToArray(),
      Skills.ToArray(),
      Bookshelf.ToArray()
    );
  }
}