using System;

public interface IConnection
{
	NegotiationData NegotiationResult { get; }

	IJsonEncoder JsonEncoder { get; set; }

	NegotiationData GetNegotiationResult();

	IJsonEncoder GetJsonEncoder();

	void SetJsonEncoder(IJsonEncoder value);

	void OnMessage(IServerMessage message);

	void TransportStarted();

	void TransportReconnected();

	void TransportAborted();

	void Error(string error);

	Uri BuildUri(SignalRRequestType requestType);

	Uri BuildUri(SignalRRequestType requestType, TransportBase transport);

	HTTPRequest PrepareRequest(HTTPRequest request, SignalRRequestType requestType);

	string ParseResponse(string response);
}
