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

	public ManagedGZipStream(Stream ABJIEFMMIEK, DeflateCompressionMode NMMPBADCFHK)
		: this(ABJIEFMMIEK, NMMPBADCFHK, false)
	{
	}

	public ManagedGZipStream(Stream ABJIEFMMIEK, DeflateCompressionMode NMMPBADCFHK, bool LOLBAGJKKPH)
	{
		deflateStream = new ManagedDeflateStream(ABJIEFMMIEK, NMMPBADCFHK, LOLBAGJKKPH);
		SetDeflateStreamFileFormatter(NMMPBADCFHK);
	}

	private void SetDeflateStreamFileFormatter(DeflateCompressionMode NMMPBADCFHK)
	{
		if (NMMPBADCFHK == DeflateCompressionMode.Compress)
		{
			IFileFormatWriter aPMCMDOBFOI = new GZipFormatter();
			deflateStream.SetFileFormatWriter(aPMCMDOBFOI);
		}
		else
		{
			IFileFormatReader iJIMLLIHKGN = new GZipDecoder();
			deflateStream.SetFileFormatReader(iJIMLLIHKGN);
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

	public override long Seek(long IPCOBJBKNAO, SeekOrigin IKOOJMAOFOD)
	{
		throw new NotSupportedException(SR.GetString("Not supported"));
	}

	public override void SetLength(long value)
	{
		throw new NotSupportedException(SR.GetString("Not supported"));
	}

	public override IAsyncResult BeginRead(byte[] HFPDMGAEJJE, int IPCOBJBKNAO, int count, AsyncCallback FCLGHDMMEBC, object LEGPNOBHGIE)
	{
		if (deflateStream == null)
		{
			throw new InvalidOperationException(SR.GetString("Object disposed"));
		}
		return deflateStream.BeginRead(HFPDMGAEJJE, IPCOBJBKNAO, count, FCLGHDMMEBC, LEGPNOBHGIE);
	}

	public override int EndRead(IAsyncResult BHNNOKGCDEG)
	{
		if (deflateStream == null)
		{
			throw new InvalidOperationException(SR.GetString("Object disposed"));
		}
		return deflateStream.EndRead(BHNNOKGCDEG);
	}

	public override IAsyncResult BeginWrite(byte[] HFPDMGAEJJE, int IPCOBJBKNAO, int count, AsyncCallback FCLGHDMMEBC, object LEGPNOBHGIE)
	{
		if (deflateStream == null)
		{
			throw new InvalidOperationException(SR.GetString("Object disposed"));
		}
		return deflateStream.BeginWrite(HFPDMGAEJJE, IPCOBJBKNAO, count, FCLGHDMMEBC, LEGPNOBHGIE);
	}

	public override void EndWrite(IAsyncResult BHNNOKGCDEG)
	{
		if (deflateStream == null)
		{
			throw new InvalidOperationException(SR.GetString("Object disposed"));
		}
		deflateStream.EndWrite(BHNNOKGCDEG);
	}

	public override int Read(byte[] HFPDMGAEJJE, int IPCOBJBKNAO, int count)
	{
		if (deflateStream == null)
		{
			throw new ObjectDisposedException(null, SR.GetString("Object disposed"));
		}
		return deflateStream.Read(HFPDMGAEJJE, IPCOBJBKNAO, count);
	}

	public override void Write(byte[] HFPDMGAEJJE, int IPCOBJBKNAO, int count)
	{
		if (deflateStream == null)
		{
			throw new ObjectDisposedException(null, SR.GetString("Object disposed"));
		}
		deflateStream.Write(HFPDMGAEJJE, IPCOBJBKNAO, count);
	}

	protected override void Dispose(bool KLCPNDHEBGP)
	{
		try
		{
			if (KLCPNDHEBGP && deflateStream != null)
			{
				deflateStream.Dispose();
			}
			deflateStream = null;
		}
		finally
		{
			base.Dispose(KLCPNDHEBGP);
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
