using Newtonsoft.Json;
using System.Collections.Generic;

[JsonObject]
public class MapsSearchAddressResult
{
	[JsonProperty]
	public Dictionary<string, float> position { get; set; }
}
