public sealed class WriterEventEmitter : IEventEmitter
{
	private readonly IEmitter emitter;

	public WriterEventEmitter(IEmitter NPIDIMCLNEM)
	{
		this.emitter = NPIDIMCLNEM;
	}

	void IEventEmitter.Emit(AliasEventInfo FNHCFCAALAE)
	{
		emitter.Emit(new AnchorAlias(FNHCFCAALAE.GetAlias()));
	}

	void IEventEmitter.Emit(ScalarEventInfo FNHCFCAALAE)
	{
		emitter.Emit(new Scalar(FNHCFCAALAE.GetAnchor(), FNHCFCAALAE.GetTag(), FNHCFCAALAE.GetRenderedValue(), FNHCFCAALAE.GetStyle(), FNHCFCAALAE.GetIsPlainImplicit(), FNHCFCAALAE.GetIsQuotedImplicit()));
	}

	void IEventEmitter.Emit(MappingStartEventInfo FNHCFCAALAE)
	{
		emitter.Emit(new MappingStart(FNHCFCAALAE.GetAnchor(), FNHCFCAALAE.GetTag(), FNHCFCAALAE.GetIsImplicit(), FNHCFCAALAE.GetStyle()));
	}

	void IEventEmitter.Emit(MappingEndEventInfo FNHCFCAALAE)
	{
		emitter.Emit(new MappingEnd());
	}

	void IEventEmitter.Emit(SequenceStartEventInfo FNHCFCAALAE)
	{
		emitter.Emit(new SequenceStart(FNHCFCAALAE.GetAnchor(), FNHCFCAALAE.GetTag(), FNHCFCAALAE.GetIsImplicit(), FNHCFCAALAE.GetStyle()));
	}

	void IEventEmitter.Emit(SequenceEndEventInfo FNHCFCAALAE)
	{
		emitter.Emit(new SequenceEnd());
	}
}
