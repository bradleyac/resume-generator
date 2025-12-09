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
  public List<string> Bullets { get; set; } = [];
}

public class ProjectModel
{
  [Required]
  public string? Name { get; set; }
  [Required]
  public string? Description { get; set; }
  [Required]
  public List<string> Technologies { get; set; } = [];
  [Required]
  public string? When { get; set; }
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
}

public class ContactModel
{
  [Required]
  public string? Email { get; set; }
  [Required]
  public string? Phone { get; set; }
  [Required]
  public string? Github { get; set; }
}

public class SkillCategoryModel
{
  [Required]
  public string? Label { get; set; }
  [Required]
  public List<string> Items { get; set; } = [];
}

public class BookModel
{
  [Required]
  public string? Title { get; set; }
  [Required]
  public string? Author { get; set; }
}

public class BioModel
{
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
}