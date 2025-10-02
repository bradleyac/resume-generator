using System;
using Microsoft.Azure.Cosmos;

namespace RGS.Backend.Shared;

public static class Extensions
{
  public static async Task<IEnumerable<T>> ToListAsync<T>(this FeedIterator<T> @this)
  {
    List<T> results = [];

    while (@this.HasMoreResults)
    {
      var next = await @this.ReadNextAsync();
      results.AddRange(next);
    }

    return results;
  }
}
