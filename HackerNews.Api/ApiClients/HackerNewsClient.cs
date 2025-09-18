using System.Net.Http.Json;
using HackerNews.Api.Models;

namespace HackerNews.Api.ApiClients
{
    public sealed class HackerNewsClient(HttpClient http) : IHackerNewsClient
    {
        public async Task<long[]> GetNewStoryIdsAsync(CancellationToken ct = default) =>
            await http.GetFromJsonAsync<long[]>("newstories.json", ct) ?? Array.Empty<long>();

        public async Task<HackerNewsItem?> GetItemAsync(long id, CancellationToken ct = default) =>
            await http.GetFromJsonAsync<HackerNewsItem>($"item/{id}.json", ct);
    }

}
