using System.Collections.Generic;
using System.Xml;

public class AiData
{
	public enum TableType
	{
		randomAnimation = -2,
		noneTable = -1,
		outcometablesforattack = 0,
		movementsTable = 1,
		dodgeTable = 2,
		attackTable = 3,
		summaryResultTable = 4,
		safeTable = 5,
		quickAttact = 6,
		shiftTable = 7,
		throwTactics = 8,
		evadeList = 9,
		block = 10
	}

	public static bool TacticsEnabled = false;

	private static List<List<global::Pair<TacticalTableHolder, global::Pair<string, string>>>> tableHolders = null;

	private static string[] TacticsTableNames;

	private static List<Tactic> tactics = new List<Tactic>();

	private static HashSet<string> EclipseExternalTactics = new HashSet<string>();

	private static Tactic defaultTactic;

	public static int MovementsStep;

	public static int MovementsTableStep;

	public static List<int> AttackTablesFrames = new List<int>();

	public const int TableHolderGroupCount = 3;

	private static bool bothBotEnabled;

	private static List<global::Pair<string, List<string>>> itemEquivalents = new List<global::Pair<string, List<string>>>();

	private static List<string> noDecisionIntervals = new List<string>();

	private static List<string> noDecisionMoves = new List<string>();

	private static List<string> unexpectedMoves = new List<string>();

	private static List<string> movesFirstIteration = new List<string>();

	private static List<string> movesLastIteration = new List<string>();

	private static List<string> missilesFirstIteration = new List<string>();

	private static List<string> missilesLastIteration = new List<string>();

	private static List<string> moveLengthIntervalsStrict = new List<string>();

	private static List<string> moveLengthIntervalsExtended = new List<string>();

	private static List<string> ignoredEnemyAnimations = new List<string>();

	private static List<string> safeDodgesAnimations = new List<string>();

	private static List<string> evadeUnsafeDodgesAnimations = new List<string>();

	private static List<string> attackMoves = new List<string>();

	private static List<string> throwableIntervals = new List<string>();

	private static List<string> throws = new List<string>();

	private static List<TemplateAnimation> emergencyDodgesAnimations = new List<TemplateAnimation>();

	private static List<TemplateAnimation> cautiousMovements = new List<TemplateAnimation>();

	private static List<TemplateAnimation> evadeThrowDodges = new List<TemplateAnimation>();

	private static List<TemplateAnimation> randomizingEnemyAnimation = new List<TemplateAnimation>();

	private static List<TemplateAnimation> missileAnimations = new List<TemplateAnimation>();

	private static List<TemplateAnimation> magicAnimations = new List<TemplateAnimation>();

	private static List<string> loadedSubtypes = new List<string>();

	private static string _DistanceNode;

	private static bool showErrorIfAnimationNotFound;

	public static List<List<global::Pair<TacticalTableHolder, global::Pair<string, string>>>> TableHolderGroups
	{
		get
		{
			return get_TablesHoldersNew();
		}
	}

	public static List<Tactic> TacticParameters
	{
		get
		{
			return get_Parameters();
		}
	}

	public static bool IsBothBotEnabled
	{
		get
		{
			return get_BothBotEnabled();
		}
	}

	public static List<string> NoDecisionIntervalList
	{
		get
		{
			return get_NoDecisionIntervals();
		}
	}

	public static List<string> NoDecisionMoveList
	{
		get
		{
			return get_NoDecisionMoves();
		}
	}

	public static List<string> UnexpectedMoveList
	{
		get
		{
			return get_UnexpectedMoves();
		}
	}

	public static List<string> MovesFirstIterationList
	{
		get
		{
			return get_MovesFirstIteration();
		}
	}

	public static List<string> MovesLastIterationList
	{
		get
		{
			return get_MovesLastIteration();
		}
	}

	public static List<string> MissilesFirstIterationList
	{
		get
		{
			return get_MissilesFirstIteration();
		}
	}

	public static List<string> MissilesLastIterationList
	{
		get
		{
			return get_MissilesLastIteration();
		}
	}

	public static List<string> MoveLengthIntervalsStrictList
	{
		get
		{
			return get_MoveLengthIntervalsStrict();
		}
	}

	public static List<string> MoveLengthIntervalsExtendedList
	{
		get
		{
			return get_MoveLengthIntervalsExtended();
		}
	}

