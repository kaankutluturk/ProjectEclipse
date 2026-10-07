using System;

namespace YamlDotNet.Core.Tokens
{
	[Serializable]
	public class Scalar : Token
	{
		private readonly string value;

		private readonly ScalarStyle style;

		public string Value
		{
			get
			{
				return value;
			}
		}

		public ScalarStyle Style
		{
			get
			{
				return style;
			}
		}

		public Scalar(string value)
			: this(value, ScalarStyle.Any)
		{
		}

		public Scalar(string value, ScalarStyle scalarStyle)
			: this(value, scalarStyle, Mark.Empty, Mark.Empty)
		{
		}

		public Scalar(string value, ScalarStyle scalarStyle, Mark startMark, Mark endMark)
			: base(startMark, endMark)
		{
			value = value;
			style = scalarStyle;
		}
	}
}
