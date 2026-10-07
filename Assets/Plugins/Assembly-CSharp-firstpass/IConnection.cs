using System;

public interface IConnection
{
	NegotiationData NegotiationResult { get; }

	IJsonEncoder JsonEncoder { get; set; }

	NegotiationData GetNegotiationResult();

	IJsonEncoder GetJsonEncoder();

	void SetJsonEncoder(IJsonEncoder value);

	void OnMessage(IServerMessage CKEHOEGLMBM);

	void TransportStarted();

	void TransportReconnected();

	void TransportAborted();

	void Error(string NEPOLDCKNJL);

	Uri BuildUri(SignalRRequestType LFLGCDNKNJI);

	Uri BuildUri(SignalRRequestType LFLGCDNKNJI, TransportBase CHMELBKHOPP);

	HTTPRequest PrepareRequest(HTTPRequest CGOIOKHEGOE, SignalRRequestType LFLGCDNKNJI);

	string ParseResponse(string GHCCHADLAEK);
}
