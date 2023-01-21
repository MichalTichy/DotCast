﻿namespace DotCast.PodcastProvider.Postgre
{
    public static class AsyncEnumeratorExtensions
    {
        public static async Task<ICollection<T>> ToListAsync<T>(this IAsyncEnumerable<T> data)
        {
            var result = new List<T>();
            await foreach (var item in data)
            {
                result.Add(item);
            }

            return result;
        }
    }
}