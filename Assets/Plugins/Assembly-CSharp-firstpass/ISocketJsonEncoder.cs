using System.Collections.Generic;

public interface ISocketJsonEncoder
{
	List<object> Decode(string json);

	string Encode(List<object> data);
}
