using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using WebAPI.DataAccess;

namespace WebAPI.Test;

public class Login : IClassFixture<WebAppFactory>
{
    private readonly CustomDbContext _dbContext;
    private readonly HttpClient _httpClient;

    public Login(WebAppFactory webAppFactory)
    {
        using var scope = webAppFactory.Services.CreateScope();

        _dbContext = scope.ServiceProvider.GetRequiredService<CustomDbContext>();
        _httpClient = webAppFactory.CreateClient();
    }

    [Fact]
    public async Task request_without_login_should_response_401()
    {
        var response = await _httpClient.GetAsync(ApplicationUrls.WeatherForecast);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
