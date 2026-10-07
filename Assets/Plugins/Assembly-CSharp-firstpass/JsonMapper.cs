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

	private static bool HasInterface(Type LFLGCDNKNJI, string name)
	{
		return LFLGCDNKNJI.GetInterface(name, true) != null;
	}

	public static PropertyInfo[] GetPublicInstanceProperties(Type LFLGCDNKNJI)
	{
		return LFLGCDNKNJI.GetProperties();
	}

	private static void AddArrayMetadata(Type LFLGCDNKNJI)
	{
		if (array_metadata.ContainsKey(LFLGCDNKNJI))
		{
			return;
		}
		ArrayMetadata value = default(ArrayMetadata);
		value.SetIsArray(LFLGCDNKNJI.IsArray);
		if (HasInterface(LFLGCDNKNJI, "System.Collections.IList"))
		{
			value.SetIsList(true);
		}
		PropertyInfo[] array = GetPublicInstanceProperties(LFLGCDNKNJI);
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
				array_metadata.Add(LFLGCDNKNJI, value);
			}
			catch (ArgumentException)
			{
			}
		}
	}

	private static void AddObjectMetadata(Type LFLGCDNKNJI)
	{
		if (object_metadata.ContainsKey(LFLGCDNKNJI))
		{
			return;
		}
		ObjectMetadata value = default(ObjectMetadata);
		if (HasInterface(LFLGCDNKNJI, "System.Collections.IDictionary"))
		{
			value.set_IsDictionary(true);
		}
		value.SetProperties(new Dictionary<string, PropertyMetadata>());
		PropertyInfo[] array = GetPublicInstanceProperties(LFLGCDNKNJI);
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
		FieldInfo[] fields = LFLGCDNKNJI.GetFields();
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
				object_metadata.Add(LFLGCDNKNJI, value);
			}
			catch (ArgumentException)
			{
			}
		}
	}

	private static void AddTypeProperties(Type LFLGCDNKNJI)
	{
		if (type_properties.ContainsKey(LFLGCDNKNJI))
		{
			return;
		}
		IList<PropertyMetadata> list = new List<PropertyMetadata>();
		PropertyInfo[] array = GetPublicInstanceProperties(LFLGCDNKNJI);
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
		FieldInfo[] fields = LFLGCDNKNJI.GetFields();
		foreach (FieldInfo cGJLHJGIGHD in fields)
		{
			list.Add(new PropertyMetadata
			{
				Info = cGJLHJGIGHD,
				IsField = true
			});
		}
		lock (type_properties_lock)
		{
			try
			{
				type_properties.Add(LFLGCDNKNJI, list);
			}
			catch (ArgumentException)
			{
			}
		}
	}

	private static MethodInfo GetConvOp(Type GKLIKMLCGFB, Type KGONIIAMHAP)
	{
		lock (conv_ops_lock)
		{
			if (!conv_ops.ContainsKey(GKLIKMLCGFB))
			{
				conv_ops.Add(GKLIKMLCGFB, new Dictionary<Type, MethodInfo>());
			}
		}
		if (conv_ops[GKLIKMLCGFB].ContainsKey(KGONIIAMHAP))
		{
			return conv_ops[GKLIKMLCGFB][KGONIIAMHAP];
		}
		MethodInfo method = GKLIKMLCGFB.GetMethod("op_Implicit", new Type[1] { KGONIIAMHAP });
		lock (conv_ops_lock)
		{
			try
			{
				conv_ops[GKLIKMLCGFB].Add(KGONIIAMHAP, method);
				return method;
			}
			catch (ArgumentException)
			{
				return conv_ops[GKLIKMLCGFB][KGONIIAMHAP];
			}
		}
	}

	private static object ReadValue(Type DHLFLIEGLOK, JsonReader reader)
	{
		reader.Read();
		if (reader.GetToken() == JsonToken.ArrayEnd)
		{
			return null;
		}
		if (reader.GetToken() == JsonToken.Null)
		{
			if (!DHLFLIEGLOK.IsClass)
			{
				throw new JsonException(string.Format("Can't assign null to an instance of type {0}", DHLFLIEGLOK));
			}
			return null;
		}
		if (reader.GetToken() == JsonToken.Double || reader.GetToken() == JsonToken.Int || reader.GetToken() == JsonToken.Long || reader.GetToken() == JsonToken.String || reader.GetToken() == JsonToken.Boolean)
		{
			Type type = reader.GetValue().GetType();
			if (DHLFLIEGLOK.IsAssignableFrom(type))
			{
				return reader.GetValue();
			}
			if (custom_importers_table.ContainsKey(type) && custom_importers_table[type].ContainsKey(DHLFLIEGLOK))
			{
				ImporterFunc iPPLMFLBMNF = custom_importers_table[type][DHLFLIEGLOK];
				return iPPLMFLBMNF(reader.GetValue());
			}
			if (base_importers_table.ContainsKey(type) && base_importers_table[type].ContainsKey(DHLFLIEGLOK))
			{
				ImporterFunc iPPLMFLBMNF2 = base_importers_table[type][DHLFLIEGLOK];
				return iPPLMFLBMNF2(reader.GetValue());
			}
			if (DHLFLIEGLOK.IsEnum)
			{
				return Enum.ToObject(DHLFLIEGLOK, reader.GetValue());
			}
			MethodInfo methodInfo = GetConvOp(DHLFLIEGLOK, type);
			if (methodInfo != null)
			{
				return methodInfo.Invoke(null, new object[1] { reader.GetValue() });
			}
			throw new JsonException(string.Format("Can't assign value '{0}' (type {1}) to type {2}", reader.GetValue(), type, DHLFLIEGLOK));
		}
		object obj = null;
		if (reader.GetToken() == JsonToken.ArrayStart)
		{
			if (DHLFLIEGLOK.FullName == "System.Object")
			{
				DHLFLIEGLOK = typeof(object[]);
			}
			AddArrayMetadata(DHLFLIEGLOK);
			ArrayMetadata gKAHCDHJLBP = array_metadata[DHLFLIEGLOK];
			if (!gKAHCDHJLBP.GetIsArray() && !gKAHCDHJLBP.GetIsList())
			{
				throw new JsonException(string.Format("Type {0} can't act as an array", DHLFLIEGLOK));
			}
			IList list;
			Type type2;
			if (!gKAHCDHJLBP.GetIsArray())
			{
				list = (IList)Activator.CreateInstance(DHLFLIEGLOK);
				type2 = gKAHCDHJLBP.GetElementType();
			}
			else
			{
				list = new ArrayList();
				type2 = DHLFLIEGLOK.GetElementType();
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
			if (gKAHCDHJLBP.GetIsArray())
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
			if (DHLFLIEGLOK == typeof(object))
			{
				DHLFLIEGLOK = typeof(Dictionary<string, object>);
			}
			AddObjectMetadata(DHLFLIEGLOK);
			ObjectMetadata iNPMDAGENOB = object_metadata[DHLFLIEGLOK];
			obj = Activator.CreateInstance(DHLFLIEGLOK);
			while (true)
			{
				reader.Read();
				if (reader.GetToken() == JsonToken.ObjectEnd)
				{
					break;
				}
				string text = (string)reader.GetValue();
				if (iNPMDAGENOB.GetProperties().ContainsKey(text))
				{
					PropertyMetadata nOCGBHBELKO = iNPMDAGENOB.GetProperties()[text];
					if (nOCGBHBELKO.IsField)
					{
						((FieldInfo)nOCGBHBELKO.Info).SetValue(obj, ReadValue(nOCGBHBELKO.Type, reader));
						continue;
					}
					PropertyInfo propertyInfo = (PropertyInfo)nOCGBHBELKO.Info;
					if (propertyInfo.CanWrite)
					{
						propertyInfo.SetValue(obj, ReadValue(nOCGBHBELKO.Type, reader), null);
					}
					else
					{
						ReadValue(nOCGBHBELKO.Type, reader);
					}
				}
				else if (!iNPMDAGENOB.GetIsDictionary())
				{
					if (!reader.GetSkipNonMembers())
					{
						throw new JsonException(string.Format("The type {0} doesn't have the property '{1}'", DHLFLIEGLOK, text));
					}
					ReadSkip(reader);
				}
				else
				{
					((IDictionary)obj).Add(text, ReadValue(iNPMDAGENOB.GetElementType(), reader));
				}
			}
		}
		return obj;
	}

	private static IJsonWrapper ReadValue(WrapperFactory DJFCIPIMOBC, JsonReader reader)
	{
		reader.Read();
		if (reader.GetToken() == JsonToken.ArrayEnd || reader.GetToken() == JsonToken.Null)
		{
			return null;
		}
		IJsonWrapper pIIMPPKAOCI = DJFCIPIMOBC();
		if (reader.GetToken() == JsonToken.String)
		{
			pIIMPPKAOCI.SetString((string)reader.GetValue());
			return pIIMPPKAOCI;
		}
		if (reader.GetToken() == JsonToken.Double)
		{
			pIIMPPKAOCI.SetDouble((double)reader.GetValue());
			return pIIMPPKAOCI;
		}
		if (reader.GetToken() == JsonToken.Int)
		{
			pIIMPPKAOCI.SetInt((int)reader.GetValue());
			return pIIMPPKAOCI;
		}
		if (reader.GetToken() == JsonToken.Long)
		{
			pIIMPPKAOCI.SetLong((long)reader.GetValue());
			return pIIMPPKAOCI;
		}
		if (reader.GetToken() == JsonToken.Boolean)
		{
			pIIMPPKAOCI.SetBoolean((bool)reader.GetValue());
			return pIIMPPKAOCI;
		}
		if (reader.GetToken() == JsonToken.ArrayStart)
		{
			pIIMPPKAOCI.SetJsonType(JsonType.Array);
			while (true)
			{
				IJsonWrapper pIIMPPKAOCI2 = ReadValue(DJFCIPIMOBC, reader);
				if (pIIMPPKAOCI2 == null && reader.GetToken() == JsonToken.ArrayEnd)
				{
					break;
				}
				pIIMPPKAOCI.Add(pIIMPPKAOCI2);
			}
		}
		else if (reader.GetToken() == JsonToken.ObjectStart)
		{
			pIIMPPKAOCI.SetJsonType(JsonType.Object);
			while (true)
			{
				reader.Read();
				if (reader.GetToken() == JsonToken.ObjectEnd)
				{
					break;
				}
				string key = (string)reader.GetValue();
				pIIMPPKAOCI[key] = ReadValue(DJFCIPIMOBC, reader);
			}
		}
		return pIIMPPKAOCI;
	}

	private static void ReadSkip(JsonReader reader)
	{
		ToWrapper(() => new JsonMockWrapper(), reader);
	}

	private static void RegisterBaseExporters()
	{
		base_exporters_table[typeof(byte)] = (object AOMLCBHAJJH, JsonWriter writer) =>
		{
			writer.Write(Convert.ToInt32((byte)AOMLCBHAJJH));
		};
		base_exporters_table[typeof(char)] = (object AOMLCBHAJJH, JsonWriter writer) =>
		{
			writer.Write(Convert.ToString((char)AOMLCBHAJJH));
		};
		base_exporters_table[typeof(DateTime)] = (object AOMLCBHAJJH, JsonWriter writer) =>
		{
			writer.Write(Convert.ToString((DateTime)AOMLCBHAJJH, datetime_format));
		};
		base_exporters_table[typeof(decimal)] = (object AOMLCBHAJJH, JsonWriter writer) =>
		{
			writer.Write((decimal)AOMLCBHAJJH);
		};
		base_exporters_table[typeof(sbyte)] = (object AOMLCBHAJJH, JsonWriter writer) =>
		{
			writer.Write(Convert.ToInt32((sbyte)AOMLCBHAJJH));
		};
		base_exporters_table[typeof(short)] = (object AOMLCBHAJJH, JsonWriter writer) =>
		{
			writer.Write(Convert.ToInt32((short)AOMLCBHAJJH));
		};
		base_exporters_table[typeof(ushort)] = (object AOMLCBHAJJH, JsonWriter writer) =>
		{
			writer.Write(Convert.ToInt32((ushort)AOMLCBHAJJH));
		};
		base_exporters_table[typeof(uint)] = (object AOMLCBHAJJH, JsonWriter writer) =>
		{
			writer.Write(Convert.ToUInt64((uint)AOMLCBHAJJH));
		};
		base_exporters_table[typeof(ulong)] = (object AOMLCBHAJJH, JsonWriter writer) =>
		{
			writer.Write((ulong)AOMLCBHAJJH);
		};
	}

	private static void RegisterBaseImporters()
	{
		ImporterFunc pAOKGMMLPNC = (object NILNDHEKNLJ) => Convert.ToByte((int)NILNDHEKNLJ);
		RegisterImporter(base_importers_table, typeof(int), typeof(byte), pAOKGMMLPNC);
		pAOKGMMLPNC = (object NILNDHEKNLJ) => Convert.ToUInt64((int)NILNDHEKNLJ);
		RegisterImporter(base_importers_table, typeof(int), typeof(ulong), pAOKGMMLPNC);
		pAOKGMMLPNC = (object NILNDHEKNLJ) => Convert.ToSByte((int)NILNDHEKNLJ);
		RegisterImporter(base_importers_table, typeof(int), typeof(sbyte), pAOKGMMLPNC);
		pAOKGMMLPNC = (object NILNDHEKNLJ) => Convert.ToInt16((int)NILNDHEKNLJ);
		RegisterImporter(base_importers_table, typeof(int), typeof(short), pAOKGMMLPNC);
		pAOKGMMLPNC = (object NILNDHEKNLJ) => Convert.ToUInt16((int)NILNDHEKNLJ);
		RegisterImporter(base_importers_table, typeof(int), typeof(ushort), pAOKGMMLPNC);
		pAOKGMMLPNC = (object NILNDHEKNLJ) => Convert.ToUInt32((int)NILNDHEKNLJ);
		RegisterImporter(base_importers_table, typeof(int), typeof(uint), pAOKGMMLPNC);
		pAOKGMMLPNC = (object NILNDHEKNLJ) => Convert.ToSingle((int)NILNDHEKNLJ);
		RegisterImporter(base_importers_table, typeof(int), typeof(float), pAOKGMMLPNC);
		pAOKGMMLPNC = (object NILNDHEKNLJ) => Convert.ToDouble((int)NILNDHEKNLJ);
		RegisterImporter(base_importers_table, typeof(int), typeof(double), pAOKGMMLPNC);
		pAOKGMMLPNC = (object NILNDHEKNLJ) => Convert.ToDecimal((double)NILNDHEKNLJ);
		RegisterImporter(base_importers_table, typeof(double), typeof(decimal), pAOKGMMLPNC);
		pAOKGMMLPNC = (object NILNDHEKNLJ) => Convert.ToUInt32((long)NILNDHEKNLJ);
		RegisterImporter(base_importers_table, typeof(long), typeof(uint), pAOKGMMLPNC);
		pAOKGMMLPNC = (object NILNDHEKNLJ) => Convert.ToChar((string)NILNDHEKNLJ);
		RegisterImporter(base_importers_table, typeof(string), typeof(char), pAOKGMMLPNC);
		pAOKGMMLPNC = (object NILNDHEKNLJ) => Convert.ToDateTime((string)NILNDHEKNLJ, datetime_format);
		RegisterImporter(base_importers_table, typeof(string), typeof(DateTime), pAOKGMMLPNC);
	}

	private static void RegisterImporter(IDictionary<Type, IDictionary<Type, ImporterFunc>> BFGHBIMJHAK, Type FMOEMAEOMCK, Type MFCKGHCBHGC, ImporterFunc PAOKGMMLPNC)
	{
		if (!BFGHBIMJHAK.ContainsKey(FMOEMAEOMCK))
		{
			BFGHBIMJHAK.Add(FMOEMAEOMCK, new Dictionary<Type, ImporterFunc>());
		}
		BFGHBIMJHAK[FMOEMAEOMCK][MFCKGHCBHGC] = PAOKGMMLPNC;
	}

	private static void WriteValue(object AOMLCBHAJJH, JsonWriter writer, bool PHKCIGJLGDM, int depth)
	{
		if (depth > max_nesting_depth)
		{
			throw new JsonException(string.Format("Max allowed object depth reached while trying to export from type {0}", AOMLCBHAJJH.GetType()));
		}
		if (AOMLCBHAJJH == null)
		{
			writer.Write(null);
			return;
		}
		if (AOMLCBHAJJH is IJsonWrapper)
		{
			if (PHKCIGJLGDM)
			{
				writer.GetTextWriter().Write(((IJsonWrapper)AOMLCBHAJJH).ToJson());
			}
			else
			{
				((IJsonWrapper)AOMLCBHAJJH).ToJson(writer);
			}
			return;
		}
		if (AOMLCBHAJJH is string)
		{
			writer.Write((string)AOMLCBHAJJH);
			return;
		}
		if (AOMLCBHAJJH is double)
		{
			writer.Write((double)AOMLCBHAJJH);
			return;
		}
		if (AOMLCBHAJJH is int)
		{
			writer.Write((int)AOMLCBHAJJH);
			return;
		}
		if (AOMLCBHAJJH is bool)
		{
			writer.Write((bool)AOMLCBHAJJH);
			return;
		}
		if (AOMLCBHAJJH is long)
		{
			writer.Write((long)AOMLCBHAJJH);
			return;
		}
		if (AOMLCBHAJJH is Array)
		{
			writer.WriteArrayStart();
			foreach (object item in (Array)AOMLCBHAJJH)
			{
				WriteValue(item, writer, PHKCIGJLGDM, depth + 1);
			}
			writer.WriteArrayEnd();
			return;
		}
		if (AOMLCBHAJJH is IList)
		{
			writer.WriteArrayStart();
			foreach (object item2 in (IList)AOMLCBHAJJH)
			{
				WriteValue(item2, writer, PHKCIGJLGDM, depth + 1);
			}
			writer.WriteArrayEnd();
			return;
		}
		if (AOMLCBHAJJH is IDictionary)
		{
			writer.WriteObjectStart();
			foreach (DictionaryEntry item3 in (IDictionary)AOMLCBHAJJH)
			{
				writer.WritePropertyName((string)item3.Key);
				WriteValue(item3.Value, writer, PHKCIGJLGDM, depth + 1);
			}
			writer.WriteObjectEnd();
			return;
		}
		Type type = AOMLCBHAJJH.GetType();
		if (custom_exporters_table.ContainsKey(type))
		{
			ExporterFunc nHMEKPMHION = custom_exporters_table[type];
			nHMEKPMHION(AOMLCBHAJJH, writer);
			return;
		}
		if (base_exporters_table.ContainsKey(type))
		{
			ExporterFunc nHMEKPMHION2 = base_exporters_table[type];
			nHMEKPMHION2(AOMLCBHAJJH, writer);
			return;
		}
		if (AOMLCBHAJJH is Enum)
		{
			Type underlyingType = Enum.GetUnderlyingType(type);
			if (underlyingType == typeof(long) || underlyingType == typeof(uint) || underlyingType == typeof(ulong))
			{
				writer.Write((ulong)AOMLCBHAJJH);
			}
			else
			{
				writer.Write((int)AOMLCBHAJJH);
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
				WriteValue(((FieldInfo)item4.Info).GetValue(AOMLCBHAJJH), writer, PHKCIGJLGDM, depth + 1);
				continue;
			}
			PropertyInfo propertyInfo = (PropertyInfo)item4.Info;
			if (propertyInfo.CanRead)
			{
				writer.WritePropertyName(item4.Info.Name);
				WriteValue(propertyInfo.GetValue(AOMLCBHAJJH, null), writer, PHKCIGJLGDM, depth + 1);
			}
		}
		writer.WriteObjectEnd();
	}

	public static string ToJson(object AOMLCBHAJJH)
	{
		lock (static_writer_lock)
		{
			static_writer.Reset();
			WriteValue(AOMLCBHAJJH, static_writer, true, 0);
			return static_writer.ToString();
		}
	}

	public static void ToJson(object AOMLCBHAJJH, JsonWriter writer)
	{
		WriteValue(AOMLCBHAJJH, writer, false, 0);
	}

	public static JsonData ToObject(JsonReader reader)
	{
		return (JsonData)ToWrapper(() => new JsonData(), reader);
	}

	public static JsonData ToObject(TextReader reader)
	{
		JsonReader iJIMLLIHKGN = new JsonReader(reader);
		return (JsonData)ToWrapper(() => new JsonData(), iJIMLLIHKGN);
	}

	public static JsonData ToObject(string EMDHMHOKGFP)
	{
		return (JsonData)ToWrapper(() => new JsonData(), EMDHMHOKGFP);
	}

	public static T ToObject<T>(JsonReader reader)
	{
		return (T)ReadValue(typeof(T), reader);
	}

	public static T ToObject<T>(TextReader reader)
	{
		JsonReader iJIMLLIHKGN = new JsonReader(reader);
		return (T)ReadValue(typeof(T), iJIMLLIHKGN);
	}

	public static T ToObject<T>(string EMDHMHOKGFP)
	{
		JsonReader iJIMLLIHKGN = new JsonReader(EMDHMHOKGFP);
		return (T)ReadValue(typeof(T), iJIMLLIHKGN);
	}

	public static IJsonWrapper ToWrapper(WrapperFactory DJFCIPIMOBC, JsonReader reader)
	{
		return ReadValue(DJFCIPIMOBC, reader);
	}

	public static IJsonWrapper ToWrapper(WrapperFactory DJFCIPIMOBC, string EMDHMHOKGFP)
	{
		JsonReader iJIMLLIHKGN = new JsonReader(EMDHMHOKGFP);
		return ReadValue(DJFCIPIMOBC, iJIMLLIHKGN);
	}

	public static void RegisterExporter<T>(global::TypedExporterFunc<T> ACIKPHKFNMH)
	{
		ExporterFunc value = (object AOMLCBHAJJH, JsonWriter writer) =>
		{
			ACIKPHKFNMH((T)AOMLCBHAJJH, writer);
		};
		custom_exporters_table[typeof(T)] = value;
	}

	public static void RegisterImporter<TJson, TValue>(global::TypedImporterFunc<TJson, TValue> PAOKGMMLPNC)
	{
		ImporterFunc pAOKGMMLPNC = (object NILNDHEKNLJ) => PAOKGMMLPNC((TJson)NILNDHEKNLJ);
		RegisterImporter(custom_importers_table, typeof(TJson), typeof(TValue), pAOKGMMLPNC);
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
