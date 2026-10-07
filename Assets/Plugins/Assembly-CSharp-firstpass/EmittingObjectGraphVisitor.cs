using System;

public sealed class EmittingObjectGraphVisitor : IObjectGraphVisitor
{
	private readonly IEventEmitter eventEmitter;

	public EmittingObjectGraphVisitor(IEventEmitter OPIGMJHGIDL)
	{
		this.eventEmitter = OPIGMJHGIDL;
	}

	bool IObjectGraphVisitor.Enter(IObjectDescriptor value)
	{
		return true;
	}

	bool IObjectGraphVisitor.EnterMapping(IObjectDescriptor KGBGENDIMBC, IObjectDescriptor value)
	{
		return true;
	}

	bool IObjectGraphVisitor.EnterMapping(IPropertyDescriptor KGBGENDIMBC, IObjectDescriptor value)
	{
		return true;
	}

	void IObjectGraphVisitor.VisitScalar(IObjectDescriptor ADDIBOMFCNH)
	{
		eventEmitter.Emit(new ScalarEventInfo(ADDIBOMFCNH));
	}

	void IObjectGraphVisitor.VisitMappingStart(IObjectDescriptor JPEFEBICPFI, Type FHNELPLPIPI, Type EJGJHBGMCDM)
	{
		eventEmitter.Emit(new MappingStartEventInfo(JPEFEBICPFI));
	}

	void IObjectGraphVisitor.VisitMappingEnd(IObjectDescriptor JPEFEBICPFI)
	{
		eventEmitter.Emit(new MappingEndEventInfo(JPEFEBICPFI));
	}

	void IObjectGraphVisitor.VisitSequenceStart(IObjectDescriptor sequence, Type LKAAAFHOAGD)
	{
		eventEmitter.Emit(new SequenceStartEventInfo(sequence));
	}

	void IObjectGraphVisitor.VisitSequenceEnd(IObjectDescriptor sequence)
	{
		eventEmitter.Emit(new SequenceEndEventInfo(sequence));
	}
}
