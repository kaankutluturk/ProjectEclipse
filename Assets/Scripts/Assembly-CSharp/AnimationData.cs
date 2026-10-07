using System.Collections.Generic;
using System.Xml;

public static class AnimationData
{
	// best guess for name
	private static Dictionary<string, TemplateAnimation> _TemplatesByName = new Dictionary<string, TemplateAnimation>();

	// best guess for name
	private static readonly Dictionary<string, InfoAnimation> _AnimationsByName = new Dictionary<string, InfoAnimation>();

	// best guess for name
	private static readonly List<InfoAnimation> _Animations = new List<InfoAnimation>();

	private static readonly List<Trick> _Tricks = new List<Trick>();

	private static readonly List<Trigger> _Triggers = new List<Trigger>();

	private static List<string> _WeaponTypeList = new List<string>();

	// best guess for name
	public static List<InfoAnimation> Animations
	{
		get
		{
			return GetAnimations();
		}
	}

	public static int AnimationCount
	{
		get
		{
			return GetAnimationCount();
		}
	}

	public static List<Trick> Tricks
	{
		get
		{
			return GetTricks();
		}
	}

	public static List<Trigger> Triggers
	{
		get
		{
			return GetTriggers();
		}
	}

	public static List<string> WeaponTypes
	{
		get
		{
			return GetWeaponTypes();
		}
	}

	public static List<InfoAnimation> GetAnimations()
	{
		return _Animations;
	}

	public static int GetAnimationCount()
	{
		return _Animations.Count;
	}

	public static List<Trick> GetTricks()
	{
		return _Tricks;
	}

	public static List<Trigger> GetTriggers()
	{
		return _Triggers;
	}

	public static List<string> GetWeaponTypes()
	{
		if (_WeaponTypeList.Count != 0)
		{
			return _WeaponTypeList;
		}
		for (int i = 0; i < _Animations.Count; i++)
		{
			List<string> list = _Animations[i].GetTacticWeapons();
			if (list.Count == 0)
			{
				continue;
			}
			bool flag = true;
			foreach (string item in _WeaponTypeList)
			{
				if (list.IndexOf(item) != -1)
				{
					flag = false;
					break;
				}
			}
			if (!flag)
			{
				continue;
			}
			foreach (string item2 in list)
			{
				_WeaponTypeList.Add(item2);
			}
		}
		return _WeaponTypeList;
	}

	public static void Load(string path, bool isDebug)
	{
		MovesParser.Parse(path, _Animations, _TemplatesByName, _Tricks, _Triggers, isDebug);
		InfoAnimation animation = null;
		for (int i = 0; i < _Animations.Count; i++)
		{
			animation = _Animations[i];
			_AnimationsByName[animation.Name] = animation;
		}
		CreateCapabilityTables();
	}

	internal static int AddExternalMoves(XmlDocument document)
	{
		int before = _Animations.Count;
		int added = MovesParser.ParseAdditional(document, _Animations, _TemplatesByName, _Tricks, _Triggers);
		if (added == 0) return 0;

		List<InfoAnimation> newMoves = _Animations.GetRange(before, added);
		for (int i = 0; i < newMoves.Count; i++)
			_AnimationsByName.Add(newMoves[i].Name, newMoves[i]);

		// Existing lower-priority moves may now transition into newly added moves. Only
		// compare old entries against the new tail to avoid duplicating established tables.
		for (int i = 0; i < before; i++)
			CreateCapabilityTable(_Animations[i], newMoves);
		for (int i = 0; i < newMoves.Count; i++)
			CreateCapabilityTable(newMoves[i], _Animations);
		_WeaponTypeList.Clear();
		return added;
	}

	internal sealed class ExternalMoveReplacementLifetime : System.IDisposable
	{
		internal sealed class Entry
		{
			internal int Index;
			internal InfoAnimation Original;
			internal InfoAnimation Replacement;
		}

		internal readonly List<Entry> Entries = new List<Entry>();
		internal readonly List<System.Action> TemplateUndo = new List<System.Action>();
		private bool _disposed;

		public void Dispose()
		{
			if (_disposed) return;
			_disposed = true;
			for (int i = TemplateUndo.Count - 1; i >= 0; i--) TemplateUndo[i]();
			bool restored = TemplateUndo.Count != 0;
			for (int i = Entries.Count - 1; i >= 0; i--)
			{
				Entry entry = Entries[i];
				if (entry.Index >= _Animations.Count ||
					!ReferenceEquals(_Animations[entry.Index], entry.Replacement)) continue;
				_Animations[entry.Index] = entry.Original;
				if (_AnimationsByName.TryGetValue(entry.Original.Name, out InfoAnimation current) &&
					ReferenceEquals(current, entry.Replacement)) _AnimationsByName[entry.Original.Name] = entry.Original;
				restored = true;
			}
			if (restored) RebuildCapabilityTables();
		}
	}