	public static List<string> IgnoredEnemyAnimationList
	{
		get
		{
			return get_IgnoredEnemyAnimations();
		}
	}

	public static List<string> SafeDodgeAnimationList
	{
		get
		{
			return get_SafeDodgesAnimations();
		}
	}

	public static List<string> EvadeUnsafeDodgeAnimationList
	{
		get
		{
			return get_EvadeUnsafeDodgesAnimations();
		}
	}

	public static List<string> AttackMoveList
	{
		get
		{
			return get_AttackMoves();
		}
	}

	public static List<string> ThrowableIntervalList
	{
		get
		{
			return get_ThrowableIntervals();
		}
	}

	public static List<string> ThrowList
	{
		get
		{
			return get_Throws();
		}
	}

	public static List<TemplateAnimation> EmergencyDodgeAnimationList
	{
		get
		{
			return get_EmergencyDodgesAnimations();
		}
	}

	public static List<TemplateAnimation> CautiousMovementList
	{
		get
		{
			return get_CautiousMovements();
		}
	}

	public static List<TemplateAnimation> EvadeThrowDodgeList
	{
		get
		{
			return get_EvadeThrowDodges();
		}
	}

	public static List<TemplateAnimation> RandomizingEnemyAnimationList
	{
		get
		{
			return get_RandomizingEnemyAnimation();
		}
	}

	public static List<TemplateAnimation> MissileAnimationList
	{
		get
		{
			return get_MissileAnimations();
		}
	}

	public static List<TemplateAnimation> MagicAnimationList
	{
		get
		{
			return get_MagicAnimations();
		}
	}

	public static string DistanceNodeName
	{
		get
		{
			return get_DistanceNode();
		}
	}

	public static bool ShowAnimationNotFoundErrors
	{
		get
		{
			return get_IsShowErrorIfAnimationNotFound();
		}
	}

	public static List<List<global::Pair<TacticalTableHolder, global::Pair<string, string>>>> get_TablesHoldersNew()
	{
		if (tableHolders == null)
		{
			tableHolders = new List<List<global::Pair<TacticalTableHolder, global::Pair<string, string>>>>();
			for (int i = 0; i < 3; i++)
			{
				tableHolders.Add(new List<global::Pair<TacticalTableHolder, global::Pair<string, string>>>());
			}
		}
		return tableHolders;
	}

	public static List<Tactic> get_Parameters()
	{
		return tactics;
	}

	public static void Load()
	{
		if (TacticsEnabled)
		{
			GameLog.Write("loadGame - loading tactics");
		}
		RefreshParameters();
	}

