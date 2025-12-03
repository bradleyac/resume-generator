using System;
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
}
