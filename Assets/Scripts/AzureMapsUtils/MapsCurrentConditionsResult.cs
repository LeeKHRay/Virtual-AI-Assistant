using Newtonsoft.Json;
using System.Collections.Generic;

[JsonObject]
public class MapsCurrentConditions
{
	[JsonProperty]
	public string phrase { get; set; }

	[JsonProperty]
	public int iconCode { get; set; }

	[JsonProperty]
	public Dictionary<string, object> temperature { get; set; }

	[JsonProperty]
	public int relativeHumidity { get; set; }

	[JsonProperty]
	public int cloudCover { get; set; }
}
