namespace Testbed.Serialization.Models.Converters
{
	using System;

	using Newtonsoft.Json;

	public class MinutePrecisionTimeSpanConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return typeof(TimeSpan).IsAssignableFrom(objectType)
				|| typeof(Nullable<TimeSpan>).IsAssignableFrom(objectType);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var unboxed = Deserialize(reader.Value.ToString());

			if (typeof(Nullable<TimeSpan>).IsAssignableFrom(objectType))
			{
				return unboxed;
			}
			else if (typeof(TimeSpan).IsAssignableFrom(objectType)
				&& unboxed.HasValue)
			{
				return unboxed.Value;
			}

			//  We can't read this as a TimeSpan. Let Json.NET attempt to convert it.
			return serializer.Deserialize(reader, objectType);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			writer.WriteValue(Serialize(GetUnboxedValue(value)));
		}

		public static TimeSpan? Deserialize(string value)
		{
			TimeSpan result;

			if (TimeSpan.TryParse(value, out result))
			{
				return result;
			}

			return null;
		}

		public static string Serialize(TimeSpan? value)
		{
			if (value.HasValue)
			{
				return value.Value.ToString(@"hh\:mm");
			}

			return null;
		}

		private static TimeSpan? GetUnboxedValue(object value)
		{
			if (typeof(Nullable<TimeSpan>).IsAssignableFrom(value.GetType()))
			{
				return (TimeSpan?)value;
			}
			else if (typeof(TimeSpan).IsAssignableFrom(value.GetType()))
			{
				return (TimeSpan)value;
			}

			return null;
		}
	}
}
