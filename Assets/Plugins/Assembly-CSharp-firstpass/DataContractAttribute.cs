using System;
using System.Diagnostics;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
public sealed class DataContractAttribute : Attribute
{
	private string name;

	private string ns;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool isReference;

	public string ContractName
	{
		get
		{
			return get_Name();
		}
		set
		{
			set_Name(value);
		}
	}

	public bool UsesReferenceSemantics
	{
		get
		{
			return GetIsReference();
		}
		set
		{
			set_IsReference(value);
		}
	}

	public string get_Name()
	{
		return name;
	}

	public void set_Name(string value)
	{
		name = value;
	}

	public string GetNamespace()
	{
		return ns;
	}

	public void set_Namespace(string value)
	{
		ns = value;
	}

	public bool GetIsReference()
	{
		return isReference;
	}

	public void set_IsReference(bool value)
	{
		isReference = value;
	}
}
