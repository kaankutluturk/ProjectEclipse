using System.IO;

public class OutWindow
{
	private byte[] _buffer;

	private uint _pos;

	private uint _windowSize;

	private uint _streamPos;

	private Stream _stream;

	public uint TrainSize;

	public void Create(uint AKOEOKJFINO)
	{
		if (_windowSize != AKOEOKJFINO)
		{
			_buffer = new byte[AKOEOKJFINO];
		}
		_windowSize = AKOEOKJFINO;
		_pos = 0u;
		_streamPos = 0u;
	}

	public void Init(Stream ABJIEFMMIEK, bool POOADOMADDK)
	{
		ReleaseStream();
		_stream = ABJIEFMMIEK;
		if (!POOADOMADDK)
		{
			_streamPos = 0u;
			_pos = 0u;
			TrainSize = 0u;
		}
	}

	public bool Train(Stream ABJIEFMMIEK)
	{
		long length = ABJIEFMMIEK.Length;
		uint num = (TrainSize = (uint)((length >= _windowSize) ? _windowSize : length));
		ABJIEFMMIEK.Position = length - num;
		_streamPos = (_pos = 0u);
		while (num != 0)
		{
			uint num2 = _windowSize - _pos;
			if (num < num2)
			{
				num2 = num;
			}
			int num3 = ABJIEFMMIEK.Read(_buffer, (int)_pos, (int)num2);
			if (num3 == 0)
			{
				return false;
			}
			num -= (uint)num3;
			_pos += (uint)num3;
			_streamPos += (uint)num3;
			if (_pos == _windowSize)
			{
				_streamPos = (_pos = 0u);
			}
		}
		return true;
	}

	public void ReleaseStream()
	{
		Flush();
		_stream = null;
	}

	public void Flush()
	{
		uint num = _pos - _streamPos;
		if (num != 0)
		{
			_stream.Write(_buffer, (int)_streamPos, (int)num);
			if (_pos >= _windowSize)
			{
				_pos = 0u;
			}
			_streamPos = _pos;
		}
	}

	public void CopyBlock(uint OIOMNNFMDOO, uint JCAJDBOMGOM)
	{
		uint num = _pos - OIOMNNFMDOO - 1;
		if (num >= _windowSize)
		{
			num += _windowSize;
		}
		while (JCAJDBOMGOM != 0)
		{
			if (num >= _windowSize)
			{
				num = 0u;
			}
			_buffer[_pos++] = _buffer[num++];
			if (_pos >= _windowSize)
			{
				Flush();
			}
			JCAJDBOMGOM--;
		}
	}

	public void PutByte(byte AAOIAEJJINO)
	{
		_buffer[_pos++] = AAOIAEJJINO;
		if (_pos >= _windowSize)
		{
			Flush();
		}
	}

	public byte GetByte(uint OIOMNNFMDOO)
	{
		uint num = _pos - OIOMNNFMDOO - 1;
		if (num >= _windowSize)
		{
			num += _windowSize;
		}
		return _buffer[num];
	}
}
