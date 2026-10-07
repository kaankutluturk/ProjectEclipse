public interface IWebSocketFrameWriter
{
	WebSocketFrameTypes get_Type();

	byte[] Get();
}
