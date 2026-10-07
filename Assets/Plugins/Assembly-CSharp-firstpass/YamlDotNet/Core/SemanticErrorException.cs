using System;
using System.Runtime.Serialization;

namespace YamlDotNet.Core
{
	[Serializable]
	public class SemanticErrorException : YamlException
	{
		public SemanticErrorException()
		{
		}

		public SemanticErrorException(string message)
			: base(message)
		{
		}

		public SemanticErrorException(Mark startMark, Mark endMark, string message)
			: base(startMark, endMark, message)
		{
		}

		public SemanticErrorException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected SemanticErrorException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