	public static void Load(List<string> ODODFFKBOEG, List<string> DKDIKAHDOBF)
	{
		if (!TacticsEnabled)
		{
			return;
		}
		RemoveDuplicateSubtypes(ODODFFKBOEG);
		string jIIFFJAJNNN = GameUtils.ShockSettings.WeaponName;
		ODODFFKBOEG.AddIfNotExist(jIIFFJAJNNN);
		ODODFFKBOEG.AddIfNotExist(string.Empty);
		if (DKDIKAHDOBF.Contains(jIIFFJAJNNN))
		{
			DKDIKAHDOBF.Remove(jIIFFJAJNNN);
		}
		List<string> list = new List<string>();
		list.Add(string.Empty);
		list.Add(GameUtils.ShockSettings.WeaponName);
		RemoveExept(list);
		loadedSubtypes.Clear();
		GameLog.Write("weaponsSubtypes");
		foreach (string item in ODODFFKBOEG)
		{
			GameLog.Write(item);
		}
		foreach (string item2 in ODODFFKBOEG)
		{
			if (loadedSubtypes.Contains(item2))
			{
				continue;
			}
			Loadfor(item2, ODODFFKBOEG);
			foreach (string item3 in ODODFFKBOEG)
			{
				if (item2 == item3 && DKDIKAHDOBF.Contains(item2))
				{
					GameLog.Write("Skipped load tactics: {0} - {1}", item2, item3);
				}
				else
				{
					LoadMovementsTablefor(item2, item3);
				}
			}
		}
		GameLog.Write("Old tactic:");
		foreach (string item4 in loadedSubtypes)
		{
			GameLog.Write("  - {0}", item4);
		}
		GameLog.Write("New tactic:");
		foreach (string item5 in ODODFFKBOEG)
		{
			GameLog.Write("  - {0}", item5);
		}
		loadedSubtypes.Clear();
		loadedSubtypes.AddIfNotExist(ODODFFKBOEG);
		GameLog.Write("Available tables:");
		GameLog.Write("  outcometablesforattack:");
		List<global::Pair<TacticalTableHolder, global::Pair<string, string>>> list2 = get_TablesHoldersNew()[0];
		foreach (global::Pair<TacticalTableHolder, global::Pair<string, string>> item6 in list2)
		{
			item6.First.RegisterTables();
			if (!item6.First.Empty())
			{
				GameLog.Write("    - {0}/{1}", item6.Second.First, item6.Second.Second);
			}
			else
			{
				GameLog.Write("outcometablesforattack = {0}/{1} *empty*", item6.Second.First, item6.Second.Second);
			}
		}
		GameLog.Write("  movementsTable:");
		List<global::Pair<TacticalTableHolder, global::Pair<string, string>>> list3 = get_TablesHoldersNew()[1];
		foreach (global::Pair<TacticalTableHolder, global::Pair<string, string>> item7 in list3)
		{
			item7.First.RegisterTables();
			if (!item7.First.Empty())
			{
				GameLog.Write("    - {0}/{1}", item7.Second.First, item7.Second.Second);
			}
			else
			{
				GameLog.Write("movementsTable = {0}/{1} *empty*", item7.Second.First, item7.Second.Second);
			}
		}
		GameLog.Write("  dodgeTable:");
		List<global::Pair<TacticalTableHolder, global::Pair<string, string>>> list4 = get_TablesHoldersNew()[2];
		foreach (global::Pair<TacticalTableHolder, global::Pair<string, string>> item8 in list4)
		{
			item8.First.RegisterTables();
			if (!item8.First.Empty())
			{
				GameLog.Write("    - {0}", item8.Second.First);
			}
			else
			{
				GameLog.Write("dodgeTable = {0} *empty*", item8.Second.First);
			}
		}
	}

	public static void ClearAll()
	{
		ClearTables();
		AttackTablesFrames.Clear();
		ClearAllTacticSettings();
	}

	public static void ClearAllTacticSettings()
	{
		itemEquivalents.Clear();
		tactics.Clear();
		noDecisionIntervals.Clear();
		noDecisionMoves.Clear();
		unexpectedMoves.Clear();
		movesFirstIteration.Clear();
		movesLastIteration.Clear();
		moveLengthIntervalsStrict.Clear();
		moveLengthIntervalsExtended.Clear();
		ignoredEnemyAnimations.Clear();
		safeDodgesAnimations.Clear();
		evadeUnsafeDodgesAnimations.Clear();
		attackMoves.Clear();
		throwableIntervals.Clear();
		throws.Clear();
		emergencyDodgesAnimations.Clear();
		cautiousMovements.Clear();
		evadeThrowDodges.Clear();
		randomizingEnemyAnimation.Clear();
		missileAnimations.Clear();
		magicAnimations.Clear();
		EclipseExternalTactics.Clear();
	}

	public static void ClearTables()
	{
		List<string> lCIGOHHEDGK = new List<string>();
		RemoveExept(lCIGOHHEDGK);
		loadedSubtypes.Clear();
	}

	public static string GetTacticsTableName(TableType LFLGCDNKNJI)
	{
		switch (LFLGCDNKNJI)
		{
		case TableType.randomAnimation:
			return "RandomAnimation";
		case TableType.noneTable:
			return "NoneTable";
		case TableType.outcometablesforattack:
			return "AttackTable";
		case TableType.movementsTable:
			return "MovementsTable";
		case TableType.dodgeTable:
			return "DodgeTable";
		case TableType.attackTable:
			return "AttackTableOld";
		case TableType.summaryResultTable:
			return "SummaryResultTable";
		case TableType.safeTable:
			return "CautiousMovements";
		case TableType.quickAttact:
			return "QuickAttack";
		case TableType.shiftTable:
			return "ShiftTable";
		case TableType.throwTactics:
			return "ThrowTactics";
		case TableType.evadeList:
			return "EvadeThrowDodges";
		case TableType.block:
			return "Block";
		default:
			return "??????";
		}
	}

