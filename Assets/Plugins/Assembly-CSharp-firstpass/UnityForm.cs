using System.Diagnostics;
using UnityEngine;

public sealed class UnityForm : HTTPFormBase
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private WWWForm form;

	public WWWForm WwwForm
	{
		get
		{
			return GetForm();
		}
		set
		{
			set_Form(value);
		}
	}

	public UnityForm()
	{
	}

	public UnityForm(WWWForm HOELLMLEBAK)
	{
		set_Form(HOELLMLEBAK);
	}

	public WWWForm GetForm()
	{
		return form;
	}

	public void set_Form(WWWForm value)
	{
		form = value;
	}

	public override void CopyFrom(HTTPFormBase KHGIIFDIHHA)
	{
		SetFields(KHGIIFDIHHA.GetFields());
		SetIsChanged(true);
		if (GetForm() != null)
		{
			return;
		}
		set_Form(new WWWForm());
		if (GetFields() == null)
		{
			return;
		}
		for (int i = 0; i < GetFields().Count; i++)
		{
			HTTPFieldData iIMHHCDGJOL = GetFields()[i];
			if (string.IsNullOrEmpty(iIMHHCDGJOL.GetText()) && iIMHHCDGJOL.GetBinary() != null)
			{
				GetForm().AddBinaryData(iIMHHCDGJOL.get_Name(), iIMHHCDGJOL.GetBinary(), iIMHHCDGJOL.GetFileName(), iIMHHCDGJOL.GetMimeType());
			}
			else
			{
				GetForm().AddField(iIMHHCDGJOL.get_Name(), iIMHHCDGJOL.GetText(), iIMHHCDGJOL.GetEncoding());
			}
		}
	}

	public override void PrepareRequest(HTTPRequest ONOCIELLAPL)
	{
		if (GetForm().headers.ContainsKey("Content-Type"))
		{
			ONOCIELLAPL.SetHeader("Content-Type", GetForm().headers["Content-Type"]);
		}
		else
		{
			ONOCIELLAPL.SetHeader("Content-Type", "application/x-www-form-urlencoded");
		}
	}

	public override byte[] GetData()
	{
		return GetForm().data;
	}
}
