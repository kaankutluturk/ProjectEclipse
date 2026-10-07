using System;
using System.IO;

namespace YamlDotNet.Core
{
	[Serializable]
	public class LookAheadBuffer : ILookAheadBuffer
	{
		private readonly TextReader input;

		private readonly char[] buffer;

		private int firstIndex;

		private int count;

		private bool endOfInput;

		public bool EndOfInput
		{
			get
			{
				return endOfInput && count == 0;
			}
		}

		public LookAheadBuffer(TextReader reader, int capacity)
		{
			if (reader == null)
			{
				throw new ArgumentNullException("input");
			}
			if (capacity < 1)
			{
				throw new ArgumentOutOfRangeException("capacity", "The capacity must be positive.");
			}
			input = reader;
			buffer = new char[capacity];
		}

		private int GetIndexForOffset(int offset)
		{
			int num = firstIndex + offset;
			if (num >= buffer.Length)
			{
				num -= buffer.Length;
			}
			return num;
		}

		public char Peek(int offset)
		{
			if (offset < 0 || offset >= buffer.Length)
			{
				throw new ArgumentOutOfRangeException("offset", "The offset must be betwwen zero and the capacity of the buffer.");
			}
			Cache(offset);
			if (offset < count)
			{
				return buffer[GetIndexForOffset(offset)];
			}
			return '\0';
		}

		public void Cache(int length)
		{
			while (length >= count)
			{
				int num = input.Read();
				if (num >= 0)
				{
					int num2 = GetIndexForOffset(count);
					buffer[num2] = (char)num;
					count++;
					continue;
				}
				endOfInput = true;
				break;
			}
		}

		public void Skip(int length)
		{
			if (length < 1 || length > count)
			{
				throw new ArgumentOutOfRangeException("length", "The length must be between 1 and the number of characters in the buffer. Use the Peek() and / or Cache() methods to fill the buffer.");
			}
			firstIndex = GetIndexForOffset(length);
			count -= length;
		}
	}
}
