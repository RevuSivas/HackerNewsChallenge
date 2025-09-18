namespace HackerNews.Api.Models
{
    public sealed record NewsStory(
        long Id,
        string Title,
        string Url,          
        string DiscussionUrl,
        string? By,
        int Score,
        DateTimeOffset Time,
        int Comments
    );

    public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);

}
