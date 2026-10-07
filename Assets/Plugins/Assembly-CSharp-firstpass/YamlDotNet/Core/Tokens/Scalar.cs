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

		public Scalar(string value, ScalarStyle KIGNIBIMLKK)
			: this(value, KIGNIBIMLKK, Mark.Empty, Mark.Empty)
		{
		}

		public Scalar(string value, ScalarStyle KIGNIBIMLKK, Mark ILENLCMAMBH, Mark PCLFFOBJJFO)
			: base(ILENLCMAMBH, PCLFFOBJJFO)
		{
			value = value;
			style = KIGNIBIMLKK;
		}
	}
}
