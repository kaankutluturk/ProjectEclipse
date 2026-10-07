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
			MagicCharge charge = new MagicCharge();
			charges.Add(charge);
			charge.Parse(childNode);
		}
	}

	private float GetChargeValue(string chargeName, ModelParameters parameters = null)
	{
		MagicCharge charge = FindCharge(chargeName);
		if (charge == null)
		{
			GameLog.Error(chargeName + " for Magic not found");
			return 0f;
		}
		if (parameters == null)
		{
			return charge.GetBase();
		}
		int attributeValue = 0;
		if (parameters.FinalAttributes.Get(charge.GetAttribute(), ref attributeValue))
		{
			return charge.GetBase() * (float)attributeValue;
		}
		return charge.GetBase();
	}

	public float GetInitialCharge(Model model)
	{
		return GetInitialCharge(model.Parameters);
	}

	public float GetPainRecharge(Model model)
	{
		return GetPainRecharge(model.Parameters);
	}

	public float GetDamageRecharge(Model model)
	{
		return GetDamageRecharge(model.Parameters);
	}

	public float GetInitialCharge(ModelParameters parameters)
	{
		return GetChargeValue("InitialCharge", parameters);
	}

	public float GetInitialCharge()
	{
		return GetChargeValue("InitialCharge");
	}

	public float GetPainRecharge(ModelParameters parameters)
	{
		return GetChargeValue("PainRecharge", parameters);
	}

	public float GetPainRecharge()
	{
		return GetChargeValue("PainRecharge");
	}

	public float GetDamageRecharge(ModelParameters parameters)
	{
		return GetChargeValue("DamageRecharge", parameters);
	}

	public float GetDamageRecharge()
	{
		return GetChargeValue("DamageRecharge");
	}
}
