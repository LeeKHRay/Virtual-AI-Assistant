using Newtonsoft.Json;

[JsonObject]
public class MapsCurrentConditionsResponse
{
	[JsonProperty]
	public MapsCurrentConditions[] results { get; set; }
}
