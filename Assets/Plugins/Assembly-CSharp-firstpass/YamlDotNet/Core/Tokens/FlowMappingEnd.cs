using System;

namespace YamlDotNet.Core.Tokens
{
	[Serializable]
	public class FlowMappingEnd : Token
	{
		public FlowMappingEnd()
			: this(Mark.Empty, Mark.Empty)
		{
		}

		public FlowMappingEnd(Mark startMark, Mark endMark)
			: base(startMark, endMark)
		{
		}
	}
}
