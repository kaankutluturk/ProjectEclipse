using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

public sealed class WebSocketResponse : HTTPResponse, IHeartbeat, IProtocol
{
	public Action<WebSocketResponse, string> OnText;

	public Action<WebSocketResponse, byte[]> OnBinary;

	public Action<WebSocketResponse, WebSocketFrameReader> OnIncompleteFrame;

	public Action<WebSocketResponse, ushort, string> OnClosed;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private TimeSpan pingFrequency;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private ushort maxFragmentSize;

	private List<WebSocketFrameReader> incompleteFrames = new List<WebSocketFrameReader>();

	private List<WebSocketFrameReader> completedFrames = new List<WebSocketFrameReader>();

	private WebSocketFrameReader closeFrame;

	private System.Threading.Thread ReceiverThread;

	private object FrameLock = new object();

	private object sendLock = new object();

	private bool closeSent;

	private bool closed;

	private DateTime lastPing = DateTime.MinValue;

	public bool IsConnectionClosed
	{
		get
		{
			return GetIsClosed();
		}
	}

	public TimeSpan PingInterval
	{
		get
		{
			return GetPingFrequency();
		}
		private set
		{
			set_PingFrequnecy(value);
		}
	}

	public ushort MaxFragmentLength
	{
		get
		{
			return GetMaxFragmentSize();
		}
		private set
		{
			set_MaxFragmentSize(value);
		}
	}

	internal WebSocketResponse(HTTPRequest ONOCIELLAPL, Stream ABJIEFMMIEK, bool IBIIADCLKCH, bool PEAJIKCANHP)
		: base(ONOCIELLAPL, ABJIEFMMIEK, IBIIADCLKCH, PEAJIKCANHP)
	{
		SetIsClosedManually(true);
		closed = false;
		set_MaxFragmentSize(32767);
	}

	public bool GetIsClosed()
	{
		return closed;
	}

	public TimeSpan GetPingFrequency()
	{
		return pingFrequency;
	}

	private void set_PingFrequnecy(TimeSpan value)
	{
		pingFrequency = value;
	}

	public ushort GetMaxFragmentSize()
	{
		return maxFragmentSize;
	}

	private void set_MaxFragmentSize(ushort value)
	{
		maxFragmentSize = value;
	}

	internal void StartReceive()
	{
		if (GetIsUpgraded())
		{
			ReceiverThread = new System.Threading.Thread(ReceiveThreadFunc);
			ReceiverThread.Name = "WebSocket Receiver Thread";
			ReceiverThread.IsBackground = true;
			ReceiverThread.Start();
		}
	}

	public void Send(string LIOGIBJBHAH)
	{
		if (LIOGIBJBHAH == null)
		{
			throw new ArgumentNullException("message must not be null!");
		}
		Send(new WebSocketTextFrame(LIOGIBJBHAH));
	}

