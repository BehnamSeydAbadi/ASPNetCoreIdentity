namespace WebAPI.Test;

internal class ApplicationUrls
{
    private const string BaseUrl = "https://localhost:7133/api/";
    internal const string WeatherForecast = $"{BaseUrl}WeatherForecast";
    internal const string Login = $"{BaseUrl}Auth/Login";
    internal const string Register = $"{BaseUrl}Auth/Register";
    internal const string ConfirmEmail = $"{BaseUrl}Auth/ConfirmEmail";
}
