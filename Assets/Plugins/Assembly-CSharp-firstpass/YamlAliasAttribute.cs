using System;
using System.Diagnostics;

[Obsolete("Please use YamlMember instead")]
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class YamlAliasAttribute : Attribute
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string alias;

	public string AliasName
	{
		get
		{
			return GetAlias();
		}
		set
		{
			set_Alias(value);
		}
	}

	public YamlAliasAttribute(string alias)
	{
		set_Alias(alias);
	}

	public string GetAlias()
	{
		return alias;
	}

	public void set_Alias(string value)
	{
		alias = value;
	}
}
