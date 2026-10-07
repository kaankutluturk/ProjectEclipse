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

	public UnityForm(WWWForm form)
	{
		set_Form(form);
	}

	public WWWForm GetForm()
	{
		return form;
	}

	public void set_Form(WWWForm value)
	{
		form = value;
	}

	public override void CopyFrom(HTTPFormBase source)
	{
		SetFields(source.GetFields());
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
			HTTPFieldData field = GetFields()[i];
			if (string.IsNullOrEmpty(field.GetText()) && field.GetBinary() != null)
			{
				GetForm().AddBinaryData(field.get_Name(), field.GetBinary(), field.GetFileName(), field.GetMimeType());
			}
			else
			{
				GetForm().AddField(field.get_Name(), field.GetText(), field.GetEncoding());
			}
		}
	}

	public override void PrepareRequest(HTTPRequest request)
	{
		if (GetForm().headers.ContainsKey("Content-Type"))
		{
			request.SetHeader("Content-Type", GetForm().headers["Content-Type"]);
		}
		else
		{
			request.SetHeader("Content-Type", "application/x-www-form-urlencoded");
		}
	}

	public override byte[] GetData()
	{
		return GetForm().data;
	}
}
