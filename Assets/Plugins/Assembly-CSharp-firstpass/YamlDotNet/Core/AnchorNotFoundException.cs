using System;
using System.Runtime.Serialization;

namespace YamlDotNet.Core
{
	[Serializable]
	public class AnchorNotFoundException : YamlException
	{
		public AnchorNotFoundException()
		{
		}

		public AnchorNotFoundException(string message)
			: base(message)
		{
		}

		public AnchorNotFoundException(Mark startMark, Mark endMark, string message)
			: base(startMark, endMark, message)
		{
		}

		public AnchorNotFoundException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected AnchorNotFoundException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
