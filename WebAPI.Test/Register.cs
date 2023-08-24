using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using WebAPI.DataAccess;
using WebAPI.DTO;

namespace WebAPI.Test;

public class Register : IClassFixture<WebAppFactory>
{
    private readonly CustomDbContext _dbContext;
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _configuration;

    private readonly HttpClient _httpClient;

    public Register(WebAppFactory webAppFactory)
    {
        var scope = webAppFactory.Services.CreateScope();

        _dbContext = scope.ServiceProvider.GetRequiredService<CustomDbContext>();
        _userManager = scope.ServiceProvider.GetService<UserManager<User>>()!;
        _configuration = webAppFactory.Configuration;

        _httpClient = webAppFactory.CreateClient();
    }

    [Fact]
    public async Task register_user()
    {
        var registerDto = new RegisterDto
        {
            Username = "Maryam_user",
            Password = "M234",
            Email = "maryam@user.com"
        };

        var response = await _httpClient.PostAsync(ApplicationUrls.Register, registerDto.ToStringContent());

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.UserName == registerDto.Username);

        user.Should().NotBeNull();
        user.Email.Should().Be(registerDto.Email);
    }

    [Fact]
    public async Task register_user_with_invalid_password_should_response_400()
    {
        var response = await _httpClient.PostAsync(ApplicationUrls.Register, new RegisterDto
        {
            Username = "Maryam_user",
            Password = "M",
            Email = "maryam@user.com"
        }.ToStringContent());

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
