using System.Collections.Generic;

public abstract class PostSendTransportBase : TransportBase
{
	protected List<HTTPRequest> sendRequestQueue = new List<HTTPRequest>();

	public PostSendTransportBase(string name, Connection connection)
		: base(name, connection)
	{
	}

	protected override void SendImpl(string payload)
	{
		HTTPRequest request = new HTTPRequest(GetConnection().BuildUri(SignalRRequestType.Send, this), HTTPMethods.Post, true, true, OnSendRequestFinished);
		request.SetFormUsage(HTTPFormUsage.UrlEncoded);
		request.AddField("data", payload);
		GetConnection().PrepareRequest(request, SignalRRequestType.Send);
		request.SetPriority(-1);
		request.Send();
		sendRequestQueue.Add(request);
	}

	private void OnSendRequestFinished(HTTPRequest request, HTTPResponse response)
	{
		sendRequestQueue.Remove(request);
		string text = string.Empty;
		switch (request.GetState())
		{
		case HTTPRequestStates.Finished:
			if (response.GetIsSuccess())
			{
				HTTPManager.GetLogger().Information("Transport - " + get_Name(), "Send - Request Finished Successfully! " + response.GetDataAsText());
				if (!string.IsNullOrEmpty(response.GetDataAsText()))
				{
					IServerMessage serverMessage = TransportBase.Parse(GetConnection().GetJsonEncoder(), response.GetDataAsText());
					if (serverMessage != null)
					{
						GetConnection().OnMessage(serverMessage);
					}
				}
			}
			else
			{
				text = string.Format("Send - Request Finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2}", response.GetStatusCode(), response.GetMessage(), response.GetDataAsText());
			}
			break;
		case HTTPRequestStates.Error:
			text = "Send - Request Finished with Error! " + ((request.GetException() == null) ? "No Exception" : (request.GetException().Message + "\n" + request.GetException().StackTrace));
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
