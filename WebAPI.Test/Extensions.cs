using Newtonsoft.Json;
using System.Text;

namespace WebAPI.Test;

internal static class Extensions
{
    internal static T? To<T>(this HttpResponseMessage response)
    {
        var result = response.Content.ReadAsStringAsync().Result;
        return JsonConvert.DeserializeObject<T>(result);
    }

    internal static StringContent ToStringContent<T>(this T @object)
    {
        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        var content = JsonConvert.SerializeObject(@object, settings);

        return new StringContent(content, Encoding.UTF8, "application/json");
    }
}