	public static bool get_BothBotEnabled()
	{
		return bothBotEnabled;
	}

	public static void ParseAnimationList(XmlNode ABPANOKOIEF, List<TemplateAnimation> BMMCGJDICOJ)
	{
		if (ABPANOKOIEF == null)
		{
			return;
		}
		foreach (XmlNode childNode in ABPANOKOIEF.ChildNodes)
		{
			if (childNode.Name == "Animation")
			{
				string gOHIIMFFFJI = childNode.Attributes["Name"].GetStringOrDefault();
				TemplateAnimation bHIDAHDCPHM = AnimationData.GetTemplateByName(gOHIIMFFFJI);
				if (bHIDAHDCPHM != null)
				{
					BMMCGJDICOJ.Add(bHIDAHDCPHM);
				}
			}
		}
	}

	public static void ParseStringList(XmlNode ABPANOKOIEF, List<string> CHLCLGKFLPP, string CEELFMIPAII = "Animation")
	{
		if (ABPANOKOIEF == null)
		{
			return;
		}
		foreach (XmlNode childNode in ABPANOKOIEF.ChildNodes)
		{
			if (childNode.Name == CEELFMIPAII)
			{
				string item = childNode.Attributes["Name"].GetStringOrDefault();
				CHLCLGKFLPP.Add(item);
			}
		}
	}

	private static void RefreshParameters()
	{
		string lOBFDOKFJIP = DirectoryController.ResolvePath("tacticSettings.xml");
		string lOBFDOKFJIP2 = DirectoryController.ResolvePath("ComputerSettings.xml");
		XmlDocument xmlDocument = XmlUtils.OpenXMLDocument(SF2Paths.GetGameDataPath(), lOBFDOKFJIP);
		if (xmlDocument != null)
		{
			TacticsCompiler.CompileTacticsSettings(xmlDocument);
		}
		if (xmlDocument != null)
		{
			RefreshTacticSetParameters(xmlDocument);
		}
		xmlDocument = XmlUtils.OpenXMLDocument(SF2Paths.GetGameDataPath(), lOBFDOKFJIP2);
		if (xmlDocument == null)
		{
			return;
		}
		XmlNode xmlNode = xmlDocument["Settings"]["TablesReduction"];
		MovementsStep = xmlNode["MovementsTables"].Attributes["Step"].ParseInt(1);
		MovementsTableStep = xmlNode["MovementsTables"].Attributes["Step"].ParseInt(1);
		string text = xmlNode["AttackTables"].Attributes["Frames"].GetStringOrDefault(string.Empty);
		string[] array = text.Split('|');
		int num = array.Length;
		if (0 < num)
		{
			string[] array2 = array;
			foreach (string iGGFGLLIGCG in array2)
			{
				AttackTablesFrames.Add(iGGFGLLIGCG.ToInt());
			}
		}
		xmlNode = xmlDocument["Settings"]["MovementsTables"]["MovementsMainIterations"];
		foreach (XmlNode childNode in xmlNode.ChildNodes)
		{
			if (childNode.Name == "Animation")
			{
				string item = childNode.Attributes["Name"].GetStringOrDefault(string.Empty);
				movesFirstIteration.Add(item);
			}
		}
		xmlNode = xmlDocument["Settings"]["MovementsTables"]["MovementsLastIteration"];
		foreach (XmlNode childNode2 in xmlNode.ChildNodes)
		{
			if (childNode2.Name == "Animation")
			{
				string item2 = childNode2.Attributes["Name"].GetStringOrDefault(string.Empty);
				movesLastIteration.Add(item2);
			}
		}
		xmlNode = xmlDocument["Settings"]["MissileTables"]["MovementsMainIterations"];
		foreach (XmlNode childNode3 in xmlNode.ChildNodes)
		{
			if (childNode3.Name == "Animation")
			{
				string item3 = childNode3.Attributes["Name"].GetStringOrDefault(string.Empty);
				missilesFirstIteration.Add(item3);
			}
		}
		xmlNode = xmlDocument["Settings"]["MissileTables"]["MovementsLastIteration"];
		foreach (XmlNode childNode4 in xmlNode.ChildNodes)
		{
			if (childNode4.Name == "Animation")
			{
				string item4 = childNode4.Attributes["Name"].GetStringOrDefault(string.Empty);
				missilesLastIteration.Add(item4);
			}
		}
		xmlNode = xmlDocument["Settings"]["MoveLengthIntervals"]["Strict"];
		foreach (XmlNode childNode5 in xmlNode.ChildNodes)
		{
			if (childNode5.Name == "Interval")
			{
				string item5 = childNode5.Attributes["Name"].GetStringOrDefault(string.Empty);
				moveLengthIntervalsStrict.Add(item5);
			}
		}
		xmlNode = xmlDocument["Settings"]["MoveLengthIntervals"]["Extended"];
		foreach (XmlNode childNode6 in xmlNode.ChildNodes)
		{
			if (childNode6.Name == "Interval")
			{
				string item6 = childNode6.Attributes["Name"].GetStringOrDefault(string.Empty);
				moveLengthIntervalsExtended.Add(item6);
			}
		}
		xmlNode = xmlDocument["Settings"]["OutcomeTables"]["Throws"]["Throws"];
		foreach (XmlNode childNode7 in xmlNode.ChildNodes)
		{
			if (childNode7.Name == "Animation")
			{
				string item7 = childNode7.Attributes["Name"].GetStringOrDefault(string.Empty);
				throws.Add(item7);
			}
		}
		xmlNode = xmlDocument["Settings"]["OutcomeTables"]["Throws"]["ThrowableIntervals"];
		foreach (XmlNode childNode8 in xmlNode.ChildNodes)
		{
			if (childNode8.Name == "Interval")
			{
				string item8 = childNode8.Attributes["Name"].GetStringOrDefault(string.Empty);
				throwableIntervals.Add(item8);
			}
		}
	}