	internal static ExternalMoveReplacementLifetime ReplaceExternalMoves(XmlDocument document,
		IReadOnlyDictionary<string, string> expectedFiles)
	{
		if (document == null || document["Movesxml"]?["Moves"] == null || expectedFiles == null)
			throw new System.ArgumentException("Native move replacements require moves and expected filenames.");
		if (document.SelectNodes("/Movesxml/Templates/Template").Count != 0 ||
			document.SelectNodes("/Movesxml/Triggers/Trigger").Count != 0)
			throw new System.InvalidOperationException("Native move replacements cannot add templates or triggers.");
		// Parsing appends each replacement to the live template lists it names.
		// Keep a snapshot so a rejected batch leaves template membership unchanged.
		var templateSnapshot = new List<KeyValuePair<List<InfoAnimation>, InfoAnimation[]>>();
		foreach (TemplateAnimation template in _TemplatesByName.Values)
			templateSnapshot.Add(new KeyValuePair<List<InfoAnimation>, InfoAnimation[]>(
				template.GetAnimations(), template.GetAnimations().ToArray()));
		var templateNames = new HashSet<string>(_TemplatesByName.Keys, System.StringComparer.Ordinal);
		var parsed = new List<InfoAnimation>();
		var lifetime = new ExternalMoveReplacementLifetime();
		try
		{
			MovesParser.ParseAdditional(document, parsed, _TemplatesByName, new List<Trick>(), new List<Trigger>());
			if (parsed.Count != expectedFiles.Count)
				throw new System.InvalidOperationException("Native replacement count does not match its guards.");
			var names = new HashSet<string>(System.StringComparer.Ordinal);
			foreach (InfoAnimation replacement in parsed)
			{
				if (!names.Add(replacement.Name) ||
					!expectedFiles.TryGetValue(replacement.Name, out string expectedFile))
					throw new System.InvalidOperationException("Unguarded or duplicate native replacement: " + replacement.Name);
				int index = -1;
				for (int i = 0; i < _Animations.Count; i++)
					if (_Animations[i].Name == replacement.Name)
					{
						if (index >= 0) throw new System.InvalidOperationException("Ambiguous native replacement target: " + replacement.Name);
						index = i;
					}
				if (index < 0 || _Animations[index].FileName != expectedFile ||
					!_AnimationsByName.TryGetValue(replacement.Name, out InfoAnimation mapped) ||
					!ReferenceEquals(mapped, _Animations[index]))
					throw new System.InvalidOperationException("Native replacement expected filename or target mismatch: " + replacement.Name);
				lifetime.Entries.Add(new ExternalMoveReplacementLifetime.Entry {
					Index = index, Original = _Animations[index], Replacement = replacement });
			}
		}
		catch
		{
			foreach (var list in templateSnapshot)
			{
				list.Key.Clear();
				list.Key.AddRange(list.Value);
			}
			foreach (string name in new List<string>(_TemplatesByName.Keys))
				if (!templateNames.Contains(name)) _TemplatesByName.Remove(name);
			throw;
		}
		try
		{
			foreach (var entry in lifetime.Entries)
			{
				_Animations[entry.Index] = entry.Replacement;
				_AnimationsByName[entry.Replacement.Name] = entry.Replacement;
				foreach (TemplateAnimation template in _TemplatesByName.Values)
					SwapTemplateMember(template, entry.Original, entry.Replacement, lifetime);
			}
			RebuildCapabilityTables();
			return lifetime;
		}
		catch
		{
			lifetime.Dispose();
			throw;
		}
	}

