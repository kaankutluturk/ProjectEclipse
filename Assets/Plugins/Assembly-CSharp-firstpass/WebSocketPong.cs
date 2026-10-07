public sealed class WebSocketPong : WebSocketBinaryFrame
{
	public WebSocketPong(WebSocketFrameReader CEHDEHABCNL)
		: base(CEHDEHABCNL.GetData())
	{
	}

	public override WebSocketFrameTypes get_Type()
	{
		return WebSocketFrameTypes.Pong;
	}
}
