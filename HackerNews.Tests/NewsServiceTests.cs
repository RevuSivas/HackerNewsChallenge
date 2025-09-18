using HackerNews.Api.ApiClients;
using HackerNews.Api.Models;
using HackerNews.Api.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;

namespace HackerNews.Tests
{
    public class NewsServiceTests
    {
        private readonly IMemoryCache _cache;
        private readonly Mock<IHackerNewsClient> _mockClient;
        private readonly Mock<ILogger<NewsService>> _mockLogger;
        private readonly NewsService _service;

        public NewsServiceTests()
        {
            _cache = new MemoryCache(new MemoryCacheOptions());
            _mockClient = new Mock<IHackerNewsClient>();
            _mockLogger = new Mock<ILogger<NewsService>>();
            _service = new NewsService(_mockClient.Object, _cache, _mockLogger.Object);
        }

        [Fact]
        public async Task GetLatestAsync_CachesIdList()
        {
            // Arrange
            var ids = new long[] { 1, 2, 3 };
            _mockClient.Setup(c => c.GetNewStoryIdsAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(ids);

            _mockClient.Setup(c => c.GetItemAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                       .Returns((long id, CancellationToken _) =>
                           Task.FromResult<HackerNewsItem?>(new HackerNewsItem
                           {
                               Id = id,
                               Title = $"Story {id}",
                               Url = $"http://x/{id}",
                               By = "test",
                               Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                               Score = 10,
                               Descendants = 0,
                               Type = "story"
                           }));

            // Act
            var first = await _service.GetLatestAsync(1, 2, null);
            var second = await _service.GetLatestAsync(1, 2, null);

            // Assert
            _mockClient.Verify(c => c.GetNewStoryIdsAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.Equal(2, first.Items.Count);
            Assert.Equal(2, second.Items.Count);
        }

        [Fact]
        public async Task GetLatestAsync_FiltersBySearch()
        {
            // Arrange
            var ids = new long[] { 1, 2 };
            _mockClient.Setup(c => c.GetNewStoryIdsAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(ids);

            _mockClient.Setup(c => c.GetItemAsync(1, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(new HackerNewsItem
                       {
                           Id = 1,
                           Title = "Angular Rocks",
                           Url = "http://a.com",
                           By = "test",
                           Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                       });

            _mockClient.Setup(c => c.GetItemAsync(2, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(new HackerNewsItem
                       {
                           Id = 2,
                           Title = "React",
                           Url = "http://b.com",
                           By = "test",
                           Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                       });

            // Act
            var result = await _service.GetLatestAsync(1, 10, "angular");

            // Assert
            Assert.Single(result.Items);
            Assert.Equal("Angular Rocks", result.Items[0].Title);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsMappedStory()
        {
            // Arrange
            _mockClient.Setup(c => c.GetItemAsync(5, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(new HackerNewsItem
                       {
                           Id = 5,
                           Title = "Hello",
                           Url = "http://test.com",
                           By = "alice",
                           Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                       });

            // Act
            var story = await _service.GetByIdAsync(5);

            // Assert
            Assert.NotNull(story);
            Assert.Equal(5, story!.Id);
            Assert.Equal("Hello", story.Title);
            Assert.Equal("http://test.com", story.Url);
        }

        [Fact]
        public async Task GetByIdAsync_FallsBackToDiscussionUrl_WhenUrlMissing()
        {
            // Arrange
            _mockClient.Setup(c => c.GetItemAsync(99, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(new HackerNewsItem
                       {
                           Id = 99,
                           Title = "No Link",
                           Url = null,
                           By = "bob",
                           Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                       });

            // Act
            var story = await _service.GetByIdAsync(99);

            // Assert
            Assert.NotNull(story);
            Assert.Equal($"https://news.ycombinator.com/item?id=99", story!.Url);
        }
    }
}
