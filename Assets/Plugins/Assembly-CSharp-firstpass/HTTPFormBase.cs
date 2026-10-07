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

	public void AddBinaryData(string fieldName, byte[] data)
	{
		AddBinaryData(fieldName, data, null, null);
	}

	public void AddBinaryData(string fieldName, byte[] data, string fileName)
	{
		AddBinaryData(fieldName, data, fileName, null);
	}

	public void AddBinaryData(string fieldName, byte[] data, string fileName, string mimeType)
	{
		if (GetFields() == null)
		{
			SetFields(new List<HTTPFieldData>());
		}
		HTTPFieldData field = new HTTPFieldData();
		field.set_Name(fieldName);
		if (fileName == null)
		{
			field.SetFileName(fieldName + ".dat");
		}
		else
		{
			field.SetFileName(fileName);
		}
		if (mimeType == null)
		{
			field.SetMimeType("application/octet-stream");
		}
		else
		{
			field.SetMimeType(mimeType);
		}
		field.set_Binary(data);
		GetFields().Add(field);
		bool changed = true;
		SetIsChanged(changed);
		SetHasBinary(changed);
	}

	public void AddField(string fieldName, string value)
	{
		AddField(fieldName, value, Encoding.UTF8);
	}

	public void AddField(string fieldName, string value, Encoding encoding)
	{
		if (GetFields() == null)
		{
			SetFields(new List<HTTPFieldData>());
		}
		HTTPFieldData field = new HTTPFieldData();
		field.set_Name(fieldName);
		field.SetFileName(null);
		field.SetMimeType("text/plain; charset=\"" + encoding.WebName + "\"");
		field.SetText(value);
		field.set_Encoding(encoding);
		GetFields().Add(field);
		SetIsChanged(true);
		SetHasLongValue(GetHasLongValue() | (value.Length > 256));
	}

	public virtual void CopyFrom(HTTPFormBase form)
	{
		SetFields(new List<HTTPFieldData>(form.GetFields()));
		SetIsChanged(true);
		SetHasBinary(form.GetHasBinary());
		SetHasLongValue(form.GetHasLongValue());
	}

	public virtual void PrepareRequest(HTTPRequest request)
	{
		throw new NotImplementedException();
	}

	public virtual byte[] GetData()
	{
		throw new NotImplementedException();
	}
}
