using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;

[JsonObject]
public class MapsTimeZone
{
	[JsonProperty]
	public MapsReferenceTime ReferenceTime { get; set; }
}
