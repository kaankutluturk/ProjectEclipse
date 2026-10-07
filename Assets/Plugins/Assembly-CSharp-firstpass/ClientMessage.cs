public struct ClientMessage
{
	public readonly Hub OwnerHub;

	public readonly string Method;

	public readonly object[] Args;

	public readonly ulong CallIdx;

	public readonly OnMethodResultDelegate ResultCallback;

	public readonly OnMethodFailedDelegate ResultErrorCallback;

	public readonly OnMethodProgressDelegate ProgressCallback;

	public ClientMessage(Hub hub, string method, object[] args, ulong callId, OnMethodResultDelegate resultCallback, OnMethodFailedDelegate errorCallback, OnMethodProgressDelegate progressCallback)
	{
		OwnerHub = hub;
		Method = method;
		Args = args;
		CallIdx = callId;
		ResultCallback = resultCallback;
		ResultErrorCallback = errorCallback;
		ProgressCallback = progressCallback;
	}
}
