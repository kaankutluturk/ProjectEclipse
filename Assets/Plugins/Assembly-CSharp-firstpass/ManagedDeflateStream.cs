using System;
using System.IO;
using System.Security;
using System.Threading;

public class ManagedDeflateStream : Stream
{
	internal delegate void AsyncWriteDelegate(byte[] HFPDMGAEJJE, int IPCOBJBKNAO, int count, bool MHBDEKPKCPF);

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

	public ManagedDeflateStream(Stream ABJIEFMMIEK, DeflateCompressionMode NMMPBADCFHK)
		: this(ABJIEFMMIEK, NMMPBADCFHK, false)
	{
	}

	public ManagedDeflateStream(Stream ABJIEFMMIEK, DeflateCompressionMode NMMPBADCFHK, bool LOLBAGJKKPH)
	{
		if (ABJIEFMMIEK == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (NMMPBADCFHK != DeflateCompressionMode.Compress && NMMPBADCFHK != DeflateCompressionMode.Decompress)
		{
			throw new ArgumentException(SR.GetString("Argument out of range"), "mode");
		}
		_stream = ABJIEFMMIEK;
		_mode = NMMPBADCFHK;
		_leaveOpen = LOLBAGJKKPH;
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

	public override long Seek(long IPCOBJBKNAO, SeekOrigin IKOOJMAOFOD)
	{
		throw new NotSupportedException(SR.GetString("Not supported"));
	}

	public override void SetLength(long value)
	{
		throw new NotSupportedException(SR.GetString("Not supported"));
	}

	public override int Read(byte[] HFPDMGAEJJE, int IPCOBJBKNAO, int count)
	{
		EnsureDecompressionMode();
		ValidateParameters(HFPDMGAEJJE, IPCOBJBKNAO, count);
		EnsureNotDisposed();
		int num = IPCOBJBKNAO;
		int num2 = count;
		while (true)
		{
			int num3 = inflater.Inflate(HFPDMGAEJJE, num, num2);
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

	private void ValidateParameters(byte[] HFPDMGAEJJE, int IPCOBJBKNAO, int count)
	{
		if (HFPDMGAEJJE == null)
		{
			throw new ArgumentNullException("array");
		}
		if (IPCOBJBKNAO < 0)
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		if (HFPDMGAEJJE.Length - IPCOBJBKNAO < count)
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

	public override IAsyncResult BeginRead(byte[] HFPDMGAEJJE, int IPCOBJBKNAO, int count, AsyncCallback FCLGHDMMEBC, object LEGPNOBHGIE)
	{
		EnsureDecompressionMode();
		if (asyncOperations != 0)
		{
			throw new InvalidOperationException(SR.GetString("Invalid begin call"));
		}
		ValidateParameters(HFPDMGAEJJE, IPCOBJBKNAO, count);
		EnsureNotDisposed();
		Interlocked.Increment(ref asyncOperations);
		try
		{
			DeflateStreamAsyncResult bOJEBIGFIKA = new DeflateStreamAsyncResult(this, LEGPNOBHGIE, FCLGHDMMEBC, HFPDMGAEJJE, IPCOBJBKNAO, count);
			bOJEBIGFIKA.isWrite = false;
			int num = inflater.Inflate(HFPDMGAEJJE, IPCOBJBKNAO, count);
			if (num != 0)
			{
				bOJEBIGFIKA.InvokeCallback(true, num);
				return bOJEBIGFIKA;
			}
			if (inflater.Finished())
			{
				bOJEBIGFIKA.InvokeCallback(true, 0);
				return bOJEBIGFIKA;
			}
			_stream.BeginRead(buffer, 0, buffer.Length, m_CallBack, bOJEBIGFIKA);
			bOJEBIGFIKA.m_CompletedSynchronously &= bOJEBIGFIKA.IsCompleted;
			return bOJEBIGFIKA;
		}
		catch
		{
			Interlocked.Decrement(ref asyncOperations);
			throw;
		}
	}

	private void ReadCallback(IAsyncResult KCLJLMAHPFI)
	{
		DeflateStreamAsyncResult bOJEBIGFIKA = (DeflateStreamAsyncResult)KCLJLMAHPFI.AsyncState;
		bOJEBIGFIKA.m_CompletedSynchronously &= KCLJLMAHPFI.CompletedSynchronously;
		int num = 0;
		try
		{
			EnsureNotDisposed();
			num = _stream.EndRead(KCLJLMAHPFI);
			if (num <= 0)
			{
				bOJEBIGFIKA.InvokeCallback(0);
				return;
			}
			inflater.SetInput(buffer, 0, num);
			num = inflater.Inflate(bOJEBIGFIKA.buffer, bOJEBIGFIKA.offset, bOJEBIGFIKA.count);
			if (num == 0 && !inflater.Finished())
			{
				_stream.BeginRead(buffer, 0, buffer.Length, m_CallBack, bOJEBIGFIKA);
			}
			else
			{
				bOJEBIGFIKA.InvokeCallback(num);
			}
		}
		catch (Exception dCJLKCFKCOM)
		{
			bOJEBIGFIKA.InvokeCallback(dCJLKCFKCOM);
		}
	}

	public override int EndRead(IAsyncResult BHNNOKGCDEG)
	{
		EnsureDecompressionMode();
		CheckEndXxxxLegalStateAndParams(BHNNOKGCDEG);
		DeflateStreamAsyncResult bOJEBIGFIKA = (DeflateStreamAsyncResult)BHNNOKGCDEG;
		AwaitAsyncIOCompletion(bOJEBIGFIKA);
		Exception ex = bOJEBIGFIKA.GetResult() as Exception;
		if (ex != null)
		{
			throw ex;
		}
		return (int)bOJEBIGFIKA.GetResult();
	}

	public override void Write(byte[] HFPDMGAEJJE, int IPCOBJBKNAO, int count)
	{
		EnsureCompressionMode();
		ValidateParameters(HFPDMGAEJJE, IPCOBJBKNAO, count);
		EnsureNotDisposed();
		InternalWrite(HFPDMGAEJJE, IPCOBJBKNAO, count, false);
	}

	internal void InternalWrite(byte[] HFPDMGAEJJE, int IPCOBJBKNAO, int count, bool MHBDEKPKCPF)
	{
		DoMaintenance(HFPDMGAEJJE, IPCOBJBKNAO, count);
		WriteDeflaterOutput(MHBDEKPKCPF);
		deflater.SetInput(HFPDMGAEJJE, IPCOBJBKNAO, count);
		WriteDeflaterOutput(MHBDEKPKCPF);
	}

	private void WriteDeflaterOutput(bool MHBDEKPKCPF)
	{
		while (!deflater.NeedsInput())
		{
			int num = deflater.GetDeflateOutput(buffer);
			if (num > 0)
			{
				DoWrite(buffer, 0, num, MHBDEKPKCPF);
			}
		}
	}

	private void DoWrite(byte[] HFPDMGAEJJE, int IPCOBJBKNAO, int count, bool MHBDEKPKCPF)
	{
		if (MHBDEKPKCPF)
		{
			IAsyncResult asyncResult = _stream.BeginWrite(HFPDMGAEJJE, IPCOBJBKNAO, count, null, null);
			_stream.EndWrite(asyncResult);
		}
		else
		{
			_stream.Write(HFPDMGAEJJE, IPCOBJBKNAO, count);
		}
	}

	private void DoMaintenance(byte[] HFPDMGAEJJE, int IPCOBJBKNAO, int count)
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
			formatWriter.UpdateWithBytesRead(HFPDMGAEJJE, IPCOBJBKNAO, count);
		}
	}

	private void PurgeBuffers(bool KLCPNDHEBGP)
	{
		if (!KLCPNDHEBGP || _stream == null)
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
				int GJBPPJIGAIG;
				flag = deflater.Finish(buffer, out GJBPPJIGAIG);
				if (GJBPPJIGAIG > 0)
				{
					DoWrite(buffer, 0, GJBPPJIGAIG, false);
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

	protected override void Dispose(bool KLCPNDHEBGP)
	{
		try
		{
			PurgeBuffers(KLCPNDHEBGP);
		}
		finally
		{
			try
			{
				if (KLCPNDHEBGP && !_leaveOpen && _stream != null)
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
					base.Dispose(KLCPNDHEBGP);
				}
			}
		}
	}

	public override IAsyncResult BeginWrite(byte[] HFPDMGAEJJE, int IPCOBJBKNAO, int count, AsyncCallback FCLGHDMMEBC, object LEGPNOBHGIE)
	{
		EnsureCompressionMode();
		if (asyncOperations != 0)
		{
			throw new InvalidOperationException(SR.GetString("Invalid begin call"));
		}
		ValidateParameters(HFPDMGAEJJE, IPCOBJBKNAO, count);
		EnsureNotDisposed();
		Interlocked.Increment(ref asyncOperations);
		try
		{
			DeflateStreamAsyncResult bOJEBIGFIKA = new DeflateStreamAsyncResult(this, LEGPNOBHGIE, FCLGHDMMEBC, HFPDMGAEJJE, IPCOBJBKNAO, count);
			bOJEBIGFIKA.isWrite = true;
			m_AsyncWriterDelegate.BeginInvoke(HFPDMGAEJJE, IPCOBJBKNAO, count, true, m_CallBack, bOJEBIGFIKA);
			bOJEBIGFIKA.m_CompletedSynchronously &= bOJEBIGFIKA.IsCompleted;
			return bOJEBIGFIKA;
		}
		catch
		{
			Interlocked.Decrement(ref asyncOperations);
			throw;
		}
	}

	private void WriteCallback(IAsyncResult BHNNOKGCDEG)
	{
		DeflateStreamAsyncResult bOJEBIGFIKA = (DeflateStreamAsyncResult)BHNNOKGCDEG.AsyncState;
		bOJEBIGFIKA.m_CompletedSynchronously &= BHNNOKGCDEG.CompletedSynchronously;
		try
		{
			m_AsyncWriterDelegate.EndInvoke(BHNNOKGCDEG);
		}
		catch (Exception dCJLKCFKCOM)
		{
			bOJEBIGFIKA.InvokeCallback(dCJLKCFKCOM);
			return;
		}
		bOJEBIGFIKA.InvokeCallback(null);
	}

	public override void EndWrite(IAsyncResult BHNNOKGCDEG)
	{
		EnsureCompressionMode();
		CheckEndXxxxLegalStateAndParams(BHNNOKGCDEG);
		DeflateStreamAsyncResult bOJEBIGFIKA = (DeflateStreamAsyncResult)BHNNOKGCDEG;
		AwaitAsyncIOCompletion(bOJEBIGFIKA);
		Exception ex = bOJEBIGFIKA.GetResult() as Exception;
		if (ex != null)
		{
			throw ex;
		}
	}

	private void CheckEndXxxxLegalStateAndParams(IAsyncResult BHNNOKGCDEG)
	{
		if (asyncOperations != 1)
		{
			throw new InvalidOperationException(SR.GetString("Invalid end call"));
		}
		if (BHNNOKGCDEG == null)
		{
			throw new ArgumentNullException("asyncResult");
		}
		EnsureNotDisposed();
		DeflateStreamAsyncResult bOJEBIGFIKA = BHNNOKGCDEG as DeflateStreamAsyncResult;
		if (bOJEBIGFIKA == null)
		{
			throw new ArgumentNullException("asyncResult");
		}
	}

	private void AwaitAsyncIOCompletion(DeflateStreamAsyncResult BHNNOKGCDEG)
	{
		try
		{
			if (!BHNNOKGCDEG.IsCompleted)
			{
				BHNNOKGCDEG.AsyncWaitHandle.WaitOne();
			}
		}
		finally
		{
			Interlocked.Decrement(ref asyncOperations);
			BHNNOKGCDEG.Close();
		}
	}
}
