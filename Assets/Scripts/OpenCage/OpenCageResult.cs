using Newtonsoft.Json;
using System.Collections.Generic;

[JsonObject]
public class OpenCageResult
{
    [JsonProperty]
    public Dictionary<string, float> geometry { get; set; }
}