	public static void RefreshTacticSetParameters(XmlNode EELFNMOHGJL)
	{
		bothBotEnabled = EELFNMOHGJL["TacticsSettings"]["BothBot"].Attributes["Enabled"].ParseBool();
		XmlNode xmlNode = EELFNMOHGJL["TacticsSettings"]["Tactics"];
		if (xmlNode != null)
		{
			ParseTactics(xmlNode);
		}
		List<string> list = new List<string>();
		XmlNode xmlNode2 = EELFNMOHGJL["TacticsSettings"]["ItemEquivalents"];
		foreach (XmlNode childNode in xmlNode2.ChildNodes)
		{
			if (childNode.Name == "Item")
			{
				string text = childNode.Attributes["Type"].GetStringOrDefault();
				if (text != "Weapon")
				{
					GameLog.Error("strange item type '{0}'", text);
				}
				string gBCLEDJAOBM = childNode.Attributes["SubType"].GetStringOrDefault();
				list.Clear();
				foreach (XmlNode childNode2 in childNode.ChildNodes)
				{
					if (childNode2.Name == "Equivalent")
					{
						string text2 = childNode2.Attributes["Type"].GetStringOrDefault();
						if (text2 != "Weapon")
						{
							GameLog.Error("strange item type '%s'", text2);
						}
						string item = childNode2.Attributes["SubType"].GetStringOrDefault();
						list.Add(item);
					}
					else
					{
						GameLog.Error("strange xml node '%s'", childNode2.Name);
					}
				}
				itemEquivalents.Add(new global::Pair<string, List<string>>(gBCLEDJAOBM, list));
			}
			else
			{
				GameLog.Error("strange xml node '%s'", childNode.Name);
			}
		}
		xmlNode2 = EELFNMOHGJL["TacticsSettings"]["NoDecision"]["Intervals"];
		ParseStringList(xmlNode2, noDecisionIntervals, "Interval");
		xmlNode2 = EELFNMOHGJL["TacticsSettings"]["NoDecision"]["Moves"];
		ParseStringList(xmlNode2, noDecisionMoves, "Move");
		xmlNode2 = EELFNMOHGJL["TacticsSettings"]["UnexpectedMoves"];
		ParseStringList(xmlNode2, unexpectedMoves, "Move");
		xmlNode2 = EELFNMOHGJL["TacticsSettings"]["IgnoredEnemyAnimations"];
		ParseStringList(xmlNode2, ignoredEnemyAnimations);
		xmlNode2 = EELFNMOHGJL["TacticsSettings"]["SafeDodges"];
		ParseStringList(xmlNode2, safeDodgesAnimations);
		xmlNode2 = EELFNMOHGJL["TacticsSettings"]["EmergencyDodges"];
		ParseAnimationList(xmlNode2, emergencyDodgesAnimations);
		xmlNode2 = EELFNMOHGJL["TacticsSettings"]["CautiousMovements"];
		ParseAnimationList(xmlNode2, cautiousMovements);
		xmlNode2 = EELFNMOHGJL["TacticsSettings"]["EvadeThrowDodges"];
		ParseAnimationList(xmlNode2, evadeThrowDodges);
		xmlNode2 = EELFNMOHGJL["TacticsSettings"]["RandomizingEnemyAnimation"];
		ParseAnimationList(xmlNode2, randomizingEnemyAnimation);
		xmlNode2 = EELFNMOHGJL["TacticsSettings"]["MissileAnimations"];
		ParseAnimationList(xmlNode2, missileAnimations);
		xmlNode2 = EELFNMOHGJL["TacticsSettings"]["MagicAnimations"];
		ParseAnimationList(xmlNode2, magicAnimations);
		xmlNode2 = EELFNMOHGJL["TacticsSettings"]["EvadeUnsafeDodges"];
		ParseStringList(xmlNode2, evadeUnsafeDodgesAnimations);
		xmlNode2 = EELFNMOHGJL["TacticsSettings"]["AttackMoves"];
		ParseStringList(xmlNode2, attackMoves);
		xmlNode2 = EELFNMOHGJL["TacticsSettings"]["DistanceNode"];
		if (xmlNode2 != null)
		{
			_DistanceNode = xmlNode2.Attributes["Name"].GetStringOrDefault();
		}
		xmlNode2 = EELFNMOHGJL["TacticsSettings"]["Debug"]["ShowErrorIfAnimationNotFound"];
		if (xmlNode2 != null)
		{
			showErrorIfAnimationNotFound = xmlNode2.Attributes["Value"].ParseBool();
		}
	}

