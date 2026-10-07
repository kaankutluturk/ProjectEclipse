using System;
using System.Collections.Generic;

public sealed class AnchorAssigningObjectGraphVisitor : ChainedObjectGraphVisitor
{
	private readonly IEventEmitter eventEmitter;

	private readonly IAliasProvider aliasProvider;

	private readonly HashSet<string> emittedAliases = new HashSet<string>();

	public AnchorAssigningObjectGraphVisitor(IObjectGraphVisitor visitor, IEventEmitter emitter, IAliasProvider aliasSource)
		: base(visitor)
	{
		this.eventEmitter = emitter;
		this.aliasProvider = aliasSource;
	}

	public override bool Enter(IObjectDescriptor value)
	{
		string text = aliasProvider.GetAlias(value.GetValue());
		if (text != null && !emittedAliases.Add(text))
		{
			IEventEmitter emitter = eventEmitter;
			AliasEventInfo aliasEvent = new AliasEventInfo(value);
			aliasEvent.set_Alias(text);
			emitter.Emit(aliasEvent);
			return false;
		}
		return base.Enter(value);
	}

	public override void VisitMappingStart(IObjectDescriptor mapping, Type keyType, Type valueType)
	{
		IEventEmitter emitter = eventEmitter;
		MappingStartEventInfo mappingStart = new MappingStartEventInfo(mapping);
		mappingStart.SetAnchor(aliasProvider.GetAlias(mapping.GetValue()));
		emitter.Emit(mappingStart);
	}

	public override void VisitSequenceStart(IObjectDescriptor sequence, Type elementType)
	{
		IEventEmitter emitter = eventEmitter;
		SequenceStartEventInfo sequenceStart = new SequenceStartEventInfo(sequence);
		sequenceStart.SetAnchor(aliasProvider.GetAlias(sequence.GetValue()));
		emitter.Emit(sequenceStart);
	}

	public override void VisitScalar(IObjectDescriptor scalar)
	{
		IEventEmitter emitter = eventEmitter;
		ScalarEventInfo scalarEvent = new ScalarEventInfo(scalar);
		scalarEvent.SetAnchor(aliasProvider.GetAlias(scalar.GetValue()));
		emitter.Emit(scalarEvent);
	}
}
