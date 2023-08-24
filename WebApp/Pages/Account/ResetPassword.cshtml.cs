using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Pages.Account;

public class ResetPasswordModel : PageModel
{
    private readonly WebApiOptions _webApiOptions;

    public ResetPasswordModel(WebApiOptions webApiOptions) => _webApiOptions = webApiOptions;

    [BindProperty]
    public DTO Dto { get; set; } = new();

    public IActionResult OnGet(string token, string email)
    {
        Dto.Token = token;
        Dto.Email = email;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (ModelState.IsValid is false) return Page();

        var response = await new HttpClient().PostAsJsonAsync<object>(
            _webApiOptions.ResetPasswordApi,
            new { Dto.Email, Dto.Password, Dto.Token });

        //TODO: Show error messages in UI

        return Page();
    }
}

public record DTO
{
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; }
    public string Email { get; set; }
    public string Token { get; set; }
}
