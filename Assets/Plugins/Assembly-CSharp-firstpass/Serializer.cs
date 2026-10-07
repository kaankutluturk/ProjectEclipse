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

	public Serializer(SerializationOptions LHONCAIFCAF = SerializationOptions.None, INamingConvention LELOAKPLJEH = null)
	{
		this.options = LHONCAIFCAF;
		this.namingConvention = LELOAKPLJEH ?? new NullNamingConvention();
		SetConverters(new List<IYamlTypeConverter>());
		foreach (IYamlTypeConverter item in YamlTypeConverters.GetConverters())
		{
			GetConverters().Add(item);
		}
		object cBMKGNIHPFO;
		if (IsOptionSet(SerializationOptions.DefaultToStaticType))
		{
			ITypeResolver oEBJGLALCDH = new StaticTypeResolver();
			cBMKGNIHPFO = oEBJGLALCDH;
		}
		else
		{
			cBMKGNIHPFO = new DynamicTypeResolver();
		}
		typeResolver = (ITypeResolver)cBMKGNIHPFO;
	}

	internal IList<IYamlTypeConverter> GetConverters()
	{
		return converters;
	}

	private void SetConverters(IList<IYamlTypeConverter> value)
	{
		converters = value;
	}

	private bool IsOptionSet(SerializationOptions LFJBBPIDBCL)
	{
		return (options & LFJBBPIDBCL) != 0;
	}

	public void RegisterTypeConverter(IYamlTypeConverter GMPKPHNBCHA)
	{
		GetConverters().Add(GMPKPHNBCHA);
	}

	public void Serialize(TextWriter writer, object OFDNAFPEAGP)
	{
		Serialize(new Emitter(writer), OFDNAFPEAGP);
	}

	public void Serialize(TextWriter writer, object OFDNAFPEAGP, Type LFLGCDNKNJI)
	{
		Serialize(new Emitter(writer), OFDNAFPEAGP, LFLGCDNKNJI);
	}

	public void Serialize(IEmitter NPIDIMCLNEM, object OFDNAFPEAGP)
	{
		if (NPIDIMCLNEM == null)
		{
			throw new ArgumentNullException("emitter");
		}
		EmitDocument(NPIDIMCLNEM, new ObjectDescriptor(OFDNAFPEAGP, (OFDNAFPEAGP == null) ? typeof(object) : OFDNAFPEAGP.GetType(), typeof(object)));
	}

	public void Serialize(IEmitter NPIDIMCLNEM, object OFDNAFPEAGP, Type LFLGCDNKNJI)
	{
		if (NPIDIMCLNEM == null)
		{
			throw new ArgumentNullException("emitter");
		}
		if (LFLGCDNKNJI == null)
		{
			throw new ArgumentNullException("type");
		}
		EmitDocument(NPIDIMCLNEM, new ObjectDescriptor(OFDNAFPEAGP, LFLGCDNKNJI, LFLGCDNKNJI));
	}

	private void EmitDocument(IEmitter NPIDIMCLNEM, IObjectDescriptor OFDNAFPEAGP)
	{
		IObjectGraphTraversalStrategy bIGFDIOHKIG = CreateTraversalStrategy();
		IEventEmitter oPIGMJHGIDL = CreateEventEmitter(NPIDIMCLNEM);
		IObjectGraphVisitor nKECMANOOEM = CreateEmittingVisitor(NPIDIMCLNEM, bIGFDIOHKIG, oPIGMJHGIDL, OFDNAFPEAGP);
		NPIDIMCLNEM.Emit(new StreamStart());
		NPIDIMCLNEM.Emit(new DocumentStart());
		bIGFDIOHKIG.Traverse(OFDNAFPEAGP, nKECMANOOEM);
		NPIDIMCLNEM.Emit(new DocumentEnd(true));
		NPIDIMCLNEM.Emit(new StreamEndEvent());
	}

	private IObjectGraphVisitor CreateEmittingVisitor(IEmitter NPIDIMCLNEM, IObjectGraphTraversalStrategy PECHNBFNJJG, IEventEmitter OPIGMJHGIDL, IObjectDescriptor OFDNAFPEAGP)
	{
		IObjectGraphVisitor gDMFLLGPLNO = new EmittingObjectGraphVisitor(OPIGMJHGIDL);
		gDMFLLGPLNO = new CustomSerializationObjectGraphVisitor(NPIDIMCLNEM, gDMFLLGPLNO, GetConverters());
		if (!IsOptionSet(SerializationOptions.DisableAliases))
		{
			AnchorAssigner cKGCHJDJLCD = new AnchorAssigner();
			PECHNBFNJJG.Traverse(OFDNAFPEAGP, cKGCHJDJLCD);
			gDMFLLGPLNO = new AnchorAssigningObjectGraphVisitor(gDMFLLGPLNO, OPIGMJHGIDL, cKGCHJDJLCD);
		}
		if (!IsOptionSet(SerializationOptions.EmitDefaults))
		{
			gDMFLLGPLNO = new DefaultExclusiveObjectGraphVisitor(gDMFLLGPLNO);
		}
		return gDMFLLGPLNO;
	}

	private IEventEmitter CreateEventEmitter(IEmitter NPIDIMCLNEM)
	{
		WriterEventEmitter jDJEJDIJLLE = new WriterEventEmitter(NPIDIMCLNEM);
		if (IsOptionSet(SerializationOptions.JsonCompatible))
		{
			return new JsonEventEmitter(jDJEJDIJLLE);
		}
		return new TypeAssigningEventEmitter(jDJEJDIJLLE, IsOptionSet(SerializationOptions.Roundtrip));
	}

	private IObjectGraphTraversalStrategy CreateTraversalStrategy()
	{
		ITypeInspector cECGLIIIJJH = new ReadablePropertiesTypeInspector(typeResolver);
		if (IsOptionSet(SerializationOptions.Roundtrip))
		{
			cECGLIIIJJH = new ReadableAndWritablePropertiesTypeInspector(cECGLIIIJJH);
		}
		cECGLIIIJJH = new NamingConventionTypeInspector(cECGLIIIJJH, namingConvention);
		cECGLIIIJJH = new YamlAttributesTypeInspector(cECGLIIIJJH);
		if (IsOptionSet(SerializationOptions.Roundtrip))
		{
			return new RoundtripObjectGraphTraversalStrategy(this, cECGLIIIJJH, typeResolver, 50);
		}
		return new FullObjectGraphTraversalStrategy(this, cECGLIIIJJH, typeResolver, 50);
	}
}
