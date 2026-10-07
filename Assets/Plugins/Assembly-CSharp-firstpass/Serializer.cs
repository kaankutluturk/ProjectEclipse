using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

public sealed class Serializer
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private IList<IYamlTypeConverter> converters;

	private readonly SerializationOptions options;

	private readonly INamingConvention namingConvention;

	private readonly ITypeResolver typeResolver;

	internal IList<IYamlTypeConverter> Converters
	{
		get
		{
			return GetConverters();
		}
		private set
		{
			SetConverters(value);
		}
	}

	public Serializer(SerializationOptions serializationOptions = SerializationOptions.None, INamingConvention convention = null)
	{
		this.options = serializationOptions;
		this.namingConvention = convention ?? new NullNamingConvention();
		SetConverters(new List<IYamlTypeConverter>());
		foreach (IYamlTypeConverter item in YamlTypeConverters.GetConverters())
		{
			GetConverters().Add(item);
		}
		object resolver;
		if (IsOptionSet(SerializationOptions.DefaultToStaticType))
		{
			ITypeResolver staticResolver = new StaticTypeResolver();
			resolver = staticResolver;
		}
		else
		{
			resolver = new DynamicTypeResolver();
		}
		typeResolver = (ITypeResolver)resolver;
	}

	internal IList<IYamlTypeConverter> GetConverters()
	{
		return converters;
	}

	private void SetConverters(IList<IYamlTypeConverter> value)
	{
		converters = value;
	}

	private bool IsOptionSet(SerializationOptions option)
	{
		return (options & option) != 0;
	}

	public void RegisterTypeConverter(IYamlTypeConverter converter)
	{
		GetConverters().Add(converter);
	}

	public void Serialize(TextWriter writer, object graph)
	{
		Serialize(new Emitter(writer), graph);
	}

	public void Serialize(TextWriter writer, object graph, Type type)
	{
		Serialize(new Emitter(writer), graph, type);
	}

	public void Serialize(IEmitter emitter, object graph)
	{
		if (emitter == null)
		{
			throw new ArgumentNullException("emitter");
		}
		EmitDocument(emitter, new ObjectDescriptor(graph, (graph == null) ? typeof(object) : graph.GetType(), typeof(object)));
	}

	public void Serialize(IEmitter emitter, object graph, Type type)
	{
		if (emitter == null)
		{
			throw new ArgumentNullException("emitter");
		}
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		EmitDocument(emitter, new ObjectDescriptor(graph, type, type));
	}

	private void EmitDocument(IEmitter emitter, IObjectDescriptor graph)
	{
		IObjectGraphTraversalStrategy traversalStrategy = CreateTraversalStrategy();
		IEventEmitter eventEmitter = CreateEventEmitter(emitter);
		IObjectGraphVisitor visitor = CreateEmittingVisitor(emitter, traversalStrategy, eventEmitter, graph);
		emitter.Emit(new StreamStart());
		emitter.Emit(new DocumentStart());
		traversalStrategy.Traverse(graph, visitor);
		emitter.Emit(new DocumentEnd(true));
		emitter.Emit(new StreamEndEvent());
	}

	private IObjectGraphVisitor CreateEmittingVisitor(IEmitter emitter, IObjectGraphTraversalStrategy traversalStrategy, IEventEmitter eventEmitter, IObjectDescriptor graph)
	{
		IObjectGraphVisitor emittingVisitor = new EmittingObjectGraphVisitor(eventEmitter);
		emittingVisitor = new CustomSerializationObjectGraphVisitor(emitter, emittingVisitor, GetConverters());
		if (!IsOptionSet(SerializationOptions.DisableAliases))
		{
			AnchorAssigner anchorAssigner = new AnchorAssigner();
			traversalStrategy.Traverse(graph, anchorAssigner);
			emittingVisitor = new AnchorAssigningObjectGraphVisitor(emittingVisitor, eventEmitter, anchorAssigner);
		}
		if (!IsOptionSet(SerializationOptions.EmitDefaults))
		{
			emittingVisitor = new DefaultExclusiveObjectGraphVisitor(emittingVisitor);
		}
		return emittingVisitor;
	}

	private IEventEmitter CreateEventEmitter(IEmitter emitter)
	{
		WriterEventEmitter writerEventEmitter = new WriterEventEmitter(emitter);
		if (IsOptionSet(SerializationOptions.JsonCompatible))
		{
			return new JsonEventEmitter(writerEventEmitter);
		}
		return new TypeAssigningEventEmitter(writerEventEmitter, IsOptionSet(SerializationOptions.Roundtrip));
	}

	private IObjectGraphTraversalStrategy CreateTraversalStrategy()
	{
		ITypeInspector inspector = new ReadablePropertiesTypeInspector(typeResolver);
		if (IsOptionSet(SerializationOptions.Roundtrip))
		{
			inspector = new ReadableAndWritablePropertiesTypeInspector(inspector);
		}
		inspector = new NamingConventionTypeInspector(inspector, namingConvention);
		inspector = new YamlAttributesTypeInspector(inspector);
		if (IsOptionSet(SerializationOptions.Roundtrip))
		{
			return new RoundtripObjectGraphTraversalStrategy(this, inspector, typeResolver, 50);
		}
		return new FullObjectGraphTraversalStrategy(this, inspector, typeResolver, 50);
	}
}
