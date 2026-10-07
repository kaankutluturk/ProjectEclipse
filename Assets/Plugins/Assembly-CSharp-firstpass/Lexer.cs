using System;
using System.IO;
using System.Text;

internal class Lexer
{
	private delegate bool StateHandler(FsmContext IEBDPKGBOGJ);

	private static int[] fsm_return_table;

	private static StateHandler[] fsm_handler_table;

	private bool allow_comments;

	private bool allow_single_quoted_strings;

	private bool end_of_input;

	private FsmContext fsm_context;

	private int input_buffer;

	private int input_char;

	private TextReader reader;

	private int state;

	private StringBuilder string_buffer;

	private string string_value;

	private int token;

	private int unichar;

	public bool AllowComments
	{
		get
		{
			return GetAllowComments();
		}
		set
		{
			SetAllowComments(value);
		}
	}

	public bool AllowSingleQuotedStrings
	{
		get
		{
			return GetAllowSingleQuotedStrings();
		}
		set
		{
			SetAllowSingleQuotedStrings(value);
		}
	}

	public bool EndOfInput
	{
		get
		{
			return GetEndOfInput();
		}
	}

	public int Token
	{
		get
		{
			return GetToken();
		}
	}

	public string StringValue
	{
		get
		{
			return GetStringValue();
		}
	}

	static Lexer()
	{
		PopulateFsmTables();
	}

	public Lexer(TextReader reader)
	{
		allow_comments = true;
		allow_single_quoted_strings = true;
		input_buffer = 0;
		string_buffer = new StringBuilder(128);
		state = 1;
		end_of_input = false;
		this.reader = reader;
		fsm_context = new FsmContext();
		fsm_context.L = this;
	}

	public bool GetAllowComments()
	{
		return allow_comments;
	}

	public void SetAllowComments(bool value)
	{
		allow_comments = value;
	}

	public bool GetAllowSingleQuotedStrings()
	{
		return allow_single_quoted_strings;
	}

	public void SetAllowSingleQuotedStrings(bool value)
	{
		allow_single_quoted_strings = value;
	}

	public bool GetEndOfInput()
	{
		return end_of_input;
	}

	public int GetToken()
	{
		return token;
	}

	public string GetStringValue()
	{
		return string_value;
	}

	private static int HexValue(int BMLBINLPOOE)
	{
		switch (BMLBINLPOOE)
		{
		case 65:
		case 97:
			return 10;
		case 66:
		case 98:
			return 11;
		case 67:
		case 99:
			return 12;
		case 68:
		case 100:
			return 13;
		case 69:
		case 101:
			return 14;
		case 70:
		case 102:
			return 15;
		default:
			return BMLBINLPOOE - 48;
		}
	}

	private static void PopulateFsmTables()
	{
		fsm_handler_table = new StateHandler[28]
		{
			State1, State2, State3, State4, State5, State6, State7, State8, State9, State10,
			State11, State12, State13, State14, State15, State16, State17, State18, State19, State20,
			State21, State22, State23, State24, State25, State26, State27, State28
		};
		fsm_return_table = new int[28]
		{
			65542, 0, 65537, 65537, 0, 65537, 0, 65537, 0, 0,
			65538, 0, 0, 0, 65539, 0, 0, 65540, 65541, 65542,
			0, 0, 65541, 65542, 0, 0, 0, 0
		};
	}

	private static char ProcessEscChar(int NLHEKGPGAME)
	{
		switch (NLHEKGPGAME)
		{
		case 34:
		case 39:
		case 47:
		case 92:
			return Convert.ToChar(NLHEKGPGAME);
		case 110:
			return '\n';
		case 116:
			return '\t';
		case 114:
			return '\r';
		case 98:
			return '\b';
		case 102:
			return '\f';
		default:
			return '?';
		}
	}

