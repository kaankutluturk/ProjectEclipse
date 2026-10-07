using System;
using System.Collections.Generic;
using System.Diagnostics;

internal sealed class EventDescriptor
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private List<SocketIOCallback> callbacks;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool onlyOnce;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool autoDecodePayload;

	private SocketIOCallback[] callbackArray;

	public List<SocketIOCallback> Callbacks
	{
		get
		{
			return GetCallbacks();
		}
		private set
		{
			SetCallbacks(value);
		}
	}

	public bool OnlyOnce
	{
		get
		{
			return GetOnlyOnce();
		}
		private set
		{
			SetOnlyOnce(value);
		}
	}

	public bool AutoDecodePayload
	{
		get
		{
			return GetAutoDecodePayload();
		}
		private set
		{
			SetAutoDecodePayload(value);
		}
	}

	public EventDescriptor(bool onlyOnce, bool autoDecodePayload, SocketIOCallback callback)
	{
		SetOnlyOnce(onlyOnce);
		SetAutoDecodePayload(autoDecodePayload);
		SetCallbacks(new List<SocketIOCallback>(1));
		if (callback != null)
		{
			GetCallbacks().Add(callback);
		}
	}

	public List<SocketIOCallback> GetCallbacks()
	{
		return callbacks;
	}

	private void SetCallbacks(List<SocketIOCallback> value)
	{
		callbacks = value;
	}

	public bool GetOnlyOnce()
	{
		return onlyOnce;
	}

	private void SetOnlyOnce(bool value)
	{
		onlyOnce = value;
	}

	public bool GetAutoDecodePayload()
	{
		return autoDecodePayload;
	}

	private void SetAutoDecodePayload(bool value)
	{
		autoDecodePayload = value;
	}

	public void Call(Socket socket, Packet packet, params object[] args)
	{
		if (callbackArray == null || callbackArray.Length < GetCallbacks().Count)
		{
			Array.Resize(ref callbackArray, GetCallbacks().Count);
		}
		GetCallbacks().CopyTo(callbackArray);
		for (int i = 0; i < callbackArray.Length; i++)
		{
			try
			{
				callbackArray[i](socket, packet, args);
			}
			catch (Exception ex)
			{
				((ISocket)socket).EmitError(SocketIOErrors.User, ex.Message + " " + ex.StackTrace);
				HTTPManager.GetLogger().Exception("EventDescriptor", "Call", ex);
			}
			if (GetOnlyOnce())
			{
				GetCallbacks().Remove(callbackArray[i]);
			}
			callbackArray[i] = null;
		}
	}
}
