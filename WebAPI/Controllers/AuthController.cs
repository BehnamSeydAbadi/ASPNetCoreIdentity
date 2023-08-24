using Email.Service;
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
    private readonly UserManager<User> _userManager;
    private readonly IEmailSender _emailSender;

    public AuthController(IConfiguration configuration, UserManager<User> userManager, IEmailSender emailSender)
    {
        _configuration = configuration;
        _userManager = userManager;
        _emailSender = emailSender;
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] CredentialDto dto)
    {
        var user = await _userManager.FindByNameAsync(dto.Username);

        if (user is null
            || await _userManager.CheckPasswordAsync(user, dto.Password) is false
            || user.EmailConfirmed is false)
            return Unauthorized();

        var claims = await _userManager.GetClaimsAsync(user);

        var token = CreateToken(claims);

        return Ok(new JwtTokenViewModel
        {
            Token = token.Token,
            Expiration = token.ExpiresAt
        });
    }

    [HttpPost("Register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var user = new User
        {
            UserName = dto.Username,
            Email = dto.Email,
            SecurityStamp = Guid.NewGuid().ToString()
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (result.Succeeded is false)
            foreach (var error in result.Errors)
                ModelState.AddModelError("Register", error.Description);


        result = await _userManager.AddClaimsAsync(user, new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, "Customer")
        });

        if (result.Succeeded is false)
            foreach (var error in result.Errors)
                ModelState.AddModelError("Register", error.Description);

        if (ModelState.IsValid is false)
            return BadRequest(ModelState);

        var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        //TODO: Should implement sending confirmation email to user.
        //      Instead I'm just sending it to client.

        return Ok(new EmailConfirmationViewModel
        {
            UserId = user.Id.ToString(),
            EmailConfirmationToken = emailConfirmationToken
        });
    }

    [HttpPatch("ConfirmEmail")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null) return BadRequest();

        var result = await _userManager.ConfirmEmailAsync(user, token);

        if (result.Succeeded is false)
            ModelState.AddModelError("ConfirmEmail", "Email confirmation failed");

        return Ok();
    }

    [HttpPost("ForgotPassword")]
    public async Task<IActionResult> ForgotPassword([FromQuery] string email)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
            return NoContent();

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        await _emailSender.SendEmailAsync(user.UserName, email, "Reset password", token);

        return Ok();
    }




    private (string Token, DateTime ExpiresAt) CreateToken(IEnumerable<Claim> claims)
    {
        var secretKey = Encoding.ASCII.GetBytes(_configuration.GetValue<string>("SecretKey")!);

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(secretKey),
            SecurityAlgorithms.HmacSha256Signature);

        var expiresAt = DateTime.Now.AddDays(1).Date;

        var jwt = new JwtSecurityToken(
            claims: claims, notBefore: DateTime.UtcNow, expires: expiresAt,
            signingCredentials: signingCredentials);

        return (new JwtSecurityTokenHandler().WriteToken(jwt), expiresAt);
    }
}
