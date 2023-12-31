﻿using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using WebAPI.DataAccess;
using WebAPI.DTO;
using WebAPI.ViewModels;

namespace WebAPI.Test;

public class Login : IClassFixture<WebAppFactory>
{
    private readonly CustomDbContext _dbContext;
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _configuration;

    private readonly HttpClient _httpClient;

    public Login(WebAppFactory webAppFactory)
    {
        var scope = webAppFactory.Services.CreateScope();

        _dbContext = scope.ServiceProvider.GetRequiredService<CustomDbContext>();
        _userManager = scope.ServiceProvider.GetService<UserManager<User>>()!;
        _configuration = webAppFactory.Configuration;

        _httpClient = webAppFactory.CreateClient();
    }

    [Fact]
    public async Task request_without_login_should_response_401()
    {
        var response = await _httpClient.GetAsync(ApplicationUrls.WeatherForecast);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task login_admin()
    {
        var username = "admin";
        var password = "admin";
        await CreateAdminAsync(username, password, isEmailConfirmed: true);

        var response = await _httpClient.PostAsync(ApplicationUrls.Login, new CredentialDto
        {
            Username = username,
            Password = password
        }.ToStringContent());

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var viewModel = response.To<JwtTokenViewModel>();

        var claims = _dbContext.UserClaims.ToArray().Select(uc => new Claim(uc.ClaimType!, uc.ClaimValue!));

        var expectedToken = CreateToken(claims);

        viewModel.Token.Should().Be(expectedToken);
    }

    [Fact]
    public async Task login_without_email_confirmation_should_response_401()
    {
        var username = "admin";
        var password = "admin";
        await CreateAdminAsync(username, password, isEmailConfirmed: false);

        var response = await _httpClient.PostAsync(ApplicationUrls.Login, new CredentialDto
        {
            Username = username,
            Password = password
        }.ToStringContent());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task confirm_email()
    {
        var viewModel = (await _httpClient.PostAsync(ApplicationUrls.Register, new RegisterDto
        {
            Username = "Maryam_user",
            Email = "maryam@user.com",
            Password = "M123"
        }.ToStringContent()))
        .To<EmailConfirmationViewModel>();

        var url = $"{ApplicationUrls.ConfirmEmail}?userId={viewModel.UserId}&token={viewModel.EmailConfirmationToken}";

        var response = await _httpClient.PatchAsync(url, null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    //[Fact]
    public async Task forgot_password()
    {
        var email = "x@outlook.com";
        await CreateAdminAsync(email: email);

        var response = await _httpClient.PostAsync($"{ApplicationUrls.ForgotPassword}?email={email}", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private string CreateToken(IEnumerable<Claim> claims)
    {
        var secretKey = Encoding.ASCII.GetBytes(_configuration.GetValue<string>("SecretKey")!);

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(secretKey),
            SecurityAlgorithms.HmacSha256Signature);

        var expiresAt = DateTime.Now.AddDays(1).Date;

        var jwt = new JwtSecurityToken(
            claims: claims, notBefore: DateTime.UtcNow, expires: expiresAt,
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    private async Task CreateAdminAsync(string username = "admin", string password = "admin", string email = "admin@admin.com", bool isEmailConfirmed = true)
    {
        var user = new User
        {
            UserName = username,
            NormalizedUserName = username.ToUpper(),
            Email = email,
            NormalizedEmail = email.ToUpper(),
            EmailConfirmed = isEmailConfirmed,
            TwoFactorEnabled = false,
            LockoutEnabled = false,
            SecurityStamp = Guid.NewGuid().ToString()
        };
        user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, password);

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _dbContext.UserClaims.AddRange(new[] {
            new IdentityUserClaim<Guid>
            {
                UserId = user.Id,
                ClaimType = ClaimTypes.Name,
                ClaimValue = user.UserName
            },
            new IdentityUserClaim<Guid>
            {
                UserId = user.Id,
                ClaimType = ClaimTypes.NameIdentifier,
                ClaimValue = user.Id.ToString()
            },
            new IdentityUserClaim<Guid>
            {
                UserId = user.Id,
                ClaimType = ClaimTypes.Email,
                ClaimValue = user.Email
            },
            new IdentityUserClaim<Guid>
            {
                UserId = user.Id,
                ClaimType = ClaimTypes.Role,
                ClaimValue = "Admin"
            },
        });
        await _dbContext.SaveChangesAsync();
    }
}
