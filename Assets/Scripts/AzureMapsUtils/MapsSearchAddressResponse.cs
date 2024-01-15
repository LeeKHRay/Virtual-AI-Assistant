using Newtonsoft.Json;

[JsonObject]
public class MapsSearchAddressResponse
{
	[JsonProperty]
	public MapsSearchAddressResult[] results { get; set; }
}
