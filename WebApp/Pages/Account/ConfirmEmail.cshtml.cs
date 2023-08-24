using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Net.Http;

namespace WebApp.Pages.Account;

public class ConfirmEmailModel : PageModel
{
    private readonly WebApiOptions _webApiOptions;
    public ConfirmEmailModel(WebApiOptions webApiOptions) => _webApiOptions = webApiOptions;

    public async Task<IActionResult> OnGetAsync(string userId, string token)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Patch, _webApiOptions.ConfirmEmailApi);

        requestMessage.Headers.Add("userId", userId);
        requestMessage.Headers.Add("token", token);

        var response = await new HttpClient().SendAsync(requestMessage);

        //TODO: Show error messages in UI

        return RedirectToPage("/Index");
    }
}
