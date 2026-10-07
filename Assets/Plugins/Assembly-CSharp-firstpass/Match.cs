internal class Match
{
	private MatchState state;

	private int position;

	private int length;

	private byte symbol;

	internal MatchState MatchState
	{
		get
		{
			return GetState();
		}
		set
		{
			set_State(value);
		}
	}

	internal int MatchPosition
	{
		get
		{
			return GetPosition();
		}
		set
		{
			set_Position(value);
		}
	}

	internal int MatchLength
	{
		get
		{
			return GetLength();
		}
		set
		{
			set_Length(value);
		}
	}

	internal byte LiteralSymbol
	{
		get
		{
			return GetSymbol();
		}
		set
		{
			set_Symbol(value);
		}
	}

	internal MatchState GetState()
	{
		return state;
	}

	internal void set_State(MatchState value)
	{
		state = value;
	}

	internal int GetPosition()
	{
		return position;
	}

	internal void set_Position(int value)
	{
		position = value;
	}

	internal int GetLength()
	{
		return length;
	}

	internal void set_Length(int value)
	{
		length = value;
	}

	internal byte GetSymbol()
	{
		return symbol;
	}

	internal void set_Symbol(byte value)
	{
		symbol = value;
	}
}
