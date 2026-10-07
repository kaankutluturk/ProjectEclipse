using System;
using System.Runtime.Serialization;

namespace YamlDotNet.Core
{
	[Serializable]
	public class SyntaxErrorException : YamlException
	{
		public SyntaxErrorException()
		{
		}

		public SyntaxErrorException(string message)
			: base(message)
		{
		}

		public SyntaxErrorException(Mark startMark, Mark endMark, string message)
			: base(startMark, endMark, message)
		{
		}

		public SyntaxErrorException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected SyntaxErrorException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