	public static void RefreshTacticParamWithRaidData(XmlDocument FJCFBLBNDNG)
	{
		TacticsCompiler.CompileTacticsSettings(FJCFBLBNDNG);
		RefreshTacticSetParameters(FJCFBLBNDNG);
	}

	public static void ParseTactics(XmlNode AFHNINCKJEE)
	{
		foreach (XmlNode childNode in AFHNINCKJEE.ChildNodes)
		{
			if (childNode.Name == "Tactic")
			{
				Tactic item = new Tactic(childNode);
				tactics.Add(item);
			}
		}
	}

	internal static void AddExternalTactic(XmlNode node)
	{
		if (node == null || node.Name != "Tactic")
			throw new System.ArgumentException("External tactic node must be a Tactic element.", "node");
		string name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		if (string.IsNullOrEmpty(name))
			throw new System.ArgumentException("External tactic requires a Name.", "node");
		for (int i = 0; i < tactics.Count; i++)
			if (tactics[i].get_Name() == name)
				throw new System.InvalidOperationException("External tactic collides with existing tactic '" + name + "'.");
		tactics.Add(new Tactic(node));
		EclipseExternalTactics.Add(name);
	}

	internal static bool RemoveExternalTactic(string name)
	{
		if (string.IsNullOrEmpty(name) || !EclipseExternalTactics.Remove(name)) return false;
		for (int i = tactics.Count - 1; i >= 0; i--)
		{
			if (tactics[i].get_Name() != name) continue;
			tactics.RemoveAt(i);
			return true;
		}
		return false;
	}

	public static string GetItemEquivalent(string LKBJNLBIDGP)
	{
		foreach (global::Pair<string, List<string>> item in itemEquivalents)
		{
			if (item.First == LKBJNLBIDGP)
			{
				return LKBJNLBIDGP;
			}
			foreach (string item2 in item.Second)
			{
				if (item2 == LKBJNLBIDGP)
				{
					return item.First;
				}
			}
		}
		return LKBJNLBIDGP;
	}

	public static Tactic GetTacticByName(string BHNDJOGLEOI)
	{
		foreach (Tactic item in tactics)
		{
			if (item.get_Name() == BHNDJOGLEOI)
			{
				return item;
			}
		}
		return defaultTactic;
	}

