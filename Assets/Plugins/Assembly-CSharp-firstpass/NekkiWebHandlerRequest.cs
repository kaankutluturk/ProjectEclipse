using System.Collections.Generic;

public class NekkiWebHandlerRequest : NekkiWebHandler
{
	private readonly List<byte> _buffer;

	public NekkiWebHandlerRequest(NekkiUri uri)
		: base(uri)
	{
		_buffer = new List<byte>();
	}

	protected override void OnDataReceived(byte[] data, int offset, int length)
	{
		_buffer.Capacity += length;
		for (int i = 0; i < length; i++)
		{
			_buffer.Add(data[i]);
		}
	}

	protected override byte[] GetData()
	{
		return _buffer.ToArray();
	}
}
