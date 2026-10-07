using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;

public class JsonMapper
{
	private static int max_nesting_depth;

	private static IFormatProvider datetime_format;

	private static IDictionary<Type, ExporterFunc> base_exporters_table;

	private static IDictionary<Type, ExporterFunc> custom_exporters_table;

	private static IDictionary<Type, IDictionary<Type, ImporterFunc>> base_importers_table;

	private static IDictionary<Type, IDictionary<Type, ImporterFunc>> custom_importers_table;

	private static IDictionary<Type, ArrayMetadata> array_metadata;

	private static readonly object array_metadata_lock;

	private static IDictionary<Type, IDictionary<Type, MethodInfo>> conv_ops;

	private static readonly object conv_ops_lock;

	private static IDictionary<Type, ObjectMetadata> object_metadata;

	private static readonly object object_metadata_lock;

	private static IDictionary<Type, IList<PropertyMetadata>> type_properties;

	private static readonly object type_properties_lock;

	private static JsonWriter static_writer;

	private static readonly object static_writer_lock;

	static JsonMapper()
	{
		array_metadata_lock = new object();
		conv_ops_lock = new object();
		object_metadata_lock = new object();
		type_properties_lock = new object();
		static_writer_lock = new object();
		max_nesting_depth = 100;
		array_metadata = new Dictionary<Type, ArrayMetadata>();
		conv_ops = new Dictionary<Type, IDictionary<Type, MethodInfo>>();
		object_metadata = new Dictionary<Type, ObjectMetadata>();
		type_properties = new Dictionary<Type, IList<PropertyMetadata>>();
		static_writer = new JsonWriter();
		datetime_format = DateTimeFormatInfo.InvariantInfo;
		base_exporters_table = new Dictionary<Type, ExporterFunc>();
		custom_exporters_table = new Dictionary<Type, ExporterFunc>();
		base_importers_table = new Dictionary<Type, IDictionary<Type, ImporterFunc>>();
		custom_importers_table = new Dictionary<Type, IDictionary<Type, ImporterFunc>>();
		RegisterBaseExporters();
		RegisterBaseImporters();
	}

	private static bool HasInterface(Type type, string name)
	{
		return type.GetInterface(name, true) != null;
	}

	public static PropertyInfo[] GetPublicInstanceProperties(Type type)
	{
		return type.GetProperties();
	}

	private static void AddArrayMetadata(Type type)
	{
		if (array_metadata.ContainsKey(type))
		{
			return;
		}
		ArrayMetadata value = default(ArrayMetadata);
		value.SetIsArray(type.IsArray);
		if (HasInterface(type, "System.Collections.IList"))
		{
			value.SetIsList(true);
		}
		PropertyInfo[] array = GetPublicInstanceProperties(type);
		foreach (PropertyInfo propertyInfo in array)
		{
			if (!(propertyInfo.Name != "Item"))
			{
				ParameterInfo[] indexParameters = propertyInfo.GetIndexParameters();
				if (indexParameters.Length == 1 && indexParameters[0].ParameterType == typeof(int))
				{
					value.set_ElementType(propertyInfo.PropertyType);
				}
			}
		}
		lock (array_metadata_lock)
		{
			try
			{
				array_metadata.Add(type, value);
			}
			catch (ArgumentException)
			{
			}
		}
	}

