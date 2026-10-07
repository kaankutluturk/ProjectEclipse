using System.Collections.Generic;
using System.IO;
using System.Linq;

public class ModelShiftTable : List<List<float>>
{
	private const string ShiftDirectory = "assets/tactics/shift/";

	private const string FileExtension = ".stb";

	private List<string> _NodeNames = new List<string>();

	private InfoAnimation _Animation;

	public List<string> NodeNames
	{
		get
		{
			return GetNodeNames();
		}
	}

	public InfoAnimation Animation
	{
		get
		{
			return GetAnimation();
		}
	}

	public ModelShiftTable()
	{
		_Animation = null;
	}

	public List<string> GetNodeNames()
	{
		return _NodeNames;
	}

	public InfoAnimation GetAnimation()
	{
		return _Animation;
	}

	public int GetNodeId(string nodeName)
	{
		int num = 0;
		for (int i = 0; i < _NodeNames.Count; i++)
		{
			if (nodeName == _NodeNames[i])
			{
				return num;
			}
			num++;
		}
		if (_Animation != null)
		{
			GameLog.Error("heel {0} not found in shift table for {1}", nodeName, _Animation.Name);
		}
		return -1;
	}

	public void LoadFromFile(InfoAnimation animation, BinaryReader buffer)
	{
		_Animation = animation;
		uint dataSize = buffer.ReadUInt32();
		LoadFromFile(buffer, (int)dataSize);
	}

	private void LoadFromFile(BinaryReader buffer, int dataSize)
	{
		if (0 < dataSize)
		{
			int num = ParseTableHeader(buffer);
			int count = _NodeNames.Count;
			int num2 = dataSize - num;
			if (num2 % 4 != 0)
			{
				GameLog.Error("count % 4 != 0");
			}
			if (num2 / 4 % count != 0)
			{
				GameLog.Error("count % nodeCount != 0");
			}
			int num3 = num2 / 4;
			List<float> list = new List<float>(num3);
			for (int i = 0; i < num3; i++)
			{
				list.Add(buffer.ReadSingle());
			}
			num2 = num3 / count;
			base.Capacity = num2;
			int num4 = 0;
			for (int j = 0; j < base.Count; j++)
			{
				Add(list.GetRange(num4, count));
				num4 += count;
			}
			list = null;
		}
	}

	private int ParseTableHeader(BinaryReader buffer)
	{
		int num = 4;
		uint num2 = buffer.ReadUInt32();
		_NodeNames.Clear();
		for (int i = 0; i < num2; i++)
		{
			string text = TacticalTableHolder.ReadNullTerminatedString(buffer);
			num += text.Length + 1;
			_NodeNames.Add(text);
		}
		return num;
	}

	public float GetDistance(int rowIndex, string nodeName)
	{
		if (rowIndex < base.Count)
		{
			int num = GetNodeId(nodeName);
			if (-1 < num)
			{
				return this.ElementAt(rowIndex)[num];
			}
			GameLog.Error("node {1} not found", nodeName);
		}
		return 0f;
	}
}
