using System.Net;
using HackerNews.Api.ApiClients;

namespace HackerNews.Tests
{

    public class HackerNewsClientTests
    {
        [Fact]
        public async Task GetNewStoryIdsAsync_ReturnsIds()
        {
            // Arrange
            var expectedJson = "[1,2,3]";
            var handler = new FakeHttpMessageHandler(expectedJson, HttpStatusCode.OK);
            var client = new HttpClient(handler) { BaseAddress = new Uri("https://fake.test/") };
            var hn = new HackerNewsClient(client);

            // Act
            var ids = await hn.GetNewStoryIdsAsync();

            // Assert
            Assert.Equal(new long[] { 1, 2, 3 }, ids);
        }

        [Fact]
        public async Task GetItemAsync_ReturnsItem()
        {
            // Arrange
            var expectedJson = """{ "id": 42, "title": "Test Title", "url": "http://example.com" }""";
            var handler = new FakeHttpMessageHandler(expectedJson, HttpStatusCode.OK);
            var client = new HttpClient(handler) { BaseAddress = new Uri("https://fake.test/") };
            var hn = new HackerNewsClient(client);

            // Act
            var item = await hn.GetItemAsync(42);

            // Assert
            Assert.NotNull(item);
            Assert.Equal(42, item!.Id);
            Assert.Equal("Test Title", item.Title);
            Assert.Equal("http://example.com", item.Url);
        }

        private class FakeHttpMessageHandler : HttpMessageHandler
        {
            private readonly string _response;
            private readonly HttpStatusCode _statusCode;

            public FakeHttpMessageHandler(string response, HttpStatusCode statusCode)
            {
                _response = response;
                _statusCode = statusCode;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var msg = new HttpResponseMessage(_statusCode)
                {
                    Content = new StringContent(_response)
                };
                msg.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                return Task.FromResult(msg);
            }
        }
    }
}
