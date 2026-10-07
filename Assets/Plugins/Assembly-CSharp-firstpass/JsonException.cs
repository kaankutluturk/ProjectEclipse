using System;

public class JsonException : Exception
{
	public JsonException()
	{
	}

	internal JsonException(ParserToken token)
		: base(string.Format("Invalid token '{0}' in input string", token))
	{
	}

	internal JsonException(ParserToken token, Exception innerException)
		: base(string.Format("Invalid token '{0}' in input string", token), innerException)
	{
	}

	internal JsonException(int character)
		: base(string.Format("Invalid character '{0}' in input string", (char)character))
	{
	}

	internal JsonException(int character, Exception innerException)
		: base(string.Format("Invalid character '{0}' in input string", (char)character), innerException)
	{
	}

	public JsonException(string message)
		: base(message)
	{
	}

	public JsonException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
