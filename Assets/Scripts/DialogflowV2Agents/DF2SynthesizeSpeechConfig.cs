using Newtonsoft.Json;

//@hoatong
namespace Syrus.Plugins.DFV2Client
{
	[JsonObject]
	public class DF2SynthesizeSpeechConfig
	{
		[JsonProperty]
		public float SpeakingRate { get; set; }

		[JsonProperty]
		public float Pitch { get; set; }

		[JsonProperty]
		public float VolumeGainDb { get; set; }

		[JsonProperty]
		public DF2VoiceSelectionParams Voice { get; set; }
	}
}
