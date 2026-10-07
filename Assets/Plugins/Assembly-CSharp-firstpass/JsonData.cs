using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

[DefaultMember("Item")]
public class JsonData : IEquatable<JsonData>, IDictionary, IList, IEnumerable, ICollection, IOrderedDictionary, IJsonWrapper
{
	private IList<JsonData> inst_array;

	private bool inst_boolean;

	private double inst_double;

	private int inst_int;

	private long inst_long;

	private IDictionary<string, JsonData> inst_object;

	private string inst_string;

	private string json;

	private JsonType type;

	private IList<KeyValuePair<string, JsonData>> object_list;

	int ICollection.Count
	{
		get
		{
			return GetCount();
		}
	}

	bool ICollection.IsSynchronized
	{
		get
		{
			return EnsureCollection().IsSynchronized;
		}
	}

	object ICollection.SyncRoot
	{
		get
		{
			return EnsureCollection().SyncRoot;
		}
	}

	bool IDictionary.IsFixedSize
	{
		get
		{
			return EnsureDictionary().IsFixedSize;
		}
	}

	bool IDictionary.IsReadOnly
	{
		get
		{
			return EnsureDictionary().IsReadOnly;
		}
	}

	ICollection IDictionary.Keys
	{
		get
		{
			EnsureDictionary();
			IList<string> list = new List<string>();
			foreach (KeyValuePair<string, JsonData> item in object_list)
			{
				list.Add(item.Key);
			}
			return (ICollection)list;
		}
	}

	ICollection IDictionary.Values
	{
		get
		{
			EnsureDictionary();
			IList<JsonData> list = new List<JsonData>();
			foreach (KeyValuePair<string, JsonData> item in object_list)
			{
				list.Add(item.Value);
			}
			return (ICollection)list;
		}
	}

	bool IList.IsFixedSize
	{
		get
		{
			return EnsureList().IsFixedSize;
		}
	}

	bool IList.IsReadOnly
	{
		get
		{
			return EnsureList().IsReadOnly;
		}
	}

