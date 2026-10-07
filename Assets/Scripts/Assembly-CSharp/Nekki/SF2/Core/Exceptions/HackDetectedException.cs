using System;
using System.Runtime.Serialization;

namespace Nekki.SF2.Core.Exceptions
{
	[Serializable]
	public class HackDetectedException : Exception
	{
		public HackDetectedException()
		{
		}

		public HackDetectedException(string message)
			: base(message)
		{
		}

		public HackDetectedException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		public HackDetectedException(SerializationInfo info, StreamingContext context)
		{
		}
	}
}
