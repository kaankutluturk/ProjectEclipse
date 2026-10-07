using Nekki.SF2.Core.Tutorials;
using Nekki.SF2.GUI;
using Nekki.SF2.GUI.Menu;
using Nekki.SF2.GUI.Shop;
using UnityEngine.UI;

public class QuestActionStoryTutorialBuyItem : QuestAction
{
	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		string cDNCPBKAHKJ = GameUtils.TutorialSettings.TutorialWeapon;
		UserItem dKCHDHMLKHN = ListSF.GetRoster().GetInventory().FindItem(cDNCPBKAHKJ);
		if (dKCHDHMLKHN != null)
		{
			FinishAction();
		}
		TutorialCanvas.get_Instance().set_BlockOn(true);
		ShopScene current = Scene<ShopScene>.get_Current();
		current.ScrollToItemByName(ShopSection.Weapon, cDNCPBKAHKJ);
		IconLabelButton goldButton = current.GetInfoPanel().GetGoldButton();
		goldButton.set_IsFlashing(true);
		goldButton.RemoveAllEventListener();
		goldButton.onClick.RemoveAllListeners();
		goldButton.onClick.AddListener(OnButtonClick);
		TutorialComponent component = goldButton.gameObject.GetComponent<TutorialComponent>();
		component.IsActive = true;
		Button skipBtn = MainMenu.get_Instance().GetSkipBtn();
		TutorialComponent tutorialComponent = ((!(skipBtn != null)) ? null : skipBtn.gameObject.GetComponent<TutorialComponent>());
		if (tutorialComponent != null)
		{
			tutorialComponent.IsActive = true;
			skipBtn.onClick.AddListener(OnSkipClicked);
		}
	}

	private void OnSkipClicked()
	{
		Button skipBtn = MainMenu.get_Instance().GetSkipBtn();
		skipBtn.onClick.AddListener(OnSkipClicked);
		OnButtonClick();
	}

	private void OnButtonClick()
	{
		TutorialCanvas.get_Instance().set_BlockOn(false);
		ShopScene instance = ShopScene.get_Instance();
		IconLabelButton goldButton = instance.GetInfoPanel().GetGoldButton();
		goldButton.set_IsFlashing(false);
		goldButton.onClick.RemoveListener(OnButtonClick);
		TutorialComponent component = goldButton.gameObject.GetComponent<TutorialComponent>();
		component.IsActive = false;
		string cDNCPBKAHKJ = GameUtils.TutorialSettings.TutorialWeapon;
		ItemInfo dJKEECEOCJB = ListSF.GetItems().GetItemByName(cDNCPBKAHKJ);
		if (dJKEECEOCJB != null)
		{
			if (ItemBuyHelper.BuyItemWithCoins(dJKEECEOCJB))
			{
				ListSF.GetRoster().GetInventory().EquipItem(dJKEECEOCJB, true);
			}
			instance.GetInfoPanel().UpdateContent();
		}
		FinishAction();
	}
}
