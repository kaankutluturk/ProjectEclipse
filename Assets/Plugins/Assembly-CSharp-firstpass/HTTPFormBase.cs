using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

public class HTTPFormBase
{
	private const int LongLength = 256;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private List<HTTPFieldData> fields;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool isChanged;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool hasBinary;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool hasLongValue;

	public List<HTTPFieldData> Fields
	{
		get
		{
			return GetFields();
		}
		set
		{
			SetFields(value);
		}
	}

	public bool IsEmpty
	{
		get
		{
			return GetIsEmpty();
		}
	}

	public bool IsChanged
	{
		get
		{
			return GetIsChanged();
		}
		protected set
		{
			SetIsChanged(value);
		}
	}

	public bool HasBinary
	{
		get
		{
			return GetHasBinary();
		}
		protected set
		{
			SetHasBinary(value);
		}
	}

	public bool HasLongValue
	{
		get
		{
			return GetHasLongValue();
		}
		protected set
		{
			SetHasLongValue(value);
		}
	}

	public List<HTTPFieldData> GetFields()
	{
		return fields;
	}

	public void SetFields(List<HTTPFieldData> value)
	{
		fields = value;
	}

	public bool GetIsEmpty()
	{
		return GetFields() == null || GetFields().Count == 0;
	}

	public bool GetIsChanged()
	{
		return isChanged;
	}

	protected void SetIsChanged(bool value)
	{
		isChanged = value;
	}

	public bool GetHasBinary()
	{
		return hasBinary;
	}

	protected void SetHasBinary(bool value)
	{
		hasBinary = value;
	}

	public bool GetHasLongValue()
	{
		return hasLongValue;
	}

	protected void SetHasLongValue(bool value)
	{
		hasLongValue = value;
	}

	public void AddBinaryData(string LKABGPANBMH, byte[] DMNBDBJNKME)
	{
		AddBinaryData(LKABGPANBMH, DMNBDBJNKME, null, null);
	}

	public void AddBinaryData(string LKABGPANBMH, byte[] DMNBDBJNKME, string PMFEIPCHENB)
	{
		AddBinaryData(LKABGPANBMH, DMNBDBJNKME, PMFEIPCHENB, null);
	}

	public void AddBinaryData(string LKABGPANBMH, byte[] DMNBDBJNKME, string PMFEIPCHENB, string KIDMMGJIEHJ)
	{
		if (GetFields() == null)
		{
			SetFields(new List<HTTPFieldData>());
		}
		HTTPFieldData iIMHHCDGJOL = new HTTPFieldData();
		iIMHHCDGJOL.set_Name(LKABGPANBMH);
		if (PMFEIPCHENB == null)
		{
			iIMHHCDGJOL.SetFileName(LKABGPANBMH + ".dat");
		}
		else
		{
			iIMHHCDGJOL.SetFileName(PMFEIPCHENB);
		}
		if (KIDMMGJIEHJ == null)
		{
			iIMHHCDGJOL.SetMimeType("application/octet-stream");
		}
		else
		{
			iIMHHCDGJOL.SetMimeType(KIDMMGJIEHJ);
		}
		iIMHHCDGJOL.set_Binary(DMNBDBJNKME);
		GetFields().Add(iIMHHCDGJOL);
		bool bAINMLLIKOL = true;
		SetIsChanged(bAINMLLIKOL);
		SetHasBinary(bAINMLLIKOL);
	}

	public void AddField(string LKABGPANBMH, string value)
	{
		AddField(LKABGPANBMH, value, Encoding.UTF8);
	}

	public void AddField(string LKABGPANBMH, string value, Encoding FOPOKALJIIJ)
	{
		if (GetFields() == null)
		{
			SetFields(new List<HTTPFieldData>());
		}
		HTTPFieldData iIMHHCDGJOL = new HTTPFieldData();
		iIMHHCDGJOL.set_Name(LKABGPANBMH);
		iIMHHCDGJOL.SetFileName(null);
		iIMHHCDGJOL.SetMimeType("text/plain; charset=\"" + FOPOKALJIIJ.WebName + "\"");
		iIMHHCDGJOL.SetText(value);
		iIMHHCDGJOL.set_Encoding(FOPOKALJIIJ);
		GetFields().Add(iIMHHCDGJOL);
		SetIsChanged(true);
		SetHasLongValue(GetHasLongValue() | (value.Length > 256));
	}

	public virtual void CopyFrom(HTTPFormBase KHGIIFDIHHA)
	{
		SetFields(new List<HTTPFieldData>(KHGIIFDIHHA.GetFields()));
		SetIsChanged(true);
		SetHasBinary(KHGIIFDIHHA.GetHasBinary());
		SetHasLongValue(KHGIIFDIHHA.GetHasLongValue());
	}

	public virtual void PrepareRequest(HTTPRequest ONOCIELLAPL)
	{
		throw new NotImplementedException();
	}

	public virtual byte[] GetData()
	{
		throw new NotImplementedException();
	}
}
