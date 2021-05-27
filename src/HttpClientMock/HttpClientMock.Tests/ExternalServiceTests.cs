using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace HttpClientMock.Tests
{
    [ExcludeFromCodeCoverage]
    public class ExternalServiceTests
    {
        [Fact]
        public async Task GetAsync_Success()
        {
            // Arrange
            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();

            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@"[ ""string 1"", ""string 2"", ""string 3"", ""string 4"" ]"),
            };

            httpMessageHandlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(httpResponseMessage);
            
            var httpClient = new HttpClient(httpMessageHandlerMock.Object);

            var externalService = new ExternalService(httpClient, Mock.Of<ILogger<ExternalService>>());

            // Act
            var response = await externalService.GetAsync().ConfigureAwait(false);

            // Assert
            Assert.NotEmpty(response);
            Assert.Contains("string 1", response);
            Assert.Contains("string 2", response);
            Assert.Contains("string 3", response);
            Assert.Contains("string 4", response);
        }

        [Fact]
        public async Task GetAsync_Failure()
        {
            // Arrange
            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();

            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent(@"[]"),
            };

            httpMessageHandlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(httpResponseMessage);

            var httpClient = new HttpClient(httpMessageHandlerMock.Object);

            var externalService = new ExternalService(httpClient, Mock.Of<ILogger<ExternalService>>());

            // Act
            var response = await externalService.GetAsync().ConfigureAwait(false);

            // Assert
            Assert.Empty(response);
        }

        [Fact]
        public void GetAsync_Exception()
        {
            // Arrange
            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();

            httpMessageHandlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ThrowsAsync(new HttpRequestException());

            var httpClient = new HttpClient(httpMessageHandlerMock.Object);

            var externalService = new ExternalService(httpClient, Mock.Of<ILogger<ExternalService>>());

            // Act
            var response = new Func<Task>(() => externalService.GetAsync());;

            // Assert
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(response);
        }
    }
}
