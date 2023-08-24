using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
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

        var user = new User
        {
            UserName = username,
            Email = "admin@admin.com",
            EmailConfirmed = true,
            TwoFactorEnabled = false,
            LockoutEnabled = false,
            NormalizedUserName = "ADMIN",
        };
        user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, password);

        _dbContext.Users.Add(user);
        _dbContext.SaveChanges();

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
        _dbContext.SaveChanges();


        var dto = new CredentialDto
        {
            Username = username,
            Password = password
        };

        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        var stringContent = new StringContent(JsonConvert.SerializeObject(dto, settings), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(ApplicationUrls.Login, stringContent);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = response.Content.ReadAsStringAsync().Result;
        var viewModel = JsonConvert.DeserializeObject<JwtTokenViewModel>(result);

        var claims = _dbContext.UserClaims.ToArray().Select(uc => new Claim(uc.ClaimType!, uc.ClaimValue!));

        var expectedToken = CreateToken(claims);

        viewModel.Token.Should().Be(expectedToken);
    }




    private string CreateToken(IEnumerable<Claim> claims)
    {
        var secretKey = Encoding.ASCII.GetBytes(_configuration.GetValue<string>("SecretKey")!);

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(secretKey),
            SecurityAlgorithms.HmacSha256Signature);

        var expiresAt = DateTime.Now.AddDays(1);

        var jwt = new JwtSecurityToken(
            claims: claims, notBefore: DateTime.UtcNow, expires: expiresAt,
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
}
