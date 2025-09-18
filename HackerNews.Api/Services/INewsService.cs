using HackerNews.Api.Models;

namespace HackerNews.Api.Services
{
    public interface INewsService
    {
        Task<PagedResponse<NewsStory>> GetLatestAsync(int page, int pageSize, string? search, CancellationToken ct = default);
        Task<NewsStory?> GetByIdAsync(long id, CancellationToken ct = default);
    }

}
