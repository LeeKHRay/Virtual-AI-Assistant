using Newtonsoft.Json;
using System.Collections.Generic;

[JsonObject]
public class OpenCageResponse
{
    [JsonProperty]
    public OpenCageResult[] results { get; set; }
}
