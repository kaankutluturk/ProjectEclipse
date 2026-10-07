public struct ClientMessage
{
	public readonly Hub OwnerHub;

	public readonly string Method;

	public readonly object[] Args;

	public readonly ulong CallIdx;

	public readonly OnMethodResultDelegate ResultCallback;

	public readonly OnMethodFailedDelegate ResultErrorCallback;

	public readonly OnMethodProgressDelegate ProgressCallback;

	public ClientMessage(Hub CGFIJCNNCKP, string FJLOLCPJACB, object[] LKIOKGCNKHE, ulong KKAADAAPLDC, OnMethodResultDelegate HKHNPNNDHFP, OnMethodFailedDelegate HFFMDOCLOHA, OnMethodProgressDelegate OODDBFJDGJO)
	{
		OwnerHub = CGFIJCNNCKP;
		Method = FJLOLCPJACB;
		Args = LKIOKGCNKHE;
		CallIdx = KKAADAAPLDC;
		ResultCallback = HKHNPNNDHFP;
		ResultErrorCallback = HFFMDOCLOHA;
		ProgressCallback = OODDBFJDGJO;
	}
}
