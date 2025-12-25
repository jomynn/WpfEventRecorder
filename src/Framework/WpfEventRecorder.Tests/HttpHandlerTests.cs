using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using WpfEventRecorder.Core.Hooks;
using WpfEventRecorder.Core.Models;
using Xunit;

namespace WpfEventRecorder.Tests
{
    public class HttpHandlerTests
    {
        [Fact]
        public async Task SendAsync_WhenActive_RecordsRequestAndResponse()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"message\":\"success\"}", Encoding.UTF8, "application/json")
                });

            using var recordingHandler = new RecordingHttpHandler(mockHandler.Object);
            recordingHandler.IsActive = true;

            var requestEntries = new List<RecordEntry>();
            var responseEntries = new List<RecordEntry>();
            recordingHandler.Requests.Subscribe(e => requestEntries.Add(e));
            recordingHandler.Responses.Subscribe(e => responseEntries.Add(e));

            using var client = new HttpClient(recordingHandler);

            // Act
            var response = await client.GetAsync("https://api.example.com/test?param=value");

            // Assert
            Assert.Single(requestEntries);
            Assert.Single(responseEntries);

            var request = requestEntries[0];
            Assert.Equal(RecordEntryType.ApiRequest, request.EntryType);
            Assert.NotNull(request.ApiInfo);
            Assert.Equal("GET", request.ApiInfo.Method);
            Assert.Contains("/test", request.ApiInfo.Path);

            var responseEntry = responseEntries[0];
            Assert.Equal(RecordEntryType.ApiResponse, responseEntry.EntryType);
            Assert.NotNull(responseEntry.ApiInfo);
            Assert.Equal(200, responseEntry.ApiInfo.StatusCode);
            Assert.True(responseEntry.ApiInfo.IsSuccess);
        }

        [Fact]
        public async Task SendAsync_WhenNotActive_DoesNotRecord()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            using var recordingHandler = new RecordingHttpHandler(mockHandler.Object);
            recordingHandler.IsActive = false;

            var entries = new List<RecordEntry>();
            recordingHandler.AllEvents.Subscribe(e => entries.Add(e));

            using var client = new HttpClient(recordingHandler);

            // Act
            await client.GetAsync("https://api.example.com/test");

            // Assert
            Assert.Empty(entries);
        }

        [Fact]
        public async Task SendAsync_PostRequest_CapturesRequestBody()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Created));

            using var recordingHandler = new RecordingHttpHandler(mockHandler.Object, captureRequestBody: true);
            recordingHandler.IsActive = true;

            var requestEntries = new List<RecordEntry>();
            recordingHandler.Requests.Subscribe(e => requestEntries.Add(e));

            using var client = new HttpClient(recordingHandler);
            var content = new StringContent("{\"name\":\"test\"}", Encoding.UTF8, "application/json");

            // Act
            await client.PostAsync("https://api.example.com/items", content);

            // Assert
            Assert.Single(requestEntries);
            var request = requestEntries[0];
            Assert.Equal("POST", request.ApiInfo?.Method);
            Assert.Contains("test", request.ApiInfo?.RequestBody);
        }

        [Fact]
        public async Task SendAsync_FailedRequest_RecordsError()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Connection failed"));

            using var recordingHandler = new RecordingHttpHandler(mockHandler.Object);
            recordingHandler.IsActive = true;

            var responseEntries = new List<RecordEntry>();
            recordingHandler.Responses.Subscribe(e => responseEntries.Add(e));

            using var client = new HttpClient(recordingHandler);

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() =>
                client.GetAsync("https://api.example.com/test"));

            Assert.Single(responseEntries);
            var response = responseEntries[0];
            Assert.False(response.ApiInfo?.IsSuccess);
            Assert.Contains("Connection failed", response.ApiInfo?.ErrorMessage);
        }

        [Fact]
        public async Task SendAsync_RecordsCorrelationId()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            using var recordingHandler = new RecordingHttpHandler(mockHandler.Object);
            recordingHandler.IsActive = true;

            var requestEntries = new List<RecordEntry>();
            var responseEntries = new List<RecordEntry>();
            recordingHandler.Requests.Subscribe(e => requestEntries.Add(e));
            recordingHandler.Responses.Subscribe(e => responseEntries.Add(e));

            using var client = new HttpClient(recordingHandler);

            // Act
            await client.GetAsync("https://api.example.com/test");

            // Assert
            Assert.Single(requestEntries);
            Assert.Single(responseEntries);

            var requestCorrelationId = requestEntries[0].CorrelationId;
            var responseCorrelationId = responseEntries[0].CorrelationId;

            Assert.NotNull(requestCorrelationId);
            Assert.NotNull(responseCorrelationId);
            Assert.Equal(requestCorrelationId, responseCorrelationId);
        }

        [Fact]
        public async Task SendAsync_RecordsDuration()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Returns(async () =>
                {
                    await Task.Delay(50);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                });

            using var recordingHandler = new RecordingHttpHandler(mockHandler.Object);
            recordingHandler.IsActive = true;

            var responseEntries = new List<RecordEntry>();
            recordingHandler.Responses.Subscribe(e => responseEntries.Add(e));

            using var client = new HttpClient(recordingHandler);

            // Act
            await client.GetAsync("https://api.example.com/test");

            // Assert
            Assert.Single(responseEntries);
            Assert.NotNull(responseEntries[0].DurationMs);
            Assert.True(responseEntries[0].DurationMs >= 50);
        }

        [Fact]
        public async Task SendAsync_ParsesQueryParameters()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            using var recordingHandler = new RecordingHttpHandler(mockHandler.Object);
            recordingHandler.IsActive = true;

            var requestEntries = new List<RecordEntry>();
            recordingHandler.Requests.Subscribe(e => requestEntries.Add(e));

            using var client = new HttpClient(recordingHandler);

            // Act
            await client.GetAsync("https://api.example.com/test?foo=bar&baz=qux");

            // Assert
            Assert.Single(requestEntries);
            var queryParams = requestEntries[0].ApiInfo?.QueryParameters;
            Assert.NotNull(queryParams);
            Assert.Equal("bar", queryParams["foo"]);
            Assert.Equal("qux", queryParams["baz"]);
        }

        [Fact]
        public async Task AllEvents_CombinesRequestsAndResponses()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            using var recordingHandler = new RecordingHttpHandler(mockHandler.Object);
            recordingHandler.IsActive = true;

            var allEntries = new List<RecordEntry>();
            recordingHandler.AllEvents.Subscribe(e => allEntries.Add(e));

            using var client = new HttpClient(recordingHandler);

            // Act
            await client.GetAsync("https://api.example.com/test");

            // Assert
            Assert.Equal(2, allEntries.Count);
            Assert.Contains(allEntries, e => e.EntryType == RecordEntryType.ApiRequest);
            Assert.Contains(allEntries, e => e.EntryType == RecordEntryType.ApiResponse);
        }

        [Fact]
        public async Task SendAsync_HandlesNon2xxStatusCode()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent("{\"error\":\"Not found\"}")
                });

            using var recordingHandler = new RecordingHttpHandler(mockHandler.Object);
            recordingHandler.IsActive = true;

            var responseEntries = new List<RecordEntry>();
            recordingHandler.Responses.Subscribe(e => responseEntries.Add(e));

            using var client = new HttpClient(recordingHandler);

            // Act
            await client.GetAsync("https://api.example.com/test");

            // Assert
            Assert.Single(responseEntries);
            var response = responseEntries[0];
            Assert.Equal(404, response.ApiInfo?.StatusCode);
            Assert.False(response.ApiInfo?.IsSuccess);
        }
    }
}
