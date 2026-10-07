using System;

namespace YamlDotNet.Core
{
	[Serializable]
	internal class SimpleKey
	{
		private readonly Cursor cursor;

		public bool IsPossible { get; set; }

		public bool IsRequired { get; private set; }

		public int TokenNumber { get; private set; }

		public int Index
		{
			get
			{
				return cursor.Index;
			}
		}

		public int Line
		{
			get
			{
				return cursor.Line;
			}
		}

		public int LineOffset
		{
			get
			{
				return cursor.LineOffset;
			}
		}

		public Mark Mark
		{
			get
			{
				return cursor.Mark();
			}
		}

		public SimpleKey()
		{
			cursor = new Cursor();
		}

		public SimpleKey(bool isPossible, bool isRequired, int tokenNumber, Cursor sourceCursor)
		{
			IsPossible = isPossible;
			IsRequired = isRequired;
			TokenNumber = tokenNumber;
			cursor = new Cursor(sourceCursor);
		}
	}
}
