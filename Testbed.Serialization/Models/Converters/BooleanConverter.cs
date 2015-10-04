namespace Testbed.Serialization.Models.Converters
{
	using System;

	using Newtonsoft.Json;

	public class BooleanConverter : JsonConverter
	{
		private const string Yes = "yes";
		private const string No = "no";

		public override bool CanConvert(Type objectType)
		{
			return typeof(bool).IsAssignableFrom(objectType)
				|| typeof(Nullable<bool>).IsAssignableFrom(objectType);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var unboxed = Deserialize(reader.Value.ToString());

			if (typeof(Nullable<bool>).IsAssignableFrom(objectType))
			{
				return unboxed;
			}
			else if (typeof(bool).IsAssignableFrom(objectType)
				&& unboxed.HasValue)
			{
				return unboxed.Value;
			}

			//  We can't read this as a boolean. Let Json.NET attempt to convert it.
			return serializer.Deserialize(reader, objectType);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			writer.WriteValue(Serialize(GetUnboxedValue(value)));
		}

		public static bool? Deserialize(string value)
		{
			switch (value.ToLowerInvariant().Trim())
			{
				case "true":
				case Yes:
				case "on":
				case "y":
				case "1":
					return true;

				case "false":
				case No:
				case "off":
				case "n":
				case "0":
					return false;
			}

			return null;
		}

		public static string Serialize(bool? value)
		{
			if (value.HasValue)
			{
				return value.Value
					? Yes
					: No;
			}

			return null;
		}

		private static bool? GetUnboxedValue(object value)
		{
			if (typeof(Nullable<bool>).IsAssignableFrom(value.GetType()))
			{
				return (bool?)value;
			}
			else if (typeof(bool).IsAssignableFrom(value.GetType()))
			{
				return (bool)value;
			}

			return null;
		}
	}
}