	public static void AddTableHolder(TacticalTableHolder IOAAHOMGEPI, string KEEMLGNLKPF, string ANCBHPMAAFI, TableType GLBPKPEIOKE)
	{
		List<global::Pair<TacticalTableHolder, global::Pair<string, string>>> list = tableHolders[(int)GLBPKPEIOKE];
		list.Add(new global::Pair<TacticalTableHolder, global::Pair<string, string>>(IOAAHOMGEPI, new global::Pair<string, string>(KEEMLGNLKPF, ANCBHPMAAFI)));
	}

	public static bool CheckIfTableExists(string LGCMGHAFEDD, TableType GLBPKPEIOKE)
	{
		List<global::Pair<TacticalTableHolder, global::Pair<string, string>>> list = tableHolders[(int)GLBPKPEIOKE];
		foreach (global::Pair<TacticalTableHolder, global::Pair<string, string>> item in list)
		{
			if (item.Second.First == LGCMGHAFEDD && item.Second.Second == LGCMGHAFEDD)
			{
				return true;
			}
		}
		return false;
	}

	public static bool CheckIfTableExists(string NDAJLDOMNLK, string AFKFIEAMFKG, TableType GLBPKPEIOKE)
	{
		List<global::Pair<TacticalTableHolder, global::Pair<string, string>>> list = tableHolders[(int)GLBPKPEIOKE];
		foreach (global::Pair<TacticalTableHolder, global::Pair<string, string>> item in list)
		{
			if (item.Second.First == NDAJLDOMNLK && item.Second.Second == AFKFIEAMFKG)
			{
				return true;
			}
		}
		return false;
	}

	public static bool CheckIfTableExists(List<string> PGHJNFEGLJE, TableType GLBPKPEIOKE)
	{
		List<global::Pair<TacticalTableHolder, global::Pair<string, string>>> list = tableHolders[(int)GLBPKPEIOKE];
		foreach (global::Pair<TacticalTableHolder, global::Pair<string, string>> item in list)
		{
			bool flag = false;
			foreach (string item2 in PGHJNFEGLJE)
			{
				if (item2 == item.Second.First)
				{
					flag = true;
					break;
				}
			}
			bool flag2 = false;
			foreach (string item3 in PGHJNFEGLJE)
			{
				if (item3 == item.Second.Second)
				{
					flag2 = true;
					break;
				}
			}
			if (flag && flag2)
			{
				return true;
			}
		}
		return false;
	}

	public static List<string> get_NoDecisionIntervals()
	{
		return noDecisionIntervals;
	}

	public static List<string> get_NoDecisionMoves()
	{
		return noDecisionMoves;
	}

	public static List<string> get_UnexpectedMoves()
	{
		return unexpectedMoves;
	}

	public static List<string> get_MovesFirstIteration()
	{
		return movesFirstIteration;
	}

	public static List<string> get_MovesLastIteration()
	{
		return movesLastIteration;
	}

	public static List<string> get_MissilesFirstIteration()
	{
		return missilesFirstIteration;
	}

	public static List<string> get_MissilesLastIteration()
	{
		return missilesLastIteration;
	}

	public static List<string> get_MoveLengthIntervalsStrict()
	{
		return moveLengthIntervalsStrict;
	}

	public static List<string> get_MoveLengthIntervalsExtended()
	{
		return moveLengthIntervalsExtended;
	}

	public static List<string> get_IgnoredEnemyAnimations()
	{
		return ignoredEnemyAnimations;
	}

	public static List<string> get_SafeDodgesAnimations()
	{
		return safeDodgesAnimations;
	}

	public static List<string> get_EvadeUnsafeDodgesAnimations()
	{
		return evadeUnsafeDodgesAnimations;
	}

	public static List<string> get_AttackMoves()
	{
		return attackMoves;
	}

	public static List<string> get_ThrowableIntervals()
	{
		return throwableIntervals;
	}

	public static List<string> get_Throws()
	{
		return throws;
	}

	public static List<TemplateAnimation> get_EmergencyDodgesAnimations()
	{
		return emergencyDodgesAnimations;
	}

	public static List<TemplateAnimation> get_CautiousMovements()
	{
		return cautiousMovements;
	}

