using System;

[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class EnumMemberAttribute : Attribute
{
	private string value;

	public string GetValue()
	{
		return value;
	}

	public void set_Value(string value)
	{
		this.value = value;
	}
}
