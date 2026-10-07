using System.Text;

public sealed class WebSocketTextFrame : WebSocketBinaryFrame
{
	public WebSocketTextFrame(string HCPNFPMHFCM)
		: base(Encoding.UTF8.GetBytes(HCPNFPMHFCM))
	{
	}

	public override WebSocketFrameTypes get_Type()
	{
		return WebSocketFrameTypes.Text;
	}
}
