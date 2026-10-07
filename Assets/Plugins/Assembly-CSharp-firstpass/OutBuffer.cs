using System.IO;

public class OutBuffer
{
	private byte[] m_Buffer;

	private uint m_Pos;

	private uint m_BufferSize;

	private Stream m_Stream;

	private ulong m_ProcessedSize;

	public OutBuffer(uint KOGACKBGCFP)
	{
		m_Buffer = new byte[KOGACKBGCFP];
		m_BufferSize = KOGACKBGCFP;
	}

	public void SetStream(Stream ABJIEFMMIEK)
	{
		m_Stream = ABJIEFMMIEK;
	}

	public void FlushStream()
	{
		m_Stream.Flush();
	}

	public void CloseStream()
	{
		m_Stream.Close();
	}

	public void ReleaseStream()
	{
		m_Stream = null;
	}

	public void Init()
	{
		m_ProcessedSize = 0uL;
		m_Pos = 0u;
	}

	public void WriteByte(byte AAOIAEJJINO)
	{
		m_Buffer[m_Pos++] = AAOIAEJJINO;
		if (m_Pos >= m_BufferSize)
		{
			FlushData();
		}
	}

	public void FlushData()
	{
		if (m_Pos != 0)
		{
			m_Stream.Write(m_Buffer, 0, (int)m_Pos);
			m_Pos = 0u;
		}
	}

	public ulong GetProcessedSize()
	{
		return m_ProcessedSize + m_Pos;
	}
}
