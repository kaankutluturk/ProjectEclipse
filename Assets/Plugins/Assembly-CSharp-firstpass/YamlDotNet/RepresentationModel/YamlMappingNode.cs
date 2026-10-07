using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.Core;

namespace YamlDotNet.RepresentationModel
{
	[Serializable]
	public class YamlMappingNode : YamlNode, IEnumerable<KeyValuePair<YamlNode, YamlNode>>, IEnumerable
	{
		[Serializable]
		public class BoxEqVolume : EqualityComparer<YamlNode>
		{
			public override int GetHashCode(YamlNode obj)
			{
				return obj.GetHashCode();
			}

			public override bool Equals(YamlNode y, YamlNode x)
			{
				return y.Equals(x);
			}
		}

		private IDictionary<YamlNode, YamlNode> children = new Dictionary<YamlNode, YamlNode>(new BoxEqVolume());

		public IDictionary<YamlNode, YamlNode> Children
		{
			get
			{
				return children;
			}
		}

		public MappingStyle Style { get; set; }

		public override IEnumerable<YamlNode> AllNodes
		{
			get
			{
				yield return this;
				foreach (KeyValuePair<YamlNode, YamlNode> child in children)
				{
					foreach (YamlNode allNode in child.Key.AllNodes)
					{
						yield return allNode;
					}
					foreach (YamlNode allNode2 in child.Value.AllNodes)
					{
						yield return allNode2;
					}
				}
			}
		}

		internal YamlMappingNode(EventReader reader, DocumentLoadingState state)
		{
			MappingStart mappingStart = reader.Expect<MappingStart>();
			Load(mappingStart, state);
			bool flag = false;
			while (!reader.Accept<MappingEnd>())
			{
				YamlNode yamlNode = YamlNode.ParseNode(reader, state);
				YamlNode yamlNode2 = YamlNode.ParseNode(reader, state);
				try
				{
					children.Add(yamlNode, yamlNode2);
				}
				catch (ArgumentException duplicateKeyException)
				{
					throw new YamlException(yamlNode.Start, yamlNode.End, "Duplicate key", duplicateKeyException);
				}
				flag |= yamlNode is YamlAliasNode || yamlNode2 is YamlAliasNode;
			}
			if (flag)
			{
				state.AddNodeWithUnresolvedAliases(this);
			}
			reader.Expect<MappingEnd>();
		}

		public YamlMappingNode()
		{
		}

		public YamlMappingNode(params KeyValuePair<YamlNode, YamlNode>[] keyValuePairs)
			: this((IEnumerable<KeyValuePair<YamlNode, YamlNode>>)keyValuePairs)
		{
		}

		public YamlMappingNode(IEnumerable<KeyValuePair<YamlNode, YamlNode>> keyValuePairs)
		{
			foreach (KeyValuePair<YamlNode, YamlNode> item in keyValuePairs)
			{
				children.Add(item);
			}
		}

		public YamlMappingNode(params YamlNode[] nodes)
			: this((IEnumerable<YamlNode>)nodes)
		{
		}

		public YamlMappingNode(IEnumerable<YamlNode> nodes)
		{
			using (IEnumerator<YamlNode> enumerator = nodes.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					YamlNode current = enumerator.Current;
					if (!enumerator.MoveNext())
					{
						throw new ArgumentException("When constructing a mapping node with a sequence, the number of elements of the sequence must be even.");
					}
					Add(current, enumerator.Current);
				}
			}
		}

		public void Remove(string key, YamlNode value)
		{
			foreach (KeyValuePair<YamlNode, YamlNode> child in children)
			{
				if (child.Key.ToString() == key && child.Value == value)
				{
					children.Remove(child.Key);
					break;
				}
			}
		}

		public void Add(YamlNode key, YamlNode value)
		{
			children.Add(key, value);
		}

		public void Add(string key, YamlNode value)
		{
			children.Add(new YamlScalarNode(key), value);
		}

		public void Add(YamlNode key, string value)
		{
			children.Add(key, new YamlScalarNode(value));
		}

		public void Add(string key, string value)
		{
			children.Add(new YamlScalarNode(key), new YamlScalarNode(value));
		}

