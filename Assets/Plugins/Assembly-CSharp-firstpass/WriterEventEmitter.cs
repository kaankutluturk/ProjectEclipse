public sealed class WriterEventEmitter : IEventEmitter
{
	private readonly IEmitter emitter;

	public WriterEventEmitter(IEmitter emitter)
	{
		this.emitter = emitter;
	}

	void IEventEmitter.Emit(AliasEventInfo eventInfo)
	{
		emitter.Emit(new AnchorAlias(eventInfo.GetAlias()));
	}

	void IEventEmitter.Emit(ScalarEventInfo eventInfo)
	{
		emitter.Emit(new Scalar(eventInfo.GetAnchor(), eventInfo.GetTag(), eventInfo.GetRenderedValue(), eventInfo.GetStyle(), eventInfo.GetIsPlainImplicit(), eventInfo.GetIsQuotedImplicit()));
	}

	void IEventEmitter.Emit(MappingStartEventInfo eventInfo)
	{
		emitter.Emit(new MappingStart(eventInfo.GetAnchor(), eventInfo.GetTag(), eventInfo.GetIsImplicit(), eventInfo.GetStyle()));
	}

	void IEventEmitter.Emit(MappingEndEventInfo eventInfo)
	{
		emitter.Emit(new MappingEnd());
	}

	void IEventEmitter.Emit(SequenceStartEventInfo eventInfo)
	{
		emitter.Emit(new SequenceStart(eventInfo.GetAnchor(), eventInfo.GetTag(), eventInfo.GetIsImplicit(), eventInfo.GetStyle()));
	}

	void IEventEmitter.Emit(SequenceEndEventInfo eventInfo)
	{
		emitter.Emit(new SequenceEnd());
	}
}
