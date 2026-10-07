using System;
using System.Collections;
using YamlDotNet.RepresentationModel;

namespace Nekki.Yaml
{
	[Serializable]
	public abstract class Node : IEnumerable
	{
		public string key { get; protected set; }

		public YamlNode value { get; protected set; }

		public string typeNode { get; protected set; }

		public override string ToString()
		{
			return value.ToString();
		}

		public string GetTypeNode()
		{
			return typeNode;
		}

		public string GetKey()
		{
			return key;
		}

		public static Node CreateNode(string nodeKey, YamlNode yamlNode)
		{
			Type type = yamlNode.GetType();
			if (type == typeof(YamlScalarNode))
			{
				return new Scalar(nodeKey, (YamlScalarNode)yamlNode);
			}
			if (type == typeof(YamlSequenceNode))
			{
				return new Sequence(nodeKey, (YamlSequenceNode)yamlNode);
			}
			if (type == typeof(YamlMappingNode))
			{
				return new Mapping(nodeKey, (YamlMappingNode)yamlNode);
			}
			return null;
		}

		public virtual IEnumerator GetEnumerator()
		{
			yield return this;
		}
	}
}
