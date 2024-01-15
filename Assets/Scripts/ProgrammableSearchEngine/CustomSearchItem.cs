using Newtonsoft.Json;
using System.Collections.Generic;

[JsonObject]
public class CustomSearchItem
{
    [JsonProperty]
    public string title { get; set; }

    [JsonProperty]
    public string link { get; set; }

    [JsonProperty]
    public string snippet { get; set; }

    [JsonProperty]
    public Dictionary<string, object> image { get; set; }


}
