using System.Xml;

public class QuestActionChangePlayerAvatar : QuestAction
{
	private string avatarExpression;

	public override void Parse(XmlNode EPKLCPOEELO)
	{
		base.Parse(EPKLCPOEELO);
		avatarExpression = EPKLCPOEELO.Attributes["Avatar"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		base.Execute(GFIHPBCEEOB);
		string FHLFEBDNIFF = string.Empty;
		GetValues(ref FHLFEBDNIFF);
		if (GameUtils.AvatarExists(FHLFEBDNIFF))
		{
			ListSF.GetRoster().SetAvatar(FHLFEBDNIFF);
		}
		FinishAction();
	}

	private void GetValues(ref string FHLFEBDNIFF)
	{
		ConditionExtension.CompareResult lNIDLHOIHIM = new ConditionExtension.CompareResult();
		QuestCondition kKDGLNECFHA = new QuestCondition();
		kKDGLNECFHA.SetParameters(Parameters);
		kKDGLNECFHA.SetValue(avatarExpression, lNIDLHOIHIM);
		FHLFEBDNIFF = lNIDLHOIHIM.ToString();
	}
}
