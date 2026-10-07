using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

internal sealed class EventSourceResponse : HTTPResponse, IProtocol
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool isClosed;

	public Action<EventSourceResponse, Message> OnMessage;

	public Action<EventSourceResponse> OnClosed;

	private System.Threading.Thread ReceiverThread;

	private object FrameLock = new object();

	private byte[] LineBuffer = new byte[1024];

	private int LineBufferPos;

	private Message currentMessage;

	private List<Message> completedMessages = new List<Message>();

	public bool IsConnectionClosed
	{
		get
		{
			return GetIsClosed();
		}
		private set
		{
			set_IsClosed(value);
		}
	}

	internal EventSourceResponse(HTTPRequest request, Stream stream, bool isStreamed, bool isFromCache)
		: base(request, stream, isStreamed, isFromCache)
	{
		SetIsClosedManually(true);
	}

	public bool GetIsClosed()
	{
		return isClosed;
	}

	private void set_IsClosed(bool value)
	{
		isClosed = value;
	}

	internal override bool Receive(int forceReadRawContentLength = -1, bool readPayloadData = true)
	{
		bool flag = base.Receive(forceReadRawContentLength, false);
		SetIsUpgraded(flag && GetStatusCode() == 200 && HasHeaderWithValue("content-type", "text/event-stream"));
		if (!GetIsUpgraded())
		{
			ReadPayload(forceReadRawContentLength);
		}
		return flag;
	}

	internal void StartReceive()
	{
		if (GetIsUpgraded())
		{
			ReceiverThread = new System.Threading.Thread(ReceiveThreadFunc);
			ReceiverThread.Name = "EventSource Receiver Thread";
			ReceiverThread.IsBackground = true;
			ReceiverThread.Start();
		}
	}

	private void ReceiveThreadFunc()
	{
		try
		{
			if (HasHeaderWithValue("transfer-encoding", "chunked"))
			{
				ReadChunked(Stream);
			}
			else
			{
				ReadRaw(Stream, -1);
			}
		}
		catch (ThreadAbortException)
		{
			BaseRequest.set_State(HTTPRequestStates.Aborted);
		}
		catch (Exception ex)
		{
			BaseRequest.set_Exception(ex);
			BaseRequest.set_State(HTTPRequestStates.Error);
		}
		finally
		{
			set_IsClosed(true);
		}
	}

	private new void ReadChunked(Stream stream)
	{
		int num = ReadChunkLength(stream);
		byte[] array = new byte[num];
		while (num != 0)
		{
			if (array.Length < num)
			{
				Array.Resize(ref array, num);
			}
			int num2 = 0;
			do
			{
				int num3 = stream.Read(array, num2, num - num2);
				if (num3 == 0)
				{
					throw new Exception("The remote server closed the connection unexpectedly!");
				}
				num2 += num3;
			}
			while (num2 < num);
			FeedData(array, num2);
			HTTPResponse.ReadTo(stream, 10);
			num = ReadChunkLength(stream);
		}
		ReadHeaders(stream);
	}

	private new void ReadRaw(Stream stream, int contentLength)
	{
		byte[] array = new byte[1024];
		int num;
		do
		{
			num = stream.Read(array, 0, array.Length);
			FeedData(array, num);
		}
		while (num > 0);
	}

	public void FeedData(byte[] buffer, int count)
	{
		if (count == -1)
		{
			count = buffer.Length;
		}
		if (count == 0)
		{
			return;
		}
		int num = 0;
		int num2;
		do
		{
			num2 = -1;
			int num3 = 1;
			for (int i = num; i < count; i++)
			{
				if (num2 != -1)
				{
					break;
				}
				if (buffer[i] == 13)
				{
					if (i + 1 < count && buffer[i + 1] == 10)
					{
						num3 = 2;
					}
					num2 = i;
				}
				else if (buffer[i] == 10)
				{
					num2 = i;
				}
			}
			int num4 = ((num2 != -1) ? num2 : count);
			if (LineBuffer.Length < LineBufferPos + (num4 - num))
			{
				Array.Resize(ref LineBuffer, LineBufferPos + (num4 - num));
			}
			Array.Copy(buffer, num, LineBuffer, LineBufferPos, num4 - num);
			LineBufferPos += num4 - num;
			if (num2 == -1)
			{
				break;
			}
			ParseLine(LineBuffer, LineBufferPos);
			LineBufferPos = 0;
			num += num2 + num3;
		}
		while (num2 != -1 && num < count);
	}

	private void ParseLine(byte[] buffer, int count)
	{
		if (count == 0)
		{
			if (currentMessage != null)
			{
				lock (FrameLock)
				{
					completedMessages.Add(currentMessage);
				}
				currentMessage = null;
			}
		}
		else
		{
			if (buffer[0] == 58)
			{
				return;
			}
			int num = -1;
			for (int i = 0; i < count; i++)
			{
				if (num != -1)
				{
					break;
				}
				if (buffer[i] == 58)
				{
					num = i;
				}
			}
			string text;
			string text2;
			if (num != -1)
			{
				text = Encoding.UTF8.GetString(buffer, 0, num);
				if (num + 1 < count && buffer[num + 1] == 32)
				{
					num++;
				}
				num++;
				if (num >= count)
				{
					return;
				}
				text2 = Encoding.UTF8.GetString(buffer, num, count - num);
			}
			else
			{
				text = Encoding.UTF8.GetString(buffer, 0, count);
				text2 = string.Empty;
			}
			if (currentMessage == null)
			{
				currentMessage = new Message();
			}
			switch (text)
			{
			case "id":
				currentMessage.SetId(text2);
				break;
			case "event":
				currentMessage.set_Event(text2);
				break;
			case "data":
			{
				if (currentMessage.GetData() != null)
				{
					Message message = currentMessage;
					message.set_Data(message.GetData() + Environment.NewLine);
				}
				Message bCHHNNNFDGI2 = currentMessage;
				bCHHNNNFDGI2.set_Data(bCHHNNNFDGI2.GetData() + text2);
				break;
			}
			case "retry":
			{
				int result;
				if (int.TryParse(text2, out result))
				{
					currentMessage.set_Retry(TimeSpan.FromMilliseconds(result));
				}
				break;
			}
			}
		}
	}

	void IProtocol.HandleEvents()
	{
		lock (FrameLock)
		{
			if (completedMessages.Count > 0)
			{
				if (OnMessage != null)
				{
					for (int i = 0; i < completedMessages.Count; i++)
					{
						try
						{
							OnMessage(this, completedMessages[i]);
						}
						catch (Exception ex)
						{
							HTTPManager.GetLogger().Exception("EventSourceMessage", "HandleEvents - OnMessage", ex);
						}
					}
				}
				completedMessages.Clear();
			}
		}
		if (!GetIsClosed())
		{
			return;
		}
		completedMessages.Clear();
		if (OnClosed == null)
		{
			return;
		}
		try
		{
			OnClosed(this);
		}
		catch (Exception mPFFFAOGBJE2)
		{
			HTTPManager.GetLogger().Exception("EventSourceMessage", "HandleEvents - OnClosed", mPFFFAOGBJE2);
		}
		finally
		{
			OnClosed = null;
		}
	}
}
