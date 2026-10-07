using System;

namespace YamlDotNet.Core.Tokens
{
	[Serializable]
	public class Key : Token
	{
		public Key()
			: this(Mark.Empty, Mark.Empty)
		{
		}

		public Key(Mark startMark, Mark endMark)
			: base(startMark, endMark)
		{
		}
	}
}
