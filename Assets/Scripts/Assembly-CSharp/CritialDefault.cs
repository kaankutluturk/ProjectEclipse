using System.Xml;

public class CritialDefault
{
	private global::Pair<float, string> _Probability = new global::Pair<float, string>(0f, null);

	private global::Pair<float, string> _Damage = new global::Pair<float, string>(0f, null);

	public void Parse(XmlNode node)
	{
		XmlNode xmlNode = node["Probability"];
		_Probability.First = xmlNode.Attributes["Base"].ParseFloat();
		_Probability.Second = xmlNode.Attributes["Attribute"].GetStringOrDefault(string.Empty);
		XmlNode xmlNode2 = node["Damage"];
		_Damage.First = xmlNode2.Attributes["Base"].ParseFloat();
		_Damage.Second = xmlNode2.Attributes["Attribute"].GetStringOrDefault(string.Empty);
	}

	public float GetProbability(Model ACENLMONNPA)
	{
		int OEMALIFPGPO = 0;
		if (ACENLMONNPA.Parameters.FinalAttributes.Get(_Probability.Second, ref OEMALIFPGPO) && !string.IsNullOrEmpty(_Probability.Second))
		{
			return _Probability.First * (float)OEMALIFPGPO;
		}
		return _Probability.First;
	}

	public float GetDamage(Model ACENLMONNPA)
	{
		int OEMALIFPGPO = 0;
		if (ACENLMONNPA.Parameters.FinalAttributes.Get(_Damage.Second, ref OEMALIFPGPO) && !string.IsNullOrEmpty(_Damage.Second))
		{
			return _Damage.First * (float)OEMALIFPGPO;
		}
		return _Damage.First;
	}
}
