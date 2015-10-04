namespace Testbed.Serialization.Models
{
	using System.Xml.Serialization;

	using Newtonsoft.Json;

	[JsonObject("error")]
	[XmlType("error")]
	public class ErrorModel
	{
		[JsonProperty("message")]
		[XmlElement("message")]
		public string Message { get; set; }
	}
}
