using System;
using System.Collections.Generic;
using System.Linq;

public class CommandLineReader
{
	private const string CUSTOM_ARGS_PREFIX = "-CustomArgs:";

	private const char CUSTOM_ARGS_SEPARATOR = ';';

	public static string[] GetCommandLineArgs()
	{
		return Environment.GetCommandLineArgs();
	}

	public static string GetCommandLine()
	{
		string[] array = GetCommandLineArgs();
		if (array.Length > 0)
		{
			return string.Join(" ", array);
		}
		AdvLog.LogError("CommandLineReader.cs - GetCommandLine() - Can't find any command line arguments!");
		return string.Empty;
	}

	public static Dictionary<string, string> GetCustomArguments()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		string[] array = GetCommandLineArgs();
		string empty = string.Empty;
		try
		{
			empty = array.Where((string argument) => argument.Contains("-CustomArgs:")).Single();
		}
		catch (Exception ex)
		{
			AdvLog.LogError(string.Concat("CommandLineReader.cs - GetCustomArguments() - Can't retrieve any custom arguments in the command line [", array, "]. Exception: ", ex));
			return dictionary;
		}
		empty = empty.Replace("-CustomArgs:", string.Empty);
		string[] array2 = empty.Split(';');
		string[] array3 = array2;
		foreach (string text in array3)
		{
			string[] array4 = text.Split('=');
			if (array4.Length == 2)
			{
				dictionary.Add(array4[0], array4[1]);
			}
			else
			{
				AdvLog.LogWarning("CommandLineReader.cs - GetCustomArguments() - The custom argument [" + text + "] seem to be malformed.");
			}
		}
		return dictionary;
	}

	public static string GetCustomArgument(string name)
	{
		Dictionary<string, string> dictionary = GetCustomArguments();
		if (dictionary.ContainsKey(name))
		{
			return dictionary[name];
		}
		AdvLog.LogError("CommandLineReader.cs - GetCustomArgument() - Can't retrieve any custom argument named [" + name + "] in the command line [" + GetCommandLine() + "].");
		return string.Empty;
	}
}
