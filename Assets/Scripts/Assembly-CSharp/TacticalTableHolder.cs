using System.Collections.Generic;
using System.IO;

public class TacticalTableHolder
{
	private class AnimationTablesForAnimation
	{
		public InfoAnimation Animation;

		public List<GroupTables> Container = new List<GroupTables>();
	}

	private List<GroupTables> _groups = new List<GroupTables>();

	private List<IntervalNew> _animationsWithIntervals;

	private List<TacticalTable> _tables;

	private List<Intervals> _intervals;

	private List<float> _distances;

	private List<int> _interframes;

	private List<AnimationTablesForAnimation> _animationTables;

	private int _tableIndex;

	private string _weaponName;

	public void Load(byte[] buffer, int tableIndex, string weaponName)
	{
		int num = buffer.Length;
		if (0 >= num)
		{
			return;
		}
		List<InfoAnimation> list = new List<InfoAnimation>();
		List<string> list2 = new List<string>();
		using (MemoryStream input = new MemoryStream(buffer))
		{
			using (BinaryReader binaryReader = new BinaryReader(input))
			{
				ReadAnimationList(binaryReader, list);
				ReadWeaponTypeList(binaryReader, list2);
				ReadTables(binaryReader, list, list2, tableIndex, weaponName);
				if (binaryReader.BaseStream.Length != binaryReader.BaseStream.Position)
				{
					GameLog.Error("pointer != buffer.end()");
				}
			}
		}
	}

	public void RegisterTables()
	{
		foreach (AnimationTablesForAnimation item in _animationTables)
		{
			item.Animation.GetTacticGroupTables()[_tableIndex].Add(new global::Pair<List<GroupTables>, string>(item.Container, _weaponName));
		}
	}

	public void Clear()
	{
		_animationTables.Clear();
		_groups.Clear();
		_tables.Clear();
		_intervals.Clear();
		_distances.Clear();
		_interframes.Clear();
	}

	public bool Empty()
	{
		if (_groups.Count == 0 || _tables.Count == 0 || _intervals.Count == 0 || _distances.Count == 0)
		{
			GameLog.Write("Empty tables detected!");
			return true;
		}
		return false;
	}

	private static void ReadAnimationList(BinaryReader reader, List<InfoAnimation> animations)
	{
		int num = (animations.Capacity = reader.ReadUInt16());
		byte[] array = reader.ReadBytes(num);
		for (int i = 0; i < num; i++)
		{
			byte count = array[i];
			string animationName = new string(reader.ReadChars(count));
			animations.Add(AnimationData.GetAnimationByName(animationName, AiData.get_IsShowErrorIfAnimationNotFound()));
		}
	}

	private static void ReadWeaponTypeList(BinaryReader reader, List<string> weaponTypes)
	{
		short num = reader.ReadInt16();
		byte[] array = reader.ReadBytes(num);
		for (int i = 0; i < num; i++)
		{
			byte count = array[i];
			weaponTypes.Add(new string(reader.ReadChars(count)));
		}
	}

