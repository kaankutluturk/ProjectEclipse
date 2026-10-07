using System.IO;

public class InBuffer
{
	private byte[] m_Buffer;

	private uint m_Pos;

	private uint m_Limit;

	private uint m_BufferSize;

	private Stream m_Stream;

	private bool m_StreamWasExhausted;

	private ulong m_ProcessedSize;

	public InBuffer(uint KOGACKBGCFP)
	{
		m_Buffer = new byte[KOGACKBGCFP];
		m_BufferSize = KOGACKBGCFP;
	}

	public void Init(Stream ABJIEFMMIEK)
	{
		m_Stream = ABJIEFMMIEK;
		m_ProcessedSize = 0uL;
		m_Limit = 0u;
		m_Pos = 0u;
		m_StreamWasExhausted = false;
	}

	public bool ReadBlock()
	{
		if (m_StreamWasExhausted)
		{
			return false;
		}
		m_ProcessedSize += m_Pos;
		int num = m_Stream.Read(m_Buffer, 0, (int)m_BufferSize);
		m_Pos = 0u;
		m_Limit = (uint)num;
		m_StreamWasExhausted = num == 0;
		return !m_StreamWasExhausted;
	}

	public void ReleaseStream()
	{
		m_Stream = null;
	}

	public bool ReadByte(byte AAOIAEJJINO)
	{
		if (m_Pos >= m_Limit && !ReadBlock())
		{
			return false;
		}
		AAOIAEJJINO = m_Buffer[m_Pos++];
		return true;
	}

	public byte ReadByte()
	{
		if (m_Pos >= m_Limit && !ReadBlock())
		{
			return byte.MaxValue;
		}
		return m_Buffer[m_Pos++];
	}

	public ulong GetProcessedSize()
	{
		return m_ProcessedSize + m_Pos;
	}
}
