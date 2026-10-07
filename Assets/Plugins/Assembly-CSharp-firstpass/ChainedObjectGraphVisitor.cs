using System;

public abstract class ChainedObjectGraphVisitor : IObjectGraphVisitor
{
	private readonly IObjectGraphVisitor nextVisitor;

	protected ChainedObjectGraphVisitor(IObjectGraphVisitor GDMFLLGPLNO)
	{
		this.nextVisitor = GDMFLLGPLNO;
	}

	public virtual bool Enter(IObjectDescriptor value)
	{
		return nextVisitor.Enter(value);
	}

	public virtual bool EnterMapping(IObjectDescriptor KGBGENDIMBC, IObjectDescriptor value)
	{
		return nextVisitor.EnterMapping(KGBGENDIMBC, value);
	}

	public virtual bool EnterMapping(IPropertyDescriptor KGBGENDIMBC, IObjectDescriptor value)
	{
		return nextVisitor.EnterMapping(KGBGENDIMBC, value);
	}

	public virtual void VisitScalar(IObjectDescriptor ADDIBOMFCNH)
	{
		nextVisitor.VisitScalar(ADDIBOMFCNH);
	}

	public virtual void VisitMappingStart(IObjectDescriptor JPEFEBICPFI, Type FHNELPLPIPI, Type EJGJHBGMCDM)
	{
		nextVisitor.VisitMappingStart(JPEFEBICPFI, FHNELPLPIPI, EJGJHBGMCDM);
	}

	public virtual void VisitMappingEnd(IObjectDescriptor JPEFEBICPFI)
	{
		nextVisitor.VisitMappingEnd(JPEFEBICPFI);
	}

	public virtual void VisitSequenceStart(IObjectDescriptor sequence, Type LKAAAFHOAGD)
	{
		nextVisitor.VisitSequenceStart(sequence, LKAAAFHOAGD);
	}

	public virtual void VisitSequenceEnd(IObjectDescriptor sequence)
	{
		nextVisitor.VisitSequenceEnd(sequence);
	}
}
