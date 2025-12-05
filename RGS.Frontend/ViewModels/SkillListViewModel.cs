using System.ComponentModel.DataAnnotations;
using R3;
using RGS.Backend.Shared.Models;

namespace RGS.Frontend.ViewModels;

public class SkillListViewModel : IDisposable
{
  public ReadOnlyReactiveProperty<SkillCategory?> Category { get; init; }
  [Required]
  [StringLength(10, MinimumLength = 2)]
  public BindableReactiveProperty<string> Label { get; }
  public BindableReactiveProperty<string[]> Items { get; }
  private CompositeDisposable _subscription = new();
  private bool _disposedValue;

  public SkillListViewModel(SkillCategory category)
  {
    Label = new BindableReactiveProperty<string>(category.Label).EnableValidation().AddTo(_subscription);
    Items = new BindableReactiveProperty<string[]>(category.Items).EnableValidation().AddTo(_subscription);
    Category = Label.CombineLatest<string, string[], SkillCategory?>(Items, (label, items) => new SkillCategory(label, items)).Skip(1).ToReadOnlyReactiveProperty(null).AddTo(_subscription);
  }

  protected virtual void Dispose(bool disposing)
  {
    if (!_disposedValue)
    {
      if (disposing)
      {
        _subscription?.Dispose();
      }

      _disposedValue = true;
    }
  }

  public void Dispose()
  {
    // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    Dispose(disposing: true);
    GC.SuppressFinalize(this);
  }
}
