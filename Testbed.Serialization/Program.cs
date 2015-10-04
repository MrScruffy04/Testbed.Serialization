namespace Testbed.Serialization
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Xml;
	using System.Xml.Serialization;

	using Newtonsoft.Json;

	using Testbed.Serialization.Models;

	class Program
	{
		#region Main Program Loop

		private static ManualResetEvent _quitEvent = new ManualResetEvent(false);

		[STAThread]
		private static void Main(string[] args)
		{
			Console.CancelKeyPress += (sender, e) =>
			{
				_quitEvent.Set();
				e.Cancel = true;
			};

			try
			{
				#region Setup
				#endregion


				ProgramBody();

				//  One of the following should be commented out. The other should be uncommented.

				//_quitEvent.WaitOne();  //  Wait on UI thread for Ctrl + C

				Console.ReadKey(true);  //  Wait for any character input
			}
			finally
			{
				#region Tear down
				#endregion
			}
		}

		#endregion





		private static void ProgramBody()
		{
			SaveObjectAndSchema(new ErrorModel { Message = "foobar", });

			SaveObjectAndSchema(new[] { new ErrorModel { Message = "foobar1", }, new ErrorModel { Message = "foobar2", }, new ErrorModel { Message = "foobar3", }, });

			SaveObjectAndSchema(new ErrorCollectionModel(
				new[] { new ErrorModel { Message = "foobar1", }, new ErrorModel { Message = "foobar2", }, new ErrorModel { Message = "foobar3", }, }
			));

			Console.WriteLine("... Done!");
		}



		private static readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
		{
			Formatting = Newtonsoft.Json.Formatting.Indented,
		};

		private static readonly XmlWriterSettings _xmlWriterSettings = new XmlWriterSettings
		{
			Encoding = Encoding.UTF8,
			NamespaceHandling = NamespaceHandling.OmitDuplicates,

			Indent = true,
		};

		private static readonly XmlSerializerNamespaces _xmlNamespaces = new XmlSerializerNamespaces(new[]
		{
			new XmlQualifiedName(string.Empty, "http://example.com/v1"),
			new XmlQualifiedName("xsi", "http://www.w3.org/2001/XMLSchema-instance"),
		});



		private static void SaveObjectAndSchema<TModel>(TModel model)
		{
			var modelName = typeof(TModel).Name
				.Replace("Model", string.Empty);

			var rootDir = Path.Combine(
				GetAssemblyDirectory(),
				"generatedFiles");

			var diExample = GetDirectory(Path.Combine(rootDir, "examples", "json"));
			var diSchema = GetDirectory(Path.Combine(rootDir, "schemas", "json"));

			using (var textWriter = new StreamWriter(Path.Combine(diExample.FullName, modelName + ".json")))
			using (var writer = new JsonTextWriter(textWriter))
			{
				var serializer = JsonSerializer.CreateDefault(_jsonSerializerSettings);

				serializer.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter { /*CamelCaseText = true*/ });

				serializer.Serialize(writer, model);
			}

			using (var textWriter = new StreamWriter(Path.Combine(diSchema.FullName, modelName + ".json")))
			using (var writer = new JsonTextWriter(textWriter))
			{
				new Newtonsoft.Json.Schema.JsonSchemaGenerator()
					.Generate(typeof(TModel))
					.WriteTo(writer);
			}


			diExample = GetDirectory(Path.Combine(rootDir, "examples", "xml"));
			diSchema = GetDirectory(Path.Combine(rootDir, "schemas", "xml"));

			using (var textWriter = new StreamWriter(Path.Combine(diExample.FullName, modelName + ".xml")))
			using (var writer = XmlWriter.Create(textWriter, _xmlWriterSettings))
			{
				new XmlSerializer(typeof(TModel), "http://example.com/v1")
					.Serialize(writer, model, _xmlNamespaces);
			}

			var schemas = new XmlSchemas();

			new XmlSchemaExporter(schemas)
				.ExportTypeMapping(new XmlReflectionImporter()
					.ImportTypeMapping(typeof(TModel)));

			foreach (var schema in schemas.OfType<System.Xml.Schema.XmlSchema>())
			{
				foreach (var schemaItem in schema.Items.OfType<System.Xml.Schema.XmlSchemaElement>())
				{
					var schemaName = char.ToUpperInvariant(schemaItem.Name[0]) + schemaItem.Name.Substring(1);

					using (var textWriter = new StreamWriter(Path.Combine(diSchema.FullName, schemaName + ".xml")))
					{
						schema.Write(textWriter);
					}				
				}
			}
		}

		private static string GetAssemblyDirectory()
		{
			var codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
			var uri = new UriBuilder(codeBase);
			var path = Uri.UnescapeDataString(uri.Path);
		
			return Path.GetDirectoryName(path);
		}

		private static DirectoryInfo GetDirectory(string path)
		{
			var di = new DirectoryInfo(path);

			if (!di.Exists)
			{
				di.Create();
			}

			return di;
		}
	}
}
