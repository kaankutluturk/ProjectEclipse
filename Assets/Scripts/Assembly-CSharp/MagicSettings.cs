using System.Collections.Generic;
using System.Xml;

public class MagicSettings
{
	private class MagicCharge
	{
		private string _Name;

		private float _Base;

		private string _AttributeName;

		public float Base
		{
			get
			{
				return GetBase();
			}
		}

		public string Attribute
		{
			get
			{
				return GetAttribute();
			}
		}

		public string get_Name()
		{
			return _Name;
		}

		public float GetBase()
		{
			return _Base;
		}

		public string GetAttribute()
		{
			return _AttributeName;
		}

		public void Parse(XmlNode node)
		{
			_Name = node.Name;
			_Base = XmlUtils.ParseFloat(node.Attributes["Base"]);
			_AttributeName = XmlUtils.ParseString(node.Attributes["Attribute"]);
		}
	}

	private List<MagicCharge> charges = new List<MagicCharge>();

	private MagicCharge FindCharge(string name)
	{
		for (int i = 0; i < charges.Count; i++)
		{
			if (charges[i].get_Name() == name)
			{
				return charges[i];
			}
		}
		return null;
	}

	public void Parse(XmlNode node)
	{
		charges.Clear();
		foreach (XmlNode childNode in node.ChildNodes)
		{
			MagicCharge eHONFFPDKOI = new MagicCharge();
			charges.Add(eHONFFPDKOI);
			eHONFFPDKOI.Parse(childNode);
		}
	}

	private float GetChargeValue(string JLEKBBJBLOE, ModelParameters IHEFAMAFBIA = null)
	{
		MagicCharge eHONFFPDKOI = FindCharge(JLEKBBJBLOE);
		if (eHONFFPDKOI == null)
		{
			GameLog.Error(JLEKBBJBLOE + " for Magic not found");
			return 0f;
		}
		if (IHEFAMAFBIA == null)
		{
			return eHONFFPDKOI.GetBase();
		}
		int OEMALIFPGPO = 0;
		if (IHEFAMAFBIA.FinalAttributes.Get(eHONFFPDKOI.GetAttribute(), ref OEMALIFPGPO))
		{
			return eHONFFPDKOI.GetBase() * (float)OEMALIFPGPO;
		}
		return eHONFFPDKOI.GetBase();
	}

	public float GetInitialCharge(Model ACENLMONNPA)
	{
		return GetInitialCharge(ACENLMONNPA.Parameters);
	}

	public float GetPainRecharge(Model ACENLMONNPA)
	{
		return GetPainRecharge(ACENLMONNPA.Parameters);
	}

	public float GetDamageRecharge(Model ACENLMONNPA)
	{
		return GetDamageRecharge(ACENLMONNPA.Parameters);
	}

	public float GetInitialCharge(ModelParameters IHEFAMAFBIA)
	{
		return GetChargeValue("InitialCharge", IHEFAMAFBIA);
	}

	public float GetInitialCharge()
	{
		return GetChargeValue("InitialCharge");
	}

	public float GetPainRecharge(ModelParameters IHEFAMAFBIA)
	{
		return GetChargeValue("PainRecharge", IHEFAMAFBIA);
	}

	public float GetPainRecharge()
	{
		return GetChargeValue("PainRecharge");
	}

	public float GetDamageRecharge(ModelParameters IHEFAMAFBIA)
	{
		return GetChargeValue("DamageRecharge", IHEFAMAFBIA);
	}

	public float GetDamageRecharge()
	{
		return GetChargeValue("DamageRecharge");
	}
}
