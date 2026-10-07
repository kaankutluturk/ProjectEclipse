using System.Text;

public sealed class WebSocketPing : WebSocketBinaryFrame
{
	public WebSocketPing(string text)
		: base(Encoding.UTF8.GetBytes(text))
	{
	}

	public override WebSocketFrameTypes get_Type()
	{
		return WebSocketFrameTypes.Ping;
	}
}
