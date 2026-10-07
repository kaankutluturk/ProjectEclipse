using System.Text;

public sealed class WebSocketTextFrame : WebSocketBinaryFrame
{
	public WebSocketTextFrame(string text)
		: base(Encoding.UTF8.GetBytes(text))
	{
	}

	public override WebSocketFrameTypes get_Type()
	{
		return WebSocketFrameTypes.Text;
	}
}