	private static bool State1(FsmContext IEBDPKGBOGJ)
	{
		while (IEBDPKGBOGJ.L.GetChar())
		{
			if (IEBDPKGBOGJ.L.input_char == 32 || (IEBDPKGBOGJ.L.input_char >= 9 && IEBDPKGBOGJ.L.input_char <= 13))
			{
				continue;
			}
			if (IEBDPKGBOGJ.L.input_char >= 49 && IEBDPKGBOGJ.L.input_char <= 57)
			{
				IEBDPKGBOGJ.L.string_buffer.Append((char)IEBDPKGBOGJ.L.input_char);
				IEBDPKGBOGJ.NextState = 3;
				return true;
			}
			switch (IEBDPKGBOGJ.L.input_char)
			{
			case 34:
				IEBDPKGBOGJ.NextState = 19;
				IEBDPKGBOGJ.Return = true;
				return true;
			case 44:
			case 58:
			case 91:
			case 93:
			case 123:
			case 125:
				IEBDPKGBOGJ.NextState = 1;
				IEBDPKGBOGJ.Return = true;
				return true;
			case 45:
				IEBDPKGBOGJ.L.string_buffer.Append((char)IEBDPKGBOGJ.L.input_char);
				IEBDPKGBOGJ.NextState = 2;
				return true;
			case 48:
				IEBDPKGBOGJ.L.string_buffer.Append((char)IEBDPKGBOGJ.L.input_char);
				IEBDPKGBOGJ.NextState = 4;
				return true;
			case 102:
				IEBDPKGBOGJ.NextState = 12;
				return true;
			case 110:
				IEBDPKGBOGJ.NextState = 16;
				return true;
			case 116:
				IEBDPKGBOGJ.NextState = 9;
				return true;
			case 39:
				if (!IEBDPKGBOGJ.L.allow_single_quoted_strings)
				{
					return false;
				}
				IEBDPKGBOGJ.L.input_char = 34;
				IEBDPKGBOGJ.NextState = 23;
				IEBDPKGBOGJ.Return = true;
				return true;
			case 47:
				if (!IEBDPKGBOGJ.L.allow_comments)
				{
					return false;
				}
				IEBDPKGBOGJ.NextState = 25;
				return true;
			default:
				return false;
			}
		}
		return true;
	}

	private static bool State2(FsmContext IEBDPKGBOGJ)
	{
		IEBDPKGBOGJ.L.GetChar();
		if (IEBDPKGBOGJ.L.input_char >= 49 && IEBDPKGBOGJ.L.input_char <= 57)
		{
			IEBDPKGBOGJ.L.string_buffer.Append((char)IEBDPKGBOGJ.L.input_char);
			IEBDPKGBOGJ.NextState = 3;
			return true;
		}
		int lJJGFHFKGHN = IEBDPKGBOGJ.L.input_char;
		if (lJJGFHFKGHN == 48)
		{
			IEBDPKGBOGJ.L.string_buffer.Append((char)IEBDPKGBOGJ.L.input_char);
			IEBDPKGBOGJ.NextState = 4;
			return true;
		}
		return false;
	}

	private static bool State3(FsmContext IEBDPKGBOGJ)
	{
		while (IEBDPKGBOGJ.L.GetChar())
		{
			if (IEBDPKGBOGJ.L.input_char >= 48 && IEBDPKGBOGJ.L.input_char <= 57)
			{
				IEBDPKGBOGJ.L.string_buffer.Append((char)IEBDPKGBOGJ.L.input_char);
				continue;
			}
			if (IEBDPKGBOGJ.L.input_char == 32 || (IEBDPKGBOGJ.L.input_char >= 9 && IEBDPKGBOGJ.L.input_char <= 13))
			{
				IEBDPKGBOGJ.Return = true;
				IEBDPKGBOGJ.NextState = 1;
				return true;
			}
			switch (IEBDPKGBOGJ.L.input_char)
			{
			case 44:
			case 93:
			case 125:
				IEBDPKGBOGJ.L.UngetChar();
				IEBDPKGBOGJ.Return = true;
				IEBDPKGBOGJ.NextState = 1;
				return true;
			case 46:
				IEBDPKGBOGJ.L.string_buffer.Append((char)IEBDPKGBOGJ.L.input_char);
				IEBDPKGBOGJ.NextState = 5;
				return true;
			case 69:
			case 101:
				IEBDPKGBOGJ.L.string_buffer.Append((char)IEBDPKGBOGJ.L.input_char);
				IEBDPKGBOGJ.NextState = 7;
				return true;
			default:
				return false;
			}
		}
		return true;
	}