	public void Send(byte[] data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data must not be null!");
		}
		if ((long)data.Length > (long)(int)GetMaxFragmentSize())
		{
			lock (sendLock)
			{
				Send(new WebSocketBinaryFrame(data, 0uL, GetMaxFragmentSize(), false));
				ulong num2;
				for (ulong num = GetMaxFragmentSize(); num < (ulong)data.Length; num += num2)
				{
					num2 = Math.Min(GetMaxFragmentSize(), (ulong)data.Length - num);
					Send(new WebSocketContinuationFrame(data, num, num2, num + num2 >= (ulong)data.Length));
				}
				return;
			}
		}
		Send(new WebSocketBinaryFrame(data));
	}

	public void Send(byte[] data, ulong IPCOBJBKNAO, ulong count)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data must not be null!");
		}
		if (IPCOBJBKNAO + count > (ulong)data.Length)
		{
			throw new ArgumentOutOfRangeException("offset + count >= data.Length");
		}
		if ((long)count > (long)(int)GetMaxFragmentSize())
		{
			lock (sendLock)
			{
				Send(new WebSocketBinaryFrame(data, IPCOBJBKNAO, GetMaxFragmentSize(), false));
				ulong num2;
				for (ulong num = IPCOBJBKNAO + GetMaxFragmentSize(); num < count; num += num2)
				{
					num2 = Math.Min(GetMaxFragmentSize(), count - num);
					Send(new WebSocketContinuationFrame(data, num, num2, num + num2 >= count));
				}
				return;
			}
		}
		Send(new WebSocketBinaryFrame(data, IPCOBJBKNAO, count, true));
	}

	public void Send(IWebSocketFrameWriter frame)
	{
		if (frame == null)
		{
			throw new ArgumentNullException("frame is null!");
		}
		if (!closed)
		{
			byte[] array = frame.Get();
			lock (sendLock)
			{
				Stream.Write(array, 0, array.Length);
				Stream.Flush();
			}
			if (frame.get_Type() == WebSocketFrameTypes.ConnectionClose)
			{
				closeSent = true;
			}
		}
	}

	public void Close()
	{
		Close(1000, "Bye!");
	}

	public void Close(ushort KJPGKHJNOMC, string CKEHOEGLMBM)
	{
		if (!closed)
		{
			Send(new WebSocketClose(KJPGKHJNOMC, CKEHOEGLMBM));
		}
	}

	public void StartPinging(int ONDDDDCAPFG)
	{
		if (ONDDDDCAPFG < 100)
		{
			throw new ArgumentException("frequency must be at least 100 millisec!");
		}
		set_PingFrequnecy(TimeSpan.FromMilliseconds(ONDDDDCAPFG));
		HTTPManager.GetHeartbeats().Subscribe(this);
	}

	private void ReceiveThreadFunc()
	{
		try
		{
			while (!closed)
			{
				try
				{
					WebSocketFrameReader hENOIJFGGOF = new WebSocketFrameReader();
					hENOIJFGGOF.Read(Stream);
					if (hENOIJFGGOF.GetHasMask())
					{
						Close(1002, "Protocol Error: masked frame received from server!");
						continue;
					}
					if (!hENOIJFGGOF.GetIsFinal())
					{
						if (OnIncompleteFrame == null)
						{
							incompleteFrames.Add(hENOIJFGGOF);
							continue;
						}
						lock (FrameLock)
						{
							completedFrames.Add(hENOIJFGGOF);
						}
						continue;
					}
					switch (hENOIJFGGOF.get_Type())
					{
					case WebSocketFrameTypes.Continuation:
						if (OnIncompleteFrame == null)
						{
							hENOIJFGGOF.Assemble(incompleteFrames);
							incompleteFrames.Clear();
							goto case WebSocketFrameTypes.Text;
						}
						lock (FrameLock)
						{
							completedFrames.Add(hENOIJFGGOF);
						}
						break;
					case WebSocketFrameTypes.Text:
					case WebSocketFrameTypes.Binary:
						lock (FrameLock)
						{
							completedFrames.Add(hENOIJFGGOF);
						}
						break;
					case WebSocketFrameTypes.Ping:
						if (!closeSent && !closed)
						{
							Send(new WebSocketPong(hENOIJFGGOF));
						}
						break;
					case WebSocketFrameTypes.ConnectionClose:
						closeFrame = hENOIJFGGOF;
						if (!closeSent)
						{
							Send(new WebSocketClose());
						}
						closed = closeSent;
						break;
					}
				}
				catch (ThreadAbortException)
				{
					incompleteFrames.Clear();
					BaseRequest.set_State(HTTPRequestStates.Aborted);
					closed = true;
				}
				catch (Exception bAINMLLIKOL)
				{
					BaseRequest.set_Exception(bAINMLLIKOL);
					BaseRequest.set_State(HTTPRequestStates.Error);
					closed = true;
				}
			}
		}
		finally
		{
			HTTPManager.GetHeartbeats().Unsubscribe(this);
		}
	}

	void IProtocol.HandleEvents()
	{
		lock (FrameLock)
		{
			for (int i = 0; i < completedFrames.Count; i++)
			{
				WebSocketFrameReader hENOIJFGGOF = completedFrames[i];
				try
				{
					WebSocketFrameTypes bECKAHJIEGE = hENOIJFGGOF.get_Type();
					if (bECKAHJIEGE == WebSocketFrameTypes.Continuation)
					{
						goto IL_0041;
					}
					if (bECKAHJIEGE != WebSocketFrameTypes.Text)
					{
						if (bECKAHJIEGE == WebSocketFrameTypes.Binary)
						{
							if (!hENOIJFGGOF.GetIsFinal())
							{
								goto IL_0041;
							}
							if (OnBinary != null)
							{
								OnBinary(this, hENOIJFGGOF.GetData());
							}
						}
					}
					else
					{
						if (!hENOIJFGGOF.GetIsFinal())
						{
							goto IL_0041;
						}
						if (OnText != null)
						{
							OnText(this, Encoding.UTF8.GetString(hENOIJFGGOF.GetData(), 0, hENOIJFGGOF.GetData().Length));
						}
					}
					goto end_IL_0021;
					IL_0041:
					if (OnIncompleteFrame != null)
					{
						OnIncompleteFrame(this, hENOIJFGGOF);
					}
					end_IL_0021:;
				}
				catch (Exception mPFFFAOGBJE)
				{
					HTTPManager.GetLogger().Exception("WebSocketResponse", "HandleEvents", mPFFFAOGBJE);
				}
			}
			completedFrames.Clear();
		}
		if (!GetIsClosed() || OnClosed == null || BaseRequest.GetState() != HTTPRequestStates.Processing)
		{
			return;
		}
		try
		{
			ushort arg = 0;
			string arg2 = string.Empty;
			if (closeFrame != null && closeFrame.GetData() != null && closeFrame.GetData().Length >= 2)
			{
				if (BitConverter.IsLittleEndian)
				{
					Array.Reverse(closeFrame.GetData(), 0, 2);
				}
				arg = BitConverter.ToUInt16(closeFrame.GetData(), 0);
				if (closeFrame.GetData().Length > 2)
				{
					arg2 = Encoding.UTF8.GetString(closeFrame.GetData(), 2, closeFrame.GetData().Length - 2);
				}
			}
			OnClosed(this, arg, arg2);
		}
		catch (Exception mPFFFAOGBJE2)
		{
			HTTPManager.GetLogger().Exception("WebSocketResponse", "HandleEvents - OnClosed", mPFFFAOGBJE2);
		}
	}

	void IHeartbeat.OnHeartbeatUpdate(TimeSpan OJOKANCMPLG)
	{
		if (lastPing == DateTime.MinValue)
		{
			lastPing = DateTime.UtcNow;
		}
		else if (DateTime.UtcNow - lastPing >= GetPingFrequency())
		{
			Send(new WebSocketPing(string.Empty));
			lastPing = DateTime.UtcNow;
		}
	}
}
