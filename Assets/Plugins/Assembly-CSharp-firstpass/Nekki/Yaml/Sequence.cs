using System;
using System.Collections;
using System.Collections.Generic;
using YamlDotNet.RepresentationModel;

namespace Nekki.Yaml
{
	[Serializable]
	public class Sequence : Node
	{
		private YamlSequenceNode _sequence;

		public List<Node> nodesInside { get; private set; }

		public Sequence(string nodeKey, YamlSequenceNode sequenceNode)
		{
			base.typeNode = "Sequence";
			base.key = nodeKey;
			base.value = sequenceNode;
			_sequence = (YamlSequenceNode)base.value;
			nodesInside = new List<Node>();
			foreach (YamlNode item in _sequence)
			{
				nodesInside.Add(Node.CreateNode(base.key, item));
			}
		}

		public Sequence(string nodeKey, Node node)
		{
			base.typeNode = "Sequence";
			base.key = nodeKey;
			base.value = new YamlSequenceNode(new YamlNode[0]);
			_sequence = (YamlSequenceNode)base.value;
			nodesInside = new List<Node>();
			Add(node);
			foreach (YamlNode item in _sequence)
			{
				nodesInside.Add(Node.CreateNode(base.key, item));
			}
		}

		public Sequence(string nodeKey, Node[] nodes)
		{
			base.typeNode = "Sequence";
			base.key = nodeKey;
			base.value = new YamlSequenceNode(new YamlNode[0]);
			_sequence = (YamlSequenceNode)base.value;
			nodesInside = new List<Node>();
			AddNodes(nodes);
		}

		public Sequence(string nodeKey, List<Node> nodes)
			: this(nodeKey, nodes.ToArray())
		{
			base.typeNode = "Sequence";
		}

		public void ReplaceAt(int index, Node newNode)
		{
			_sequence.UpdateNode(nodesInside[index].value, newNode.value);
			nodesInside[index] = newNode;
		}

		public void Replace(List<Node> nodes)
		{
			foreach (Node item in nodesInside)
			{
				_sequence.Remove(item.value);
			}
			foreach (Node item2 in nodes)
			{
				_sequence.Add(item2.value);
			}
			nodesInside = nodes;
		}

		public void Remove(Node node)
		{
			_sequence.Remove(node.value);
			nodesInside.Remove(node);
		}

		public void Add(Node node)
		{
			_sequence.Add(node.value);
			nodesInside.Add(node);
		}

		public void AddNodes(Node[] nodes)
		{
			foreach (Node node in nodes)
			{
				_sequence.Add(node.value);
				nodesInside.Add(node);
			}
		}

		public void AddNodes(List<Node> nodes)
		{
			foreach (Node item in nodes)
			{
				_sequence.Add(item.value);
				nodesInside.Add(item);
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

		public override IEnumerator GetEnumerator()
		{
			foreach (Node item in nodesInside)
			{
				yield return item;
			}
		}
	}
}
