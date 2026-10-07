using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

public sealed class Deserializer
{
	private class TypeDescriptorProxy : ITypeInspector
	{
		public ITypeInspector TypeDescriptor;

		public IEnumerable<IPropertyDescriptor> GetProperties(Type LFLGCDNKNJI, object EGJHGBCEPHO)
		{
			return TypeDescriptor.GetProperties(LFLGCDNKNJI, EGJHGBCEPHO);
		}

		public IPropertyDescriptor GetProperty(Type LFLGCDNKNJI, object EGJHGBCEPHO, string name, bool GNFDAJLHBCN)
		{
			return TypeDescriptor.GetProperty(LFLGCDNKNJI, EGJHGBCEPHO, name, GNFDAJLHBCN);
		}
	}

	private static readonly Dictionary<string, Type> predefinedTagMappings = new Dictionary<string, Type>
	{
		{
			"tag:yaml.org,2002:map",
			typeof(Dictionary<object, object>)
		},
		{
			"tag:yaml.org,2002:bool",
			typeof(bool)
		},
		{
			"tag:yaml.org,2002:float",
			typeof(double)
		},
		{
			"tag:yaml.org,2002:int",
			typeof(int)
		},
		{
			"tag:yaml.org,2002:str",
			typeof(string)
		},
		{
			"tag:yaml.org,2002:timestamp",
			typeof(DateTime)
		}
	};

	private readonly Dictionary<string, Type> tagMappings;

	private readonly List<IYamlTypeConverter> converters;

	private TypeDescriptorProxy typeDescriptor = new TypeDescriptorProxy();

	private IValueDeserializer valueDeserializer;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private IList<INodeDeserializer> nodeDeserializers;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private IList<INodeTypeResolver> typeResolvers;

	public IList<INodeDeserializer> NodeDeserializers
	{
		get
		{
			return GetNodeDeserializers();
		}
		private set
		{
			SetNodeDeserializers(value);
		}
	}

	public IList<INodeTypeResolver> TypeResolvers
	{
		get
		{
			return GetTypeResolvers();
		}
		private set
		{
			SetTypeResolvers(value);
		}
	}

	public Deserializer(IObjectFactory EJPHFDCKCCE = null, INamingConvention LELOAKPLJEH = null, bool GNFDAJLHBCN = false)
	{
		EJPHFDCKCCE = EJPHFDCKCCE ?? new DefaultObjectFactory();
		LELOAKPLJEH = LELOAKPLJEH ?? new NullNamingConvention();
		typeDescriptor.TypeDescriptor = new YamlAttributesTypeInspector(new NamingConventionTypeInspector(new ReadableAndWritablePropertiesTypeInspector(new ReadablePropertiesTypeInspector(new StaticTypeResolver())), LELOAKPLJEH));
		converters = new List<IYamlTypeConverter>();
		foreach (IYamlTypeConverter item in YamlTypeConverters.GetConverters())
		{
			converters.Add(item);
		}
		SetNodeDeserializers(new List<INodeDeserializer>());
		GetNodeDeserializers().Add(new TypeConverterNodeDeserializer(converters));
		GetNodeDeserializers().Add(new NullNodeDeserializer());
		GetNodeDeserializers().Add(new ScalarNodeDeserializer());
		GetNodeDeserializers().Add(new ArrayNodeDeserializer());
		GetNodeDeserializers().Add(new GenericDictionaryNodeDeserializer(EJPHFDCKCCE));
		GetNodeDeserializers().Add(new NonGenericDictionaryNodeDeserializer(EJPHFDCKCCE));
		GetNodeDeserializers().Add(new GenericCollectionNodeDeserializer(EJPHFDCKCCE));
		GetNodeDeserializers().Add(new NonGenericListNodeDeserializer(EJPHFDCKCCE));
		GetNodeDeserializers().Add(new EnumerableNodeDeserializer());
		GetNodeDeserializers().Add(new ObjectNodeDeserializer(EJPHFDCKCCE, typeDescriptor, GNFDAJLHBCN));
		tagMappings = new Dictionary<string, Type>(predefinedTagMappings);
		SetTypeResolvers(new List<INodeTypeResolver>());
		GetTypeResolvers().Add(new TagNodeTypeResolver(tagMappings));
		GetTypeResolvers().Add(new TypeNameInTagNodeTypeResolver());
		GetTypeResolvers().Add(new DefaultContainersNodeTypeResolver());
		valueDeserializer = new AliasValueDeserializer(new NodeValueDeserializer(GetNodeDeserializers(), GetTypeResolvers()));
	}

	public IList<INodeDeserializer> GetNodeDeserializers()
	{
		return nodeDeserializers;
	}

	private void SetNodeDeserializers(IList<INodeDeserializer> value)
	{
		nodeDeserializers = value;
	}

	public IList<INodeTypeResolver> GetTypeResolvers()
	{
		return typeResolvers;
	}

	private void SetTypeResolvers(IList<INodeTypeResolver> value)
	{
		typeResolvers = value;
	}

	public void RegisterTagMapping(string EDLADAAKMDF, Type LFLGCDNKNJI)
	{
		tagMappings.Add(EDLADAAKMDF, LFLGCDNKNJI);
	}

	public void RegisterTypeConverter(IYamlTypeConverter OOBNDNCCFJI)
	{
		converters.Add(OOBNDNCCFJI);
	}

	public T Deserialize<T>(TextReader NILNDHEKNLJ)
	{
		return (T)Deserialize(NILNDHEKNLJ, typeof(T));
	}

	public object Deserialize(TextReader NILNDHEKNLJ)
	{
		return Deserialize(NILNDHEKNLJ, typeof(object));
	}

	public object Deserialize(TextReader NILNDHEKNLJ, Type LFLGCDNKNJI)
	{
		return Deserialize(new EventReader(new YamlEventParser(NILNDHEKNLJ)), LFLGCDNKNJI);
	}

	public T Deserialize<T>(EventReader reader)
	{
		return (T)Deserialize(reader, typeof(T));
	}

	public object Deserialize(EventReader reader)
	{
		return Deserialize(reader, typeof(object));
	}

	public object Deserialize(EventReader reader, Type LFLGCDNKNJI)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (LFLGCDNKNJI == null)
		{
			throw new ArgumentNullException("type");
		}
		bool flag = reader.Allow<StreamStart>() != null;
		bool flag2 = reader.Allow<DocumentStart>() != null;
		object result = null;
		if (!reader.Accept<DocumentEnd>() && !reader.Accept<StreamEndEvent>())
		{
			using (SerializerState mLKGCMPCCCB = new SerializerState())
			{
				result = valueDeserializer.DeserializeValue(reader, LFLGCDNKNJI, mLKGCMPCCCB, valueDeserializer);
				mLKGCMPCCCB.OnDeserialization();
			}
		}
		if (flag2)
		{
			reader.Expect<DocumentEnd>();
		}
		if (flag)
		{
			reader.Expect<StreamEndEvent>();
		}
		return result;
	}
}
