namespace WebAPI.DTO;

public record ResetPasswordDto
{
    public required string Password { get; init; }
    public required string Email { get; init; }
    public required string Token { get; init; }
}
