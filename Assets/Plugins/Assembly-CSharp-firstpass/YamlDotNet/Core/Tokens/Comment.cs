using System;

namespace YamlDotNet.Core.Tokens
{
	[Serializable]
	public class Comment : Token
	{
		public string Value { get; private set; }

		public bool IsInline { get; private set; }

		public Comment(string value, bool isInline)
			: this(value, isInline, Mark.Empty, Mark.Empty)
		{
		}

		public Comment(string value, bool isInline, Mark startMark, Mark endMark)
			: base(startMark, endMark)
		{
			IsInline = isInline;
			Value = value;
		}
	}
}
