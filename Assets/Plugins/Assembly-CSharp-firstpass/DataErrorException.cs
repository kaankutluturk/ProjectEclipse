using System;

internal class DataErrorException : ApplicationException
{
	public DataErrorException()
		: base("Data Error")
	{
	}
}
