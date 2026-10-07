using System.Collections.Generic;
using System.Xml;

public class QuestActionForeach : QuestAction
{
	public enum ForeachType
	{
		FOREACH_NONE = 0,
		FOREACH_ITEMS = 1,
		FOREACH_DELIVERY_ITEMS = 2,
		FOREACH_DELIVERY_UPGRADES = 3,
		FOREACH_PAID_ITEMS = 4,
		FOREACH_BATTLES = 5,
		FOREACH_DELIVERY_ENCHANTMENTS = 6
	}

	private bool continueLoop;

	private bool isLooping;

	private QuestStage bodyStage;

	private List<string> nodes = new List<string>();

	private int index = -1;

	private int nodeCount;

	private QuestParameters questParameters;

	private string name;

	private ForeachType foreachType;

	public override void Parse(XmlNode EPKLCPOEELO)
	{
		base.Parse(EPKLCPOEELO);
		foreachType = GetType(EPKLCPOEELO.Attributes["Type"].GetStringOrDefault(string.Empty));
		name = EPKLCPOEELO.Attributes["Name"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		base.Execute(GFIHPBCEEOB);
		if (ListSF.GetInstance().IsEclipseQuestSuppressed(name))
		{
			FinishAction();
			return;
		}
		index = -1;
		nodeCount = 0;
		questParameters = GFIHPBCEEOB;
		bodyStage = ListSF.GetInstance().GetQuestByName(name);
		nodes.Clear();
		switch (foreachType)
		{
		case ForeachType.FOREACH_ITEMS:
			CollectUserItems();
			break;
		case ForeachType.FOREACH_DELIVERY_ITEMS:
			RunDeliveryItems(false);
			break;
		case ForeachType.FOREACH_DELIVERY_UPGRADES:
			RunDeliveryItems(true);
			break;
		case ForeachType.FOREACH_PAID_ITEMS:
			CollectPaidItems();
			break;
		case ForeachType.FOREACH_BATTLES:
			CollectBattles();
			break;
		case ForeachType.FOREACH_DELIVERY_ENCHANTMENTS:
			CollectDeliveryEnchantments();
			break;
		}
		nodeCount = nodes.Count;
		continueLoop = true;
		RunLoop();
	}

	private void RunLoop()
	{
		isLooping = true;
		while (continueLoop)
		{
			Run();
		}
		isLooping = false;
	}

	private void Run()
	{
		continueLoop = false;
		index++;
		if (bodyStage == null || index > nodeCount - 1)
		{
			if (foreachType == ForeachType.FOREACH_DELIVERY_ITEMS || foreachType == ForeachType.FOREACH_DELIVERY_UPGRADES)
			{
				Roster nKGLHEGIKKP = ListSF.GetRoster();
				nKGLHEGIKKP.GetInventory().ItemDelivered.RemoveListener(OnDeliveryItemAdded);
				switch (foreachType)
				{
				case ForeachType.FOREACH_DELIVERY_ITEMS:
					nKGLHEGIKKP.GetInventory().GetDeliveredItems().Clear();
					break;
				case ForeachType.FOREACH_DELIVERY_UPGRADES:
					nKGLHEGIKKP.GetInventory().GetDeliveredUpgrades().Clear();
					break;
				case ForeachType.FOREACH_DELIVERY_ENCHANTMENTS:
					nKGLHEGIKKP.GetInventory().DeliveredRecipes.Clear();
					break;
				}
			}
			else if (foreachType == ForeachType.FOREACH_DELIVERY_ENCHANTMENTS)
			{
				Roster nKGLHEGIKKP2 = ListSF.GetRoster();
				nKGLHEGIKKP2.RemoveEventListener(1, OnDeliveryEnchantmentAdded);
				nKGLHEGIKKP2.GetInventory().ClearDeliveredRecipes();
			}
			FinishAction();
			continueLoop = false;
		}
		else
		{
			questParameters.iteratorValue = nodes[index];
			if (bodyStage.Compare(questParameters))
			{
				bodyStage.AddEventListener(1, OnQuestComplete);
				bodyStage.StartActions(questParameters, false);
			}
			else
			{
				continueLoop = true;
			}
		}
	}

	private void OnQuestComplete(object data)
	{
		QuestStage mLLKDGBEGJI = (QuestStage)data;
		mLLKDGBEGJI.RemoveEventListener(1, OnQuestComplete);
		continueLoop = true;
		if (!isLooping)
		{
			RunLoop();
		}
	}

	private void CollectUserItems()
	{
		List<UserItem> list = ListSF.GetRoster().GetInventory().GetItems();
		foreach (UserItem item in list)
		{
			nodes.Add(item.get_Name());
		}
	}

	private void RunDeliveryItems(bool EIOPLHKAEPK)
	{
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		List<UserItem> list = ((!EIOPLHKAEPK) ? nKGLHEGIKKP.GetInventory().GetDeliveredItems() : nKGLHEGIKKP.GetInventory().GetDeliveredUpgrades());
		foreach (UserItem item in list)
		{
			nodes.Add(item.get_Name());
		}
		nKGLHEGIKKP.GetInventory().ItemDelivered.AddListener(OnDeliveryItemAdded);
	}

	private void CollectPaidItems()
	{
		List<ItemInfo> list = ListSF.GetItems().GetItemsByType("RealMoneyItem");
		if (list == null)
		{
			return;
		}
		foreach (ItemInfo item in list)
		{
			nodes.Add(item.Name);
		}
	}

	private void CollectDeliveryEnchantments()
	{
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		List<RecipeItemInfo> lFADKPKKFMP = nKGLHEGIKKP.GetInventory().DeliveredRecipes;
		foreach (RecipeItemInfo item in lFADKPKKFMP)
		{
			nodes.Add(item.ToString());
		}
		nKGLHEGIKKP.AddEventListener(1, OnDeliveryEnchantmentAdded);
	}

	private void CollectBattles()
	{
		List<Battle> list = ListSF.GetInstance().GetBattles();
		foreach (Battle item in list)
		{
			nodes.Add(item.GetZoneBattleKey());
		}
	}

	private void OnDeliveryItemAdded(object data)
	{
		if (data != null)
		{
			UserItem dKCHDHMLKHN = (UserItem)data;
			if ((foreachType == ForeachType.FOREACH_DELIVERY_ITEMS && !dKCHDHMLKHN.GetIsUpgrade()) || (foreachType == ForeachType.FOREACH_DELIVERY_UPGRADES && dKCHDHMLKHN.GetIsUpgrade()))
			{
				nodes.Add(dKCHDHMLKHN.get_Name());
			}
			nodeCount = nodes.Count;
		}
	}

	private void OnDeliveryEnchantmentAdded(object data)
	{
		if (data != null)
		{
			RecipeItemInfo bNJOCBKNPMG = (RecipeItemInfo)data;
			if (foreachType == ForeachType.FOREACH_DELIVERY_ENCHANTMENTS)
			{
				nodes.Add(bNJOCBKNPMG.ToString());
				nodeCount = nodes.Count;
			}
		}
	}

	private ForeachType GetType(string _type)
	{
		switch (_type)
		{
		case "Items":
			return ForeachType.FOREACH_ITEMS;
		case "DeliveryItems":
			return ForeachType.FOREACH_DELIVERY_ITEMS;
		case "PaidItems":
			return ForeachType.FOREACH_PAID_ITEMS;
		case "DeliveryUpgrades":
			return ForeachType.FOREACH_DELIVERY_UPGRADES;
		case "Battles":
			return ForeachType.FOREACH_BATTLES;
		case "DeliveryEnchantments":
			return ForeachType.FOREACH_DELIVERY_ENCHANTMENTS;
		default:
			return ForeachType.FOREACH_NONE;
		}
	}
}
