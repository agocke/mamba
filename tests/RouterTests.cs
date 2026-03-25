using Xunit;

namespace Mamba.Tests;

public class RouterTests
{
    [Fact]
    public async Task SimpleRoute_ReturnsExpectedResponse()
    {
        // Arrange
        var router = new ReqRouter.Builder()
            .MapGet("/hello", () => "Hello, world!")
            .Build();

        var port = GetAvailablePort();
        var url = $"http://127.0.0.1:{port}";

        var server = HttpServer.Start(url, router);
        await server.WaitForReadyAsync();

        try
        {
            // Act
            using var client = new HttpClient();
            var response = await client.GetAsync($"{url}/hello", TestContext.Current.CancellationToken);
            var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Hello, world!", content);
        }
        finally
        {
            await server.StopAsync();
        }
    }

    [Fact]
    public async Task UnmatchedRoute_Returns404()
    {
        // Arrange
        var router = new ReqRouter.Builder()
            .MapGet("/hello", () => "Hello, world!")
            .Build();

        var port = GetAvailablePort();
        var url = $"http://127.0.0.1:{port}";

        var server = HttpServer.Start(url, router);
        await server.WaitForReadyAsync();

        try
        {
            // Act
            using var client = new HttpClient();
            var response = await client.GetAsync($"{url}/notfound", TestContext.Current.CancellationToken);
            var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal("Not Found", content);
        }
        finally
        {
            await server.StopAsync();
        }
    }

    [Fact]
    public async Task MultipleRoutes_MatchCorrectly()
    {
        // Arrange
        var router = new ReqRouter.Builder()
            .MapGet("/one", () => "Route One")
            .MapGet("/two", () => "Route Two")
            .Build();

        var port = GetAvailablePort();
        var url = $"http://127.0.0.1:{port}";

        var server = HttpServer.Start(url, router);
        await server.WaitForReadyAsync();

        try
        {
            using var client = new HttpClient();

            // Act & Assert
            var response1 = await client.GetAsync($"{url}/one", TestContext.Current.CancellationToken);
            Assert.Equal("Route One", await response1.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

            var response2 = await client.GetAsync($"{url}/two", TestContext.Current.CancellationToken);
            Assert.Equal("Route Two", await response2.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        }
        finally
        {
            await server.StopAsync();
        }
    }

    private static int GetAvailablePort()
    {
        using var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}
