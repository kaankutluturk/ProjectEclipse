using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace YamlDotNet.RepresentationModel
{
	[Serializable]
	[DebuggerDisplay("{Value}")]
	public class YamlScalarNode : YamlNode
	{
		public string Value { get; set; }

		public ScalarStyle Style { get; set; }

		public override IEnumerable<YamlNode> AllNodes
		{
			get
			{
				yield return this;
			}
		}

		internal YamlScalarNode(EventReader reader, DocumentLoadingState state)
		{
			Scalar scalarEvent = reader.Expect<Scalar>();
			Load(scalarEvent, state);
			Value = scalarEvent.GetValue();
			Style = scalarEvent.GetStyle();
		}

		public YamlScalarNode()
		{
		}

		public YamlScalarNode(string value)
		{
			Value = value;
		}

		internal override void ResolveAliases(DocumentLoadingState state)
		{
			throw new NotSupportedException("Resolving an alias on a scalar node does not make sense");
		}

		internal override void Emit(IEmitter emitter, EmitterState state)
		{
			emitter.Emit(new Scalar(base.Anchor, base.Tag, Value, Style, true, false));
		}

		public override void Accept(IYamlVisitor visitor)
		{
			visitor.Visit(this);
		}

		public override bool Equals(object obj)
		{
			YamlScalarNode yamlScalarNode = obj as YamlScalarNode;
			return yamlScalarNode != null && Equals(yamlScalarNode) && YamlNode.SafeEquals(Value, yamlScalarNode.Value);
		}

		public override int GetHashCode()
		{
			return YamlNode.CombineHashCodes(base.GetHashCode(), YamlNode.GetHashCodeOrZero(Value));
		}

		[SpecialName]
		public static YamlScalarNode op_Implicit(string value)
		{
			return new YamlScalarNode(value);
		}

		[SpecialName]
		public static string op_Explicit(YamlScalarNode value)
		{
			return value.Value;
		}

		public override string ToString()
		{
			return Value;
		}

		public override YamlNode Clone()
		{
			return new YamlScalarNode(Value);
		}
	}
}
