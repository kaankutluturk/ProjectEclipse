using System;

public abstract class ChainedEventEmitter : IEventEmitter
{
	protected readonly IEventEmitter nextEmitter;

	protected ChainedEventEmitter(IEventEmitter JDJEJDIJLLE)
	{
		if (JDJEJDIJLLE == null)
		{
			throw new ArgumentNullException("nextEmitter");
		}
		this.nextEmitter = JDJEJDIJLLE;
	}

	public virtual void Emit(AliasEventInfo FNHCFCAALAE)
	{
		nextEmitter.Emit(FNHCFCAALAE);
	}

	public virtual void Emit(ScalarEventInfo FNHCFCAALAE)
	{
		nextEmitter.Emit(FNHCFCAALAE);
	}

	public virtual void Emit(MappingStartEventInfo FNHCFCAALAE)
	{
		nextEmitter.Emit(FNHCFCAALAE);
	}

	public virtual void Emit(MappingEndEventInfo FNHCFCAALAE)
	{
		nextEmitter.Emit(FNHCFCAALAE);
	}

	public virtual void Emit(SequenceStartEventInfo FNHCFCAALAE)
	{
		nextEmitter.Emit(FNHCFCAALAE);
	}

	public virtual void Emit(SequenceEndEventInfo FNHCFCAALAE)
	{
		nextEmitter.Emit(FNHCFCAALAE);
	}
}
