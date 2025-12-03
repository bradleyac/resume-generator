using System;
using System.ComponentModel.DataAnnotations;

namespace RGS.Backend.Shared.ViewModels;

public class BindableString
{
  [Required]
  public string Value { get; set; } = "";
}
