using System.Collections.Generic;
using System.Xml;

public class QuestActionGivePerk : QuestAction
{
	private string applyToExpression;

	private string itemExpression;

	private XmlDocument _node = new XmlDocument();

	public override void Parse(XmlNode EPKLCPOEELO)
	{
		base.Parse(EPKLCPOEELO);
		applyToExpression = EPKLCPOEELO.Attributes["ApplyTo"].GetStringOrDefault(string.Empty);
		itemExpression = EPKLCPOEELO.Attributes["Item"].GetStringOrDefault(string.Empty);
		CopyNodeToNode(EPKLCPOEELO, _node);
	}

	private void CopyNodeToNode(XmlNode node, XmlNode EAFDAPNLMJD)
	{
		foreach (XmlNode childNode in node.ChildNodes)
		{
			EAFDAPNLMJD.AppendImportedClone(childNode);
		}
	}

	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		base.Execute(GFIHPBCEEOB);
		try
		{
			ConditionExtension.CompareResult lNIDLHOIHIM = new ConditionExtension.CompareResult();
			QuestCondition kKDGLNECFHA = new QuestCondition();
			kKDGLNECFHA.SetParameters(GFIHPBCEEOB);
			lNIDLHOIHIM.Clear();
			kKDGLNECFHA.SetValue(applyToExpression, lNIDLHOIHIM);
			string text = lNIDLHOIHIM.ToString();
			if (text == "Player")
			{
				ApplyPerksToPlayer(lNIDLHOIHIM, kKDGLNECFHA);
			}
			else if (text == "Item")
			{
				ApplyPerksToItem(lNIDLHOIHIM, kKDGLNECFHA);
			}
		}
		catch (System.Exception exception)
		{
			// A malformed/already-maxed imported reward must not strand the global
			// quest queue.  That previously made unrelated map controls, including
			// Eclipse, appear completely unresponsive for the rest of the session.
			Roster roster = ListSF.GetRoster();
			if (roster != null)
			{
				roster.UseLevelOverride = false;
			}
			UnityEngine.Debug.LogWarning("[Quest] GivePerk skipped invalid imported reward: " + exception.Message);
		}
		finally
		{
			FinishAction();
		}
	}

	private void ApplyPerksToPlayer(ConditionExtension.CompareResult DCJLKCFKCOM, QuestCondition IOFGGOCEIAM)
	{
		XmlDocument xmlDocument = new XmlDocument();
		CopyNodeToNode(_node, xmlDocument);
		ResolveNodeAttributes(xmlDocument, DCJLKCFKCOM, IOFGGOCEIAM);
		GameUtils.PerkItemList.ParseUserPerks(xmlDocument, false);
		AddPlayerPerks(xmlDocument);
	}

	private void ResolveNodeAttributes(XmlNode node, ConditionExtension.CompareResult DCJLKCFKCOM, QuestCondition IOFGGOCEIAM)
	{
		foreach (XmlNode childNode in node.ChildNodes)
		{
			foreach (XmlAttribute attribute in childNode.Attributes)
			{
				DCJLKCFKCOM.Clear();
				IOFGGOCEIAM.SetValue(attribute.Value, DCJLKCFKCOM);
				attribute.Value = DCJLKCFKCOM.ToString();
			}
			ResolveNodeAttributes(childNode, DCJLKCFKCOM, IOFGGOCEIAM);
		}
	}

	private void AddPlayerPerks(XmlNode node)
	{
		foreach (XmlNode childNode in node.ChildNodes)
		{
			string text = childNode.Attributes["Name"].GetStringOrDefault(string.Empty);
			int gCAPLEJMMPM = childNode.Attributes["Level"].ParseInt();
			int aKKLOMFOLNO = childNode.Attributes["UpgradeLevel"].ParseInt();
			PerkInfoItem aCONCDFDNJH = FindPerkInfo(text);
			if (aCONCDFDNJH != null)
			{
				RosterPerkInfo gAKDPKLHHFF = new RosterPerkInfo();
				gAKDPKLHHFF.Name = text;
				gAKDPKLHHFF.Level = gCAPLEJMMPM;
				gAKDPKLHHFF.UpgradeLevel = aKKLOMFOLNO;
				ReadPerkPairs(childNode, gAKDPKLHHFF);
				ListSF.GetRoster().GetPerks().AddOrUpgradePerk(gAKDPKLHHFF);
			}
		}
	}

	private void ReadPerkPairs(XmlNode node, RosterPerkInfo EMBBNNBFODN)
	{
		XmlNode xmlNode = node["Set"];
		if (xmlNode == null)
		{
			return;
		}
		foreach (XmlAttribute attribute in xmlNode.Attributes)
		{
			EMBBNNBFODN.Pairs[attribute.Name] = attribute.Value;
		}
	}

	private PerkInfoItem FindPerkInfo(string name)
	{
		PerkInfoItem aCONCDFDNJH = GameUtils.PerkItemList.FindUserPerk(name);
		if (aCONCDFDNJH == null)
		{
			aCONCDFDNJH = GameUtils.PerkItemList.FindProgressionPerk(name);
		}
		if (aCONCDFDNJH == null)
		{
			aCONCDFDNJH = GameUtils.PerkItemList.FindBasePerk(name);
		}
		return aCONCDFDNJH;
	}

	private void ApplyPerksToItem(ConditionExtension.CompareResult DCJLKCFKCOM, QuestCondition IOFGGOCEIAM)
	{
		XmlDocument xmlDocument = new XmlDocument();
		CopyNodeToNode(_node, xmlDocument);
		ResolveNodeAttributes(xmlDocument, DCJLKCFKCOM, IOFGGOCEIAM);
		List<PerkStruct> hALHGEGADKA = ParsePerkStructs(xmlDocument);
		DCJLKCFKCOM.Clear();
		IOFGGOCEIAM.SetValue(itemExpression, DCJLKCFKCOM);
		string gOHIIMFFFJI = DCJLKCFKCOM.ToString();
		UserItem dKCHDHMLKHN = ListSF.GetRoster().GetInventory().FindItem(gOHIIMFFFJI);
		if (dKCHDHMLKHN != null)
		{
			dKCHDHMLKHN.ReplaceEnchantments(hALHGEGADKA, dKCHDHMLKHN.GetUpgradeLevel(), ListSF.GetRoster().GetLevel());
		}
	}

	private List<PerkStruct> ParsePerkStructs(XmlNode node)
	{
		List<PerkStruct> list = new List<PerkStruct>();
		foreach (XmlNode childNode in node.ChildNodes)
		{
			PerkStruct item = new PerkStruct(childNode);
			list.Add(item);
		}
		return list;
	}
}
