using System.Collections.Generic;
using System.Xml;

public class QuestActionToggleItems : QuestAction
{
	private string _toggle;

	private string labelExpression;

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		_toggle = node.Attributes["Toggle"].GetStringOrDefault("on");
		labelExpression = node.Attributes["Label"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		ConditionExtension.CompareResult result = new ConditionExtension.CompareResult();
		QuestCondition condition = new QuestCondition();
		condition.SetParameters(parameters);
		condition.SetValue(labelExpression, result);
		string lockName = result.resultSTR;
		condition.SetValue(_toggle, result);
		bool flag = result.resultSTR == "on";
		Roster roster = ListSF.GetRoster();
		if (flag)
		{
			if (roster.AddShopLock(lockName, true))
			{
				ApplyShopLockToItems(lockName, true);
			}
		}
		else if (roster.RemoveShopLock(lockName))
		{
			ApplyShopLockToItems(lockName, false);
		}
		FinishAction();
	}

	private void ApplyShopLockToItems(string groupId, bool locked)
	{
		if (locked)
		{
			List<ItemInfo> list = ListSF.GetItems().GetAllItems();
			int num = ListSF.GetRoster().GetLevel();
			{
				foreach (ItemInfo item in list)
				{
					if (item.IsShopVisible && item.GroupId == groupId)
					{
						ListSF.GetItems().SetNewAddItem(item, true, (!(item.Type == "RealMoneyItem")) ? num : item.ItemLevel);
					}
				}
				return;
			}
		}
		ListSF.GetItems().ClearNewFlagsForGroup(groupId);
	}
}
