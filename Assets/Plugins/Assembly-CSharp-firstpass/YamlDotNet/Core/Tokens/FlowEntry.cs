using System;

namespace YamlDotNet.Core.Tokens
{
	[Serializable]
	public class FlowEntry : Token
	{
		public FlowEntry()
			: this(Mark.Empty, Mark.Empty)
		{
		}

		public FlowEntry(Mark startMark, Mark endMark)
			: base(startMark, endMark)
		{
		}
	}
}
