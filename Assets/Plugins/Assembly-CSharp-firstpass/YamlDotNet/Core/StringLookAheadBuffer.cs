using System;

namespace YamlDotNet.Core
{
	[Serializable]
	internal class StringLookAheadBuffer : ILookAheadBuffer
	{
		private readonly string value;

		public int Position { get; private set; }

		public int Length
		{
			get
			{
				return value.Length;
			}
		}

		public bool EndOfInput
		{
			get
			{
				return IsOutside(Position);
			}
		}

		public StringLookAheadBuffer(string value)
		{
			value = value;
		}

		public char Peek(int offset)
		{
			int num = Position + offset;
			return (!IsOutside(num)) ? value[num] : '\0';
		}

		private bool IsOutside(int index)
		{
			return index >= value.Length;
		}

		public void Skip(int length)
		{
			if (length < 0)
			{
				throw new ArgumentOutOfRangeException("length", "The length must be positive.");
			}
			Position += length;
		}
	}
}
