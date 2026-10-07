using System;
using System.Diagnostics;

public class BinaryReaderNekki : IDisposable
{
	private const int BoolSize = 1;

	private const int Int16Size = 2;

	private const int Int32Size = 4;

	private const int Int64Size = 8;

	public readonly int Size;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int _position;

	private byte[] _bytes;

	public int CursorPosition
	{
		get
		{
			return GetPosition();
		}
		private set
		{
			set_Position(value);
		}
	}

	public BinaryReaderNekki(byte[] data)
	{
		_bytes = data;
		Size = _bytes.Length;
	}

	public int GetPosition()
	{
		return _position;
	}

	private void set_Position(int value)
	{
		_position = value;
	}

	public virtual bool ReadBoolean()
	{
		bool result = BitConverter.ToBoolean(_bytes, GetPosition());
		SetPosition(1);
		return result;
	}

	public virtual byte ReadByte()
	{
		byte result = _bytes[GetPosition()];
		SetPosition(1);
		return result;
	}

	public virtual short ReadInt16()
	{
		short result = BitConverter.ToInt16(_bytes, GetPosition());
		SetPosition(2);
		return result;
	}

	public virtual int ReadInt32()
	{
		int result = BitConverter.ToInt32(_bytes, GetPosition());
		SetPosition(4);
		return result;
	}

	public virtual long ReadInt64()
	{
		long result = BitConverter.ToInt64(_bytes, GetPosition());
		SetPosition(8);
		return result;
	}

	public virtual float ReadSingle()
	{
		float result = BitConverter.ToSingle(_bytes, GetPosition());
		SetPosition(4);
		return result;
	}

	public float[] ConvertByteArrayToFloat(int count)
	{
		int num = count * 4;
		float[] array = new float[count];
		Buffer.BlockCopy(_bytes, GetPosition(), array, 0, num);
		SetPosition(num);
		return array;
	}

	private void SetPosition(int value)
	{
		set_Position(GetPosition() + value);
	}

	public void Dispose()
	{
		_bytes = null;
	}
}
