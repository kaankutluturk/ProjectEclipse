using System;
using System.Runtime.Serialization;

namespace YamlDotNet.Core
{
	[Serializable]
	public class DuplicateAnchorException : YamlException
	{
		public DuplicateAnchorException()
		{
		}

		public DuplicateAnchorException(string message)
			: base(message)
		{
		}

		public DuplicateAnchorException(Mark startMark, Mark endMark, string message)
			: base(startMark, endMark, message)
		{
		}

		public DuplicateAnchorException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected DuplicateAnchorException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