	private static bool State4(FsmContext IEBDPKGBOGJ)
	{
		IEBDPKGBOGJ.L.GetChar();
		if (IEBDPKGBOGJ.L.input_char == 32 || (IEBDPKGBOGJ.L.input_char >= 9 && IEBDPKGBOGJ.L.input_char <= 13))
		{
			IEBDPKGBOGJ.Return = true;
			IEBDPKGBOGJ.NextState = 1;
			return true;
		}
		switch (IEBDPKGBOGJ.L.input_char)
		{
		case 44:
		case 93:
		case 125:
			IEBDPKGBOGJ.L.UngetChar();
			IEBDPKGBOGJ.Return = true;
			IEBDPKGBOGJ.NextState = 1;
			return true;
		case 46:
			IEBDPKGBOGJ.L.string_buffer.Append((char)IEBDPKGBOGJ.L.input_char);
			IEBDPKGBOGJ.NextState = 5;
			return true;
		case 69:
		case 101:
			IEBDPKGBOGJ.L.string_buffer.Append((char)IEBDPKGBOGJ.L.input_char);
			IEBDPKGBOGJ.NextState = 7;
			return true;
		default:
			return false;
		}
	}

	private static bool State5(FsmContext IEBDPKGBOGJ)
	{
		IEBDPKGBOGJ.L.GetChar();
		if (IEBDPKGBOGJ.L.input_char >= 48 && IEBDPKGBOGJ.L.input_char <= 57)
		{
			IEBDPKGBOGJ.L.string_buffer.Append((char)IEBDPKGBOGJ.L.input_char);
			IEBDPKGBOGJ.NextState = 6;
			return true;
		}
		return false;
	}

	private static bool State6(FsmContext IEBDPKGBOGJ)
	{
		while (IEBDPKGBOGJ.L.GetChar())
		{
			if (IEBDPKGBOGJ.L.input_char >= 48 && IEBDPKGBOGJ.L.input_char <= 57)
			{
				IEBDPKGBOGJ.L.string_buffer.Append((char)IEBDPKGBOGJ.L.input_char);
				continue;
			}
			if (IEBDPKGBOGJ.L.input_char == 32 || (IEBDPKGBOGJ.L.input_char >= 9 && IEBDPKGBOGJ.L.input_char <= 13))
			{
				IEBDPKGBOGJ.Return = true;
				IEBDPKGBOGJ.NextState = 1;
				return true;
			}
			switch (IEBDPKGBOGJ.L.input_char)
			{
			case 44:
			case 93:
			case 125:
				IEBDPKGBOGJ.L.UngetChar();
				IEBDPKGBOGJ.Return = true;
				IEBDPKGBOGJ.NextState = 1;
				return true;
			case 69:
			case 101:
				IEBDPKGBOGJ.L.string_buffer.Append((char)IEBDPKGBOGJ.L.input_char);
				IEBDPKGBOGJ.NextState = 7;
				return true;
			default:
				return false;
			}
		}
		return true;
	}

	private static bool State7(FsmContext IEBDPKGBOGJ)
	{
		IEBDPKGBOGJ.L.GetChar();
		if (IEBDPKGBOGJ.L.input_char >= 48 && IEBDPKGBOGJ.L.input_char <= 57)
		{
			IEBDPKGBOGJ.L.string_buffer.Append((char)IEBDPKGBOGJ.L.input_char);
			IEBDPKGBOGJ.NextState = 8;
			return true;
		}
		int lJJGFHFKGHN = IEBDPKGBOGJ.L.input_char;
		if (lJJGFHFKGHN == 43 || lJJGFHFKGHN == 45)
		{
			IEBDPKGBOGJ.L.string_buffer.Append((char)IEBDPKGBOGJ.L.input_char);
			IEBDPKGBOGJ.NextState = 8;
			return true;
		}
		return false;
	}

