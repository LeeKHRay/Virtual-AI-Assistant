using Newtonsoft.Json;

[JsonObject]
public class CustomSearchResponse
{
    [JsonProperty]
    public CustomSearchItem[] items { get; set; }
}