		public bool HasKey(string key)
		{
			foreach (KeyValuePair<YamlNode, YamlNode> child in children)
			{
				if (((YamlScalarNode)child.Key).Value == key)
				{
					return true;
				}
			}
			return false;
		}

		public YamlNode GetNode(string key)
		{
			foreach (KeyValuePair<YamlNode, YamlNode> child in children)
			{
				if (((YamlScalarNode)child.Key).Value == key)
				{
					return child.Value;
				}
			}
			return null;
		}

		internal override void ResolveAliases(DocumentLoadingState state)
		{
			Dictionary<YamlNode, YamlNode> dictionary = null;
			Dictionary<YamlNode, YamlNode> dictionary2 = null;
			foreach (KeyValuePair<YamlNode, YamlNode> child in children)
			{
				if (child.Key is YamlAliasNode)
				{
					if (dictionary == null)
					{
						dictionary = new Dictionary<YamlNode, YamlNode>();
					}
					dictionary.Add(child.Key, state.GetNode(child.Key.Anchor, true, child.Key.Start, child.Key.End));
				}
				if (child.Value is YamlAliasNode)
				{
					if (dictionary2 == null)
					{
						dictionary2 = new Dictionary<YamlNode, YamlNode>();
					}
					dictionary2.Add(child.Key, state.GetNode(child.Value.Anchor, true, child.Value.Start, child.Value.End));
				}
			}
			if (dictionary2 != null)
			{
				foreach (KeyValuePair<YamlNode, YamlNode> item in dictionary2)
				{
					children[item.Key] = item.Value;
				}
			}
			if (dictionary == null)
			{
				return;
			}
			foreach (KeyValuePair<YamlNode, YamlNode> item2 in dictionary)
			{
				YamlNode value = children[item2.Key];
				children.Remove(item2.Key);
				children.Add(item2.Value, value);
			}
		}

		internal override void Emit(IEmitter emitter, EmitterState state)
		{
			emitter.Emit(new MappingStart(base.Anchor, base.Tag, true, Style));
			foreach (KeyValuePair<YamlNode, YamlNode> child in children)
			{
				child.Key.Save(emitter, state);
				child.Value.Save(emitter, state);
			}
			emitter.Emit(new MappingEnd());
		}

		public override void Accept(IYamlVisitor visitor)
		{
			visitor.Visit(this);
		}

		public override bool Equals(object obj)
		{
			YamlMappingNode yamlMappingNode = obj as YamlMappingNode;
			if (yamlMappingNode == null || !Equals(yamlMappingNode) || children.Count != yamlMappingNode.children.Count)
			{
				return false;
			}
			foreach (KeyValuePair<YamlNode, YamlNode> child in children)
			{
				YamlNode value;
				if (!yamlMappingNode.children.TryGetValue(child.Key, out value) || !YamlNode.SafeEquals(child.Value, value))
				{
					return false;
				}
			}
			return true;
		}

		public override int GetHashCode()
		{
			int num = base.GetHashCode();
			foreach (KeyValuePair<YamlNode, YamlNode> child in children)
			{
				num = YamlNode.CombineHashCodes(num, YamlNode.GetHashCodeOrZero(child.Key));
				num = YamlNode.CombineHashCodes(num, YamlNode.GetHashCodeOrZero(child.Value));
			}
			return num;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder("{");
			foreach (KeyValuePair<YamlNode, YamlNode> child in children)
			{
				if (stringBuilder.Length > 2)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append(child.Key).Append(": ").Append(child.Value);
			}
			stringBuilder.Append("}");
			return stringBuilder.ToString();
		}

		public IEnumerator<KeyValuePair<YamlNode, YamlNode>> GetEnumerator()
		{
			return children.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public override YamlNode Clone()
		{
			YamlMappingNode yamlMappingNode = new YamlMappingNode(new YamlNode[0]);
			foreach (KeyValuePair<YamlNode, YamlNode> child in Children)
			{
				yamlMappingNode.Add(child.Key.ToString(), child.Value.Clone());
			}
			return yamlMappingNode;
		}
	}
}
