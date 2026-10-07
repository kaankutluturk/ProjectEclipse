using System;

namespace YamlDotNet.Core.Tokens
{
	[Serializable]
	public class DocumentEnd : Token
	{
		public DocumentEnd()
			: this(Mark.Empty, Mark.Empty)
		{
		}

		public DocumentEnd(Mark startMark, Mark endMark)
			: base(startMark, endMark)
		{
		}
	}
}
