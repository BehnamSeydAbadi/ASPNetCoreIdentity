namespace WebAPI.ViewModels;

public record JwtTokenViewModel
{
    public string Token { get; set; }
    public DateTime Expiration { get; set; }
}