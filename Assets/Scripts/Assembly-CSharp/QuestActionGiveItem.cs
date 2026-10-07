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

	public override void Parse(XmlNode EPKLCPOEELO)
	{
		base.Parse(EPKLCPOEELO);
		itemNameExpression = EPKLCPOEELO.Attributes["Name"].GetStringOrDefault(string.Empty);
		putOnExpression = EPKLCPOEELO.Attributes["PutOn"].GetStringOrDefault(string.Empty);
		quantityExpression = EPKLCPOEELO.Attributes["Quantity"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		base.Execute(GFIHPBCEEOB);
		ConditionExtension.CompareResult lNIDLHOIHIM = new ConditionExtension.CompareResult();
		QuestCondition kKDGLNECFHA = new QuestCondition();
		kKDGLNECFHA.SetParameters(Parameters);
		kKDGLNECFHA.SetValue(itemNameExpression, lNIDLHOIHIM);
		string gOHIIMFFFJI = string.Empty;
		int num = 0;
		ItemInfo dJKEECEOCJB = null;
		ItemInfo dJKEECEOCJB2 = null;
		List<string> list = new List<string>(lNIDLHOIHIM.resultSTR.Split('|'));
		int count = list.Count;
		if (count > 0)
		{
			gOHIIMFFFJI = list[0];
		}
		if (list.Count >= 2)
		{
			num = list[1].ToInt();
		}
		dJKEECEOCJB = ListSF.GetItems().GetItemByName(gOHIIMFFFJI);
		if (dJKEECEOCJB != null && num > 0)
		{
			dJKEECEOCJB2 = dJKEECEOCJB.GetUpgradeItemAtOrAboveUpgradeLevel(num);
		}
		if (dJKEECEOCJB == null)
		{
			GameLog.Error("QuestActionGiveItem - cant find item \"%s\" with upgrade \"%i\"", lNIDLHOIHIM.resultSTR, num);
			return;
		}
		bool flag = false;
        bool grantedItem = false;
		kKDGLNECFHA.SetValue(putOnExpression, lNIDLHOIHIM);
		bool flag2 = lNIDLHOIHIM.resultNumber > 0.0;
		kKDGLNECFHA.SetValue(quantityExpression, lNIDLHOIHIM);
		int num2 = (int)lNIDLHOIHIM.resultNumber;
		ListSF oPLPFMFAGMN = ListSF.GetInstance();
		if (flag2)
		{
			ListSF.UnequipOtherItemsOfType(dJKEECEOCJB);
		}
		if (num2 > 0 || (num2 <= 0 && ListSF.GetUserItem(dJKEECEOCJB.Name) == null))
		{
			Roster nKGLHEGIKKP = ListSF.GetRoster();
			UserItem dKCHDHMLKHN = ListSF.AddItem(dJKEECEOCJB, num2, 0L, flag2);
            grantedItem = dKCHDHMLKHN != null;
			if (dJKEECEOCJB2 != null)
			{
				dKCHDHMLKHN.SetAcquireType("Upgrade");
				dKCHDHMLKHN.SetUpgradeLevel(dJKEECEOCJB2.UpgradeLevel);
				dKCHDHMLKHN.RefreshUpgradeState(nKGLHEGIKKP.GetLevel());
			}
			if (dJKEECEOCJB.ItemLevel <= nKGLHEGIKKP.GetLevel())
			{
				dJKEECEOCJB.SetIsNew(true);
			}
		}
		else if (num2 == 0)
		{
			if (flag2)
			{
				UserItem nDMCFNGEPOA = ListSF.GetUserItem(dJKEECEOCJB.Name);
				flag = ListSF.UseItem(nDMCFNGEPOA, true);
			}
		}
		else
		{
			UserItem nDMCFNGEPOA2 = ListSF.GetUserItem(dJKEECEOCJB.Name);
			ListSF.RemoveItem(nDMCFNGEPOA2, Math.Abs(num2));
		}
		if (flag)
		{
			ShopScene current = Scene<ShopScene>.get_Current();
			if (current != null)
			{
				ItemAction pCKPFBFHKJH = ItemAction.Item_Equip;
			}
		}
		if (num2 <= 0 && !flag2 && !grantedItem)
		{
			UserItem dKCHDHMLKHN2 = ListSF.GetUserItem(dJKEECEOCJB.Name);
			if (dKCHDHMLKHN2 != null)
			{
				dKCHDHMLKHN2.GetInfo().SetIsNew(false);
				ListSF.GetRoster().SaveCounterItems();
			}
		}
		ListSF.GetInstance().OnItemGiven(dJKEECEOCJB2);
        if (grantedItem)
        {
            var menu = Nekki.SF2.GUI.Menu.MainMenu.get_Instance();
            if (menu != null) menu.UpdateNewItems();
        }
		FinishAction();
	}
}
