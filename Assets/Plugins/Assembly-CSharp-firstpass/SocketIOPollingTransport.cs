using System;
using System.Collections.Generic;
using System.Diagnostics;

internal sealed class SocketIOPollingTransport : ITransport
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private SocketIOTransportState state;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private SocketManager manager;

	private HTTPRequest lastRequest;

	private HTTPRequest pollRequest;

	private Packet packetWithAttachment;

	public SocketIOTransportState CurrentState
	{
		get
		{
			return GetState();
		}
		private set
		{
			set_State(value);
		}
	}

	public SocketManager TransportManager
	{
		get
		{
			return GetManager();
		}
		private set
		{
			SetManager(value);
		}
	}

	public bool HasPendingRequest
	{
		get
		{
			return GetIsRequestInProgress();
		}
	}

	public SocketIOPollingTransport(SocketManager socketManager)
	{
		SetManager(socketManager);
	}

	public SocketIOTransportState GetState()
	{
		return state;
	}

	private void set_State(SocketIOTransportState value)
	{
		state = value;
	}

	public SocketManager GetManager()
	{
		return manager;
	}

	private void SetManager(SocketManager value)
	{
		manager = value;
	}

	public bool GetIsRequestInProgress()
	{
		return lastRequest != null;
	}

	public void OpenTransport()
	{
		object[] obj = new object[6]
		{
			GetManager().GetUri().ToString(),
			4,
			GetManager().GetTimestamp().ToString(),
			null,
			null,
			null
		};
		SocketManager manager = GetManager();
		ulong num;
		manager.set_RequestCounter((num = manager.GetRequestCounter()) + 1);
		num = num;
		obj[3] = num.ToString();
		obj[4] = GetManager().GetHandshake().GetSid();
		obj[5] = (GetManager().GetOptions().GetQueryParamsOnlyForHandshake() ? string.Empty : GetManager().GetOptions().BuildQueryParams());
		HTTPRequest request = new HTTPRequest(new Uri(string.Format("{0}?EIO={1}&transport=polling&t={2}-{3}&sid={4}{5}&b64=true", obj)), OnRequestFinished);
		request.SetDisableCache(true);
		request.SetDisableRetry(true);
		request.Send();
		set_State(SocketIOTransportState.Opening);
	}

	public void Close()
	{
		if (GetState() != SocketIOTransportState.Closed)
		{
			set_State(SocketIOTransportState.Closed);
		}
	}

	public void Send(Packet packet)
	{
		Send(new List<Packet> { packet });
	}

	public void Send(List<Packet> packets)
	{
		if (GetState() != SocketIOTransportState.Open)
		{
			throw new Exception("Transport is not in Open state!");
		}
		if (GetIsRequestInProgress())
		{
			throw new Exception("Sending packets are still in progress!");
		}
		byte[] array = null;
		try
		{
			array = packets[0].EncodeBinary();
			for (int i = 1; i < packets.Count; i++)
			{
				byte[] array2 = packets[i].EncodeBinary();
				Array.Resize(ref array, array.Length + array2.Length);
				Array.Copy(array2, 0, array, array.Length - array2.Length, array2.Length);
			}
			packets.Clear();
		}
		catch (Exception ex)
		{
			((IManager)GetManager()).EmitError(SocketIOErrors.Internal, ex.Message + " " + ex.StackTrace);
			return;
		}
		object[] obj = new object[6]
		{
			GetManager().GetUri().ToString(),
			4,
			GetManager().GetTimestamp().ToString(),
			null,
			null,
			null
		};
		SocketManager manager = GetManager();
		ulong num;
		manager.set_RequestCounter((num = manager.GetRequestCounter()) + 1);
		num = num;
		obj[3] = num.ToString();
		obj[4] = GetManager().GetHandshake().GetSid();
		obj[5] = (GetManager().GetOptions().GetQueryParamsOnlyForHandshake() ? string.Empty : GetManager().GetOptions().BuildQueryParams());
		lastRequest = new HTTPRequest(new Uri(string.Format("{0}?EIO={1}&transport=polling&t={2}-{3}&sid={4}{5}&b64=true", obj)), HTTPMethods.Post, OnRequestFinished);
		lastRequest.SetDisableCache(true);
		lastRequest.SetHeader("Content-Type", "application/octet-stream");
		lastRequest.set_RawData(array);
		lastRequest.Send();
	}

	private void OnRequestFinished(HTTPRequest request, HTTPResponse response)
	{
		lastRequest = null;
		if (GetState() == SocketIOTransportState.Closed)
		{
			return;
		}
		string text = null;
		switch (request.GetState())
		{
		case HTTPRequestStates.Finished:
			if (HTTPManager.GetLogger().GetLevel() <= Loglevels.All)
			{
				HTTPManager.GetLogger().Verbose("PollingTransport", "OnRequestFinished: " + response.GetDataAsText());
			}
			if (response.GetIsSuccess())
			{
				ParseResponse(response);
				break;
			}
			text = string.Format("Polling - Request finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2} Uri: {3}", response.GetStatusCode(), response.GetMessage(), response.GetDataAsText(), request.GetCurrentUri());
			break;
		case HTTPRequestStates.Error:
			text = ((request.GetException() == null) ? "No Exception" : (request.GetException().Message + "\n" + request.GetException().StackTrace));
			break;
		case HTTPRequestStates.Aborted:
			text = string.Format("Polling - Request({0}) Aborted!", request.GetCurrentUri());
			break;
		case HTTPRequestStates.ConnectionTimedOut:
			text = string.Format("Polling - Connection Timed Out! Uri: {0}", request.GetCurrentUri());
			break;
		case HTTPRequestStates.TimedOut:
			text = string.Format("Polling - Processing the request({0}) Timed Out!", request.GetCurrentUri());
			break;
		}
		if (!string.IsNullOrEmpty(text))
		{
			((IManager)GetManager()).OnTransportError((ITransport)this, text);
		}
	}

	public void Poll()
	{
		if (pollRequest == null && GetState() != SocketIOTransportState.Paused)
		{
			object[] obj = new object[6]
			{
				GetManager().GetUri().ToString(),
				4,
				GetManager().GetTimestamp().ToString(),
				null,
				null,
				null
			};
			SocketManager manager = GetManager();
			ulong num;
			manager.set_RequestCounter((num = manager.GetRequestCounter()) + 1);
			num = num;
			obj[3] = num.ToString();
			obj[4] = GetManager().GetHandshake().GetSid();
			obj[5] = (GetManager().GetOptions().GetQueryParamsOnlyForHandshake() ? string.Empty : GetManager().GetOptions().BuildQueryParams());
			pollRequest = new HTTPRequest(new Uri(string.Format("{0}?EIO={1}&transport=polling&t={2}-{3}&sid={4}{5}&b64=true", obj)), HTTPMethods.Get, OnPollRequestFinished);
			pollRequest.SetDisableCache(true);
			pollRequest.SetDisableRetry(true);
			pollRequest.Send();
		}
	}

	private void OnPollRequestFinished(HTTPRequest request, HTTPResponse response)
	{
		pollRequest = null;
		if (GetState() == SocketIOTransportState.Closed)
		{
			return;
		}
		string text = null;
		switch (request.GetState())
		{
		case HTTPRequestStates.Finished:
			if (HTTPManager.GetLogger().GetLevel() <= Loglevels.All)
			{
				HTTPManager.GetLogger().Verbose("PollingTransport", "OnPollRequestFinished: " + response.GetDataAsText());
			}
			if (response.GetIsSuccess())
			{
				ParseResponse(response);
				break;
			}
			text = string.Format("Polling - Request finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2} Uri: {3}", response.GetStatusCode(), response.GetMessage(), response.GetDataAsText(), request.GetCurrentUri());
			break;
		case HTTPRequestStates.Error:
			text = ((request.GetException() == null) ? "No Exception" : (request.GetException().Message + "\n" + request.GetException().StackTrace));
			break;
		case HTTPRequestStates.Aborted:
			text = string.Format("Polling - Request({0}) Aborted!", request.GetCurrentUri());
			break;
		case HTTPRequestStates.ConnectionTimedOut:
			text = string.Format("Polling - Connection Timed Out! Uri: {0}", request.GetCurrentUri());
			break;
		case HTTPRequestStates.TimedOut:
			text = string.Format("Polling - Processing the request({0}) Timed Out!", request.GetCurrentUri());
			break;
		}
		if (!string.IsNullOrEmpty(text))
		{
			((IManager)GetManager()).OnTransportError((ITransport)this, text);
		}
	}

	private void OnPacket(Packet packet)
	{
		if (packet.GetAttachmentCount() != 0 && !packet.GetHasAllAttachment())
		{
			packetWithAttachment = packet;
			return;
		}
		TransportEventTypes transportEvent = packet.GetTransportEvent();
		if (transportEvent == TransportEventTypes.Message && packet.GetSocketIOEvent() == SocketIOEventType.Connect && GetState() == SocketIOTransportState.Opening)
		{
			set_State(SocketIOTransportState.Open);
			if (!((IManager)GetManager()).OnTransportConnected((ITransport)this))
			{
				return;
			}
		}
		((IManager)GetManager()).OnPacket(packet);
	}

	private void ParseResponse(HTTPResponse response)
	{
		try
		{
			if (response == null || response.GetData() == null || response.GetData().Length < 1)
			{
				return;
			}
			string text = response.GetDataAsText();
			if (text == "ok")
			{
				return;
			}
			int num = text.IndexOf(':', 0);
			int num2 = 0;
			while (num >= 0 && num < text.Length)
			{
				int num3 = int.Parse(text.Substring(num2, num - num2));
				string text2 = text.Substring(++num, num3);
				if (text2.Length > 2 && text2[0] == 'b' && text2[1] == '4')
				{
					byte[] attachmentData = Convert.FromBase64String(text2.Substring(2));
					if (packetWithAttachment != null)
					{
						packetWithAttachment.AddAttachmentFromServer(attachmentData, true);
						if (packetWithAttachment.GetHasAllAttachment())
						{
							try
							{
								OnPacket(packetWithAttachment);
							}
							catch (Exception ex)
							{
								HTTPManager.GetLogger().Exception("PollingTransport", "ParseResponse - OnPacket with attachment", ex);
								((IManager)GetManager()).EmitError(SocketIOErrors.Internal, ex.Message + " " + ex.StackTrace);
							}
							finally
							{
								packetWithAttachment = null;
							}
						}
					}
				}
				else
				{
					try
					{
						Packet packet = new Packet(text2);
						OnPacket(packet);
					}
					catch (Exception ex2)
					{
						HTTPManager.GetLogger().Exception("PollingTransport", "ParseResponse - OnPacket", ex2);
						((IManager)GetManager()).EmitError(SocketIOErrors.Internal, ex2.Message + " " + ex2.StackTrace);
					}
				}
				num2 = num + num3;
				num = text.IndexOf(':', num2);
			}
		}
		catch (Exception ex3)
		{
			((IManager)GetManager()).EmitError(SocketIOErrors.Internal, ex3.Message + " " + ex3.StackTrace);
			HTTPManager.GetLogger().Exception("PollingTransport", "ParseResponse", ex3);
		}
	}
}
