using System.Xml;
using Nekki.SF2.GUI;
using Nekki.SF2.GUI.Map;
using Nekki.SF2.GUI.Shop;

public class QuestActionChangeTab : QuestAction
{
	protected string tabExpression = string.Empty;

	protected string focusExpression = string.Empty;

	protected SliderType _TabType;

	protected ScreenType _ScreenType = ScreenType.ModuleNone;

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		tabExpression = node.Attributes["Tab"].GetStringOrDefault(string.Empty);
		focusExpression = node.Attributes["Focus"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		string empty = string.Empty;
		ConditionExtension.CompareResult result = new ConditionExtension.CompareResult();
		QuestCondition condition = new QuestCondition();
		condition.SetParameters(parameters);
		condition.SetValue(tabExpression, result);
		empty = result.ToString();
		_TabType = ParseTabType(empty);
		_ScreenType = GetScreenForTab(_TabType);
		ScreenType currentScreen = Module.GetInstance().GetCurrentScreenType();
		if (currentScreen == _ScreenType)
		{
			switch (_ScreenType)
			{
			case ScreenType.ModuleShop:
			{
				ShopScene current3 = Scene<ShopScene>.get_Current();
				if (current3 != null)
				{
					break;
				}
				Module.GetInstance().AddEventListener(1, OnModuleChanged);
				return;
			}
			case ScreenType.ModuleProfile:
			{
				ProfileScene current2 = Scene<ProfileScene>.get_Current();
				if (current2 != null)
				{
					break;
				}
				Module.GetInstance().AddEventListener(1, OnModuleChanged);
				return;
			}
			case ScreenType.ModuleMap:
			{
				MapScene current = Scene<MapScene>.get_Current();
				if (current != null)
				{
					current.ScrollToItemByName(_TabType);
					break;
				}
				Module.GetInstance().AddEventListener(1, OnModuleChanged);
				return;
			}
			}
		}
		FinishAction();
	}

	protected void OnModuleChanged(object data)
	{
		switch (_ScreenType)
		{
		case ScreenType.ModuleShop:
		{
			ShopScene current2 = Scene<ShopScene>.get_Current();
			if (!(current2 != null))
			{
			}
			break;
		}
		case ScreenType.ModuleProfile:
		{
			ProfileScene current3 = Scene<ProfileScene>.get_Current();
			if (!(current3 != null))
			{
			}
			break;
		}
		case ScreenType.ModuleMap:
		{
			MapScene current = Scene<MapScene>.get_Current();
			if (current != null)
			{
				current.ScrollToItemByName(_TabType);
			}
			break;
		}
		}
		FinishAction();
		Module.GetInstance().RemoveEventListener(1, OnModuleChanged);
	}

	protected SliderType ParseTabType(string tabName)
	{
		switch (tabName)
		{
		case "Weapon":
			return SliderType.SliderWeapon;
		case "Armor":
			return SliderType.SliderArmor;
		case "Helm":
			return SliderType.SliderHelmet;
		case "Ranged":
			return SliderType.SliderMissile;
		case "Magic":
			return SliderType.SliderMagic;
		case "Ruby":
			return SliderType.SliderRuby;
		case "Free":
			return SliderType.SliderFree;
		case "Perks":
			return SliderType.SliderPerks;
		case "Moves":
			return SliderType.SliderTricks;
		case "Achievements":
			return SliderType.SliderAchievements;
		case "QuestItems":
			return SliderType.SliderSeals;
		case "RaidMapStage":
			return SliderType.SliderRaidMap;
		case "StoryMapStage":
			return SliderType.SliderStoryMap;
		default:
			return SliderType.SliderNone;
		}
	}

	protected ScreenType GetScreenForTab(SliderType _sliderType)
	{
		switch (_sliderType)
		{
		case SliderType.SliderWeapon:
		case SliderType.SliderArmor:
		case SliderType.SliderHelmet:
		case SliderType.SliderMissile:
		case SliderType.SliderMagic:
		case SliderType.SliderRuby:
		case SliderType.SliderFree:
			return ScreenType.ModuleShop;
		case SliderType.SliderPerks:
		case SliderType.SliderTricks:
		case SliderType.SliderAchievements:
		case SliderType.SliderSeals:
			return ScreenType.ModuleProfile;
		case SliderType.SliderStoryMap:
		case SliderType.SliderRaidMap:
			return ScreenType.ModuleMap;
		default:
			return ScreenType.ModuleNone;
		}
	}
}
