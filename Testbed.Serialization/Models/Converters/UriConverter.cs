namespace Testbed.Serialization.Models.Converters
{
	using System;

	using Newtonsoft.Json;

	public class UriConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return typeof(Uri).IsAssignableFrom(objectType);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var uri = Deserialize(reader.Value.ToString());

			if (uri != null)
			{
				return uri;
			}

			//  We can't read this. Let Json.NET attempt to convert it.
			return serializer.Deserialize(reader, objectType);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			writer.WriteValue(Serialize(value as Uri));
		}

		public static Uri Deserialize(string value)
		{
			Uri result;

			if (!Uri.TryCreate(value, UriKind.Absolute, out result))
			{
				return null;
			}

			return result;
		}

		public static string Serialize(Uri value)
		{
			if (value == null)
			{
				return null;
			}

			return value.IsAbsoluteUri
				? value.AbsoluteUri
				: value.OriginalString;
		}
	}
}