	public static List<TemplateAnimation> get_EvadeThrowDodges()
	{
		return evadeThrowDodges;
	}

	public static List<TemplateAnimation> get_RandomizingEnemyAnimation()
	{
		return randomizingEnemyAnimation;
	}

	public static List<TemplateAnimation> get_MissileAnimations()
	{
		return missileAnimations;
	}

	public static List<TemplateAnimation> get_MagicAnimations()
	{
		return magicAnimations;
	}

	public static string get_DistanceNode()
	{
		return _DistanceNode;
	}

	public static bool get_IsShowErrorIfAnimationNotFound()
	{
		return showErrorIfAnimationNotFound;
	}

	private static void Loadfor(string LEPELMEGAOE, List<string> LDNMLKGMABH)
	{
		LoadShiftTablesfor(LEPELMEGAOE);
	}

	private static void LoadShiftTablesfor(string LEBFGLIGPOK)
	{
		TacticsArchiver.LoadArchive(LEBFGLIGPOK);
	}

	private static void LoadMovementsTablefor(string KEEMLGNLKPF, List<string> LDNMLKGMABH)
	{
		foreach (string item in LDNMLKGMABH)
		{
			LoadMovementsTablefor(KEEMLGNLKPF, item);
		}
	}

	private static void LoadMovementsTablefor(string KEEMLGNLKPF, string ANCBHPMAAFI)
	{
		TacticsArchiver.LoadArchive(KEEMLGNLKPF, ANCBHPMAAFI);
	}

	private static void RemoveExept(List<string> LCIGOHHEDGK)
	{
		TableType[] array = new TableType[3]
		{
			TableType.dodgeTable,
			TableType.movementsTable,
			TableType.outcometablesforattack
		};
		List<InfoAnimation> list = AnimationData.GetAnimations();
		foreach (InfoAnimation item in list)
		{
			item.ShiftTable.Clear();
			item.ResetModelBindings();
			for (int i = 0; i < array.Length; i++)
			{
				List<global::Pair<List<GroupTables>, string>> list2 = item.GetTacticGroupTables()[i];
				foreach (global::Pair<List<GroupTables>, string> item2 in list2)
				{
					if (item2.First != null)
					{
						item2.First = null;
						item2.Second = "delete";
					}
				}
				list2.Clear();
			}
		}
		for (int j = 0; j < array.Length; j++)
		{
			List<global::Pair<TacticalTableHolder, global::Pair<string, string>>> list3 = get_TablesHoldersNew()[j];
			for (int k = 0; k < list3.Count; k++)
			{
				global::Pair<TacticalTableHolder, global::Pair<string, string>> cCKLNOPEKHO = list3[k];
				if (!cCKLNOPEKHO.First.Empty())
				{
					if (CheckExceptionWeapons(cCKLNOPEKHO.Second.First, cCKLNOPEKHO.Second.Second, LCIGOHHEDGK))
					{
						GameLog.Write("Skipping exeptional weapons (%s/%s) on tactic table clear", cCKLNOPEKHO.Second.First, cCKLNOPEKHO.Second.Second);
					}
					else
					{
						list3.RemoveAt(k);
						k--;
					}
				}
			}
		}
	}

	private static bool CheckExceptionWeapons(string NDAJLDOMNLK, string AFKFIEAMFKG, List<string> LCIGOHHEDGK)
	{
		bool flag = false;
		bool flag2 = false;
		string text = NDAJLDOMNLK;
		foreach (string item in LCIGOHHEDGK)
		{
			if (text == item)
			{
				flag = true;
				break;
			}
		}
		text = AFKFIEAMFKG;
		foreach (string item2 in LCIGOHHEDGK)
		{
			if (text == item2)
			{
				flag2 = true;
				break;
			}
		}
		return flag && flag2;
	}

	private static void RemoveDuplicateSubtypes(List<string> JIGEFEPNCIN)
	{
		for (int i = 0; i < JIGEFEPNCIN.Count; i++)
		{
			for (int j = i + 1; j < JIGEFEPNCIN.Count; j++)
			{
				if (JIGEFEPNCIN[i] == JIGEFEPNCIN[j])
				{
					JIGEFEPNCIN.RemoveAt(j);
					j--;
				}
			}
		}
	}
}