	// A replacement takes the original's place in templates it also declares,
	// and in the per-move template named after the move, so template lookups keep
	// their native order. Templates only the original declared drop it.
	private static void SwapTemplateMember(TemplateAnimation template, InfoAnimation original,
		InfoAnimation replacement, ExternalMoveReplacementLifetime lifetime)
	{
		List<InfoAnimation> members = template.GetAnimations();
		int originalIndex = members.IndexOf(original);
		int replacementIndex = members.IndexOf(replacement);
		if (originalIndex < 0)
		{
			if (replacementIndex >= 0)
				lifetime.TemplateUndo.Add(() => members.Remove(replacement));
			return;
		}
		if (replacementIndex >= 0 || template.get_Name() == original.Name)
		{
			if (replacementIndex >= 0) members.RemoveAt(replacementIndex);
			originalIndex = members.IndexOf(original);
			members[originalIndex] = replacement;
			lifetime.TemplateUndo.Add(() =>
			{
				int current = members.IndexOf(replacement);
				if (current >= 0) members[current] = original;
			});
			return;
		}
		members.RemoveAt(originalIndex);
		lifetime.TemplateUndo.Add(() =>
		{
			if (!members.Contains(original)) members.Insert(System.Math.Min(originalIndex, members.Count), original);
		});
	}

	internal static void RebuildCapabilityTables()
	{
		foreach (InfoAnimation move in _Animations)
			move.PriorityConflicts.HigherPriorityMoves.Clear();
		CreateCapabilityTables();
		_WeaponTypeList.Clear();
	}

	public static void ClearAnimations()
	{
		_Animations.Clear();
		_AnimationsByName.Clear();
		InfoAnimation.ClearAnimationCache();
		_Tricks.Clear();
		_TemplatesByName.Clear();
		_Triggers.Clear();
		MovesParser.ClearCaches();
	}

	public static void CreateCapabilityTables()
	{
		// Key-condition discovery walks a move's condition tree and allocates a list.
		// Snapshot it once per move for this pass, rather than once per move pair.
		// This cache is deliberately local: mod patches can replace keys before a rebuild.
		var keys = new List<ConditionKeys>[_Animations.Count];
		for (int i = 0; i < _Animations.Count; i++) keys[i] = _Animations[i].CollectKeyConditions();
		for (int i = 0; i < _Animations.Count; i++)
			CreateCapabilityTable(_Animations[i], keys[i], _Animations, keys);
	}

	public static void CreateCapabilityTable(InfoAnimation animation, List<InfoAnimation> animations)
	{
		List<ConditionKeys> list = animation.CollectKeyConditions();
		if (list.Count == 0) return;
		var keys = new List<ConditionKeys>[animations.Count];
		for (int i = 0; i < animations.Count; i++)
			if (animation.Priority < animations[i].Priority) keys[i] = animations[i].CollectKeyConditions();
		CreateCapabilityTable(animation, list, animations, keys);
	}

	private static void CreateCapabilityTable(InfoAnimation animation, List<ConditionKeys> list,
		List<InfoAnimation> animations, List<ConditionKeys>[] keys)
	{
		int count = list.Count;
		if (0 >= count)
		{
			return;
		}
		for (int candidate = 0; candidate < animations.Count; candidate++)
		{
			InfoAnimation item = animations[candidate];
			if (animation.Priority >= item.Priority)
			{
				continue;
			}
			List<ConditionKeys> list2 = keys[candidate];
			int count2 = list2.Count;
			if (0 >= count2)
			{
				continue;
			}
			bool flag = false;
			foreach (ConditionKeys item2 in list)
			{
				KeyData requiredKeys = item2.RequiredKeys;
				foreach (ConditionKeys item3 in list2)
				{
					KeyData capabilityKey = item3.RequiredKeys;
					if (capabilityKey.IsVariable(requiredKeys))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					break;
				}
			}
			if (flag)
			{
				animation.PriorityConflicts.HigherPriorityMoves.Add(item);
			}
		}
	}

	public static void CollectAvailableAnimations(List<InfoAnimation> animations, List<ItemInfo> items, bool isWeapon = false, List<string> excludedNames = null, SceneTypes sceneType = SceneTypes.SceneFight, List<PerkInfoItem> selfPerks = null, List<PerkInfoItem> otherPerks = null)
	{
		List<ConditionAnimation> list = new List<ConditionAnimation>();
		ModelConditions conditions = new ModelConditions();
		conditions.Items = items;
		conditions.IsWeapon = isWeapon;
		conditions.SceneType = sceneType;
		conditions.SelfPerks = selfPerks;
		conditions.OtherPerks = otherPerks;
		foreach (InfoAnimation lNKJIIGBEDum in _Animations)
		{
			list = lNKJIIGBEDum.MoveData.Locks;
			if (lNKJIIGBEDum.AreConditionsMet(conditions, list) && (excludedNames == null || !lNKJIIGBEDum.CheckAnimationName(excludedNames)))
			{
				animations.Add(lNKJIIGBEDum);
			}
		}
	}

