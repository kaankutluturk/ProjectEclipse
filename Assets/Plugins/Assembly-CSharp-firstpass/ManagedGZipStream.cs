using System;
using System.IO;

public class ManagedGZipStream : Stream
{
	private ManagedDeflateStream deflateStream;

	public Stream BaseStream
	{
		get
		{
			return GetBaseStream();
		}
	}

	public ManagedGZipStream(Stream stream, DeflateCompressionMode compressionMode)
		: this(stream, compressionMode, false)
	{
	}

	public ManagedGZipStream(Stream stream, DeflateCompressionMode compressionMode, bool leaveOpen)
	{
		deflateStream = new ManagedDeflateStream(stream, compressionMode, leaveOpen);
		SetDeflateStreamFileFormatter(compressionMode);
	}

	private void SetDeflateStreamFileFormatter(DeflateCompressionMode compressionMode)
	{
		if (compressionMode == DeflateCompressionMode.Compress)
		{
			IFileFormatWriter fileFormatWriter = new GZipFormatter();
			deflateStream.SetFileFormatWriter(fileFormatWriter);
		}
		else
		{
			IFileFormatReader fileFormatReader = new GZipDecoder();
			deflateStream.SetFileFormatReader(fileFormatReader);
		}
	}

	public override bool CanRead
	{
		get
		{
			if (deflateStream == null)
			{
				return false;
			}
			return deflateStream.CanRead;
		}
	}
	public override bool CanWrite
	{
		get
		{
			if (deflateStream == null)
			{
				return false;
			}
			return deflateStream.CanWrite;
		}
	}
	public override bool CanSeek
	{
		get
		{
			if (deflateStream == null)
			{
				return false;
			}
			return deflateStream.CanSeek;
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
		BaseStream.Flush();
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotSupportedException(SR.GetString("Not supported"));
	}

	public override void SetLength(long value)
	{
		throw new NotSupportedException(SR.GetString("Not supported"));
	}

	public override IAsyncResult BeginRead(byte[] array, int offset, int count, AsyncCallback asyncCallback, object asyncState)
	{
		if (deflateStream == null)
		{
			throw new InvalidOperationException(SR.GetString("Object disposed"));
		}
		return deflateStream.BeginRead(array, offset, count, asyncCallback, asyncState);
	}

	public override int EndRead(IAsyncResult asyncResult)
	{
		if (deflateStream == null)
		{
			throw new InvalidOperationException(SR.GetString("Object disposed"));
		}
		return deflateStream.EndRead(asyncResult);
	}

	public override IAsyncResult BeginWrite(byte[] array, int offset, int count, AsyncCallback asyncCallback, object asyncState)
	{
		if (deflateStream == null)
		{
			throw new InvalidOperationException(SR.GetString("Object disposed"));
		}
		return deflateStream.BeginWrite(array, offset, count, asyncCallback, asyncState);
	}

	public override void EndWrite(IAsyncResult asyncResult)
	{
		if (deflateStream == null)
		{
			throw new InvalidOperationException(SR.GetString("Object disposed"));
		}
		deflateStream.EndWrite(asyncResult);
	}

	public override int Read(byte[] array, int offset, int count)
	{
		if (deflateStream == null)
		{
			throw new ObjectDisposedException(null, SR.GetString("Object disposed"));
		}
		return deflateStream.Read(array, offset, count);
	}

	public override void Write(byte[] array, int offset, int count)
	{
		if (deflateStream == null)
		{
			throw new ObjectDisposedException(null, SR.GetString("Object disposed"));
		}
		deflateStream.Write(array, offset, count);
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposing && deflateStream != null)
			{
				deflateStream.Dispose();
			}
			deflateStream = null;
		}
		finally
		{
			base.Dispose(disposing);
		}
	}

	public Stream GetBaseStream()
	{
		if (deflateStream != null)
		{
			return deflateStream.GetBaseStream();
		}
		return null;
	}
}
