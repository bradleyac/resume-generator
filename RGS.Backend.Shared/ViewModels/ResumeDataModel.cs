using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using RGS.Backend.Shared.Models;

namespace RGS.Backend.Shared.ViewModels;

public class JobModel
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

  public Job ValidatedUnwrap()
  {
    Validator.ValidateObject(this, new ValidationContext(this));
    return new Job(Title!, Company!, Location!, Start!, End!, [.. Bullets.Select(b => b.Value!)]);
  }
}

public class ProjectModel
{
  [Required]
  public string? Name { get; set; }
  [Required]
  public string? Description { get; set; }
  [Required]
  public List<BindableString> Technologies { get; set; } = [];
  [Required]
  public string? When { get; set; }
  public Project ValidatedUnwrap()
  {
    Validator.ValidateObject(this, new ValidationContext(this));
    return new Project(Name!, Description!, [.. Technologies.Select(t => t.Value!)], When!);
  }
}

public class EducationModel
{
  [Required]
  public string? Degree { get; set; }
  [Required]
  public string? School { get; set; }
  [Required]
  public string? Location { get; set; }
  [Required]
  public string? Graduation { get; set; }
  public Education ValidatedUnwrap()
  {
    Validator.ValidateObject(this, new ValidationContext(this));
    return new Education(Degree!, School!, Location!, Graduation!);
  }
}

public class ContactModel
{
  [Required]
  public string? Email { get; set; }
  [Required]
  public string? Phone { get; set; }
  [Required]
  public string? Github { get; set; }
  public Contact ValidatedUnrawp()
  {
    Validator.ValidateObject(this, new ValidationContext(this));
    return new Contact(Email!, Phone!, Github!);
  }
}

public class SkillCategoryModel
{
  [Required]
  public string? Label { get; set; }
  [Required]
  public List<BindableString> Items { get; set; } = [];
  public SkillCategory ValidatedUnwrap()
  {
    Validator.ValidateObject(this, new ValidationContext(this));
    return new SkillCategory(Label!, [.. Items.Select(i => i.Value!)]);
  }
}

public class BookModel
{
  [Required]
  public string? Title { get; set; }
  [Required]
  public string? Author { get; set; }
  public Book ValidatedUnwrap()
  {
    Validator.ValidateObject(this, new ValidationContext(this));
    return new Book(Title!, Author!);
  }
}

public class ResumeDataModel
{
  [Required]
  public string? id { get; set; }
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
  public string? CoverLetter { get; set; }
  [Required]
  public ContactModel? Contact { get; set; }
  [Required]
  public List<JobModel> Jobs { get; set; } = [];
  [Required]
  public List<ProjectModel> Projects { get; set; } = [];
  [Required]
  public List<EducationModel> Education { get; set; } = [];
  [Required]
  public List<SkillCategoryModel> Skills { get; set; } = [];
  [Required]
  public List<BookModel> Bookshelf { get; set; } = [];
  public ResumeData ValidatedUnwrap(string userId)
  {
    Validator.ValidateObject(this, new ValidationContext(this));
    return new ResumeData
    (
      id!,
      userId,
      IsMaster,
      Name!,
      Title!,
      About!,
      StreetAddress!,
      City!,
      State!,
      Zip!,
      Contact!.ValidatedUnrawp(),
      [.. Jobs.Select(j => j.ValidatedUnwrap())],
      [.. Projects.Select(p => p.ValidatedUnwrap())],
      [.. Education.Select(e => e.ValidatedUnwrap())],
      [.. Skills.Select(s => s.ValidatedUnwrap())],
      [.. Bookshelf.Select(b => b.ValidatedUnwrap())],
      CoverLetter: CoverLetter
    );
  }
}