	public static void CollectAvailableTriggers(List<Trigger> triggers, List<ItemInfo> items, bool isWeapon = false, SceneTypes sceneType = SceneTypes.SceneFight, List<PerkInfoItem> selfPerks = null, List<PerkInfoItem> otherPerks = null)
	{
		triggers.Clear();
		List<ConditionAnimation> list = new List<ConditionAnimation>();
		ModelConditions conditions = new ModelConditions();
		conditions.Items = items;
		conditions.IsWeapon = isWeapon;
		conditions.SceneType = sceneType;
		conditions.SelfPerks = selfPerks;
		conditions.OtherPerks = otherPerks;
		foreach (Trigger item in _Triggers)
		{
			list = item.Definition.ExtraConditions;
			if (item.CheckConditions(conditions, list))
			{
				triggers.Add(item);
			}
		}
	}

	public static void AddTemplateAnimationsByNames(List<string> names, List<InfoAnimation> animations)
	{
		for (int i = 0; i < names.Count; i++)
		{
			AddTemplateAnimations(names[i], animations);
		}
	}

	public static void AddTemplateAnimations(string name, List<InfoAnimation> animations)
	{
		if (_TemplatesByName.ContainsKey(name))
		{
			if (animations.Count == 0)
			{
				animations.AddRange(_TemplatesByName[name].GetAnimations());
			}
			else
			{
				animations.AddIfNotExist(_TemplatesByName[name].GetAnimations());
			}
		}
	}

	public static TemplateAnimation GetTemplateByName(string name)
	{
		if (_TemplatesByName.ContainsKey(name))
		{
			return _TemplatesByName[name];
		}
		return null;
	}

	public static InfoAnimation GetStanceAnimation(ItemInfo itemInfo)
	{
		if (itemInfo == null)
		{
			return GetAnimationByName("StanceIdle");
		}
		string item = "Stance";
		string itemName = itemInfo.Name;
		foreach (InfoAnimation lNKJIIGBEDum in _Animations)
		{
			List<string> list = lNKJIIGBEDum.GetTemplateNames();
			List<string> list2 = lNKJIIGBEDum.GetTacticWeapons();
			if (((list2.Count == 0 && string.IsNullOrEmpty(itemName)) || (list2.Count != 0 && list2.IndexOf(itemName) != -1)) && list.IndexOf(item) != -1)
			{
				return lNKJIIGBEDum;
			}
		}
		return GetAnimationByName("StanceIdle");
	}

	public static void CollectAvailableTricks(List<Trick> tricks, List<ItemInfo> items, bool isWeapon = false, List<string> excludedNames = null, List<PerkInfoItem> selfPerks = null, SceneTypes sceneType = SceneTypes.SceneFight)
	{
		List<InfoAnimation> list = new List<InfoAnimation>();
		CollectAvailableAnimations(list, items, isWeapon, excludedNames, sceneType, selfPerks);
		foreach (Trick item in _Tricks)
		{
			foreach (InfoAnimation item2 in list)
			{
				if (item.Animation == item2)
				{
					tricks.Add(item);
				}
			}
		}
	}

	public static InfoAnimation GetAnimationByName(string name, bool logAsError = true)
	{
		InfoAnimation value = null;
		if (_AnimationsByName.TryGetValue(name, out value))
		{
			return value;
		}
		if (logAsError)
		{
			GameLog.Error("Animation " + name + " not found");
		}
		else
		{
			GameLog.Write("Animation " + name + " not found");
		}
		return null;
	}

	public static void CollectPivotNodeNames(List<string> names, List<InfoAnimation> animations = null)
	{
		List<InfoAnimation> list = ((animations != null) ? animations : _Animations);
		foreach (InfoAnimation item in list)
		{
			InfoAnimation.MovePivot alignData = item.MoveData.AlignData;
			if (alignData.PivotObjectType != InfoAnimation.AlignObjectType.ObjectNodes || alignData.PositionModelType != ModelType.ModelTargetType.MODEL_THIS || alignData.PositionObjectType != InfoAnimation.AlignObjectType.ObjectPivot)
			{
				continue;
			}
			string pivotPart = item.MoveData.AlignData.PivotPart;
			if (string.IsNullOrEmpty(pivotPart))
			{
				continue;
			}
			bool flag = true;
			foreach (string item2 in names)
			{
				if (item2 == pivotPart)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				names.Add(pivotPart);
			}
		}
	}
}
