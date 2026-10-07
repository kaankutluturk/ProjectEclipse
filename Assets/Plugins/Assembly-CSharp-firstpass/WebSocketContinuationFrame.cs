public sealed class WebSocketContinuationFrame : WebSocketBinaryFrame
{
	public WebSocketContinuationFrame(byte[] data, bool isFinal)
		: base(data, 0uL, (ulong)data.Length, isFinal)
	{
	}

	public WebSocketContinuationFrame(byte[] data, ulong LCCLEFMKLPB, ulong length, bool isFinal)
		: base(data, LCCLEFMKLPB, length, isFinal)
	{
	}

	public override WebSocketFrameTypes get_Type()
	{
		return WebSocketFrameTypes.Continuation;
	}
}
