using System;
using System.Collections;
using System.Collections.Generic;
using YamlDotNet.RepresentationModel;

namespace Nekki.Yaml
{
	[Serializable]
	public class Mapping : Node
	{
		private YamlMappingNode _mapping;

		public List<Node> nodesInside { get; private set; }

		public Mapping(string nodeKey, Node[] nodes)
		{
			base.typeNode = "Mapping";
			base.key = nodeKey;
			base.value = new YamlMappingNode(new YamlNode[0]);
			_mapping = (YamlMappingNode)base.value;
			nodesInside = new List<Node>();
			foreach (Node node in nodes)
			{
				_mapping.Add(node.key, node.value);
				nodesInside.Add(Node.CreateNode(node.key, node.value));
			}
		}

		public Mapping(string nodeKey, List<Node> nodes)
			: this(nodeKey, nodes.ToArray())
		{
			base.typeNode = "Mapping";
		}

		public Mapping(Mapping source)
			: this(source.key, (YamlMappingNode)source.value)
		{
			base.typeNode = "Mapping";
		}

		public Mapping(string nodeKey, YamlMappingNode mappingNode)
		{
			base.typeNode = "Mapping";
			base.key = nodeKey;
			base.value = mappingNode;
			_mapping = (YamlMappingNode)base.value;
			nodesInside = new List<Node>();
			foreach (KeyValuePair<YamlNode, YamlNode> item in _mapping)
			{
				nodesInside.Add(Node.CreateNode(item.Key.ToString(), item.Value));
			}
		}

		public int GetCount()
		{
			return nodesInside.Count;
		}

		public Node GetNodesByIndex(int index)
		{
			if (index < nodesInside.Count)
			{
				return nodesInside[index];
			}
			return null;
		}

		public List<Node> GetNodes()
		{
			return nodesInside;
		}

		public void Add(Node node)
		{
			_mapping.Add(node.key, node.value);
			nodesInside.Add(node);
		}

		public void AddNodes(Node[] nodes)
		{
			foreach (Node node in nodes)
			{
				_mapping.Add(node.key, node.value);
				nodesInside.Add(node);
			}
		}

		public void Remove(string nodeKey, string value)
		{
			foreach (Node item in nodesInside)
			{
				if (item.key == nodeKey && item.value.ToString() == value)
				{
					nodesInside.Remove(item);
					_mapping.Remove(nodeKey, item.value);
					break;
				}
			}
		}

		public void Remove(Node node)
		{
			if (node != null)
			{
				Remove(node.key, node.value.ToString());
			}
		}

		public Mapping GetMapping(string name)
		{
			if (!_mapping.HasKey(name))
			{
				return null;
			}
			YamlNode yamlNode = _mapping.GetNode(name);
			Type type = yamlNode.GetType();
			if (type == typeof(YamlMappingNode))
			{
				return new Mapping(name, (YamlMappingNode)yamlNode);
			}
			return null;
		}

		public Sequence GetSequence(string name)
		{
			if (!_mapping.HasKey(name))
			{
				return null;
			}
			Sequence result = null;
			foreach (Node item in nodesInside)
			{
				if (item.key.Equals(name) && item is Sequence)
				{
					result = (Sequence)item;
					break;
				}
			}
			return result;
		}

		public Scalar GetText(string name)
		{
			if (!_mapping.HasKey(name))
			{
				return null;
			}
			YamlNode yamlNode = _mapping.GetNode(name);
			Type type = yamlNode.GetType();
			if (type == typeof(YamlScalarNode))
			{
				return new Scalar(name, (YamlScalarNode)yamlNode);
			}
			return null;
		}

		public Node GetNode(string name)
		{
			if (!_mapping.HasKey(name))
			{
				return null;
			}
			YamlNode yamlNode = _mapping.GetNode(name);
			Type type = yamlNode.GetType();
			if (type == typeof(YamlScalarNode))
			{
				return new Scalar(name, (YamlScalarNode)yamlNode);
			}
			if (type == typeof(YamlSequenceNode))
			{
				return new Sequence(name, (YamlSequenceNode)yamlNode);
			}
			if (type == typeof(YamlMappingNode))
			{
				return new Mapping(name, (YamlMappingNode)yamlNode);
			}
			return null;
		}

		public override IEnumerator GetEnumerator()
		{
			foreach (Node item in nodesInside)
			{
				yield return item;
			}
		}
	}
}
