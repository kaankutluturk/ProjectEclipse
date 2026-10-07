using System;

public abstract class ChainedEventEmitter : IEventEmitter
{
	protected readonly IEventEmitter nextEmitter;

	protected ChainedEventEmitter(IEventEmitter emitter)
	{
		if (emitter == null)
		{
			throw new ArgumentNullException("nextEmitter");
		}
		this.nextEmitter = emitter;
	}

	public virtual void Emit(AliasEventInfo eventInfo)
	{
		nextEmitter.Emit(eventInfo);
	}

	public virtual void Emit(ScalarEventInfo eventInfo)
	{
		nextEmitter.Emit(eventInfo);
	}

	public virtual void Emit(MappingStartEventInfo eventInfo)
	{
		nextEmitter.Emit(eventInfo);
	}

	public virtual void Emit(MappingEndEventInfo eventInfo)
	{
		nextEmitter.Emit(eventInfo);
	}

	public virtual void Emit(SequenceStartEventInfo eventInfo)
	{
		nextEmitter.Emit(eventInfo);
	}

	public virtual void Emit(SequenceEndEventInfo eventInfo)
	{
		nextEmitter.Emit(eventInfo);
	}
}
