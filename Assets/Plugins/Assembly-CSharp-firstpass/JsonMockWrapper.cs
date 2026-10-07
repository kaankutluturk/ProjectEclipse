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

	object IDictionary.this[object key]
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
	object IOrderedDictionary.get_Item(int index)
	{
		return LitJson_002EIOrderedDictionary_002Eget_Item(index);
	}

	void IOrderedDictionary.set_Item(int index, object value)
	{
		LitJson_002EIOrderedDictionary_002Eset_Item(index, value);
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

	public void SetBoolean(bool boolValue)
	{
	}

	public void SetDouble(double doubleValue)
	{
	}

	public void SetInt(int intValue)
	{
	}

	public void SetJsonType(JsonType jsonType)
	{
	}

	public void SetLong(long longValue)
	{
	}

	public void SetString(string stringValue)
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

	void IList.Insert(int i, object item)
	{
	}

	void IList.Remove(object value)
	{
	}

	void IList.RemoveAt(int index)
	{
	}

	void ICollection.CopyTo(Array array, int index)
	{
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return null;
	}

	void IDictionary.Add(object key, object entryValue)
	{
	}

	void IDictionary.Clear()
	{
	}

	bool IDictionary.Contains(object key)
	{
		return false;
	}

	void IDictionary.Remove(object key)
	{
	}

	IDictionaryEnumerator IDictionary.GetEnumerator()
	{
		return null;
	}

	private object LitJson_002EIOrderedDictionary_002Eget_Item(int index)
	{
		return null;
	}

	private void LitJson_002EIOrderedDictionary_002Eset_Item(int index, object value)
	{
	}

	IDictionaryEnumerator IOrderedDictionary.GetEnumerator()
	{
		return null;
	}

	void IOrderedDictionary.Insert(int i, object key, object entryValue)
	{
	}

	void IOrderedDictionary.RemoveAt(int i)
	{
	}
}