	private static bool State8(FsmContext IEBDPKGBOGJ)
	{
		while (IEBDPKGBOGJ.L.GetChar())
		{
			if (IEBDPKGBOGJ.L.input_char >= 48 && IEBDPKGBOGJ.L.input_char <= 57)
			{
				IEBDPKGBOGJ.L.string_buffer.Append((char)IEBDPKGBOGJ.L.input_char);
				continue;
			}
			if (IEBDPKGBOGJ.L.input_char == 32 || (IEBDPKGBOGJ.L.input_char >= 9 && IEBDPKGBOGJ.L.input_char <= 13))
			{
				IEBDPKGBOGJ.Return = true;
				IEBDPKGBOGJ.NextState = 1;
				return true;
			}
			int lJJGFHFKGHN = IEBDPKGBOGJ.L.input_char;
			if (lJJGFHFKGHN == 44 || lJJGFHFKGHN == 93 || lJJGFHFKGHN == 125)
			{
				IEBDPKGBOGJ.L.UngetChar();
				IEBDPKGBOGJ.Return = true;
				IEBDPKGBOGJ.NextState = 1;
				return true;
			}
			return false;
		}
		return true;
	}

	private static bool State9(FsmContext IEBDPKGBOGJ)
	{
		IEBDPKGBOGJ.L.GetChar();
		int lJJGFHFKGHN = IEBDPKGBOGJ.L.input_char;
		if (lJJGFHFKGHN == 114)
		{
			IEBDPKGBOGJ.NextState = 10;
			return true;
		}
		return false;
	}

	private static bool State10(FsmContext IEBDPKGBOGJ)
	{
		IEBDPKGBOGJ.L.GetChar();
		int lJJGFHFKGHN = IEBDPKGBOGJ.L.input_char;
		if (lJJGFHFKGHN == 117)
		{
			IEBDPKGBOGJ.NextState = 11;
			return true;
		}
		return false;
	}

	private static bool State11(FsmContext IEBDPKGBOGJ)
	{
		IEBDPKGBOGJ.L.GetChar();
		int lJJGFHFKGHN = IEBDPKGBOGJ.L.input_char;
		if (lJJGFHFKGHN == 101)
		{
			IEBDPKGBOGJ.Return = true;
			IEBDPKGBOGJ.NextState = 1;
			return true;
		}
		return false;
	}

	private static bool State12(FsmContext IEBDPKGBOGJ)
	{
		IEBDPKGBOGJ.L.GetChar();
		int lJJGFHFKGHN = IEBDPKGBOGJ.L.input_char;
		if (lJJGFHFKGHN == 97)
		{
			IEBDPKGBOGJ.NextState = 13;
			return true;
		}
		return false;
	}

	private static bool State13(FsmContext IEBDPKGBOGJ)
	{
		IEBDPKGBOGJ.L.GetChar();
		int lJJGFHFKGHN = IEBDPKGBOGJ.L.input_char;
		if (lJJGFHFKGHN == 108)
		{
			IEBDPKGBOGJ.NextState = 14;
			return true;
		}
		return false;
	}

	private static bool State14(FsmContext IEBDPKGBOGJ)
	{
		IEBDPKGBOGJ.L.GetChar();
		int lJJGFHFKGHN = IEBDPKGBOGJ.L.input_char;
		if (lJJGFHFKGHN == 115)
		{
			IEBDPKGBOGJ.NextState = 15;
			return true;
		}
		return false;
	}

	private static bool State15(FsmContext IEBDPKGBOGJ)
	{
		IEBDPKGBOGJ.L.GetChar();
		int lJJGFHFKGHN = IEBDPKGBOGJ.L.input_char;
		if (lJJGFHFKGHN == 101)
		{
			IEBDPKGBOGJ.Return = true;
			IEBDPKGBOGJ.NextState = 1;
			return true;
		}
		return false;
	}

