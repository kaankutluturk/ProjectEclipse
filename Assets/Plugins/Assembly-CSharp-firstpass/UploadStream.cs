using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

public sealed class UploadStream : Stream
{
	private MemoryStream ReadBuffer = new MemoryStream();

	private MemoryStream WriteBuffer = new MemoryStream();

	private bool noMoreData;

	private AutoResetEvent ARE = new AutoResetEvent(false);

	private object locker = new object();

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string name;

	public string StreamName
	{
		get
		{
			return get_Name();
		}
		private set
		{
			set_Name(value);
		}
	}

	private bool IsReadBufferEmpty
	{
		get
		{
			return CheckReadBufferEmpty();
		}
	}






	public UploadStream(string name)
		: this()
	{
		set_Name(name);
	}

	public UploadStream()
	{
		ReadBuffer = new MemoryStream();
		WriteBuffer = new MemoryStream();
		set_Name(string.Empty);
	}

	public string get_Name()
	{
		return name;
	}

	private void set_Name(string value)
	{
		name = value;
	}

	private bool CheckReadBufferEmpty()
	{
		lock (locker)
		{
			return ReadBuffer.Position == ReadBuffer.Length;
		}
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		if (noMoreData)
		{
			if (ReadBuffer.Position != ReadBuffer.Length)
			{
				return ReadBuffer.Read(buffer, offset, count);
			}
			if (WriteBuffer.Length <= 0)
			{
				HTTPManager.GetLogger().Information("UploadStream", string.Format("{0} - Read - End Of Stream", get_Name()));
				return -1;
			}
			SwitchBuffers();
		}
		if (CheckReadBufferEmpty())
		{
			ARE.WaitOne();
			lock (locker)
			{
				if (CheckReadBufferEmpty() && WriteBuffer.Length > 0)
				{
					SwitchBuffers();
				}
			}
		}
		int num = -1;
		lock (locker)
		{
			return ReadBuffer.Read(buffer, offset, count);
		}
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		if (noMoreData)
		{
			throw new ArgumentException("noMoreData already set!");
		}
		lock (locker)
		{
			WriteBuffer.Write(buffer, offset, count);
			SwitchBuffers();
		}
		ARE.Set();
	}

	public override void Flush()
	{
		Finish();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			HTTPManager.GetLogger().Information("UploadStream", string.Format("{0} - Dispose", get_Name()));
			ReadBuffer.Dispose();
			ReadBuffer = null;
			WriteBuffer.Dispose();
			WriteBuffer = null;
			ARE.Close();
			ARE = null;
		}
		base.Dispose(disposing);
	}

	public void Finish()
	{
		if (noMoreData)
		{
			throw new ArgumentException("noMoreData already set!");
		}
		HTTPManager.GetLogger().Information("UploadStream", string.Format("{0} - Finish", get_Name()));
		noMoreData = true;
		ARE.Set();
	}

	private bool SwitchBuffers()
	{
		lock (locker)
		{
			if (ReadBuffer.Position == ReadBuffer.Length)
			{
				WriteBuffer.Seek(0L, SeekOrigin.Begin);
				ReadBuffer.SetLength(0L);
				MemoryStream oldReadBuffer = WriteBuffer;
				WriteBuffer = ReadBuffer;
				ReadBuffer = oldReadBuffer;
				return true;
			}
		}
		return false;
	}

	public override bool CanRead
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public override bool CanSeek
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public override bool CanWrite
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public override long Length
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public override long Position
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotImplementedException();
	}

	public override void SetLength(long value)
	{
		throw new NotImplementedException();
	}
}
