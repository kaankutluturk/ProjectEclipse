using System.Collections.Generic;

public sealed class SocketIODefaultJsonEncoder : ISocketJsonEncoder
{
	public List<object> Decode(string EMDHMHOKGFP)
	{
		return Json.Decode(EMDHMHOKGFP) as List<object>;
	}

	public string Encode(List<object> AOMLCBHAJJH)
	{
		return Json.Encode(AOMLCBHAJJH);
	}
}
