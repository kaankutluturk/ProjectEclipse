using System;
using System.Diagnostics;
using System.IO;

public class WebSocketBinaryFrame : IWebSocketFrameWriter
{
	private static readonly byte[] NoData = new byte[0];

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool isFinal;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private byte[] data;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private ulong pos;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private ulong length;

	public bool FinalFragment
	{
		get
		{
			return GetIsFinal();
		}
		protected set
		{
			set_IsFinal(value);
		}
	}

	protected ulong Pos
	{
		get
		{
			return GetPos();
		}
		set
		{
			SetPos(value);
		}
	}

	protected ulong DataLength
	{
		get
		{
			return GetLength();
		}
		set
		{
			set_Length(value);
		}
	}

	public WebSocketBinaryFrame(byte[] data)
		: this(data, 0uL, (ulong)((data == null) ? 0 : data.Length), true)
	{
	}

	public WebSocketBinaryFrame(byte[] data, bool isFinal)
		: this(data, 0uL, (ulong)((data == null) ? 0 : data.Length), isFinal)
	{
	}

	public WebSocketBinaryFrame(byte[] data, ulong LCCLEFMKLPB, ulong length, bool isFinal)
	{
		set_Data(data);
		SetPos(LCCLEFMKLPB);
		set_Length(length);
		set_IsFinal(isFinal);
	}

	public virtual WebSocketFrameTypes get_Type()
	{
		return WebSocketFrameTypes.Binary;
	}

	public bool GetIsFinal()
	{
		return isFinal;
	}

	protected void set_IsFinal(bool value)
	{
		isFinal = value;
	}

	protected byte[] GetData()
	{
		return data;
	}

	protected void set_Data(byte[] value)
	{
		data = value;
	}

	protected ulong GetPos()
	{
		return pos;
	}

	protected void SetPos(ulong value)
	{
		pos = value;
	}

	protected ulong GetLength()
	{
		return length;
	}

	protected void set_Length(ulong value)
	{
		length = value;
	}

	public virtual byte[] Get()
	{
		if (GetData() == null)
		{
			set_Data(NoData);
		}
		using (MemoryStream memoryStream = new MemoryStream((int)GetLength() + 9))
		{
			byte b = (byte)(GetIsFinal() ? 128u : 0u);
			memoryStream.WriteByte((byte)((uint)b | (uint)get_Type()));
			if (GetLength() < 126)
			{
				memoryStream.WriteByte((byte)(0x80 | (byte)GetLength()));
			}
			else if (GetLength() < 65535)
			{
				memoryStream.WriteByte(254);
				byte[] bytes = BitConverter.GetBytes((ushort)GetLength());
				if (BitConverter.IsLittleEndian)
				{
					Array.Reverse(bytes, 0, bytes.Length);
				}
				memoryStream.Write(bytes, 0, bytes.Length);
			}
			else
			{
				memoryStream.WriteByte(byte.MaxValue);
				byte[] bytes2 = BitConverter.GetBytes(GetLength());
				if (BitConverter.IsLittleEndian)
				{
					Array.Reverse(bytes2, 0, bytes2.Length);
				}
				memoryStream.Write(bytes2, 0, bytes2.Length);
			}
			byte[] bytes3 = BitConverter.GetBytes(GetHashCode());
			memoryStream.Write(bytes3, 0, bytes3.Length);
			for (ulong num = GetPos(); num < GetPos() + GetLength(); num++)
			{
				memoryStream.WriteByte((byte)(GetData()[num] ^ bytes3[(num - GetPos()) % 4]));
			}
			return memoryStream.ToArray();
		}
	}
}