	private static bool State16(FsmContext IEBDPKGBOGJ)
	{
		IEBDPKGBOGJ.L.GetChar();
		int lJJGFHFKGHN = IEBDPKGBOGJ.L.input_char;
		if (lJJGFHFKGHN == 117)
		{
			IEBDPKGBOGJ.NextState = 17;
			return true;
		}
		return false;
	}

	private static bool State17(FsmContext IEBDPKGBOGJ)
	{
		IEBDPKGBOGJ.L.GetChar();
		int lJJGFHFKGHN = IEBDPKGBOGJ.L.input_char;
		if (lJJGFHFKGHN == 108)
		{
			IEBDPKGBOGJ.NextState = 18;
			return true;
		}
		return false;
	}

	private static bool State18(FsmContext IEBDPKGBOGJ)
	{
		IEBDPKGBOGJ.L.GetChar();
		int lJJGFHFKGHN = IEBDPKGBOGJ.L.input_char;
		if (lJJGFHFKGHN == 108)
		{
			IEBDPKGBOGJ.Return = true;
			IEBDPKGBOGJ.NextState = 1;
			return true;
		}
		return false;
	}

	private static bool State19(FsmContext IEBDPKGBOGJ)
	{
		while (IEBDPKGBOGJ.L.GetChar())
		{
			switch (IEBDPKGBOGJ.L.input_char)
			{
			case 34:
				IEBDPKGBOGJ.L.UngetChar();
				IEBDPKGBOGJ.Return = true;
				IEBDPKGBOGJ.NextState = 20;
				return true;
			case 92:
				IEBDPKGBOGJ.StateStack = 19;
				IEBDPKGBOGJ.NextState = 21;
				return true;
			}
			IEBDPKGBOGJ.L.string_buffer.Append((char)IEBDPKGBOGJ.L.input_char);
		}
		return true;
	}

	private static bool State20(FsmContext IEBDPKGBOGJ)
	{
		IEBDPKGBOGJ.L.GetChar();
		int lJJGFHFKGHN = IEBDPKGBOGJ.L.input_char;
		if (lJJGFHFKGHN == 34)
		{
			IEBDPKGBOGJ.Return = true;
			IEBDPKGBOGJ.NextState = 1;
			return true;
		}
		return false;
	}

	private static bool State21(FsmContext IEBDPKGBOGJ)
	{
		IEBDPKGBOGJ.L.GetChar();
		switch (IEBDPKGBOGJ.L.input_char)
		{
		case 117:
			IEBDPKGBOGJ.NextState = 22;
			return true;
		case 34:
		case 39:
		case 47:
		case 92:
		case 98:
		case 102:
		case 110:
		case 114:
		case 116:
			IEBDPKGBOGJ.L.string_buffer.Append(ProcessEscChar(IEBDPKGBOGJ.L.input_char));
			IEBDPKGBOGJ.NextState = IEBDPKGBOGJ.StateStack;
			return true;
		default:
			return false;
		}
	}

	private static bool State22(FsmContext IEBDPKGBOGJ)
	{
		int num = 0;
		int num2 = 4096;
		IEBDPKGBOGJ.L.unichar = 0;
		while (IEBDPKGBOGJ.L.GetChar())
		{
			if ((IEBDPKGBOGJ.L.input_char >= 48 && IEBDPKGBOGJ.L.input_char <= 57) || (IEBDPKGBOGJ.L.input_char >= 65 && IEBDPKGBOGJ.L.input_char <= 70) || (IEBDPKGBOGJ.L.input_char >= 97 && IEBDPKGBOGJ.L.input_char <= 102))
			{
				IEBDPKGBOGJ.L.unichar += HexValue(IEBDPKGBOGJ.L.input_char) * num2;
				num++;
				num2 /= 16;
				if (num == 4)
				{
					IEBDPKGBOGJ.L.string_buffer.Append(Convert.ToChar(IEBDPKGBOGJ.L.unichar));
					IEBDPKGBOGJ.NextState = IEBDPKGBOGJ.StateStack;
					return true;
				}
				continue;
			}
			return false;
		}
		return true;
	}

