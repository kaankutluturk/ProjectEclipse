using System;

namespace YamlDotNet.Core.Tokens
{
	[Serializable]
	public class Value : Token
	{
		public Value()
			: this(Mark.Empty, Mark.Empty)
		{
		}

		public Value(Mark startMark, Mark endMark)
			: base(startMark, endMark)
		{
		}
	}
}
