using System.Collections.Generic;

public sealed class SocketIODefaultJsonEncoder : ISocketJsonEncoder
{
	public List<object> Decode(string json)
	{
		return Json.Decode(json) as List<object>;
	}

	public string Encode(List<object> arguments)
	{
		return Json.Encode(arguments);
	}
}
