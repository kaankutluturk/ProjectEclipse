using System;

namespace YamlDotNet.Core.Tokens
{
	[Serializable]
	public class Tag : Token
	{
		private readonly string handle;

		private readonly string suffix;

		public string Handle
		{
			get
			{
				return handle;
			}
		}

		public string Suffix
		{
			get
			{
				return suffix;
			}
		}

		public Tag(string tagHandle, string tagSuffix)
			: this(tagHandle, tagSuffix, Mark.Empty, Mark.Empty)
		{
		}

		public Tag(string tagHandle, string tagSuffix, Mark startMark, Mark endMark)
			: base(startMark, endMark)
		{
			handle = tagHandle;
			suffix = tagSuffix;
		}
	}
}
