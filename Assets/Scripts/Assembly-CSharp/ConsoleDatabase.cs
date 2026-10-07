using System;
using System.Collections.Generic;
using System.Diagnostics;

public static class ConsoleDatabase
{
	public delegate string CommandFunction(params string[] arguments);

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

		public CommandData(CommandFunction handler, bool ignoreCase)
		{
			SetHandler(handler);
			set_IgnoreCase(ignoreCase);
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

	public static string ExecuteCommand(string commandLine)
	{
		if (!string.IsNullOrEmpty(commandLine))
		{
			commandLine = commandLine.Trim(' ');
			int num = ((!commandLine.Contains(" ")) ? commandLine.Length : commandLine.IndexOf(" ", StringComparison.Ordinal));
			string text = commandLine.Substring(0, num).ToLower();
			commandLine = commandLine.Remove(0, num);
			commandLine = commandLine.Trim(' ');
			if (HasCommand(text))
			{
				CommandData command = _commands[text];
				if (command.GetIgnoreCase())
				{
					commandLine = commandLine.ToLower();
				}
				string[] arguments = commandLine.Split(_ArgsSeparators, StringSplitOptions.RemoveEmptyEntries);
				return command.GetHandler()(arguments);
			}
		}
		return "Unknown command!";
	}

	public static void RegisterCommand(string name, CommandFunction handler, bool ignoreCase = true)
	{
		_commands[name.ToLower()] = new CommandData(handler, ignoreCase);
	}

	public static void UnregisterCommand(string name)
	{
		_commands.Remove(name.ToLower());
	}
}
