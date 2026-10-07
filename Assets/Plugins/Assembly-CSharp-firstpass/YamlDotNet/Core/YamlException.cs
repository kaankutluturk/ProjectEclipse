using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace YamlDotNet.Core
{
	[Serializable]
	public class YamlException : Exception
	{
		public Mark Start { get; private set; }

		public Mark End { get; private set; }

		public YamlException()
		{
		}

		public YamlException(string message)
			: base(message)
		{
		}

		public YamlException(Mark startMark, Mark endMark, string message)
			: this(startMark, endMark, message, null)
		{
		}

		public YamlException(Mark startMark, Mark endMark, string message, Exception innerException)
			: base(string.Format("({0}) - ({1}): {2}", startMark, endMark, message), innerException)
		{
			Start = startMark;
			End = endMark;
		}

		public YamlException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected YamlException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			Start = (Mark)info.GetValue("Start", typeof(Mark));
			End = (Mark)info.GetValue("End", typeof(Mark));
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("Start", Start);
			info.AddValue("End", End);
		}
	}
}
