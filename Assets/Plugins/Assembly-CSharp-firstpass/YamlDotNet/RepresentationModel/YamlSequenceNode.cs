using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace YamlDotNet.RepresentationModel
{
	[Serializable]
	[DebuggerDisplay("Count = {children.Count}")]
	public class YamlSequenceNode : YamlNode, IEnumerable<YamlNode>, IEnumerable
	{
		private readonly IList<YamlNode> children = new List<YamlNode>();

		public IList<YamlNode> Children
		{
			get
			{
				return children;
			}
		}

		public SequenceStyle Style { get; set; }

		public override IEnumerable<YamlNode> AllNodes
		{
			get
			{
				yield return this;
				foreach (YamlNode child in children)
				{
					foreach (YamlNode allNode in child.AllNodes)
					{
						yield return allNode;
					}
				}
			}
		}

		internal YamlSequenceNode(EventReader reader, DocumentLoadingState state)
		{
			SequenceStart sequenceStart = reader.Expect<SequenceStart>();
			Load(sequenceStart, state);
			bool flag = false;
			while (!reader.Accept<SequenceEnd>())
			{
				YamlNode yamlNode = YamlNode.ParseNode(reader, state);
				children.Add(yamlNode);
				flag |= yamlNode is YamlAliasNode;
			}
			if (flag)
			{
				state.AddNodeWithUnresolvedAliases(this);
			}
			reader.Expect<SequenceEnd>();
		}

		public YamlSequenceNode()
		{
		}

		public YamlSequenceNode(params YamlNode[] nodes)
			: this((IEnumerable<YamlNode>)nodes)
		{
		}

		public YamlSequenceNode(IEnumerable<YamlNode> nodes)
		{
			foreach (YamlNode item in nodes)
			{
				children.Add(item);
			}
		}

		public void Add(YamlNode child)
		{
			children.Add(child);
		}

		public void Add(string child)
		{
			children.Add(new YamlScalarNode(child));
		}

		public void Remove(YamlNode node)
		{
			foreach (YamlNode child in children)
			{
				if (child == node)
				{
					children.Remove(child);
					break;
				}
			}
		}

		public void Replace(YamlNode replacement, Predicate<YamlNode> match)
		{
			for (int i = 0; i < children.Count; i++)
			{
				if (match(children[i]))
				{
					children[i] = replacement;
					break;
				}
			}
		}

		public void UpdateNode(YamlNode oldNode, YamlNode newNode)
		{
			for (int i = 0; i < children.Count; i++)
			{
				if (children[i] == oldNode)
				{
					children[i] = newNode;
					break;
				}
			}
		}

		internal override void ResolveAliases(DocumentLoadingState state)
		{
			for (int i = 0; i < children.Count; i++)
			{
				if (children[i] is YamlAliasNode)
				{
					children[i] = state.GetNode(children[i].Anchor, true, children[i].Start, children[i].End);
				}
			}
		}

		internal override void Emit(IEmitter emitter, EmitterState state)
		{
			emitter.Emit(new SequenceStart(base.Anchor, base.Tag, true, Style));
			foreach (YamlNode child in children)
			{
				child.Save(emitter, state);
			}
			emitter.Emit(new SequenceEnd());
		}

		public override void Accept(IYamlVisitor visitor)
		{
			visitor.Visit(this);
		}

		public override bool Equals(object obj)
		{
			YamlSequenceNode yamlSequenceNode = obj as YamlSequenceNode;
			if (yamlSequenceNode == null || !Equals(yamlSequenceNode) || children.Count != yamlSequenceNode.children.Count)
			{
				return false;
			}
			for (int i = 0; i < children.Count; i++)
			{
				if (!YamlNode.SafeEquals(children[i], yamlSequenceNode.children[i]))
				{
					return false;
				}
			}
			return true;
		}

		public override int GetHashCode()
		{
			int num = base.GetHashCode();
			foreach (YamlNode child in children)
			{
				num = YamlNode.CombineHashCodes(num, YamlNode.GetHashCodeOrZero(child));
			}
			return num;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder("[");
			foreach (YamlNode child in children)
			{
				stringBuilder.Append(child);
				stringBuilder.Append(", ");
			}
			stringBuilder.Remove(stringBuilder.Length - 2, 2);
			stringBuilder.Append("]");
			return stringBuilder.ToString();
		}

		public IEnumerator<YamlNode> GetEnumerator()
		{
			return Children.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public override YamlNode Clone()
		{
			YamlSequenceNode yamlSequenceNode = new YamlSequenceNode();
			foreach (YamlNode child in Children)
			{
				yamlSequenceNode.Add(child.Clone());
			}
			return yamlSequenceNode;
		}
	}
}
