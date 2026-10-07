using System;
using System.Diagnostics;
using UnityEngine;

public sealed class SampleDescriptor
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool isLabel;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private Type type;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string displayName;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string description;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string codeBlock;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool isSelected;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private GameObject unityObject;

	public bool IsLabel
	{
		get
		{
			return GetIsLabel();
		}
		set
		{
			SetIsLabel(value);
		}
	}

	public string DisplayName
	{
		get
		{
			return GetDisplayName();
		}
		set
		{
			SetDisplayName(value);
		}
	}

	public string DescriptionText
	{
		get
		{
			return GetDescription();
		}
		set
		{
			set_Description(value);
		}
	}

	public string CodeBlock
	{
		get
		{
			return GetCodeBlock();
		}
		set
		{
			SetCodeBlock(value);
		}
	}

	public bool IsSelected
	{
		get
		{
			return GetIsSelected();
		}
		set
		{
			SetIsSelected(value);
		}
	}

	public GameObject SampleObject
	{
		get
		{
			return GetUnityObject();
		}
		set
		{
			set_UnityObject(value);
		}
	}

	public bool IsSampleRunning
	{
		get
		{
			return GetIsRunning();
		}
	}

	public SampleDescriptor(Type type, string displayName, string description, string codeBlock)
	{
		set_Type(type);
		SetDisplayName(displayName);
		set_Description(description);
		SetCodeBlock(codeBlock);
	}

	public bool GetIsLabel()
	{
		return isLabel;
	}

	public void SetIsLabel(bool value)
	{
		isLabel = value;
	}

	public Type get_Type()
	{
		return type;
	}

	public void set_Type(Type value)
	{
		type = value;
	}

	public string GetDisplayName()
	{
		return displayName;
	}

	public void SetDisplayName(string value)
	{
		displayName = value;
	}

	public string GetDescription()
	{
		return description;
	}

	public void set_Description(string value)
	{
		description = value;
	}

	public string GetCodeBlock()
	{
		return codeBlock;
	}

	public void SetCodeBlock(string value)
	{
		codeBlock = value;
	}

	public bool GetIsSelected()
	{
		return isSelected;
	}

	public void SetIsSelected(bool value)
	{
		isSelected = value;
	}

	public GameObject GetUnityObject()
	{
		return unityObject;
	}

	public void set_UnityObject(GameObject value)
	{
		unityObject = value;
	}

	public bool GetIsRunning()
	{
		return GetUnityObject() != null;
	}

	public void CreateUnityObject()
	{
		if (!(GetUnityObject() != null))
		{
			set_UnityObject(new GameObject(GetDisplayName()));
			GetUnityObject().AddComponent(get_Type());
		}
	}

	public void DestroyUnityObject()
	{
		if (GetUnityObject() != null)
		{
			UnityEngine.Object.Destroy(GetUnityObject());
			set_UnityObject(null);
		}
	}
}
