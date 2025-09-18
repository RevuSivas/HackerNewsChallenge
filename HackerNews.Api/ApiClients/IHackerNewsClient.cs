using HackerNews.Api.Models;

namespace HackerNews.Api.ApiClients
{
    public interface IHackerNewsClient
    {
        Task<long[]> GetNewStoryIdsAsync(CancellationToken ct = default);
        Task<HackerNewsItem?> GetItemAsync(long id, CancellationToken ct = default);
    }

}