	private static void AddObjectMetadata(Type type)
	{
		if (object_metadata.ContainsKey(type))
		{
			return;
		}
		ObjectMetadata value = default(ObjectMetadata);
		if (HasInterface(type, "System.Collections.IDictionary"))
		{
			value.set_IsDictionary(true);
		}
		value.SetProperties(new Dictionary<string, PropertyMetadata>());
		PropertyInfo[] array = GetPublicInstanceProperties(type);
		foreach (PropertyInfo propertyInfo in array)
		{
			if (propertyInfo.Name == "Item")
			{
				ParameterInfo[] indexParameters = propertyInfo.GetIndexParameters();
				if (indexParameters.Length == 1 && indexParameters[0].ParameterType == typeof(string))
				{
					value.set_ElementType(propertyInfo.PropertyType);
				}
			}
			else
			{
				PropertyMetadata value2 = new PropertyMetadata
				{
					Info = propertyInfo,
					Type = propertyInfo.PropertyType
				};
				value.GetProperties().Add(propertyInfo.Name, value2);
			}
		}
		FieldInfo[] fields = type.GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			PropertyMetadata value3 = new PropertyMetadata
			{
				Info = fieldInfo,
				IsField = true,
				Type = fieldInfo.FieldType
			};
			value.GetProperties().Add(fieldInfo.Name, value3);
		}
		lock (object_metadata_lock)
		{
			try
			{
				object_metadata.Add(type, value);
			}
			catch (ArgumentException)
			{
			}
		}
	}

	private static void AddTypeProperties(Type type)
	{
		if (type_properties.ContainsKey(type))
		{
			return;
		}
		IList<PropertyMetadata> list = new List<PropertyMetadata>();
		PropertyInfo[] array = GetPublicInstanceProperties(type);
		foreach (PropertyInfo propertyInfo in array)
		{
			if (!(propertyInfo.Name == "Item"))
			{
				list.Add(new PropertyMetadata
				{
					Info = propertyInfo,
					IsField = false
				});
			}
		}
		FieldInfo[] fields = type.GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			list.Add(new PropertyMetadata
			{
				Info = fieldInfo,
				IsField = true
			});
		}
		lock (type_properties_lock)
		{
			try
			{
				type_properties.Add(type, list);
			}
			catch (ArgumentException)
			{
			}
		}
	}

	private static MethodInfo GetConvOp(Type targetType, Type sourceType)
	{
		lock (conv_ops_lock)
		{
			if (!conv_ops.ContainsKey(targetType))
			{
				conv_ops.Add(targetType, new Dictionary<Type, MethodInfo>());
			}
		}
		if (conv_ops[targetType].ContainsKey(sourceType))
		{
			return conv_ops[targetType][sourceType];
		}
		MethodInfo method = targetType.GetMethod("op_Implicit", new Type[1] { sourceType });
		lock (conv_ops_lock)
		{
			try
			{
				conv_ops[targetType].Add(sourceType, method);
				return method;
			}
			catch (ArgumentException)
			{
				return conv_ops[targetType][sourceType];
			}
		}
	}

	private static object ReadValue(Type valueType, JsonReader reader)
	{
		reader.Read();
		if (reader.GetToken() == JsonToken.ArrayEnd)
		{
			return null;
		}
		if (reader.GetToken() == JsonToken.Null)
		{
			if (!valueType.IsClass)
			{
				throw new JsonException(string.Format("Can't assign null to an instance of type {0}", valueType));
			}
			return null;
		}
		if (reader.GetToken() == JsonToken.Double || reader.GetToken() == JsonToken.Int || reader.GetToken() == JsonToken.Long || reader.GetToken() == JsonToken.String || reader.GetToken() == JsonToken.Boolean)
		{
			Type type = reader.GetValue().GetType();
			if (valueType.IsAssignableFrom(type))
			{
				return reader.GetValue();
			}
			if (custom_importers_table.ContainsKey(type) && custom_importers_table[type].ContainsKey(valueType))
			{
				ImporterFunc importer = custom_importers_table[type][valueType];
				return importer(reader.GetValue());
			}
			if (base_importers_table.ContainsKey(type) && base_importers_table[type].ContainsKey(valueType))
			{
				ImporterFunc importer = base_importers_table[type][valueType];
				return importer(reader.GetValue());
			}
			if (valueType.IsEnum)
			{
				return Enum.ToObject(valueType, reader.GetValue());
			}
			MethodInfo methodInfo = GetConvOp(valueType, type);
			if (methodInfo != null)
			{
				return methodInfo.Invoke(null, new object[1] { reader.GetValue() });
			}
			throw new JsonException(string.Format("Can't assign value '{0}' (type {1}) to type {2}", reader.GetValue(), type, valueType));
		}
		object obj = null;
		if (reader.GetToken() == JsonToken.ArrayStart)
		{
			if (valueType.FullName == "System.Object")
			{
				valueType = typeof(object[]);
			}
			AddArrayMetadata(valueType);
			ArrayMetadata arrayMetadata = array_metadata[valueType];
			if (!arrayMetadata.GetIsArray() && !arrayMetadata.GetIsList())
			{
				throw new JsonException(string.Format("Type {0} can't act as an array", valueType));
			}
			IList list;
			Type type2;
			if (!arrayMetadata.GetIsArray())
			{
				list = (IList)Activator.CreateInstance(valueType);
				type2 = arrayMetadata.GetElementType();
			}
			else
			{
				list = new ArrayList();
				type2 = valueType.GetElementType();
			}
			while (true)
			{
				object obj2 = ReadValue(type2, reader);
				if (obj2 == null && reader.GetToken() == JsonToken.ArrayEnd)
				{
					break;
				}
				list.Add(obj2);
			}
			if (arrayMetadata.GetIsArray())
			{
				int count = list.Count;
				obj = Array.CreateInstance(type2, count);
				for (int i = 0; i < count; i++)
				{
					((Array)obj).SetValue(list[i], i);
				}
			}
			else
			{
				obj = list;
			}
		}
		else if (reader.GetToken() == JsonToken.ObjectStart)
		{
			if (valueType == typeof(object))
			{
				valueType = typeof(Dictionary<string, object>);
			}
			AddObjectMetadata(valueType);
			ObjectMetadata objectMetadata = object_metadata[valueType];
			obj = Activator.CreateInstance(valueType);
			while (true)
			{
				reader.Read();
				if (reader.GetToken() == JsonToken.ObjectEnd)
				{
					break;
				}
				string text = (string)reader.GetValue();
				if (objectMetadata.GetProperties().ContainsKey(text))
				{
					PropertyMetadata propertyMetadata = objectMetadata.GetProperties()[text];
					if (propertyMetadata.IsField)
					{
						((FieldInfo)propertyMetadata.Info).SetValue(obj, ReadValue(propertyMetadata.Type, reader));
						continue;
					}
					PropertyInfo propertyInfo = (PropertyInfo)propertyMetadata.Info;
					if (propertyInfo.CanWrite)
					{
						propertyInfo.SetValue(obj, ReadValue(propertyMetadata.Type, reader), null);
					}
					else
					{
						ReadValue(propertyMetadata.Type, reader);
					}
				}
				else if (!objectMetadata.GetIsDictionary())
				{
					if (!reader.GetSkipNonMembers())
					{
						throw new JsonException(string.Format("The type {0} doesn't have the property '{1}'", valueType, text));
					}
					ReadSkip(reader);
				}
				else
				{
					((IDictionary)obj).Add(text, ReadValue(objectMetadata.GetElementType(), reader));
				}
			}
		}
		return obj;
	}

	private static IJsonWrapper ReadValue(WrapperFactory factory, JsonReader reader)
	{
		reader.Read();
		if (reader.GetToken() == JsonToken.ArrayEnd || reader.GetToken() == JsonToken.Null)
		{
			return null;
		}
		IJsonWrapper wrapper = factory();
		if (reader.GetToken() == JsonToken.String)
		{
			wrapper.SetString((string)reader.GetValue());
			return wrapper;
		}
		if (reader.GetToken() == JsonToken.Double)
		{
			wrapper.SetDouble((double)reader.GetValue());
			return wrapper;
		}
		if (reader.GetToken() == JsonToken.Int)
		{
			wrapper.SetInt((int)reader.GetValue());
			return wrapper;
		}
		if (reader.GetToken() == JsonToken.Long)
		{
			wrapper.SetLong((long)reader.GetValue());
			return wrapper;
		}
		if (reader.GetToken() == JsonToken.Boolean)
		{
			wrapper.SetBoolean((bool)reader.GetValue());
			return wrapper;
		}
		if (reader.GetToken() == JsonToken.ArrayStart)
		{
			wrapper.SetJsonType(JsonType.Array);
			while (true)
			{
				IJsonWrapper item = ReadValue(factory, reader);
				if (item == null && reader.GetToken() == JsonToken.ArrayEnd)
				{
					break;
				}
				wrapper.Add(item);
			}
		}
		else if (reader.GetToken() == JsonToken.ObjectStart)
		{
			wrapper.SetJsonType(JsonType.Object);
			while (true)
			{
				reader.Read();
				if (reader.GetToken() == JsonToken.ObjectEnd)
				{
					break;
				}
				string key = (string)reader.GetValue();
				wrapper[key] = ReadValue(factory, reader);
			}
		}
		return wrapper;
	}

	private static void ReadSkip(JsonReader reader)
	{
		ToWrapper(() => new JsonMockWrapper(), reader);
	}

	private static void RegisterBaseExporters()
	{
		base_exporters_table[typeof(byte)] = (object obj, JsonWriter writer) =>
		{
			writer.Write(Convert.ToInt32((byte)obj));
		};
		base_exporters_table[typeof(char)] = (object obj, JsonWriter writer) =>
		{
			writer.Write(Convert.ToString((char)obj));
		};
		base_exporters_table[typeof(DateTime)] = (object obj, JsonWriter writer) =>
		{
			writer.Write(Convert.ToString((DateTime)obj, datetime_format));
		};
		base_exporters_table[typeof(decimal)] = (object obj, JsonWriter writer) =>
		{
			writer.Write((decimal)obj);
		};
		base_exporters_table[typeof(sbyte)] = (object obj, JsonWriter writer) =>
		{
			writer.Write(Convert.ToInt32((sbyte)obj));
		};
		base_exporters_table[typeof(short)] = (object obj, JsonWriter writer) =>
		{
			writer.Write(Convert.ToInt32((short)obj));
		};
		base_exporters_table[typeof(ushort)] = (object obj, JsonWriter writer) =>
		{
			writer.Write(Convert.ToInt32((ushort)obj));
		};
		base_exporters_table[typeof(uint)] = (object obj, JsonWriter writer) =>
		{
			writer.Write(Convert.ToUInt64((uint)obj));
		};
		base_exporters_table[typeof(ulong)] = (object obj, JsonWriter writer) =>
		{
			writer.Write((ulong)obj);
		};
	}

	private static void RegisterBaseImporters()
	{
		ImporterFunc importer = (object input) => Convert.ToByte((int)input);
		RegisterImporter(base_importers_table, typeof(int), typeof(byte), importer);
		importer = (object input) => Convert.ToUInt64((int)input);
		RegisterImporter(base_importers_table, typeof(int), typeof(ulong), importer);
		importer = (object input) => Convert.ToSByte((int)input);
		RegisterImporter(base_importers_table, typeof(int), typeof(sbyte), importer);
		importer = (object input) => Convert.ToInt16((int)input);
		RegisterImporter(base_importers_table, typeof(int), typeof(short), importer);
		importer = (object input) => Convert.ToUInt16((int)input);
		RegisterImporter(base_importers_table, typeof(int), typeof(ushort), importer);
		importer = (object input) => Convert.ToUInt32((int)input);
		RegisterImporter(base_importers_table, typeof(int), typeof(uint), importer);
		importer = (object input) => Convert.ToSingle((int)input);
		RegisterImporter(base_importers_table, typeof(int), typeof(float), importer);
		importer = (object input) => Convert.ToDouble((int)input);
		RegisterImporter(base_importers_table, typeof(int), typeof(double), importer);
		importer = (object input) => Convert.ToDecimal((double)input);
		RegisterImporter(base_importers_table, typeof(double), typeof(decimal), importer);
		importer = (object input) => Convert.ToUInt32((long)input);
		RegisterImporter(base_importers_table, typeof(long), typeof(uint), importer);
		importer = (object input) => Convert.ToChar((string)input);
		RegisterImporter(base_importers_table, typeof(string), typeof(char), importer);
		importer = (object input) => Convert.ToDateTime((string)input, datetime_format);
		RegisterImporter(base_importers_table, typeof(string), typeof(DateTime), importer);
	}

	private static void RegisterImporter(IDictionary<Type, IDictionary<Type, ImporterFunc>> importersTable, Type jsonType, Type valueType, ImporterFunc importer)
	{
		if (!importersTable.ContainsKey(jsonType))
		{
			importersTable.Add(jsonType, new Dictionary<Type, ImporterFunc>());
		}
		importersTable[jsonType][valueType] = importer;
	}

	private static void WriteValue(object obj, JsonWriter writer, bool isTopLevel, int depth)
	{
		if (depth > max_nesting_depth)
		{
			throw new JsonException(string.Format("Max allowed object depth reached while trying to export from type {0}", obj.GetType()));
		}
		if (obj == null)
		{
			writer.Write(null);
			return;
		}
		if (obj is IJsonWrapper)
		{
			if (isTopLevel)
			{
				writer.GetTextWriter().Write(((IJsonWrapper)obj).ToJson());
			}
			else
			{
				((IJsonWrapper)obj).ToJson(writer);
			}
			return;
		}
		if (obj is string)
		{
			writer.Write((string)obj);
			return;
		}
		if (obj is double)
		{
			writer.Write((double)obj);
			return;
		}
		if (obj is int)
		{
			writer.Write((int)obj);
			return;
		}
		if (obj is bool)
		{
			writer.Write((bool)obj);
			return;
		}
		if (obj is long)
		{
			writer.Write((long)obj);
			return;
		}
		if (obj is Array)
		{
			writer.WriteArrayStart();
			foreach (object item in (Array)obj)
			{
				WriteValue(item, writer, isTopLevel, depth + 1);
			}
			writer.WriteArrayEnd();
			return;
		}
		if (obj is IList)
		{
			writer.WriteArrayStart();
			foreach (object item2 in (IList)obj)
			{
				WriteValue(item2, writer, isTopLevel, depth + 1);
			}
			writer.WriteArrayEnd();
			return;
		}
		if (obj is IDictionary)
		{
			writer.WriteObjectStart();
			foreach (DictionaryEntry item3 in (IDictionary)obj)
			{
				writer.WritePropertyName((string)item3.Key);
				WriteValue(item3.Value, writer, isTopLevel, depth + 1);
			}
			writer.WriteObjectEnd();
			return;
		}
		Type type = obj.GetType();
		if (custom_exporters_table.ContainsKey(type))
		{
			ExporterFunc exporter = custom_exporters_table[type];
			exporter(obj, writer);
			return;
		}
		if (base_exporters_table.ContainsKey(type))
		{
			ExporterFunc exporter = base_exporters_table[type];
			exporter(obj, writer);
			return;
		}
		if (obj is Enum)
		{
			Type underlyingType = Enum.GetUnderlyingType(type);
			if (underlyingType == typeof(long) || underlyingType == typeof(uint) || underlyingType == typeof(ulong))
			{
				writer.Write((ulong)obj);
			}
			else
			{
				writer.Write((int)obj);
			}
			return;
		}
		AddTypeProperties(type);
		IList<PropertyMetadata> list = type_properties[type];
		writer.WriteObjectStart();
		foreach (PropertyMetadata item4 in list)
		{
			if (item4.IsField)
			{
				writer.WritePropertyName(item4.Info.Name);
				WriteValue(((FieldInfo)item4.Info).GetValue(obj), writer, isTopLevel, depth + 1);
				continue;
			}
			PropertyInfo propertyInfo = (PropertyInfo)item4.Info;
			if (propertyInfo.CanRead)
			{
				writer.WritePropertyName(item4.Info.Name);
				WriteValue(propertyInfo.GetValue(obj, null), writer, isTopLevel, depth + 1);
			}
		}
		writer.WriteObjectEnd();
	}

	public static string ToJson(object obj)
	{
		lock (static_writer_lock)
		{
			static_writer.Reset();
			WriteValue(obj, static_writer, true, 0);
			return static_writer.ToString();
		}
	}

	public static void ToJson(object obj, JsonWriter writer)
	{
		WriteValue(obj, writer, false, 0);
	}

	public static JsonData ToObject(JsonReader reader)
	{
		return (JsonData)ToWrapper(() => new JsonData(), reader);
	}

	public static JsonData ToObject(TextReader reader)
	{
		JsonReader jsonReader = new JsonReader(reader);
		return (JsonData)ToWrapper(() => new JsonData(), jsonReader);
	}

	public static JsonData ToObject(string json)
	{
		return (JsonData)ToWrapper(() => new JsonData(), json);
	}

	public static T ToObject<T>(JsonReader reader)
	{
		return (T)ReadValue(typeof(T), reader);
	}

	public static T ToObject<T>(TextReader reader)
	{
		JsonReader jsonReader = new JsonReader(reader);
		return (T)ReadValue(typeof(T), jsonReader);
	}

	public static T ToObject<T>(string json)
	{
		JsonReader jsonReader = new JsonReader(json);
		return (T)ReadValue(typeof(T), jsonReader);
	}

	public static IJsonWrapper ToWrapper(WrapperFactory factory, JsonReader reader)
	{
		return ReadValue(factory, reader);
	}

	public static IJsonWrapper ToWrapper(WrapperFactory factory, string json)
	{
		JsonReader jsonReader = new JsonReader(json);
		return ReadValue(factory, jsonReader);
	}

	public static void RegisterExporter<T>(global::TypedExporterFunc<T> typedExporter)
	{
		ExporterFunc value = (object obj, JsonWriter writer) =>
		{
			typedExporter((T)obj, writer);
		};
		custom_exporters_table[typeof(T)] = value;
	}

	public static void RegisterImporter<TJson, TValue>(global::TypedImporterFunc<TJson, TValue> typedImporter)
	{
		ImporterFunc importer = (object input) => typedImporter((TJson)input);
		RegisterImporter(custom_importers_table, typeof(TJson), typeof(TValue), importer);
	}

	public static void UnregisterExporters()
	{
		custom_exporters_table.Clear();
	}

	public static void UnregisterImporters()
	{
		custom_importers_table.Clear();
	}
}
