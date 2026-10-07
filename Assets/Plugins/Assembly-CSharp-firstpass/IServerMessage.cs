public interface IServerMessage
{
	MessageTypes get_Type();

	void Parse(object data);
}
