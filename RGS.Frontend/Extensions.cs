using System;
using Fluxor;
using R3;

namespace RGS.Frontend;

public static class Extensions
{
  public static Observable<T1> SelectItem1<T1, T2>(this Observable<(T1, T2)> @this)
  {
    return @this.Select(pair => pair.Item1);
  }

  public static Observable<T2> SelectItem2<T1, T2>(this Observable<(T1, T2)> @this)
  {
    return @this.Select(pair => pair.Item2);
  }

  public static IEnumerable<(T, int)> WithIndex<T>(this IEnumerable<T> @this) => @this.Select((e, i) => (e, i));

  public static IEnumerable<T> ReplaceAt<T>(this IEnumerable<T> @this, int index, Func<T, T> transform) => @this.Select((e, i) => i == index ? transform(e) : e);
  public static IEnumerable<T> RemoveAt<T>(this IEnumerable<T> @this, int index) => @this.Where((e, i) => i != index);

  public static Observable<TValue> ValueChanges<TState, TValue>(this IStateSelection<TState, TValue> @this) => Observable.FromEventHandler<TValue>(a => @this.SelectedValueChanged += a, a => @this.SelectedValueChanged -= a).Select(args => args.e);
}