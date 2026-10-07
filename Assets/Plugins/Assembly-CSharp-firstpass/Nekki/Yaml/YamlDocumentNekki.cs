using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using YamlDotNet.RepresentationModel;

namespace Nekki.Yaml
{
	[Serializable]
	public class YamlDocumentNekki
	{
		private YamlStream _yamlStream;

		private YamlDocument _yamlDocument;

		private Mapping _rootMapping;

		private string _content;

		public YamlDocumentNekki()
		{
			_yamlStream = new YamlStream();
		}

		public static YamlDocumentNekki LoadFromFile(string path)
		{
			if (!File.Exists(path))
			{
				AdvLog.LogError("YAML file is not exists!!!");
				return null;
			}
			using (TextReader textReader = new StreamReader(path))
			{
				return LoadFromString(textReader.ReadToEnd());
			}
		}

		public static YamlDocumentNekki LoadFromString(string yamlText)
		{
			YamlDocumentNekki yamlDocumentNekki = new YamlDocumentNekki();
			yamlDocumentNekki._yamlStream.Load(new StringReader(yamlText));
			yamlDocumentNekki._yamlDocument = yamlDocumentNekki._yamlStream.Documents[0];
			if (yamlDocumentNekki._yamlDocument.RootNode is YamlMappingNode)
			{
				yamlDocumentNekki._rootMapping = new Mapping("Root", (YamlMappingNode)yamlDocumentNekki._yamlDocument.RootNode);
			}
			yamlDocumentNekki._content = yamlText;
			return yamlDocumentNekki;
		}

		public override string ToString()
		{
			return _yamlDocument.ToString();
		}

		public void SaveToFile(string path, bool assignAnchors = true)
		{
			using (TextWriter textWriter = new StreamWriter(path, false, Encoding.UTF8))
			{
				_yamlStream.Save(textWriter, assignAnchors);
			}
		}

		public string SaveToString()
		{
			StringWriter stringWriter = new StringWriter();
			_yamlStream.Save(stringWriter);
			return stringWriter.ToString();
		}

		public Node GetRoot(string name)
		{
			return _rootMapping.GetNode(name);
		}

		public Mapping GetRoot(int index = 0)
		{
			return _rootMapping;
		}

		public void Serialize(string path)
		{
			TextReader textReader = new StringReader(_content);
			Deserializer deserializer = new Deserializer();
			object obj = deserializer.Deserialize(textReader);
			if (obj == null)
			{
				return;
			}
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			using (FileStream serializationStream = File.OpenWrite(path))
			{
				binaryFormatter.Serialize(serializationStream, obj);
			}
		}
	}
}
