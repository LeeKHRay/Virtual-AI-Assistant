using Newtonsoft.Json;

//@hoatong
namespace Syrus.Plugins.DFV2Client
{
	[JsonObject]
	public class DF2VoiceSelectionParams
	{
		[JsonProperty]
		public string Name { get; set; }
	}
}
