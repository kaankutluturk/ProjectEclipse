using System.Text;

public sealed class WebSocketPing : WebSocketBinaryFrame
{
	public WebSocketPing(string CKEHOEGLMBM)
		: base(Encoding.UTF8.GetBytes(CKEHOEGLMBM))
	{
	}

	public override WebSocketFrameTypes get_Type()
	{
		return WebSocketFrameTypes.Ping;
	}
}
