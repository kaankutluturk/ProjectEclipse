using System.Collections.Generic;
using System.Xml;

public class QuestActionToggleItems : QuestAction
{
	private string _toggle;

	private string labelExpression;

	public override void Parse(XmlNode EPKLCPOEELO)
	{
		base.Parse(EPKLCPOEELO);
		_toggle = EPKLCPOEELO.Attributes["Toggle"].GetStringOrDefault("on");
		labelExpression = EPKLCPOEELO.Attributes["Label"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		base.Execute(GFIHPBCEEOB);
		ConditionExtension.CompareResult lNIDLHOIHIM = new ConditionExtension.CompareResult();
		QuestCondition kKDGLNECFHA = new QuestCondition();
		kKDGLNECFHA.SetParameters(GFIHPBCEEOB);
		kKDGLNECFHA.SetValue(labelExpression, lNIDLHOIHIM);
		string iBBAMMHHBFE = lNIDLHOIHIM.resultSTR;
		kKDGLNECFHA.SetValue(_toggle, lNIDLHOIHIM);
		bool flag = lNIDLHOIHIM.resultSTR == "on";
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		if (flag)
		{
			if (nKGLHEGIKKP.AddShopLock(iBBAMMHHBFE, true))
			{
				ApplyShopLockToItems(iBBAMMHHBFE, true);
			}
		}
		else if (nKGLHEGIKKP.RemoveShopLock(iBBAMMHHBFE))
		{
			ApplyShopLockToItems(iBBAMMHHBFE, false);
		}
		FinishAction();
	}

	private void ApplyShopLockToItems(string ECNLPLIBNHF, bool PEJELKNFEKJ)
	{
		if (PEJELKNFEKJ)
		{
			List<ItemInfo> list = ListSF.GetItems().GetAllItems();
			int num = ListSF.GetRoster().GetLevel();
			{
				foreach (ItemInfo item in list)
				{
					if (item.IsShopVisible && item.GroupId == ECNLPLIBNHF)
					{
						ListSF.GetItems().SetNewAddItem(item, true, (!(item.Type == "RealMoneyItem")) ? num : item.ItemLevel);
					}
				}
				return;
			}
		}
		ListSF.GetItems().ClearNewFlagsForGroup(ECNLPLIBNHF);
	}
}
