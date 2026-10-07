using System;
using System.Collections;

public class JsonMockWrapper : IDictionary, IList, IEnumerable, ICollection, IOrderedDictionary, IJsonWrapper
{
	bool IList.IsFixedSize
	{
		get
		{
			return true;
		}
	}

	bool IList.IsReadOnly
	{
		get
		{
			return true;
		}
	}

	object IList.this[int index]
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	int ICollection.Count
	{
		get
		{
			return 0;
		}
	}

	bool ICollection.IsSynchronized
	{
		get
		{
			return false;
		}
	}

	object ICollection.SyncRoot
	{
		get
		{
			return null;
		}
	}

	bool IDictionary.IsFixedSize
	{
		get
		{
			return true;
		}
	}

	bool IDictionary.IsReadOnly
	{
		get
		{
			return true;
		}
	}

	ICollection IDictionary.Keys
	{
		get
		{
			return null;
		}
	}

	ICollection IDictionary.Values
	{
		get
		{
			return null;
		}
	}

	object IDictionary.this[object KGBGENDIMBC]
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	// C# has no syntax for parameterized property 'IOrderedDictionary.Item'.
	object IOrderedDictionary.get_Item(int OOPOEMNCCGH)
	{
		return LitJson_002EIOrderedDictionary_002Eget_Item(OOPOEMNCCGH);
	}

	void IOrderedDictionary.set_Item(int OOPOEMNCCGH, object value)
	{
		LitJson_002EIOrderedDictionary_002Eset_Item(OOPOEMNCCGH, value);
	}

	public bool IsArray
	{
		get
		{
			return GetIsArray();
		}
	}

	public bool IsBoolean
	{
		get
		{
			return GetIsBoolean();
		}
	}

	public bool IsDouble
	{
		get
		{
			return GetIsDouble();
		}
	}

	public bool IsInt
	{
		get
		{
			return GetIsInt();
		}
	}

	public bool IsLong
	{
		get
		{
			return GetIsLong();
		}
	}

	public bool IsObject
	{
		get
		{
			return GetIsObject();
		}
	}

	public bool IsString
	{
		get
		{
			return GetIsString();
		}
	}

	object IOrderedDictionary.get_DLKPBAJDHBO(int index)
	{
		throw new NotSupportedException();
	}

	void IOrderedDictionary.set_DLKPBAJDHBO(int index, object value)
	{
		throw new NotSupportedException();
	}

	public bool GetIsArray()
	{
		return false;
	}

	public bool GetIsBoolean()
	{
		return false;
	}

	public bool GetIsDouble()
	{
		return false;
	}

	public bool GetIsInt()
	{
		return false;
	}

	public bool GetIsLong()
	{
		return false;
	}

	public bool GetIsObject()
	{
		return false;
	}

	public bool GetIsString()
	{
		return false;
	}

	public bool GetBoolean()
	{
		return false;
	}

	public double GetDouble()
	{
		return 0.0;
	}

	public int GetInt()
	{
		return 0;
	}

	public JsonType GetJsonType()
	{
		return JsonType.None;
	}

	public long GetLong()
	{
		return 0L;
	}

	public string GetString()
	{
		return string.Empty;
	}

	public void SetBoolean(bool PKHDLOGJKAD)
	{
	}

	public void SetDouble(double PKHDLOGJKAD)
	{
	}

	public void SetInt(int PKHDLOGJKAD)
	{
	}

	public void SetJsonType(JsonType LFLGCDNKNJI)
	{
	}

	public void SetLong(long PKHDLOGJKAD)
	{
	}

	public void SetString(string PKHDLOGJKAD)
	{
	}

	public string ToJson()
	{
		return string.Empty;
	}

	public void ToJson(JsonWriter writer)
	{
	}

	int IList.Add(object value)
	{
		return 0;
	}

	void IList.Clear()
	{
	}

	bool IList.Contains(object value)
	{
		return false;
	}

	int IList.IndexOf(object value)
	{
		return -1;
	}

	void IList.Insert(int i, object AFIEJABPAKA)
	{
	}

	void IList.Remove(object value)
	{
	}

	void IList.RemoveAt(int index)
	{
	}

	void ICollection.CopyTo(Array HFPDMGAEJJE, int index)
	{
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return null;
	}

	void IDictionary.Add(object KJBMNAEJIHG, object AFIEJABPAKA)
	{
	}

	void IDictionary.Clear()
	{
	}

	bool IDictionary.Contains(object KGBGENDIMBC)
	{
		return false;
	}

	void IDictionary.Remove(object KGBGENDIMBC)
	{
	}

	IDictionaryEnumerator IDictionary.GetEnumerator()
	{
		return null;
	}

	private object LitJson_002EIOrderedDictionary_002Eget_Item(int OOPOEMNCCGH)
	{
		return null;
	}

	private void LitJson_002EIOrderedDictionary_002Eset_Item(int OOPOEMNCCGH, object value)
	{
	}

	IDictionaryEnumerator IOrderedDictionary.GetEnumerator()
	{
		return null;
	}

	void IOrderedDictionary.Insert(int i, object KJBMNAEJIHG, object AFIEJABPAKA)
	{
	}

	void IOrderedDictionary.RemoveAt(int i)
	{
	}
}
