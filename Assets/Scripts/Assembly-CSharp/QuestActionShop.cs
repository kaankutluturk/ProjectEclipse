using System.Xml;
using Nekki.SF2.GUI;
using Nekki.SF2.GUI.Shop;

public class QuestActionShop : QuestAction
{
	private string tabExpression = string.Empty;

	private string item = string.Empty;

	private SliderType _sliderType;

	private string itemName = string.Empty;

	private ItemInfo itemInfo;

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		tabExpression = node.Attributes["Tab"].GetStringOrDefault(string.Empty);
		item = node.Attributes["Item"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		ConditionExtension.CompareResult result = new ConditionExtension.CompareResult();
		ConditionExtension.CompareResult lNIDLHOIHIM2 = new ConditionExtension.CompareResult();
		QuestCondition condition = new QuestCondition();
		condition.SetParameters(parameters);
		if (!string.IsNullOrEmpty(tabExpression))
		{
			condition.SetValue(tabExpression, result);
		}
		if (!string.IsNullOrEmpty(item))
		{
			condition.SetValue(item, lNIDLHOIHIM2);
		}
		_sliderType = ParseSliderType(result.ToString());
		itemName = lNIDLHOIHIM2.ToString();
		itemInfo = ListSF.GetItems().GetItemByName(itemName);
		ShowShopItem();
	}

	private SliderType ParseSliderType(string name)
	{
		switch (name)
		{
		case "Weapon":
			return SliderType.SliderWeapon;
		case "Ranged":
			return SliderType.SliderMissile;
		case "Magic":
			return SliderType.SliderMagic;
		case "Armor":
			return SliderType.SliderArmor;
		case "Helm":
			return SliderType.SliderHelmet;
		case "Ruby":
			return SliderType.SliderRuby;
		case "Free":
			return SliderType.SliderFree;
		case "RaidConsumable":
			return SliderType.SliderRaidItemPack;
		default:
			return SliderType.SliderNone;
		}
	}

	private void OnModuleChangedScrollToItem(object data)
	{
		ShopScene current = Scene<ShopScene>.get_Current();
		if (current != null)
		{
			current.ScrollToItemByName(_sliderType, itemName);
		}
		Module.GetInstance().RemoveEventListener(1, OnModuleChangedScrollToItem);
		FinishAction();
	}

	private void OnModuleChangedComplete(object data)
	{
		Module.GetInstance().RemoveEventListener(1, OnModuleChangedComplete);
		FinishAction();
	}

	private void ShowShopItem()
	{
		ScreenType currentScreen = Module.GetInstance().GetCurrentScreenType();
		ShopScene current = Scene<ShopScene>.get_Current();
		bool flag = current != null;
		bool flag2 = currentScreen == ScreenType.ModuleShop;
		if (flag && _sliderType != SliderType.SliderNone)
		{
			current.ScrollToItemByName(_sliderType, itemName);
			FinishAction();
		}
		else if (flag2)
		{
			Module.GetInstance().AddEventListener(1, OnModuleChangedScrollToItem);
		}
		else
		{
			Module.GetInstance().AddEventListener(1, OnModuleChangedComplete);
			Module.OpenScreen(ScreenType.ModuleShop, new DelayedStrike(_sliderType, itemInfo, true));
		}
	}
}
