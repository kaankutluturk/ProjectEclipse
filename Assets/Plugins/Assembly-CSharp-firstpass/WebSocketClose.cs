using System;
using System.IO;
using System.Text;

public sealed class WebSocketClose : WebSocketBinaryFrame
{
	public WebSocketClose()
		: base(null)
	{
	}

	public WebSocketClose(ushort code, string message)
		: base(GetCloseData(code, message))
	{
	}

	public override WebSocketFrameTypes get_Type()
	{
		return WebSocketFrameTypes.ConnectionClose;
	}

	private static byte[] GetCloseData(ushort code, string message)
	{
		int byteCount = Encoding.UTF8.GetByteCount(message);
		using (MemoryStream memoryStream = new MemoryStream(2 + byteCount))
		{
			byte[] bytes = BitConverter.GetBytes(code);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(bytes, 0, bytes.Length);
			}
			memoryStream.Write(bytes, 0, bytes.Length);
			bytes = Encoding.UTF8.GetBytes(message);
			memoryStream.Write(bytes, 0, bytes.Length);
			return memoryStream.ToArray();
		}
	}
}
