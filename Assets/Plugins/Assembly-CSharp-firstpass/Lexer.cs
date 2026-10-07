using System;
using System.IO;
using System.Text;

internal class Lexer
{
	private delegate bool StateHandler(FsmContext context);

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

	private static int HexValue(int digit)
	{
		switch (digit)
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
			return digit - 48;
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

	private static char ProcessEscChar(int escapedChar)
	{
		switch (escapedChar)
		{
		case 34:
		case 39:
		case 47:
		case 92:
			return Convert.ToChar(escapedChar);
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

	private static bool State1(FsmContext context)
	{
		while (context.L.GetChar())
		{
			if (context.L.input_char == 32 || (context.L.input_char >= 9 && context.L.input_char <= 13))
			{
				continue;
			}
			if (context.L.input_char >= 49 && context.L.input_char <= 57)
			{
				context.L.string_buffer.Append((char)context.L.input_char);
				context.NextState = 3;
				return true;
			}
			switch (context.L.input_char)
			{
			case 34:
				context.NextState = 19;
				context.Return = true;
				return true;
			case 44:
			case 58:
			case 91:
			case 93:
			case 123:
			case 125:
				context.NextState = 1;
				context.Return = true;
				return true;
			case 45:
				context.L.string_buffer.Append((char)context.L.input_char);
				context.NextState = 2;
				return true;
			case 48:
				context.L.string_buffer.Append((char)context.L.input_char);
				context.NextState = 4;
				return true;
			case 102:
				context.NextState = 12;
				return true;
			case 110:
				context.NextState = 16;
				return true;
			case 116:
				context.NextState = 9;
				return true;
			case 39:
				if (!context.L.allow_single_quoted_strings)
				{
					return false;
				}
				context.L.input_char = 34;
				context.NextState = 23;
				context.Return = true;
				return true;
			case 47:
				if (!context.L.allow_comments)
				{
					return false;
				}
				context.NextState = 25;
				return true;
			default:
				return false;
			}
		}
		return true;
	}

	private static bool State2(FsmContext context)
	{
		context.L.GetChar();
		if (context.L.input_char >= 49 && context.L.input_char <= 57)
		{
			context.L.string_buffer.Append((char)context.L.input_char);
			context.NextState = 3;
			return true;
		}
		int inputChar = context.L.input_char;
		if (inputChar == 48)
		{
			context.L.string_buffer.Append((char)context.L.input_char);
			context.NextState = 4;
			return true;
		}
		return false;
	}

	private static bool State3(FsmContext context)
	{
		while (context.L.GetChar())
		{
			if (context.L.input_char >= 48 && context.L.input_char <= 57)
			{
				context.L.string_buffer.Append((char)context.L.input_char);
				continue;
			}
			if (context.L.input_char == 32 || (context.L.input_char >= 9 && context.L.input_char <= 13))
			{
				context.Return = true;
				context.NextState = 1;
				return true;
			}
			switch (context.L.input_char)
			{
			case 44:
			case 93:
			case 125:
				context.L.UngetChar();
				context.Return = true;
				context.NextState = 1;
				return true;
			case 46:
				context.L.string_buffer.Append((char)context.L.input_char);
				context.NextState = 5;
				return true;
			case 69:
			case 101:
				context.L.string_buffer.Append((char)context.L.input_char);
				context.NextState = 7;
				return true;
			default:
				return false;
			}
		}
		return true;
	}

	private static bool State4(FsmContext context)
	{
		context.L.GetChar();
		if (context.L.input_char == 32 || (context.L.input_char >= 9 && context.L.input_char <= 13))
		{
			context.Return = true;
			context.NextState = 1;
			return true;
		}
		switch (context.L.input_char)
		{
		case 44:
		case 93:
		case 125:
			context.L.UngetChar();
			context.Return = true;
			context.NextState = 1;
			return true;
		case 46:
			context.L.string_buffer.Append((char)context.L.input_char);
			context.NextState = 5;
			return true;
		case 69:
		case 101:
			context.L.string_buffer.Append((char)context.L.input_char);
			context.NextState = 7;
			return true;
		default:
			return false;
		}
	}

	private static bool State5(FsmContext context)
	{
		context.L.GetChar();
		if (context.L.input_char >= 48 && context.L.input_char <= 57)
		{
			context.L.string_buffer.Append((char)context.L.input_char);
			context.NextState = 6;
			return true;
		}
		return false;
	}

	private static bool State6(FsmContext context)
	{
		while (context.L.GetChar())
		{
			if (context.L.input_char >= 48 && context.L.input_char <= 57)
			{
				context.L.string_buffer.Append((char)context.L.input_char);
				continue;
			}
			if (context.L.input_char == 32 || (context.L.input_char >= 9 && context.L.input_char <= 13))
			{
				context.Return = true;
				context.NextState = 1;
				return true;
			}
			switch (context.L.input_char)
			{
			case 44:
			case 93:
			case 125:
				context.L.UngetChar();
				context.Return = true;
				context.NextState = 1;
				return true;
			case 69:
			case 101:
				context.L.string_buffer.Append((char)context.L.input_char);
				context.NextState = 7;
				return true;
			default:
				return false;
			}
		}
		return true;
	}

	private static bool State7(FsmContext context)
	{
		context.L.GetChar();
		if (context.L.input_char >= 48 && context.L.input_char <= 57)
		{
			context.L.string_buffer.Append((char)context.L.input_char);
			context.NextState = 8;
			return true;
		}
		int inputChar = context.L.input_char;
		if (inputChar == 43 || inputChar == 45)
		{
			context.L.string_buffer.Append((char)context.L.input_char);
			context.NextState = 8;
			return true;
		}
		return false;
	}

	private static bool State8(FsmContext context)
	{
		while (context.L.GetChar())
		{
			if (context.L.input_char >= 48 && context.L.input_char <= 57)
			{
				context.L.string_buffer.Append((char)context.L.input_char);
				continue;
			}
			if (context.L.input_char == 32 || (context.L.input_char >= 9 && context.L.input_char <= 13))
			{
				context.Return = true;
				context.NextState = 1;
				return true;
			}
			int inputChar = context.L.input_char;
			if (inputChar == 44 || inputChar == 93 || inputChar == 125)
			{
				context.L.UngetChar();
				context.Return = true;
				context.NextState = 1;
				return true;
			}
			return false;
		}
		return true;
	}

	private static bool State9(FsmContext context)
	{
		context.L.GetChar();
		int inputChar = context.L.input_char;
		if (inputChar == 114)
		{
			context.NextState = 10;
			return true;
		}
		return false;
	}

	private static bool State10(FsmContext context)
	{
		context.L.GetChar();
		int inputChar = context.L.input_char;
		if (inputChar == 117)
		{
			context.NextState = 11;
			return true;
		}
		return false;
	}

	private static bool State11(FsmContext context)
	{
		context.L.GetChar();
		int inputChar = context.L.input_char;
		if (inputChar == 101)
		{
			context.Return = true;
			context.NextState = 1;
			return true;
		}
		return false;
	}

	private static bool State12(FsmContext context)
	{
		context.L.GetChar();
		int inputChar = context.L.input_char;
		if (inputChar == 97)
		{
			context.NextState = 13;
			return true;
		}
		return false;
	}

	private static bool State13(FsmContext context)
	{
		context.L.GetChar();
		int inputChar = context.L.input_char;
		if (inputChar == 108)
		{
			context.NextState = 14;
			return true;
		}
		return false;
	}

	private static bool State14(FsmContext context)
	{
		context.L.GetChar();
		int inputChar = context.L.input_char;
		if (inputChar == 115)
		{
			context.NextState = 15;
			return true;
		}
		return false;
	}

	private static bool State15(FsmContext context)
	{
		context.L.GetChar();
		int inputChar = context.L.input_char;
		if (inputChar == 101)
		{
			context.Return = true;
			context.NextState = 1;
			return true;
		}
		return false;
	}

	private static bool State16(FsmContext context)
	{
		context.L.GetChar();
		int inputChar = context.L.input_char;
		if (inputChar == 117)
		{
			context.NextState = 17;
			return true;
		}
		return false;
	}

	private static bool State17(FsmContext context)
	{
		context.L.GetChar();
		int inputChar = context.L.input_char;
		if (inputChar == 108)
		{
			context.NextState = 18;
			return true;
		}
		return false;
	}

	private static bool State18(FsmContext context)
	{
		context.L.GetChar();
		int inputChar = context.L.input_char;
		if (inputChar == 108)
		{
			context.Return = true;
			context.NextState = 1;
			return true;
		}
		return false;
	}

	private static bool State19(FsmContext context)
	{
		while (context.L.GetChar())
		{
			switch (context.L.input_char)
			{
			case 34:
				context.L.UngetChar();
				context.Return = true;
				context.NextState = 20;
				return true;
			case 92:
				context.StateStack = 19;
				context.NextState = 21;
				return true;
			}
			context.L.string_buffer.Append((char)context.L.input_char);
		}
		return true;
	}

	private static bool State20(FsmContext context)
	{
		context.L.GetChar();
		int inputChar = context.L.input_char;
		if (inputChar == 34)
		{
			context.Return = true;
			context.NextState = 1;
			return true;
		}
		return false;
	}

	private static bool State21(FsmContext context)
	{
		context.L.GetChar();
		switch (context.L.input_char)
		{
		case 117:
			context.NextState = 22;
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
			context.L.string_buffer.Append(ProcessEscChar(context.L.input_char));
			context.NextState = context.StateStack;
			return true;
		default:
			return false;
		}
	}

	private static bool State22(FsmContext context)
	{
		int num = 0;
		int num2 = 4096;
		context.L.unichar = 0;
		while (context.L.GetChar())
		{
			if ((context.L.input_char >= 48 && context.L.input_char <= 57) || (context.L.input_char >= 65 && context.L.input_char <= 70) || (context.L.input_char >= 97 && context.L.input_char <= 102))
			{
				context.L.unichar += HexValue(context.L.input_char) * num2;
				num++;
				num2 /= 16;
				if (num == 4)
				{
					context.L.string_buffer.Append(Convert.ToChar(context.L.unichar));
					context.NextState = context.StateStack;
					return true;
				}
				continue;
			}
			return false;
		}
		return true;
	}

	private static bool State23(FsmContext context)
	{
		while (context.L.GetChar())
		{
			switch (context.L.input_char)
			{
			case 39:
				context.L.UngetChar();
				context.Return = true;
				context.NextState = 24;
				return true;
			case 92:
				context.StateStack = 23;
				context.NextState = 21;
				return true;
			}
			context.L.string_buffer.Append((char)context.L.input_char);
		}
		return true;
	}

	private static bool State24(FsmContext context)
	{
		context.L.GetChar();
		int inputChar = context.L.input_char;
		if (inputChar == 39)
		{
			context.L.input_char = 34;
			context.Return = true;
			context.NextState = 1;
			return true;
		}
		return false;
	}

	private static bool State25(FsmContext context)
	{
		context.L.GetChar();
		switch (context.L.input_char)
		{
		case 42:
			context.NextState = 27;
			return true;
		case 47:
			context.NextState = 26;
			return true;
		default:
			return false;
		}
	}

	private static bool State26(FsmContext context)
	{
		while (context.L.GetChar())
		{
			if (context.L.input_char == 10)
			{
				context.NextState = 1;
				return true;
			}
		}
		return true;
	}

	private static bool State27(FsmContext context)
	{
		while (context.L.GetChar())
		{
			if (context.L.input_char == 42)
			{
				context.NextState = 28;
				return true;
			}
		}
		return true;
	}

	private static bool State28(FsmContext context)
	{
		while (context.L.GetChar())
		{
			if (context.L.input_char == 42)
			{
				continue;
			}
			if (context.L.input_char == 47)
			{
				context.NextState = 1;
				return true;
			}
			context.NextState = 27;
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
			int bufferedChar = input_buffer;
			input_buffer = 0;
			return bufferedChar;
		}
		return reader.Read();
	}

	public bool NextToken()
	{
		fsm_context.Return = false;
		while (true)
		{
			StateHandler handler = fsm_handler_table[state - 1];
			if (!handler(fsm_context))
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
