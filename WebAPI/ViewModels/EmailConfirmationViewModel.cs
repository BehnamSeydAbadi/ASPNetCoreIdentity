namespace WebAPI.ViewModels;

public record EmailConfirmationViewModel
{
    public string UserId { get; set; }
    public string EmailConfirmationToken { get; set; }
}
