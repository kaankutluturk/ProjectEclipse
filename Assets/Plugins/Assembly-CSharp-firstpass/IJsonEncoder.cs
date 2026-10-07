using System.Collections.Generic;

public interface IJsonEncoder
{
	string Encode(object obj);

	IDictionary<string, object> DecodeMessage(string json);
}
