using System;
using System.Collections.Generic;
using YamlDotNet.Core;

namespace YamlDotNet.RepresentationModel
{
	[Serializable]
	public abstract class YamlNode
	{
		public string Anchor { get; set; }

		public string Tag { get; set; }

		public Mark Start { get; private set; }

		public Mark End { get; private set; }

		public abstract IEnumerable<YamlNode> AllNodes { get; }

		public YamlNode()
		{
		}

		internal void Load(NodeEvent nodeEvent, DocumentLoadingState state)
		{
			Tag = nodeEvent.GetTag();
			if (nodeEvent.GetAnchor() != null)
			{
				Anchor = nodeEvent.GetAnchor();
				state.AddAnchor(this);
			}
			Start = nodeEvent.GetStart();
			End = nodeEvent.GetEnd();
		}

		internal static YamlNode ParseNode(EventReader reader, DocumentLoadingState state)
		{
			if (reader.Accept<Scalar>())
			{
				return new YamlScalarNode(reader, state);
			}
			if (reader.Accept<SequenceStart>())
			{
				return new YamlSequenceNode(reader, state);
			}
			if (reader.Accept<MappingStart>())
			{
				return new YamlMappingNode(reader, state);
			}
			if (reader.Accept<AnchorAlias>())
			{
				AnchorAlias aliasEvent = reader.Expect<AnchorAlias>();
				return state.GetNode(aliasEvent.GetValue(), false, aliasEvent.GetStart(), aliasEvent.GetEnd()) ?? new YamlAliasNode(aliasEvent.GetValue());
			}
			throw new ArgumentException("The current event is of an unsupported type.", "events");
		}

		internal abstract void ResolveAliases(DocumentLoadingState state);

		internal void Save(IEmitter emitter, EmitterState state)
		{
			if (!string.IsNullOrEmpty(Anchor) && !state.GetEmittedAnchors().Add(Anchor))
			{
				emitter.Emit(new AnchorAlias(Anchor));
			}
			else
			{
				Emit(emitter, state);
			}
		}

		internal abstract void Emit(IEmitter emitter, EmitterState state);

		public abstract void Accept(IYamlVisitor visitor);

		protected bool Equals(YamlNode other)
		{
			return SafeEquals(Tag, other.Tag);
		}

		protected static bool SafeEquals(object left, object right)
		{
			if (left != null)
			{
				return left.Equals(right);
			}
			if (right != null)
			{
				return right.Equals(left);
			}
			return true;
		}

		public override int GetHashCode()
		{
			return GetHashCodeOrZero(Tag);
		}

		protected static int GetHashCodeOrZero(object value)
		{
			return (value != null) ? value.GetHashCode() : 0;
		}

		protected static int CombineHashCodes(int hash1, int hash2)
		{
			return ((hash1 << 5) + hash1) ^ hash2;
		}

		public abstract YamlNode Clone();
	}
}
