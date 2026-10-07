using System;

namespace YamlDotNet.Core.Tokens
{
	[Serializable]
	public class BlockEnd : Token
	{
		public BlockEnd()
			: this(Mark.Empty, Mark.Empty)
		{
		}

		public BlockEnd(Mark startMark, Mark endMark)
			: base(startMark, endMark)
		{
		}
	}
}
