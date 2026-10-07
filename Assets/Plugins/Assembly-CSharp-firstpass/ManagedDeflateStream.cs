using System;
using System.IO;
using System.Security;
using System.Threading;

public class ManagedDeflateStream : Stream
{
	internal delegate void AsyncWriteDelegate(byte[] array, int offset, int count, bool isAsync);

	private enum WorkerType : byte
	{
		Managed = 0,
		Unknown = 1
	}

	internal const int DefaultBufferSize = 8192;

	private Stream _stream;

	private DeflateCompressionMode _mode;

	private bool _leaveOpen;

	private Inflater inflater;

	private IDeflater deflater;

	private byte[] buffer;

	private int asyncOperations;

	private readonly AsyncCallback m_CallBack;

	private readonly AsyncWriteDelegate m_AsyncWriterDelegate;

	private IFileFormatWriter formatWriter;

	private bool wroteHeader;

	private bool wroteBytes;

	public Stream BaseStream
	{
		get
		{
			return GetBaseStream();
		}
	}

	public ManagedDeflateStream(Stream stream, DeflateCompressionMode compressionMode)
		: this(stream, compressionMode, false)
	{
	}

	public ManagedDeflateStream(Stream stream, DeflateCompressionMode compressionMode, bool leaveOpen)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (compressionMode != DeflateCompressionMode.Compress && compressionMode != DeflateCompressionMode.Decompress)
		{
			throw new ArgumentException(SR.GetString("Argument out of range"), "mode");
		}
		_stream = stream;
		_mode = compressionMode;
		_leaveOpen = leaveOpen;
		switch (_mode)
		{
		case DeflateCompressionMode.Decompress:
			if (!_stream.CanRead)
			{
				throw new ArgumentException(SR.GetString("Not a readable stream"), "stream");
			}
			inflater = new Inflater();
			m_CallBack = ReadCallback;
			break;
		case DeflateCompressionMode.Compress:
			if (!_stream.CanWrite)
			{
				throw new ArgumentException(SR.GetString("Not a writeable stream"), "stream");
			}
			deflater = CreateDeflater();
			m_AsyncWriterDelegate = InternalWrite;
			m_CallBack = WriteCallback;
			break;
		}
		buffer = new byte[8192];
	}

	private static IDeflater CreateDeflater()
	{
		if (GetDeflaterType() == WorkerType.Managed)
		{
			return new DeflaterManaged();
		}
		throw new SystemException("Program entered an unexpected state.");
	}

	[SecuritySafeCritical]
	private static WorkerType GetDeflaterType()
	{
		return WorkerType.Managed;
	}

	internal void SetFileFormatReader(IFileFormatReader reader)
	{
		if (reader != null)
		{
			inflater.SetFileFormatReader(reader);
		}
	}

	internal void SetFileFormatWriter(IFileFormatWriter writer)
	{
		if (writer != null)
		{
			formatWriter = writer;
		}
	}

	public Stream GetBaseStream()
	{
		return _stream;
	}

	public override bool CanRead
	{
		get
		{
			if (_stream == null)
			{
				return false;
			}
			return _mode == DeflateCompressionMode.Decompress && _stream.CanRead;
		}
	}
	public override bool CanWrite
	{
		get
		{
			if (_stream == null)
			{
				return false;
			}
			return _mode == DeflateCompressionMode.Compress && _stream.CanWrite;
		}
	}
	public override bool CanSeek
	{
		get
		{
			return false;
		}
	}
	public override long Length
	{
		get
		{
			throw new NotSupportedException(SR.GetString("Not supported"));
		}
	}
	public override long Position
	{
		get
		{
			throw new NotSupportedException(SR.GetString("Not supported"));
		}
		set
		{
			throw new NotSupportedException(SR.GetString("Not supported"));
		}
	}

	public override void Flush()
	{
		if (_stream != null)
		{
			_stream.Flush();
		}
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotSupportedException(SR.GetString("Not supported"));
	}

	public override void SetLength(long value)
	{
		throw new NotSupportedException(SR.GetString("Not supported"));
	}

	public override int Read(byte[] array, int offset, int count)
	{
		EnsureDecompressionMode();
		ValidateParameters(array, offset, count);
		EnsureNotDisposed();
		int num = offset;
		int num2 = count;
		while (true)
		{
			int num3 = inflater.Inflate(array, num, num2);
			num += num3;
			num2 -= num3;
			if (num2 == 0 || inflater.Finished())
			{
				break;
			}
			int num4 = _stream.Read(buffer, 0, buffer.Length);
			if (num4 == 0)
			{
				break;
			}
			inflater.SetInput(buffer, 0, num4);
		}
		return count - num2;
	}

	private void ValidateParameters(byte[] array, int offset, int count)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		if (array.Length - offset < count)
		{
			throw new ArgumentException(SR.GetString("Invalid argument offset count"));
		}
	}

	private void EnsureNotDisposed()
	{
		if (_stream == null)
		{
			throw new ObjectDisposedException(null, SR.GetString("Object disposed"));
		}
	}

	private void EnsureDecompressionMode()
	{
		if (_mode != DeflateCompressionMode.Decompress)
		{
			throw new InvalidOperationException(SR.GetString("Cannot read from deflate stream"));
		}
	}

	private void EnsureCompressionMode()
	{
		if (_mode != DeflateCompressionMode.Compress)
		{
			throw new InvalidOperationException(SR.GetString("Cannot write to deflate stream"));
		}
	}

	public override IAsyncResult BeginRead(byte[] array, int offset, int count, AsyncCallback asyncCallback, object asyncState)
	{
		EnsureDecompressionMode();
		if (asyncOperations != 0)
		{
			throw new InvalidOperationException(SR.GetString("Invalid begin call"));
		}
		ValidateParameters(array, offset, count);
		EnsureNotDisposed();
		Interlocked.Increment(ref asyncOperations);
		try
		{
			DeflateStreamAsyncResult deflateAsyncResult = new DeflateStreamAsyncResult(this, asyncState, asyncCallback, array, offset, count);
			deflateAsyncResult.isWrite = false;
			int num = inflater.Inflate(array, offset, count);
			if (num != 0)
			{
				deflateAsyncResult.InvokeCallback(true, num);
				return deflateAsyncResult;
			}
			if (inflater.Finished())
			{
				deflateAsyncResult.InvokeCallback(true, 0);
				return deflateAsyncResult;
			}
			_stream.BeginRead(buffer, 0, buffer.Length, m_CallBack, deflateAsyncResult);
			deflateAsyncResult.m_CompletedSynchronously &= deflateAsyncResult.IsCompleted;
			return deflateAsyncResult;
		}
		catch
		{
			Interlocked.Decrement(ref asyncOperations);
			throw;
		}
	}

	private void ReadCallback(IAsyncResult asyncResult)
	{
		DeflateStreamAsyncResult deflateAsyncResult = (DeflateStreamAsyncResult)asyncResult.AsyncState;
		deflateAsyncResult.m_CompletedSynchronously &= asyncResult.CompletedSynchronously;
		int num = 0;
		try
		{
			EnsureNotDisposed();
			num = _stream.EndRead(asyncResult);
			if (num <= 0)
			{
				deflateAsyncResult.InvokeCallback(0);
				return;
			}
			inflater.SetInput(buffer, 0, num);
			num = inflater.Inflate(deflateAsyncResult.buffer, deflateAsyncResult.offset, deflateAsyncResult.count);
			if (num == 0 && !inflater.Finished())
			{
				_stream.BeginRead(buffer, 0, buffer.Length, m_CallBack, deflateAsyncResult);
			}
			else
			{
				deflateAsyncResult.InvokeCallback(num);
			}
		}
		catch (Exception exception)
		{
			deflateAsyncResult.InvokeCallback(exception);
		}
	}

	public override int EndRead(IAsyncResult asyncResult)
	{
		EnsureDecompressionMode();
		CheckEndXxxxLegalStateAndParams(asyncResult);
		DeflateStreamAsyncResult deflateAsyncResult = (DeflateStreamAsyncResult)asyncResult;
		AwaitAsyncIOCompletion(deflateAsyncResult);
		Exception ex = deflateAsyncResult.GetResult() as Exception;
		if (ex != null)
		{
			throw ex;
		}
		return (int)deflateAsyncResult.GetResult();
	}

	public override void Write(byte[] array, int offset, int count)
	{
		EnsureCompressionMode();
		ValidateParameters(array, offset, count);
		EnsureNotDisposed();
		InternalWrite(array, offset, count, false);
	}

	internal void InternalWrite(byte[] array, int offset, int count, bool isAsync)
	{
		DoMaintenance(array, offset, count);
		WriteDeflaterOutput(isAsync);
		deflater.SetInput(array, offset, count);
		WriteDeflaterOutput(isAsync);
	}

	private void WriteDeflaterOutput(bool isAsync)
	{
		while (!deflater.NeedsInput())
		{
			int num = deflater.GetDeflateOutput(buffer);
			if (num > 0)
			{
				DoWrite(buffer, 0, num, isAsync);
			}
		}
	}

	private void DoWrite(byte[] array, int offset, int count, bool isAsync)
	{
		if (isAsync)
		{
			IAsyncResult asyncResult = _stream.BeginWrite(array, offset, count, null, null);
			_stream.EndWrite(asyncResult);
		}
		else
		{
			_stream.Write(array, offset, count);
		}
	}

	private void DoMaintenance(byte[] data, int offset, int count)
	{
		if (count <= 0)
		{
			return;
		}
		wroteBytes = true;
		if (formatWriter != null)
		{
			if (!wroteHeader)
			{
				byte[] array = formatWriter.GetHeader();
				_stream.Write(array, 0, array.Length);
				wroteHeader = true;
			}
			formatWriter.UpdateWithBytesRead(data, offset, count);
		}
	}

	private void PurgeBuffers(bool disposing)
	{
		if (!disposing || _stream == null)
		{
			return;
		}
		Flush();
		if (_mode != DeflateCompressionMode.Compress)
		{
			return;
		}
		if (wroteBytes)
		{
			WriteDeflaterOutput(false);
			bool flag;
			do
			{
				int bytesProduced;
				flag = deflater.Finish(buffer, out bytesProduced);
				if (bytesProduced > 0)
				{
					DoWrite(buffer, 0, bytesProduced, false);
				}
			}
			while (!flag);
		}
		if (formatWriter != null && wroteHeader)
		{
			byte[] array = formatWriter.GetFooter();
			_stream.Write(array, 0, array.Length);
		}
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			PurgeBuffers(disposing);
		}
		finally
		{
			try
			{
				if (disposing && !_leaveOpen && _stream != null)
				{
					_stream.Dispose();
				}
			}
			finally
			{
				_stream = null;
				try
				{
					if (deflater != null)
					{
						deflater.Dispose();
					}
				}
				finally
				{
					deflater = null;
					base.Dispose(disposing);
				}
			}
		}
	}

	public override IAsyncResult BeginWrite(byte[] array, int offset, int count, AsyncCallback asyncCallback, object asyncState)
	{
		EnsureCompressionMode();
		if (asyncOperations != 0)
		{
			throw new InvalidOperationException(SR.GetString("Invalid begin call"));
		}
		ValidateParameters(array, offset, count);
		EnsureNotDisposed();
		Interlocked.Increment(ref asyncOperations);
		try
		{
			DeflateStreamAsyncResult deflateAsyncResult = new DeflateStreamAsyncResult(this, asyncState, asyncCallback, array, offset, count);
			deflateAsyncResult.isWrite = true;
			m_AsyncWriterDelegate.BeginInvoke(array, offset, count, true, m_CallBack, deflateAsyncResult);
			deflateAsyncResult.m_CompletedSynchronously &= deflateAsyncResult.IsCompleted;
			return deflateAsyncResult;
		}
		catch
		{
			Interlocked.Decrement(ref asyncOperations);
			throw;
		}
	}

	private void WriteCallback(IAsyncResult asyncResult)
	{
		DeflateStreamAsyncResult deflateAsyncResult = (DeflateStreamAsyncResult)asyncResult.AsyncState;
		deflateAsyncResult.m_CompletedSynchronously &= asyncResult.CompletedSynchronously;
		try
		{
			m_AsyncWriterDelegate.EndInvoke(asyncResult);
		}
		catch (Exception exception)
		{
			deflateAsyncResult.InvokeCallback(exception);
			return;
		}
		deflateAsyncResult.InvokeCallback(null);
	}

	public override void EndWrite(IAsyncResult asyncResult)
	{
		EnsureCompressionMode();
		CheckEndXxxxLegalStateAndParams(asyncResult);
		DeflateStreamAsyncResult deflateAsyncResult = (DeflateStreamAsyncResult)asyncResult;
		AwaitAsyncIOCompletion(deflateAsyncResult);
		Exception ex = deflateAsyncResult.GetResult() as Exception;
		if (ex != null)
		{
			throw ex;
		}
	}

	private void CheckEndXxxxLegalStateAndParams(IAsyncResult asyncResult)
	{
		if (asyncOperations != 1)
		{
			throw new InvalidOperationException(SR.GetString("Invalid end call"));
		}
		if (asyncResult == null)
		{
			throw new ArgumentNullException("asyncResult");
		}
		EnsureNotDisposed();
		DeflateStreamAsyncResult deflateAsyncResult = asyncResult as DeflateStreamAsyncResult;
		if (deflateAsyncResult == null)
		{
			throw new ArgumentNullException("asyncResult");
		}
	}

	private void AwaitAsyncIOCompletion(DeflateStreamAsyncResult asyncResult)
	{
		try
		{
			if (!asyncResult.IsCompleted)
			{
				asyncResult.AsyncWaitHandle.WaitOne();
			}
		}
		finally
		{
			Interlocked.Decrement(ref asyncOperations);
			asyncResult.Close();
		}
	}
}
