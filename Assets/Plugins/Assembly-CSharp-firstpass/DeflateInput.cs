internal class DeflateInput
{
	internal struct InputState
	{
		internal int count;

		internal int startIndex;
	}

	private byte[] buffer;

	private int count;

	private int startIndex;

	internal int Count
	{
		get
		{
			return GetCount();
		}
		set
		{
			SetCount(value);
		}
	}

	internal int StartIndex
	{
		get
		{
			return GetStartIndex();
		}
		set
		{
			SetStartIndex(value);
		}
	}

	internal byte[] GetBuffer()
	{
		return buffer;
	}

	internal void set_Buffer(byte[] value)
	{
		buffer = value;
	}

	internal int GetCount()
	{
		return count;
	}

	internal void SetCount(int value)
	{
		count = value;
	}

	internal int GetStartIndex()
	{
		return startIndex;
	}

	internal void SetStartIndex(int value)
	{
		startIndex = value;
	}

	internal void ConsumeBytes(int HDKKKCDKFEE)
	{
		startIndex += HDKKKCDKFEE;
		count -= HDKKKCDKFEE;
	}

	internal InputState DumpState()
	{
		InputState result = default(InputState);
		result.count = count;
		result.startIndex = startIndex;
		return result;
	}

	internal void RestoreState(InputState state)
	{
		count = state.count;
		startIndex = state.startIndex;
	}
}
