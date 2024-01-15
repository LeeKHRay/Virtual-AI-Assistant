using System.Collections.Generic;
using Newtonsoft.Json;

[JsonObject]
public class MapsTimezoneResponse
{
	[JsonProperty]
	public MapsTimeZone[] TimeZones { get; set; }
}
