using System;
using System.Collections.Generic;
using System.Diagnostics;

public static class ConsoleDatabase
{
	public delegate string CommandFunction(params string[] LKIOKGCNKHE);

	private class CommandData
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private CommandFunction _handler;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private bool _lowercaseArguments;

		public CommandFunction Handler
		{
			get
			{
				return GetHandler();
			}
			private set
			{
				SetHandler(value);
			}
		}

		public bool LowercaseArguments
		{
			get
			{
				return GetIgnoreCase();
			}
			private set
			{
				set_IgnoreCase(value);
			}
		}

		public CommandData(CommandFunction MHAEIBNCMPL, bool DLEJFILIEGL)
		{
			SetHandler(MHAEIBNCMPL);
			set_IgnoreCase(DLEJFILIEGL);
		}

		public CommandFunction GetHandler()
		{
			return _handler;
		}

		private void SetHandler(CommandFunction value)
		{
			_handler = value;
		}

		public bool GetIgnoreCase()
		{
			return _lowercaseArguments;
		}

		private void set_IgnoreCase(bool value)
		{
			_lowercaseArguments = value;
		}
	}

	private const string _CommandArgsSeparator = " ";

	private static readonly char[] _ArgsSeparators = new char[1] { ' ' };

	private static readonly Dictionary<string, CommandData> _commands = new Dictionary<string, CommandData>();

	public static bool HasCommand(string name)
	{
		name = name.ToLower();
		return _commands.ContainsKey(name) && _commands[name] != null;
	}

	public static string ExecuteCommand(string LEKEGLMDAHA)
	{
		if (!string.IsNullOrEmpty(LEKEGLMDAHA))
		{
			LEKEGLMDAHA = LEKEGLMDAHA.Trim(' ');
			int num = ((!LEKEGLMDAHA.Contains(" ")) ? LEKEGLMDAHA.Length : LEKEGLMDAHA.IndexOf(" ", StringComparison.Ordinal));
			string text = LEKEGLMDAHA.Substring(0, num).ToLower();
			LEKEGLMDAHA = LEKEGLMDAHA.Remove(0, num);
			LEKEGLMDAHA = LEKEGLMDAHA.Trim(' ');
			if (HasCommand(text))
			{
				CommandData kHGKFJFOEBE = _commands[text];
				if (kHGKFJFOEBE.GetIgnoreCase())
				{
					LEKEGLMDAHA = LEKEGLMDAHA.ToLower();
				}
				string[] lKIOKGCNKHE = LEKEGLMDAHA.Split(_ArgsSeparators, StringSplitOptions.RemoveEmptyEntries);
				return kHGKFJFOEBE.GetHandler()(lKIOKGCNKHE);
			}
		}
		return "Unknown command!";
	}

	public static void RegisterCommand(string name, CommandFunction LEPDMLGJCKI, bool DLEJFILIEGL = true)
	{
		_commands[name.ToLower()] = new CommandData(LEPDMLGJCKI, DLEJFILIEGL);
	}

	public static void UnregisterCommand(string name)
	{
		_commands.Remove(name.ToLower());
	}
}
