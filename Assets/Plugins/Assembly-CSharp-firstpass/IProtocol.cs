public interface IProtocol
{
	bool IsConnectionClosed { get; }

	bool GetIsClosed();

	void HandleEvents();
}
