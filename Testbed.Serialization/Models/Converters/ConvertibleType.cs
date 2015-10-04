namespace Testbed.Serialization.Models.Converters
{
	using System;
	using System.Linq;
	using System.Xml.Schema;
	using System.Xml.Serialization;

	using Newtonsoft.Json;

	public class ConvertibleType<TValue, TConverter> : IXmlSerializable where TConverter : JsonConverter, new()
	{
		private static readonly Lazy<XmlSchema> _xmlSchema;
		private static readonly TConverter _converter;
		private static readonly Lazy<Func<string, TValue>> _deserialize;
		private static readonly Lazy<Func<TValue, string>> _serialize;

		static ConvertibleType()
		{
			_xmlSchema = new Lazy<XmlSchema>(() => BuildXmlSchema());
			_converter = Activator.CreateInstance<TConverter>();
			_deserialize = new Lazy<Func<string, TValue>>(() => BuildDeserialize());
			_serialize = new Lazy<Func<TValue, string>>(() => BuildSerialize());
		}

		public TValue Value { get; set; }

		public XmlSchema GetSchema()
		{
			return _xmlSchema.Value;
		}

		public void ReadXml(System.Xml.XmlReader reader)
		{
			reader.MoveToContent();

			reader.ReadStartElement();

			Value = _converter.CanConvert(typeof(TValue))
				? _deserialize.Value(reader.ReadElementContentAsString())
				: default(TValue);

			reader.ReadEndElement();
		}

		public void WriteXml(System.Xml.XmlWriter writer)
		{
			writer.WriteString(
				_converter.CanConvert(typeof(TValue))
				? _serialize.Value(Value)
				: null);
		}

		public static implicit operator TValue(ConvertibleType<TValue, TConverter> serializableType)
		{
			if (serializableType == null)
			{
				return default(TValue);
			}

			return serializableType.Value;
		}

		public static implicit operator ConvertibleType<TValue, TConverter>(TValue value)
		{
			return new ConvertibleType<TValue, TConverter>
			{
				Value = value,
			};
		}

		private static XmlSchema BuildXmlSchema()
		{
			var typeName = typeof(TValue).Name;

			var schemas = new XmlSchemas();

			new XmlSchemaExporter(schemas)
				.ExportTypeMapping(new XmlReflectionImporter()
					.ImportTypeMapping(typeof(TValue)));

			return schemas
				.OfType<XmlSchema>()
				.FirstOrDefault(schema => schema.Items
					.OfType<XmlSchemaElement>()
					.Any(element => typeName.Equals(element.Name, StringComparison.OrdinalIgnoreCase)));
		}

		private static Func<string, TValue> BuildDeserialize()
		{
			var mi = typeof(TConverter)
				.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
				.SingleOrDefault(m => 
				{
					var parameters = m.GetParameters();

					return m.Name == "Deserialize"
						&& parameters.Length == 1
						&& parameters[0].ParameterType.IsAssignableFrom(typeof(string))
						&& m.ReturnType.IsAssignableFrom(typeof(TValue));
				});

			if (mi == null)
			{
				return str => default(TValue);
			}

			return str => (TValue)mi.Invoke(null, new object[] { str });
		}

		private static Func<TValue, string> BuildSerialize()
		{
			var mi = typeof(TConverter)
				.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
				.SingleOrDefault(m =>
				{
					var parameters = m.GetParameters();

					return m.Name == "Serialize"
						&& parameters.Length == 1
						&& parameters[0].ParameterType.IsAssignableFrom(typeof(TValue))
						&& m.ReturnType.IsAssignableFrom(typeof(string));
				});

			if (mi == null)
			{
				return value => null;
			}

			return value => (string)mi.Invoke(null, new object[] { value });
		}
	}
}