	private void ReadTables(BinaryReader reader, List<InfoAnimation> animations, List<string> weaponTypes, int tableIndex, string weaponName)
	{
		_tableIndex = tableIndex;
		_weaponName = weaponName;
		int num = (int)reader.ReadUInt32();
		int num2 = (int)reader.ReadUInt32();
		_groups = new List<GroupTables>(num2);
		for (int i = 0; i < num2; i++)
		{
			_groups.Add(new GroupTables());
		}
		int num3 = (int)reader.ReadUInt32();
		_tables = new List<TacticalTable>(num3);
		for (int j = 0; j < num3; j++)
		{
			_tables.Add(new TacticalTable());
		}
		int num4 = (int)reader.ReadUInt32();
		_intervals = new List<Intervals>(num4);
		for (int k = 0; k < num4; k++)
		{
			_intervals.Add(new Intervals());
		}
		int num5 = (int)reader.ReadUInt32();
		_animationsWithIntervals = new List<IntervalNew>(num5);
		for (int l = 0; l < num5; l++)
		{
			_animationsWithIntervals.Add(new IntervalNew());
		}
		int num6 = (int)reader.ReadUInt32();
		int num7 = (int)reader.ReadUInt32();
		_animationTables = new List<AnimationTablesForAnimation>(num);
		for (int m = 0; m < num; m++)
		{
			ushort num8 = reader.ReadUInt16();
			_animationTables.Add(new AnimationTablesForAnimation());
			if (num8 < animations.Count)
			{
				_animationTables[m].Animation = animations[num8];
			}
			else
			{
				_animationTables[m].Animation = null;
			}
		}
		short num9 = reader.ReadInt16();
		_distances = new List<float>(num6);
		for (int n = 0; n < num6; n++)
		{
			_distances.Add((num9 == 0) ? reader.ReadInt16() : (num9 * reader.ReadInt16()));
		}
		_interframes = new List<int>(num7);
		for (int num10 = 0; num10 < num7; num10++)
		{
			_interframes.Add(reader.ReadInt32());
		}
		List<InfoAnimation> list = new List<InfoAnimation>(num5);
		int num11 = 0;
		int num12 = 0;
		for (int num13 = num5; num12 < num13; num12++)
		{
			list.Add(animations[reader.ReadUInt16()]);
			if (list[num12] == null)
			{
				num11++;
			}
		}
		List<InfoAnimation> list2 = new List<InfoAnimation>(num11);
		for (int num14 = 0; num14 < num11; num14++)
		{
			list2.Add(new InfoAnimation());
		}
		int num15 = 0;
		int num16 = 0;
		int num17 = 0;
		int num18 = 0;
		int num19 = 0;
		int num20 = 0;
		int num21 = 0;
		foreach (AnimationTablesForAnimation item in _animationTables)
		{
			ushort num22 = reader.ReadUInt16();
			if (item.Animation == null)
			{
				item.Animation = new InfoAnimation();
				list2.Add(item.Animation);
			}
			item.Container = _groups.GetRange(num15, num22);
			num15 += num22;
			foreach (GroupTables item2 in item.Container)
			{
				ushort index = reader.ReadUInt16();
				item2.GroupLabel = weaponTypes[index];
				num22 = reader.ReadUInt16();
				item2.Tables = _tables.GetRange(num16, num22);
				num16 += num22;
				foreach (TacticalTable item3 in item2.Tables)
				{
					item3.Label = ReadNullTerminatedString(reader);
					num22 = reader.ReadUInt16();
					if (0 < num22)
					{
						item3.IntervalList = _intervals.GetRange(num17, num22);
						num17 += num22;
						item3.FirstFrameIndex = reader.ReadInt16();
						foreach (Intervals item4 in item3.IntervalList)
						{
							ushort num23 = reader.ReadUInt16();
							item4.Items = _animationsWithIntervals.GetRange(num21, num23);
							num21 += num23;
							if (0 >= num23)
							{
								continue;
							}
							foreach (IntervalNew item5 in item4.Items)
							{
								item5.Animation = list[num19];
								num19++;
							}
							foreach (IntervalNew item6 in item4.Items)
							{
								ushort num24 = reader.ReadUInt16();
								item6.Distances = _distances.GetRange(num18, num24);
								num18 += num24;
							}
							foreach (IntervalNew item7 in item4.Items)
							{
								ushort num25 = reader.ReadUInt16();
								item7.Interframes = _interframes.GetRange(num20, num25);
								num20 += num25;
							}
						}
					}
					else
					{
						item3.IntervalList.Clear();
						item3.FirstFrameIndex = 0;
					}
				}
			}
		}
		if (num15 != _groups.Count)
		{
			GameLog.Error("pGroups != _groups.end()");
		}
		if (num16 != _tables.Count)
		{
			GameLog.Error("pTables != _tables.end()");
		}
		if (num17 != _intervals.Count)
		{
			GameLog.Error("pFrames != _intervals.end()");
		}
		if (num18 != _distances.Count)
		{
			GameLog.Error("pDistances != _distances.end()");
		}
		if (num19 != list.Count)
		{
			GameLog.Error("pAnimations != _animations.end()");
		}
		if (num20 != _interframes.Count)
		{
			GameLog.Error("pInterframes != _interframes.end()");
		}
		if (num21 != _animationsWithIntervals.Count)
		{
			GameLog.Error("pAnimIntervals != _animationsWithIntervals.end()");
		}
		foreach (InfoAnimation item8 in list2)
		{
			foreach (AnimationTablesForAnimation item9 in _animationTables)
			{
				if (item9.Animation == item8)
				{
					_animationTables.Remove(item9);
					GameLog.Write(string.Format("AnimationTablesForAnimation remove for %s", item8.Name));
					break;
				}
			}
		}
	}

	public static string ReadNullTerminatedString(BinaryReader reader)
	{
		string text = string.Empty;
		for (char c = reader.ReadChar(); c != 0; c = reader.ReadChar())
		{
			text += c;
		}
		return text;
	}
}
