using System.Collections.Generic;

public abstract class PostSendTransportBase : TransportBase
{
	protected List<HTTPRequest> sendRequestQueue = new List<HTTPRequest>();

	public PostSendTransportBase(string name, Connection EPDOEDFFPFD)
		: base(name, EPDOEDFFPFD)
	{
	}

	protected override void SendImpl(string EMDHMHOKGFP)
	{
		HTTPRequest iPLGNIDJDCF = new HTTPRequest(GetConnection().BuildUri(SignalRRequestType.Send, this), HTTPMethods.Post, true, true, OnSendRequestFinished);
		iPLGNIDJDCF.SetFormUsage(HTTPFormUsage.UrlEncoded);
		iPLGNIDJDCF.AddField("data", EMDHMHOKGFP);
		GetConnection().PrepareRequest(iPLGNIDJDCF, SignalRRequestType.Send);
		iPLGNIDJDCF.SetPriority(-1);
		iPLGNIDJDCF.Send();
		sendRequestQueue.Add(iPLGNIDJDCF);
	}

	private void OnSendRequestFinished(HTTPRequest CGOIOKHEGOE, HTTPResponse BEIGFGCBICO)
	{
		sendRequestQueue.Remove(CGOIOKHEGOE);
		string text = string.Empty;
		switch (CGOIOKHEGOE.GetState())
		{
		case HTTPRequestStates.Finished:
			if (BEIGFGCBICO.GetIsSuccess())
			{
				HTTPManager.GetLogger().Information("Transport - " + get_Name(), "Send - Request Finished Successfully! " + BEIGFGCBICO.GetDataAsText());
				if (!string.IsNullOrEmpty(BEIGFGCBICO.GetDataAsText()))
				{
					IServerMessage bNGPAAAKBOP = TransportBase.Parse(GetConnection().GetJsonEncoder(), BEIGFGCBICO.GetDataAsText());
					if (bNGPAAAKBOP != null)
					{
						GetConnection().OnMessage(bNGPAAAKBOP);
					}
				}
			}
			else
			{
				text = string.Format("Send - Request Finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2}", BEIGFGCBICO.GetStatusCode(), BEIGFGCBICO.GetMessage(), BEIGFGCBICO.GetDataAsText());
			}
			break;
		case HTTPRequestStates.Error:
			text = "Send - Request Finished with Error! " + ((CGOIOKHEGOE.GetException() == null) ? "No Exception" : (CGOIOKHEGOE.GetException().Message + "\n" + CGOIOKHEGOE.GetException().StackTrace));
			break;
		case HTTPRequestStates.Aborted:
			text = "Send - Request Aborted!";
			break;
		case HTTPRequestStates.ConnectionTimedOut:
			text = "Send - Connection Timed Out!";
			break;
		case HTTPRequestStates.TimedOut:
			text = "Send - Processing the request Timed Out!";
			break;
		}
		if (!string.IsNullOrEmpty(text))
		{
			GetConnection().Error(text);
		}
	}
}
