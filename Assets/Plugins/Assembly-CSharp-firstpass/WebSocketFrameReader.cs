using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

public sealed class WebSocketFrameReader
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool isFinal;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private WebSocketFrameTypes type;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool hasMask;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private ulong length;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private byte[] mask;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private byte[] data;

	public bool FinalFragment
	{
		get
		{
			return GetIsFinal();
		}
		private set
		{
			set_IsFinal(value);
		}
	}

	public bool HasMask
	{
		get
		{
			return GetHasMask();
		}
		private set
		{
			SetHasMask(value);
		}
	}

	public ulong DataLength
	{
		get
		{
			return GetLength();
		}
		private set
		{
			set_Length(value);
		}
	}

	public byte[] Mask
	{
		get
		{
			return GetMask();
		}
		private set
		{
			SetMask(value);
		}
	}

	public bool GetIsFinal()
	{
		return isFinal;
	}

	private void set_IsFinal(bool value)
	{
		isFinal = value;
	}

	public WebSocketFrameTypes get_Type()
	{
		return type;
	}

	private void set_Type(WebSocketFrameTypes value)
	{
		type = value;
	}

	public bool GetHasMask()
	{
		return hasMask;
	}

	private void SetHasMask(bool value)
	{
		hasMask = value;
	}

	public ulong GetLength()
	{
		return length;
	}

	private void set_Length(ulong value)
	{
		length = value;
	}

	public byte[] GetMask()
	{
		return mask;
	}

	private void SetMask(byte[] value)
	{
		mask = value;
	}

	public byte[] GetData()
	{
		return data;
	}

	private void set_Data(byte[] value)
	{
		data = value;
	}

	internal void Read(Stream ABJIEFMMIEK)
	{
		byte b = (byte)ABJIEFMMIEK.ReadByte();
		set_IsFinal((b & 0x80) != 0);
		set_Type((WebSocketFrameTypes)(b & 0xF));
		b = (byte)ABJIEFMMIEK.ReadByte();
		SetHasMask((b & 0x80) != 0);
		set_Length((ulong)(b & 0x7F));
		if (GetLength() == 126)
		{
			byte[] array = new byte[2];
			ABJIEFMMIEK.ReadBuffer(array);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(array, 0, array.Length);
			}
			set_Length(BitConverter.ToUInt16(array, 0));
		}
		else if (GetLength() == 127)
		{
			byte[] array2 = new byte[8];
			ABJIEFMMIEK.ReadBuffer(array2);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(array2, 0, array2.Length);
			}
			set_Length(BitConverter.ToUInt64(array2, 0));
		}
		if (GetHasMask())
		{
			SetMask(new byte[4]);
			ABJIEFMMIEK.Read(GetMask(), 0, 4);
		}
		set_Data(new byte[GetLength()]);
		if (GetLength() == 0)
		{
			return;
		}
		int num = 0;
		do
		{
			num += ABJIEFMMIEK.Read(GetData(), num, GetData().Length - num);
		}
		while (num < GetData().Length);
		if (GetHasMask())
		{
			for (int i = 0; i < GetData().Length; i++)
			{
				GetData()[i] = (byte)(GetData()[i] ^ GetMask()[i % 4]);
			}
		}
	}

	internal void Assemble(List<WebSocketFrameReader> DAGGODDBKDD)
	{
		DAGGODDBKDD.Add(this);
		ulong num = 0uL;
		for (int i = 0; i < DAGGODDBKDD.Count; i++)
		{
			num += DAGGODDBKDD[i].GetLength();
		}
		byte[] array = new byte[num];
		ulong num2 = 0uL;
		for (int j = 0; j < DAGGODDBKDD.Count; j++)
		{
			Array.Copy(DAGGODDBKDD[j].GetData(), 0, array, (int)num2, (int)DAGGODDBKDD[j].GetLength());
			num2 += DAGGODDBKDD[j].GetLength();
		}
		set_Type(DAGGODDBKDD[0].get_Type());
		set_Length(num);
		set_Data(array);
	}
}
