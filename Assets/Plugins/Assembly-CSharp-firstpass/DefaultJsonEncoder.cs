using System.Collections.Generic;

public sealed class DefaultJsonEncoder : IJsonEncoder
{
	public string Encode(object obj)
	{
		return Json.Encode(obj);
	}

	public IDictionary<string, object> DecodeMessage(string message)
	{
		bool success = false;
		IDictionary<string, object> dictionary = Json.Decode(message, ref success) as IDictionary<string, object>;
		return (!success) ? null : dictionary;
	}
}
