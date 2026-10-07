public sealed class WebSocketPong : WebSocketBinaryFrame
{
	public WebSocketPong(WebSocketFrameReader pingFrame)
		: base(pingFrame.GetData())
	{
	}

	public override WebSocketFrameTypes get_Type()
	{
		return WebSocketFrameTypes.Pong;
	}
}
