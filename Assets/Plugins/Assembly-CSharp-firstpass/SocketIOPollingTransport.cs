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

	public SocketIOPollingTransport(SocketManager BJGMPDIKEJC)
	{
		SetManager(BJGMPDIKEJC);
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
		SocketManager mFANOMMMCFG = GetManager();
		ulong num;
		mFANOMMMCFG.set_RequestCounter((num = mFANOMMMCFG.GetRequestCounter()) + 1);
		num = num;
		obj[3] = num.ToString();
		obj[4] = GetManager().GetHandshake().GetSid();
		obj[5] = (GetManager().GetOptions().GetQueryParamsOnlyForHandshake() ? string.Empty : GetManager().GetOptions().BuildQueryParams());
		HTTPRequest iPLGNIDJDCF = new HTTPRequest(new Uri(string.Format("{0}?EIO={1}&transport=polling&t={2}-{3}&sid={4}{5}&b64=true", obj)), OnRequestFinished);
		iPLGNIDJDCF.SetDisableCache(true);
		iPLGNIDJDCF.SetDisableRetry(true);
		iPLGNIDJDCF.Send();
		set_State(SocketIOTransportState.Opening);
	}

	public void Close()
	{
		if (GetState() != SocketIOTransportState.Closed)
		{
			set_State(SocketIOTransportState.Closed);
		}
	}

	public void Send(Packet NPKADBPBKIG)
	{
		Send(new List<Packet> { NPKADBPBKIG });
	}

	public void Send(List<Packet> DPGGBKDLDJE)
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
			array = DPGGBKDLDJE[0].EncodeBinary();
			for (int i = 1; i < DPGGBKDLDJE.Count; i++)
			{
				byte[] array2 = DPGGBKDLDJE[i].EncodeBinary();
				Array.Resize(ref array, array.Length + array2.Length);
				Array.Copy(array2, 0, array, array.Length - array2.Length, array2.Length);
			}
			DPGGBKDLDJE.Clear();
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
		SocketManager mFANOMMMCFG = GetManager();
		ulong num;
		mFANOMMMCFG.set_RequestCounter((num = mFANOMMMCFG.GetRequestCounter()) + 1);
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

	private void OnRequestFinished(HTTPRequest CGOIOKHEGOE, HTTPResponse BEIGFGCBICO)
	{
		lastRequest = null;
		if (GetState() == SocketIOTransportState.Closed)
		{
			return;
		}
		string text = null;
		switch (CGOIOKHEGOE.GetState())
		{
		case HTTPRequestStates.Finished:
			if (HTTPManager.GetLogger().GetLevel() <= Loglevels.All)
			{
				HTTPManager.GetLogger().Verbose("PollingTransport", "OnRequestFinished: " + BEIGFGCBICO.GetDataAsText());
			}
			if (BEIGFGCBICO.GetIsSuccess())
			{
				ParseResponse(BEIGFGCBICO);
				break;
			}
			text = string.Format("Polling - Request finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2} Uri: {3}", BEIGFGCBICO.GetStatusCode(), BEIGFGCBICO.GetMessage(), BEIGFGCBICO.GetDataAsText(), CGOIOKHEGOE.GetCurrentUri());
			break;
		case HTTPRequestStates.Error:
			text = ((CGOIOKHEGOE.GetException() == null) ? "No Exception" : (CGOIOKHEGOE.GetException().Message + "\n" + CGOIOKHEGOE.GetException().StackTrace));
			break;
		case HTTPRequestStates.Aborted:
			text = string.Format("Polling - Request({0}) Aborted!", CGOIOKHEGOE.GetCurrentUri());
			break;
		case HTTPRequestStates.ConnectionTimedOut:
			text = string.Format("Polling - Connection Timed Out! Uri: {0}", CGOIOKHEGOE.GetCurrentUri());
			break;
		case HTTPRequestStates.TimedOut:
			text = string.Format("Polling - Processing the request({0}) Timed Out!", CGOIOKHEGOE.GetCurrentUri());
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
			SocketManager mFANOMMMCFG = GetManager();
			ulong num;
			mFANOMMMCFG.set_RequestCounter((num = mFANOMMMCFG.GetRequestCounter()) + 1);
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

	private void OnPollRequestFinished(HTTPRequest CGOIOKHEGOE, HTTPResponse BEIGFGCBICO)
	{
		pollRequest = null;
		if (GetState() == SocketIOTransportState.Closed)
		{
			return;
		}
		string text = null;
		switch (CGOIOKHEGOE.GetState())
		{
		case HTTPRequestStates.Finished:
			if (HTTPManager.GetLogger().GetLevel() <= Loglevels.All)
			{
				HTTPManager.GetLogger().Verbose("PollingTransport", "OnPollRequestFinished: " + BEIGFGCBICO.GetDataAsText());
			}
			if (BEIGFGCBICO.GetIsSuccess())
			{
				ParseResponse(BEIGFGCBICO);
				break;
			}
			text = string.Format("Polling - Request finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2} Uri: {3}", BEIGFGCBICO.GetStatusCode(), BEIGFGCBICO.GetMessage(), BEIGFGCBICO.GetDataAsText(), CGOIOKHEGOE.GetCurrentUri());
			break;
		case HTTPRequestStates.Error:
			text = ((CGOIOKHEGOE.GetException() == null) ? "No Exception" : (CGOIOKHEGOE.GetException().Message + "\n" + CGOIOKHEGOE.GetException().StackTrace));
			break;
		case HTTPRequestStates.Aborted:
			text = string.Format("Polling - Request({0}) Aborted!", CGOIOKHEGOE.GetCurrentUri());
			break;
		case HTTPRequestStates.ConnectionTimedOut:
			text = string.Format("Polling - Connection Timed Out! Uri: {0}", CGOIOKHEGOE.GetCurrentUri());
			break;
		case HTTPRequestStates.TimedOut:
			text = string.Format("Polling - Processing the request({0}) Timed Out!", CGOIOKHEGOE.GetCurrentUri());
			break;
		}
		if (!string.IsNullOrEmpty(text))
		{
			((IManager)GetManager()).OnTransportError((ITransport)this, text);
		}
	}

	private void OnPacket(Packet NPKADBPBKIG)
	{
		if (NPKADBPBKIG.GetAttachmentCount() != 0 && !NPKADBPBKIG.GetHasAllAttachment())
		{
			packetWithAttachment = NPKADBPBKIG;
			return;
		}
		TransportEventTypes hJDLGPHLPNF = NPKADBPBKIG.GetTransportEvent();
		if (hJDLGPHLPNF == TransportEventTypes.Message && NPKADBPBKIG.GetSocketIOEvent() == SocketIOEventType.Connect && GetState() == SocketIOTransportState.Opening)
		{
			set_State(SocketIOTransportState.Open);
			if (!((IManager)GetManager()).OnTransportConnected((ITransport)this))
			{
				return;
			}
		}
		((IManager)GetManager()).OnPacket(NPKADBPBKIG);
	}

	private void ParseResponse(HTTPResponse BEIGFGCBICO)
	{
		try
		{
			if (BEIGFGCBICO == null || BEIGFGCBICO.GetData() == null || BEIGFGCBICO.GetData().Length < 1)
			{
				return;
			}
			string text = BEIGFGCBICO.GetDataAsText();
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
					byte[] jGMLAFOPBBC = Convert.FromBase64String(text2.Substring(2));
					if (packetWithAttachment != null)
					{
						packetWithAttachment.AddAttachmentFromServer(jGMLAFOPBBC, true);
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
						Packet nPKADBPBKIG = new Packet(text2);
						OnPacket(nPKADBPBKIG);
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
