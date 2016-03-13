using Newtonsoft.Json;

namespace XamarinActivator.Models
{
	internal class XamarinSession
	{
		[JsonProperty ("error")]
		public string Error;

		[JsonProperty ("success")]
		public bool Success;

		[JsonProperty ("token")]
		public string Token;

		[JsonProperty ("user")]
		public XamarinUser User;
	}
}
