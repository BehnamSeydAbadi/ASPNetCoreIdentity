using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using RestSharp;
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
        try
        {
            var httpClient = new HttpClient();
            var response = await httpClient.PostAsJsonAsync<object>(
                _webApiOptions.ResetPasswordApi,
                new { Dto.Email, Dto.Password, Dto.Token });
        }
        catch (Exception ex)
        {
            throw;
        }
        

        //var restClient = new RestClient();
        //var restRequest = new RestRequest(_webApiOptions.ResetPasswordApi, Method.Post);

        //var body = JsonConvert.SerializeObject(new { Dto.Email, Dto.Password, Dto.Token });
        //restRequest.AddJsonBody(body, contentType: ContentType.Json);

        //await restClient.PostAsync(restRequest);

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
