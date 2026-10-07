using System;

namespace YamlDotNet.Core.Tokens
{
	[Serializable]
	public class StreamStart : Token
	{
		public StreamStart()
			: this(Mark.Empty, Mark.Empty)
		{
		}

		public StreamStart(Mark startMark, Mark endMark)
			: base(startMark, endMark)
		{
		}
	}
}