	private static bool State23(FsmContext IEBDPKGBOGJ)
	{
		while (IEBDPKGBOGJ.L.GetChar())
		{
			switch (IEBDPKGBOGJ.L.input_char)
			{
			case 39:
				IEBDPKGBOGJ.L.UngetChar();
				IEBDPKGBOGJ.Return = true;
				IEBDPKGBOGJ.NextState = 24;
				return true;
			case 92:
				IEBDPKGBOGJ.StateStack = 23;
				IEBDPKGBOGJ.NextState = 21;
				return true;
			}
			IEBDPKGBOGJ.L.string_buffer.Append((char)IEBDPKGBOGJ.L.input_char);
		}
		return true;
	}

	private static bool State24(FsmContext IEBDPKGBOGJ)
	{
		IEBDPKGBOGJ.L.GetChar();
		int lJJGFHFKGHN = IEBDPKGBOGJ.L.input_char;
		if (lJJGFHFKGHN == 39)
		{
			IEBDPKGBOGJ.L.input_char = 34;
			IEBDPKGBOGJ.Return = true;
			IEBDPKGBOGJ.NextState = 1;
			return true;
		}
		return false;
	}

	private static bool State25(FsmContext IEBDPKGBOGJ)
	{
		IEBDPKGBOGJ.L.GetChar();
		switch (IEBDPKGBOGJ.L.input_char)
		{
		case 42:
			IEBDPKGBOGJ.NextState = 27;
			return true;
		case 47:
			IEBDPKGBOGJ.NextState = 26;
			return true;
		default:
			return false;
		}
	}

	private static bool State26(FsmContext IEBDPKGBOGJ)
	{
		while (IEBDPKGBOGJ.L.GetChar())
		{
			if (IEBDPKGBOGJ.L.input_char == 10)
			{
				IEBDPKGBOGJ.NextState = 1;
				return true;
			}
		}
		return true;
	}

	private static bool State27(FsmContext IEBDPKGBOGJ)
	{
		while (IEBDPKGBOGJ.L.GetChar())
		{
			if (IEBDPKGBOGJ.L.input_char == 42)
			{
				IEBDPKGBOGJ.NextState = 28;
				return true;
			}
		}
		return true;
	}

	private static bool State28(FsmContext IEBDPKGBOGJ)
	{
		while (IEBDPKGBOGJ.L.GetChar())
		{
			if (IEBDPKGBOGJ.L.input_char == 42)
			{
				continue;
			}
			if (IEBDPKGBOGJ.L.input_char == 47)
			{
				IEBDPKGBOGJ.NextState = 1;
				return true;
			}
			IEBDPKGBOGJ.NextState = 27;
			return true;
		}
		return true;
	}

	private bool GetChar()
	{
		if ((input_char = NextChar()) != -1)
		{
			return true;
		}
		end_of_input = true;
		return false;
	}

	private int NextChar()
	{
		if (input_buffer != 0)
		{
			int kCLLHLMNMKO = input_buffer;
			input_buffer = 0;
			return kCLLHLMNMKO;
		}
		return reader.Read();
	}

	public bool NextToken()
	{
		fsm_context.Return = false;
		while (true)
		{
			StateHandler mGLALMHHOGL = fsm_handler_table[state - 1];
			if (!mGLALMHHOGL(fsm_context))
			{
				throw new JsonException(input_char);
			}
			if (end_of_input)
			{
				return false;
			}
			if (fsm_context.Return)
			{
				break;
			}
			state = fsm_context.NextState;
		}
		string_value = string_buffer.ToString();
		string_buffer.Remove(0, string_buffer.Length);
		token = fsm_return_table[state - 1];
		if (token == 65542)
		{
			token = input_char;
		}
		state = fsm_context.NextState;
		return true;
	}

	private void UngetChar()
	{
		input_buffer = input_char;
	}
}
