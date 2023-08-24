using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebAPI.DataAccess;
using WebAPI.DTO;
using WebAPI.ViewModels;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;

    public AuthController(IConfiguration configuration, SignInManager<User> signInManager, UserManager<User> userManager)
    {
        _configuration = configuration;
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Authentication([FromBody] CredentialDto dto)
    {
        var user = await _userManager.FindByNameAsync(dto.Username);

        if (user is null || await _userManager.CheckPasswordAsync(user, dto.Password) is false)
            return Unauthorized();

        var claims = await _userManager.GetClaimsAsync(user);

        var token = CreateToken(claims);

        return Ok(new JwtTokenViewModel
        {
            Token = token.Token,
            Expiration = token.ExpiresAt
        });
    }


    private (string Token, DateTime ExpiresAt) CreateToken(IEnumerable<Claim> claims)
    {
        var secretKey = Encoding.ASCII.GetBytes(_configuration.GetValue<string>("SecretKey")!);

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(secretKey),
            SecurityAlgorithms.HmacSha256Signature);

        var expiresAt = DateTime.Now.AddDays(1);

        var jwt = new JwtSecurityToken(
            claims: claims, notBefore: DateTime.UtcNow, expires: expiresAt,
            signingCredentials: signingCredentials);

        return (new JwtSecurityTokenHandler().WriteToken(jwt), expiresAt);
    }
}
