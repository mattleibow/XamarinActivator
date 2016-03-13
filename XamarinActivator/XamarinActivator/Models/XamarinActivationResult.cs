using Newtonsoft.Json;

namespace XamarinActivator.Models
{
	internal class XamarinActivationResult
	{
		[JsonProperty ("usedMachines")]
		public int UsedMachines;

		[JsonProperty ("messageDetail")]
		public string MessageDetail;

		[JsonProperty ("message")]
		public string Message;

		[JsonProperty ("license")]
		public string License;

		[JsonProperty ("level")]
		public string Level;

		[JsonProperty ("code")]
		public XamarinResponseCode Code;

		[JsonProperty ("allowedMachines")]
		public int AllowedMachines;
	}
}
