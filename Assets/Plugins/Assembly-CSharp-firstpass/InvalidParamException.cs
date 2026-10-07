using System;

internal class InvalidParamException : ApplicationException
{
	public InvalidParamException()
		: base("Invalid Parameter")
	{
	}
}
