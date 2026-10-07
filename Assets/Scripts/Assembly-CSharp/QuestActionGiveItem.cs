using System;
using System.Collections.Generic;
using System.Xml;
using Nekki.SF2.GUI;
using Nekki.SF2.GUI.Shop;

public class QuestActionGiveItem : QuestAction
{
	private string quantityExpression = string.Empty;

	private string itemNameExpression = string.Empty;

	private string putOnExpression = string.Empty;

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		itemNameExpression = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		putOnExpression = node.Attributes["PutOn"].GetStringOrDefault(string.Empty);
		quantityExpression = node.Attributes["Quantity"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		ConditionExtension.CompareResult result = new ConditionExtension.CompareResult();
		QuestCondition condition = new QuestCondition();
		condition.SetParameters(Parameters);
		condition.SetValue(itemNameExpression, result);
		string itemName = string.Empty;
		int num = 0;
		ItemInfo itemInfo = null;
		ItemInfo upgradedItemInfo = null;
		List<string> list = new List<string>(result.resultSTR.Split('|'));
		int count = list.Count;
		if (count > 0)
		{
			itemName = list[0];
		}
		if (list.Count >= 2)
		{
			num = list[1].ToInt();
		}
		itemInfo = ListSF.GetItems().GetItemByName(itemName);
		if (itemInfo != null && num > 0)
		{
			upgradedItemInfo = itemInfo.GetUpgradeItemAtOrAboveUpgradeLevel(num);
		}
		if (itemInfo == null)
		{
			GameLog.Error("QuestActionGiveItem - cant find item \"%s\" with upgrade \"%i\"", result.resultSTR, num);
			return;
		}
		bool flag = false;
        bool grantedItem = false;
		condition.SetValue(putOnExpression, result);
		bool flag2 = result.resultNumber > 0.0;
		condition.SetValue(quantityExpression, result);
		int num2 = (int)result.resultNumber;
		ListSF listInstance = ListSF.GetInstance();
		if (flag2)
		{
			ListSF.UnequipOtherItemsOfType(itemInfo);
		}
		if (num2 > 0 || (num2 <= 0 && ListSF.GetUserItem(itemInfo.Name) == null))
		{
			Roster roster = ListSF.GetRoster();
			UserItem userItem = ListSF.AddItem(itemInfo, num2, 0L, flag2);
            grantedItem = userItem != null;
			if (upgradedItemInfo != null)
			{
				userItem.SetAcquireType("Upgrade");
				userItem.SetUpgradeLevel(upgradedItemInfo.UpgradeLevel);
				userItem.RefreshUpgradeState(roster.GetLevel());
			}
			if (itemInfo.ItemLevel <= roster.GetLevel())
			{
				itemInfo.SetIsNew(true);
			}
		}
		else if (num2 == 0)
		{
			if (flag2)
			{
				UserItem existingItem = ListSF.GetUserItem(itemInfo.Name);
				flag = ListSF.UseItem(existingItem, true);
			}
		}
		else
		{
			UserItem removedUserItem = ListSF.GetUserItem(itemInfo.Name);
			ListSF.RemoveItem(removedUserItem, Math.Abs(num2));
		}
		if (flag)
		{
			ShopScene current = Scene<ShopScene>.get_Current();
			if (current != null)
			{
				ItemAction itemAction = ItemAction.Item_Equip;
			}
		}
		if (num2 <= 0 && !flag2 && !grantedItem)
		{
			UserItem ownedItem = ListSF.GetUserItem(itemInfo.Name);
			if (ownedItem != null)
			{
				ownedItem.GetInfo().SetIsNew(false);
				ListSF.GetRoster().SaveCounterItems();
			}
		}
		ListSF.GetInstance().OnItemGiven(upgradedItemInfo);
        if (grantedItem)
        {
            var menu = Nekki.SF2.GUI.Menu.MainMenu.get_Instance();
            if (menu != null) menu.UpdateNewItems();
        }
		FinishAction();
	}
}
