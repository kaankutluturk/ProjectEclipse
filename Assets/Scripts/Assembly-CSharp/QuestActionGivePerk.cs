using System.Collections.Generic;
using System.Xml;

public class QuestActionGivePerk : QuestAction
{
	private string applyToExpression;

	private string itemExpression;

	private XmlDocument _node = new XmlDocument();

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		applyToExpression = node.Attributes["ApplyTo"].GetStringOrDefault(string.Empty);
		itemExpression = node.Attributes["Item"].GetStringOrDefault(string.Empty);
		CopyNodeToNode(node, _node);
	}

	private void CopyNodeToNode(XmlNode node, XmlNode targetNode)
	{
		foreach (XmlNode childNode in node.ChildNodes)
		{
			targetNode.AppendImportedClone(childNode);
		}
	}

	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		try
		{
			ConditionExtension.CompareResult result = new ConditionExtension.CompareResult();
			QuestCondition condition = new QuestCondition();
			condition.SetParameters(parameters);
			result.Clear();
			condition.SetValue(applyToExpression, result);
			string text = result.ToString();
			if (text == "Player")
			{
				ApplyPerksToPlayer(result, condition);
			}
			else if (text == "Item")
			{
				ApplyPerksToItem(result, condition);
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

	private void ApplyPerksToPlayer(ConditionExtension.CompareResult result, QuestCondition condition)
	{
		XmlDocument xmlDocument = new XmlDocument();
		CopyNodeToNode(_node, xmlDocument);
		ResolveNodeAttributes(xmlDocument, result, condition);
		GameUtils.PerkItemList.ParseUserPerks(xmlDocument, false);
		AddPlayerPerks(xmlDocument);
	}

	private void ResolveNodeAttributes(XmlNode node, ConditionExtension.CompareResult result, QuestCondition condition)
	{
		foreach (XmlNode childNode in node.ChildNodes)
		{
			foreach (XmlAttribute attribute in childNode.Attributes)
			{
				result.Clear();
				condition.SetValue(attribute.Value, result);
				attribute.Value = result.ToString();
			}
			ResolveNodeAttributes(childNode, result, condition);
		}
	}

	private void AddPlayerPerks(XmlNode node)
	{
		foreach (XmlNode childNode in node.ChildNodes)
		{
			string text = childNode.Attributes["Name"].GetStringOrDefault(string.Empty);
			int level = childNode.Attributes["Level"].ParseInt();
			int upgradeLevel = childNode.Attributes["UpgradeLevel"].ParseInt();
			PerkInfoItem perkInfo = FindPerkInfo(text);
			if (perkInfo != null)
			{
				RosterPerkInfo rosterPerkInfo = new RosterPerkInfo();
				rosterPerkInfo.Name = text;
				rosterPerkInfo.Level = level;
				rosterPerkInfo.UpgradeLevel = upgradeLevel;
				ReadPerkPairs(childNode, rosterPerkInfo);
				ListSF.GetRoster().GetPerks().AddOrUpgradePerk(rosterPerkInfo);
			}
		}
	}

	private void ReadPerkPairs(XmlNode node, RosterPerkInfo rosterPerkInfo)
	{
		XmlNode xmlNode = node["Set"];
		if (xmlNode == null)
		{
			return;
		}
		foreach (XmlAttribute attribute in xmlNode.Attributes)
		{
			rosterPerkInfo.Pairs[attribute.Name] = attribute.Value;
		}
	}

	private PerkInfoItem FindPerkInfo(string name)
	{
		PerkInfoItem perkInfo = GameUtils.PerkItemList.FindUserPerk(name);
		if (perkInfo == null)
		{
			perkInfo = GameUtils.PerkItemList.FindProgressionPerk(name);
		}
		if (perkInfo == null)
		{
			perkInfo = GameUtils.PerkItemList.FindBasePerk(name);
		}
		return perkInfo;
	}

	private void ApplyPerksToItem(ConditionExtension.CompareResult result, QuestCondition condition)
	{
		XmlDocument xmlDocument = new XmlDocument();
		CopyNodeToNode(_node, xmlDocument);
		ResolveNodeAttributes(xmlDocument, result, condition);
		List<PerkStruct> enchantments = ParsePerkStructs(xmlDocument);
		result.Clear();
		condition.SetValue(itemExpression, result);
		string itemName = result.ToString();
		UserItem userItem = ListSF.GetRoster().GetInventory().FindItem(itemName);
		if (userItem != null)
		{
			userItem.ReplaceEnchantments(enchantments, userItem.GetUpgradeLevel(), ListSF.GetRoster().GetLevel());
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
