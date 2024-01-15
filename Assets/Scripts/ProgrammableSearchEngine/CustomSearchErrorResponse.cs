using Newtonsoft.Json;

[JsonObject]
public class CustomSearchErrorResponse
{
	[JsonProperty]
	public Error error { get; set; }

	[JsonObject]
	public class Error
	{
		[JsonProperty]
		public string code { get; set; }

		[JsonProperty]
		public string message { get; set; }
	}

}