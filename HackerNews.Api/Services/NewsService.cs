using HackerNews.Api.ApiClients;
using HackerNews.Api.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace HackerNews.Api.Services
{

    public class NewsService(IHackerNewsClient hn, IMemoryCache cache, ILogger<NewsService> log) : INewsService
    {
        private static readonly TimeSpan IdListTtl = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan ItemTtl = TimeSpan.FromMinutes(10);
        private const int SearchScanLimit = 200;   // scan recent N for search
        private const int MaxConcurrency = 10;    // be nice to HN API

        public async Task<PagedResponse<NewsStory>> GetLatestAsync(int page, int pageSize, string? search, CancellationToken ct = default)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var storyIds = await cache.GetOrCreateAsync("new:ids", async e =>
            {
                e.AbsoluteExpirationRelativeToNow = IdListTtl; // TODO: tweak if needed
                return await hn.GetNewStoryIdsAsync(ct);
            }) ?? Array.Empty<long>();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var toScan = storyIds.Take(SearchScanLimit).ToArray();
                var items = await FetchManyAsync(toScan, ct);
                var filtered = items
                    .Where(i => !string.IsNullOrWhiteSpace(i.Title) &&
                                i.Title!.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .Select(ToNewsStory)
                    .ToList();

                var total = filtered.Count;
                var slice = filtered.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                return new PagedResponse<NewsStory>(slice, total, page, pageSize);
            }
            else
            {
                var pageIds = storyIds.Skip((page - 1) * pageSize).Take(pageSize).ToArray();
                var items = await FetchManyAsync(pageIds, ct);
                var list = items.Select(ToNewsStory).ToList();
                return new PagedResponse<NewsStory>(list, storyIds.Length, page, pageSize);
            }
        }

        public async Task<NewsStory?> GetByIdAsync(long id, CancellationToken ct = default)
        {
            var item = await GetItemCachedAsync(id, ct);
            return item is null ? null : ToNewsStory(item);
        }

        private async Task<List<HackerNewsItem>> FetchManyAsync(IEnumerable<long> ids, CancellationToken ct)
        {
            var sem = new SemaphoreSlim(MaxConcurrency);
            var bag = new ConcurrentBag<HackerNewsItem>();

            var tasks = ids.Select(async id =>
            {
                await sem.WaitAsync(ct);
                try
                {
                    var item = await GetItemCachedAsync(id, ct);
                    if (item is not null) bag.Add(item);
                }
                finally { sem.Release(); }
            });

            await Task.WhenAll(tasks);
            // Preserve “newest-ish” order (ids list is newest-first)
            var set = new HashSet<long>(ids);
            return ids.Where(set.Contains)
                      .Select(id => bag.FirstOrDefault(i => i.Id == id))
                      .Where(i => i is not null)!
                      .ToList()!;
        }

        private Task<HackerNewsItem?> GetItemCachedAsync(long id, CancellationToken ct) =>
            cache.GetOrCreateAsync($"item:{id}", async e =>
            {
                e.AbsoluteExpirationRelativeToNow = ItemTtl;
                return await hn.GetItemAsync(id, ct);
            });

        private static NewsStory ToNewsStory(HackerNewsItem i)
        {
            var discussion = $"https://news.ycombinator.com/item?id={i.Id}";
            var link = string.IsNullOrWhiteSpace(i.Url) ? discussion : i.Url!;

            return new NewsStory(
                Id: i.Id,
                Title: i.Title ?? "(untitled)",
                Url: link,
                DiscussionUrl: discussion,
                By: i.By,
                Score: i.Score ?? 0,
                Time: DateTimeOffset.FromUnixTimeSeconds(i.Time),
                Comments: i.Descendants ?? 0
            );
        }
    }

}
