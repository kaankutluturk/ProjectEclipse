using System.Collections.Generic;

public sealed class SocketIOLitJsonEncoder : ISocketJsonEncoder
{
	public List<object> Decode(string json)
	{
		JsonReader reader = new JsonReader(json);
		return JsonMapper.ToObject<List<object>>(reader);
	}

	public string Encode(List<object> arguments)
	{
		JsonWriter writer = new JsonWriter();
		JsonMapper.ToJson(arguments, writer);
		return writer.ToString();
	}
}
