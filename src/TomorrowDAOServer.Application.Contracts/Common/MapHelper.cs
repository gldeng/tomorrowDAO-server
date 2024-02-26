using Newtonsoft.Json;

namespace TomorrowDAOServer.Common;

public static class MapHelper
{
    public static T MapJsonConvert<T>(string jsonString)
    {
        return JsonConvert.DeserializeObject<T>(jsonString);
    }
}