	object IDictionary.this[object key]
	{
		get
		{
			return EnsureDictionary()[key];
		}
		set
		{
			if (!(key is string))
			{
				throw new ArgumentException("The key has to be a string");
			}
			JsonData jsonData = ToJsonData(value);
			set_Item((string)key, jsonData);
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

	object IList.this[int index]
	{
		get
		{
			return EnsureList()[index];
		}
		set
		{
			EnsureList();
			JsonData jsonData = ToJsonData(value);
			set_Item(index, jsonData);
		}
	}

	public int Count
	{
		get
		{
			return GetCount();
		}
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

	public ICollection<string> Keys
	{
		get
		{
			return GetKeys();
		}
	}

	// C# has no syntax for parameterized property 'DLKPBAJDHBO'.
	public JsonData get_DLKPBAJDHBO(string key)
	{
		return get_Item(key);
	}

	public void set_DLKPBAJDHBO(string key, JsonData value)
	{
		set_Item(key, value);
	}

	// C# has no syntax for parameterized property 'DLKPBAJDHBO'.
	public JsonData get_DLKPBAJDHBO(int index)
	{
		return get_Item(index);
	}

	object IOrderedDictionary.get_DLKPBAJDHBO(int index)
	{
		return get_DLKPBAJDHBO(index);
	}

	void IOrderedDictionary.set_DLKPBAJDHBO(int index, object value)
	{
		set_DLKPBAJDHBO(index, ToJsonData(value));
	}

	public void set_DLKPBAJDHBO(int index, JsonData value)
	{
		set_Item(index, value);
	}

	public JsonData()
	{
	}

	public JsonData(bool boolValue)
	{
		type = JsonType.Boolean;
		inst_boolean = boolValue;
	}

	public JsonData(double number)
	{
		type = JsonType.Double;
		inst_double = number;
	}

	public JsonData(int number)
	{
		type = JsonType.Int;
		inst_int = number;
	}

	public JsonData(long number)
	{
		type = JsonType.Long;
		inst_long = number;
	}

	public JsonData(object obj)
	{
		if (obj is bool)
		{
			type = JsonType.Boolean;
			inst_boolean = (bool)obj;
			return;
		}
		if (obj is double)
		{
			type = JsonType.Double;
			inst_double = (double)obj;
			return;
		}
		if (obj is int)
		{
			type = JsonType.Int;
			inst_int = (int)obj;
			return;
		}
		if (obj is long)
		{
			type = JsonType.Long;
			inst_long = (long)obj;
			return;
		}
		if (obj is string)
		{
			type = JsonType.String;
			inst_string = (string)obj;
			return;
		}
		throw new ArgumentException("Unable to wrap the given object with JsonData");
	}

	public JsonData(string stringValue)
	{
		type = JsonType.String;
		inst_string = stringValue;
	}

	public int GetCount()
	{
		return EnsureCollection().Count;
	}

	public bool GetIsArray()
	{
		return type == JsonType.Array;
	}

	public bool GetIsBoolean()
	{
		return type == JsonType.Boolean;
	}

	public bool GetIsDouble()
	{
		return type == JsonType.Double;
	}

	public bool GetIsInt()
	{
		return type == JsonType.Int;
	}

	public bool GetIsLong()
	{
		return type == JsonType.Long;
	}

	public bool GetIsObject()
	{
		return type == JsonType.Object;
	}

	public bool GetIsString()
	{
		return type == JsonType.String;
	}

	public ICollection<string> GetKeys()
	{
		EnsureDictionary();
		return inst_object.Keys;
	}

	private bool LitJson_002EIJsonWrapper_002Eget_IsArray()
	{
		return GetIsArray();
	}

	private bool LitJson_002EIJsonWrapper_002Eget_IsBoolean()
	{
		return GetIsBoolean();
	}

	private bool LitJson_002EIJsonWrapper_002Eget_IsDouble()
	{
		return GetIsDouble();
	}

	private bool LitJson_002EIJsonWrapper_002Eget_IsInt()
	{
		return GetIsInt();
	}

	private bool LitJson_002EIJsonWrapper_002Eget_IsLong()
	{
		return GetIsLong();
	}

	private bool LitJson_002EIJsonWrapper_002Eget_IsObject()
	{
		return GetIsObject();
	}

	private bool LitJson_002EIJsonWrapper_002Eget_IsString()
	{
		return GetIsString();
	}

	private object LitJson_002EIOrderedDictionary_002Eget_Item(int index)
	{
		EnsureDictionary();
		return object_list[index].Value;
	}

	private void LitJson_002EIOrderedDictionary_002Eset_Item(int index, object value)
	{
		EnsureDictionary();
		JsonData jsonData = ToJsonData(value);
		KeyValuePair<string, JsonData> keyValuePair = object_list[index];
		inst_object[keyValuePair.Key] = jsonData;
		KeyValuePair<string, JsonData> keyValuePair2 = new KeyValuePair<string, JsonData>(keyValuePair.Key, jsonData);
		object_list[index] = keyValuePair2;
	}

	public JsonData get_Item(string key)
	{
		EnsureDictionary();
		return inst_object[key];
	}

	public void set_Item(string key, JsonData value)
	{
		EnsureDictionary();
		KeyValuePair<string, JsonData> keyValuePair = new KeyValuePair<string, JsonData>(key, value);
		if (inst_object.ContainsKey(key))
		{
			for (int i = 0; i < object_list.Count; i++)
			{
				if (object_list[i].Key == key)
				{
					object_list[i] = keyValuePair;
					break;
				}
			}
		}
		else
		{
			object_list.Add(keyValuePair);
		}
		inst_object[key] = value;
		json = null;
	}

	public JsonData get_Item(int index)
	{
		EnsureCollection();
		if (type == JsonType.Array)
		{
			return inst_array[index];
		}
		return object_list[index].Value;
	}

	public void set_Item(int index, JsonData value)
	{
		EnsureCollection();
		if (type == JsonType.Array)
		{
			inst_array[index] = value;
		}
		else
		{
			KeyValuePair<string, JsonData> keyValuePair = object_list[index];
			KeyValuePair<string, JsonData> keyValuePair2 = new KeyValuePair<string, JsonData>(keyValuePair.Key, value);
			object_list[index] = keyValuePair2;
			inst_object[keyValuePair.Key] = keyValuePair2.Value;
		}
		json = null;
	}

	[SpecialName]
	public static JsonData op_Implicit(bool data)
	{
		return new JsonData(data);
	}

	[SpecialName]
	public static JsonData op_Implicit(double data)
	{
		return new JsonData(data);
	}

	[SpecialName]
	public static JsonData op_Implicit(int data)
	{
		return new JsonData(data);
	}

	[SpecialName]
	public static JsonData op_Implicit(long data)
	{
		return new JsonData(data);
	}

	[SpecialName]
	public static JsonData op_Implicit(string data)
	{
		return new JsonData(data);
	}

	public static explicit operator bool(JsonData data)
	{
		if (data.type != JsonType.Boolean)
		{
			throw new InvalidCastException("Instance of JsonData doesn't hold a double");
		}
		return data.inst_boolean;
	}

	public static explicit operator double(JsonData data)
	{
		if (data.type != JsonType.Double)
		{
			throw new InvalidCastException("Instance of JsonData doesn't hold a double");
		}
		return data.inst_double;
	}

	public static explicit operator int(JsonData data)
	{
		if (data.type != JsonType.Int)
		{
			throw new InvalidCastException("Instance of JsonData doesn't hold an int");
		}
		return data.inst_int;
	}

	public static explicit operator long(JsonData data)
	{
		if (data.type != JsonType.Long)
		{
			throw new InvalidCastException("Instance of JsonData doesn't hold an int");
		}
		return data.inst_long;
	}

	public static explicit operator string(JsonData data)
	{
		if (data.type != JsonType.String)
		{
			throw new InvalidCastException("Instance of JsonData doesn't hold a string");
		}
		return data.inst_string;
	}

	void ICollection.CopyTo(Array array, int index)
	{
		EnsureCollection().CopyTo(array, index);
	}

	void IDictionary.Add(object key, object value)
	{
		JsonData jsonData = ToJsonData(value);
		EnsureDictionary().Add(key, jsonData);
		KeyValuePair<string, JsonData> item = new KeyValuePair<string, JsonData>((string)key, jsonData);
		object_list.Add(item);
		json = null;
	}

	void IDictionary.Clear()
	{
		EnsureDictionary().Clear();
		object_list.Clear();
		json = null;
	}

	bool IDictionary.Contains(object key)
	{
		return EnsureDictionary().Contains(key);
	}

	IDictionaryEnumerator IDictionary.GetEnumerator()
	{
		return ((IOrderedDictionary)this).GetEnumerator();
	}

	void IDictionary.Remove(object key)
	{
		EnsureDictionary().Remove(key);
		for (int i = 0; i < object_list.Count; i++)
		{
			if (object_list[i].Key == (string)key)
			{
				object_list.RemoveAt(i);
				break;
			}
		}
		json = null;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return EnsureCollection().GetEnumerator();
	}

	bool IJsonWrapper.GetBoolean()
	{
		if (type != JsonType.Boolean)
		{
			throw new InvalidOperationException("JsonData instance doesn't hold a boolean");
		}
		return inst_boolean;
	}

	double IJsonWrapper.GetDouble()
	{
		if (type != JsonType.Double)
		{
			throw new InvalidOperationException("JsonData instance doesn't hold a double");
		}
		return inst_double;
	}

	int IJsonWrapper.GetInt()
	{
		if (type != JsonType.Int)
		{
			throw new InvalidOperationException("JsonData instance doesn't hold an int");
		}
		return inst_int;
	}

	long IJsonWrapper.GetLong()
	{
		if (type != JsonType.Long)
		{
			throw new InvalidOperationException("JsonData instance doesn't hold a long");
		}
		return inst_long;
	}

	string IJsonWrapper.GetString()
	{
		if (type != JsonType.String)
		{
			throw new InvalidOperationException("JsonData instance doesn't hold a string");
		}
		return inst_string;
	}

	void IJsonWrapper.SetBoolean(bool boolValue)
	{
		type = JsonType.Boolean;
		inst_boolean = boolValue;
		json = null;
	}

	void IJsonWrapper.SetDouble(double doubleValue)
	{
		type = JsonType.Double;
		inst_double = doubleValue;
		json = null;
	}

	void IJsonWrapper.SetInt(int intValue)
	{
		type = JsonType.Int;
		inst_int = intValue;
		json = null;
	}

	void IJsonWrapper.SetLong(long longValue)
	{
		type = JsonType.Long;
		inst_long = longValue;
		json = null;
	}

	void IJsonWrapper.SetString(string stringValue)
	{
		type = JsonType.String;
		inst_string = stringValue;
		json = null;
	}

	string IJsonWrapper.ToJson()
	{
		return ToJson();
	}

	void IJsonWrapper.ToJson(JsonWriter writer)
	{
		ToJson(writer);
	}

	int IList.Add(object value)
	{
		return Add(value);
	}

	void IList.Clear()
	{
		EnsureList().Clear();
		json = null;
	}

	bool IList.Contains(object value)
	{
		return EnsureList().Contains(value);
	}

	int IList.IndexOf(object value)
	{
		return EnsureList().IndexOf(value);
	}

	void IList.Insert(int index, object value)
	{
		EnsureList().Insert(index, value);
		json = null;
	}

	void IList.Remove(object value)
	{
		EnsureList().Remove(value);
		json = null;
	}

	void IList.RemoveAt(int index)
	{
		EnsureList().RemoveAt(index);
		json = null;
	}

	IDictionaryEnumerator IOrderedDictionary.GetEnumerator()
	{
		EnsureDictionary();
		return new OrderedDictionaryEnumerator(object_list.GetEnumerator());
	}

	void IOrderedDictionary.Insert(int index, object key, object value)
	{
		string text = (string)key;
		JsonData jsonData = ToJsonData(value);
		set_Item(text, jsonData);
		KeyValuePair<string, JsonData> item = new KeyValuePair<string, JsonData>(text, jsonData);
		object_list.Insert(index, item);
	}

	void IOrderedDictionary.RemoveAt(int index)
	{
		EnsureDictionary();
		inst_object.Remove(object_list[index].Key);
		object_list.RemoveAt(index);
	}

	private ICollection EnsureCollection()
	{
		if (type == JsonType.Array)
		{
			return (ICollection)inst_array;
		}
		if (type == JsonType.Object)
		{
			return (ICollection)inst_object;
		}
		throw new InvalidOperationException("The JsonData instance has to be initialized first");
	}

	private IDictionary EnsureDictionary()
	{
		if (type == JsonType.Object)
		{
			return (IDictionary)inst_object;
		}
		if (type != JsonType.None)
		{
			throw new InvalidOperationException("Instance of JsonData is not a dictionary");
		}
		type = JsonType.Object;
		inst_object = new Dictionary<string, JsonData>();
		object_list = new List<KeyValuePair<string, JsonData>>();
		return (IDictionary)inst_object;
	}

	private IList EnsureList()
	{
		if (type == JsonType.Array)
		{
			return (IList)inst_array;
		}
		if (type != JsonType.None)
		{
			throw new InvalidOperationException("Instance of JsonData is not a list");
		}
		type = JsonType.Array;
		inst_array = new List<JsonData>();
		return (IList)inst_array;
	}

	private JsonData ToJsonData(object obj)
	{
		if (obj == null)
		{
			return null;
		}
		if (obj is JsonData)
		{
			return (JsonData)obj;
		}
		return new JsonData(obj);
	}

	private static void WriteJson(IJsonWrapper wrapper, JsonWriter writer)
	{
		if (wrapper == null)
		{
			writer.Write(null);
		}
		else if (wrapper.GetIsString())
		{
			writer.Write(wrapper.GetString());
		}
		else if (wrapper.GetIsBoolean())
		{
			writer.Write(wrapper.GetBoolean());
		}
		else if (wrapper.GetIsDouble())
		{
			writer.Write(wrapper.GetDouble());
		}
		else if (wrapper.GetIsInt())
		{
			writer.Write(wrapper.GetInt());
		}
		else if (wrapper.GetIsLong())
		{
			writer.Write(wrapper.GetLong());
		}
		else if (wrapper.GetIsArray())
		{
			writer.WriteArrayStart();
			foreach (object item in (IEnumerable)wrapper)
			{
				WriteJson((JsonData)item, writer);
			}
			writer.WriteArrayEnd();
		}
		else
		{
			if (!wrapper.GetIsObject())
			{
				return;
			}
			writer.WriteObjectStart();
			foreach (DictionaryEntry item2 in (IDictionary)wrapper)
			{
				writer.WritePropertyName((string)item2.Key);
				WriteJson((JsonData)item2.Value, writer);
			}
			writer.WriteObjectEnd();
		}
	}

	public int Add(object value)
	{
		JsonData jsonData = ToJsonData(value);
		json = null;
		return EnsureList().Add(jsonData);
	}

	public void Clear()
	{
		if (GetIsObject())
		{
			((IDictionary)this).Clear();
		}
		else if (GetIsArray())
		{
			((IList)this).Clear();
		}
	}

	public bool Equals(JsonData other)
	{
		if (other == null)
		{
			return false;
		}
		if (other.type != type)
		{
			return false;
		}
		switch (type)
		{
		case JsonType.None:
			return true;
		case JsonType.Object:
			return inst_object.Equals(other.inst_object);
		case JsonType.Array:
			return inst_array.Equals(other.inst_array);
		case JsonType.String:
			return inst_string.Equals(other.inst_string);
		case JsonType.Int:
			return inst_int.Equals(other.inst_int);
		case JsonType.Long:
			return inst_long.Equals(other.inst_long);
		case JsonType.Double:
			return inst_double.Equals(other.inst_double);
		case JsonType.Boolean:
			return inst_boolean.Equals(other.inst_boolean);
		default:
			return false;
		}
	}

	public JsonType GetJsonType()
	{
		return type;
	}

	public void SetJsonType(JsonType jsonType)
	{
		if (this.type != jsonType)
		{
			switch (jsonType)
			{
			case JsonType.Object:
				inst_object = new Dictionary<string, JsonData>();
				object_list = new List<KeyValuePair<string, JsonData>>();
				break;
			case JsonType.Array:
				inst_array = new List<JsonData>();
				break;
			case JsonType.String:
				inst_string = null;
				break;
			case JsonType.Int:
				inst_int = 0;
				break;
			case JsonType.Long:
				inst_long = 0L;
				break;
			case JsonType.Double:
				inst_double = 0.0;
				break;
			case JsonType.Boolean:
				inst_boolean = false;
				break;
			}
			this.type = jsonType;
		}
	}

	public string ToJson()
	{
		if (json != null)
		{
			return json;
		}
		StringWriter stringWriter = new StringWriter();
		JsonWriter writer = new JsonWriter(stringWriter);
		writer.SetValidate(false);
		WriteJson(this, writer);
		json = stringWriter.ToString();
		return json;
	}

	public void ToJson(JsonWriter writer)
	{
		bool previousValidate = writer.GetValidate();
		writer.SetValidate(false);
		WriteJson(this, writer);
		writer.SetValidate(previousValidate);
	}

	public override string ToString()
	{
		switch (type)
		{
		case JsonType.Array:
			return "JsonData array";
		case JsonType.Boolean:
			return inst_boolean.ToString();
		case JsonType.Double:
			return inst_double.ToString();
		case JsonType.Int:
			return inst_int.ToString();
		case JsonType.Long:
			return inst_long.ToString();
		case JsonType.Object:
			return "JsonData object";
		case JsonType.String:
			return inst_string;
		default:
			return "Uninitialized JsonData";
		}
	}
}
