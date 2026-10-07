using System;
using System.Runtime.Serialization;

namespace YamlDotNet.Core
{
	[Serializable]
	public class ForwardAnchorNotSupportedException : YamlException
	{
		public ForwardAnchorNotSupportedException()
		{
		}

		public ForwardAnchorNotSupportedException(string message)
			: base(message)
		{
		}

		public ForwardAnchorNotSupportedException(Mark startMark, Mark endMark, string message)
			: base(startMark, endMark, message)
		{
		}

		public ForwardAnchorNotSupportedException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected ForwardAnchorNotSupportedException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
