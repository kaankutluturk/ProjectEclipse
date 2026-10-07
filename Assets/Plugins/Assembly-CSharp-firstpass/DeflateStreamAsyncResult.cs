using System;
using System.Threading;

internal class DeflateStreamAsyncResult : IAsyncResult
{
	public byte[] buffer;

	public int offset;

	public int count;

	public bool isWrite;

	private object m_AsyncObject;

	private object m_AsyncState;

	private AsyncCallback m_AsyncCallback;

	private object m_AsyncResult;

	internal bool m_CompletedSynchronously;

	private int m_InvokedCallback;

	private int m_Completed;

	private object m_Event;

	public object AsyncStateObject
	{
		get
		{
			return AsyncState;
		}
	}

	public WaitHandle CompletionWaitHandle
	{
		get
		{
			return AsyncWaitHandle;
		}
	}

	public bool IsCompletedSynchronously
	{
		get
		{
			return CompletedSynchronously;
		}
	}

	public bool HasCompleted
	{
		get
		{
			return IsCompleted;
		}
	}

	internal object Result
	{
		get
		{
			return GetResult();
		}
	}

	public DeflateStreamAsyncResult(object FKBFNLAMILO, object LEGPNOBHGIE, AsyncCallback FCLGHDMMEBC, byte[] buffer, int IPCOBJBKNAO, int count)
	{
		this.buffer = buffer;
		this.offset = IPCOBJBKNAO;
		this.count = count;
		m_CompletedSynchronously = true;
		m_AsyncObject = FKBFNLAMILO;
		m_AsyncState = LEGPNOBHGIE;
		m_AsyncCallback = FCLGHDMMEBC;
	}

	public object AsyncState
	{
		get
		{
			return m_AsyncState;
		}
	}

	public WaitHandle AsyncWaitHandle
	{
		get
		{
		int oBPDLMDNOEM = m_Completed;
		if (m_Event == null)
		{
			Interlocked.CompareExchange(ref m_Event, new ManualResetEvent(oBPDLMDNOEM != 0), null);
		}
		ManualResetEvent manualResetEvent = (ManualResetEvent)m_Event;
		if (oBPDLMDNOEM == 0 && m_Completed != 0)
		{
			manualResetEvent.Set();
		}
		return manualResetEvent;
		}
	}

	public bool CompletedSynchronously
	{
		get
		{
			return m_CompletedSynchronously;
		}
	}

	public bool IsCompleted
	{
		get
		{
			return m_Completed != 0;
		}
	}

	internal object GetResult()
	{
		return m_AsyncResult;
	}

	internal void Close()
	{
		if (m_Event != null)
		{
			((ManualResetEvent)m_Event).Close();
		}
	}

	internal void InvokeCallback(bool ALLIOBCJDGG, object DCJLKCFKCOM)
	{
		Complete(ALLIOBCJDGG, DCJLKCFKCOM);
	}

	internal void InvokeCallback(object DCJLKCFKCOM)
	{
		Complete(DCJLKCFKCOM);
	}

	private void Complete(bool ALLIOBCJDGG, object DCJLKCFKCOM)
	{
		m_CompletedSynchronously = ALLIOBCJDGG;
		Complete(DCJLKCFKCOM);
	}

	private void Complete(object DCJLKCFKCOM)
	{
		m_AsyncResult = DCJLKCFKCOM;
		Interlocked.Increment(ref m_Completed);
		if (m_Event != null)
		{
			((ManualResetEvent)m_Event).Set();
		}
		if (Interlocked.Increment(ref m_InvokedCallback) == 1 && m_AsyncCallback != null)
		{
			m_AsyncCallback(this);
		}
	}
}
