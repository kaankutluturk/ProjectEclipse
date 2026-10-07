using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

public sealed class Deserializer
{
	private class TypeDescriptorProxy : ITypeInspector
	{
		public ITypeInspector TypeDescriptor;

		public IEnumerable<IPropertyDescriptor> GetProperties(Type type, object container)
		{
			return TypeDescriptor.GetProperties(type, container);
		}

		public IPropertyDescriptor GetProperty(Type type, object container, string name, bool ignoreUnmatched)
		{
			return TypeDescriptor.GetProperty(type, container, name, ignoreUnmatched);
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

	public Deserializer(IObjectFactory objectFactory = null, INamingConvention namingConvention = null, bool ignoreUnmatched = false)
	{
		objectFactory = objectFactory ?? new DefaultObjectFactory();
		namingConvention = namingConvention ?? new NullNamingConvention();
		typeDescriptor.TypeDescriptor = new YamlAttributesTypeInspector(new NamingConventionTypeInspector(new ReadableAndWritablePropertiesTypeInspector(new ReadablePropertiesTypeInspector(new StaticTypeResolver())), namingConvention));
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
		GetNodeDeserializers().Add(new GenericDictionaryNodeDeserializer(objectFactory));
		GetNodeDeserializers().Add(new NonGenericDictionaryNodeDeserializer(objectFactory));
		GetNodeDeserializers().Add(new GenericCollectionNodeDeserializer(objectFactory));
		GetNodeDeserializers().Add(new NonGenericListNodeDeserializer(objectFactory));
		GetNodeDeserializers().Add(new EnumerableNodeDeserializer());
		GetNodeDeserializers().Add(new ObjectNodeDeserializer(objectFactory, typeDescriptor, ignoreUnmatched));
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

	public void RegisterTagMapping(string tag, Type type)
	{
		tagMappings.Add(tag, type);
	}

	public void RegisterTypeConverter(IYamlTypeConverter converter)
	{
		converters.Add(converter);
	}

	public T Deserialize<T>(TextReader input)
	{
		return (T)Deserialize(input, typeof(T));
	}

	public object Deserialize(TextReader input)
	{
		return Deserialize(input, typeof(object));
	}

	public object Deserialize(TextReader input, Type type)
	{
		return Deserialize(new EventReader(new YamlEventParser(input)), type);
	}

	public T Deserialize<T>(EventReader reader)
	{
		return (T)Deserialize(reader, typeof(T));
	}

	public object Deserialize(EventReader reader)
	{
		return Deserialize(reader, typeof(object));
	}

	public object Deserialize(EventReader reader, Type type)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		bool flag = reader.Allow<StreamStart>() != null;
		bool flag2 = reader.Allow<DocumentStart>() != null;
		object result = null;
		if (!reader.Accept<DocumentEnd>() && !reader.Accept<StreamEndEvent>())
		{
			using (SerializerState state = new SerializerState())
			{
				result = valueDeserializer.DeserializeValue(reader, type, state, valueDeserializer);
				state.OnDeserialization();
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
