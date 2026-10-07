using System;
using System.Collections.Generic;
using System.Xml;
using CodeStage.AntiCheat.ObscuredTypes;

public class UpgradeData : IComparable<UpgradeData>
{
	public struct UpgradeFieldFlags
	{
		public bool HasDeliveryTime;

		public bool HasBonusDeliveryPrice;

		public bool HasType;

		public bool HasPrice;

		public bool HasBonusPrice;

		public bool Level;

		public bool HasUpgradeLevel;

		public bool HasMilestone;
	}

	public struct UpgradeValues
	{
		public Attributes Attributes;

		public ObscuredLong BonusDeliveryPrice;

		public ObscuredLong BonusPrice;

		public ObscuredLong Price;

		public string ItemType;

		public long DeliveryTime;

		public int Level;

		public int UpgradeLevel;

		public int Milestone;
	}

	public UpgradeValues Values;

	public UpgradeFieldFlags HasValues;

	public int UpgradeIndex;

	public UpgradeData(XmlNode node, string LFLGCDNKNJI)
	{
		HasValues.HasBonusDeliveryPrice = false;
		HasValues.HasBonusPrice = false;
		HasValues.HasDeliveryTime = false;
		HasValues.Level = false;
		HasValues.HasMilestone = false;
		HasValues.HasPrice = false;
		HasValues.HasUpgradeLevel = false;
		HasValues.HasType = false;
		Values.BonusDeliveryPrice = (ObscuredLong)(0L);
		Values.BonusPrice = (ObscuredLong)(0L);
		Values.Price = (ObscuredLong)(0L);
		Values.DeliveryTime = 0L;
		Values.Level = 0;
		Values.Milestone = 0;
		Values.UpgradeLevel = 0;
		Values.ItemType = LFLGCDNKNJI;
		Values.Attributes = new Attributes();
		List<WarriorAttribute> iBLHIAHECLK = GameUtils.WarriorAttributeList.AttributeList;
		foreach (WarriorAttribute item in iBLHIAHECLK)
		{
			XmlAttribute xmlAttribute = node.Attributes[item.get_Name()];
			if (xmlAttribute != null)
			{
				Values.Attributes.Set(item.get_Name(), xmlAttribute.ParseInt());
			}
		}
		XmlAttribute xmlAttribute2 = node.Attributes["DeliveryTime"];
		if (xmlAttribute2 != null)
		{
			HasValues.HasDeliveryTime = true;
			Values.DeliveryTime = xmlAttribute2.ParseLong(0L);
		}
		XmlAttribute xmlAttribute3 = node.Attributes["BonusDeliveryPrice"];
		if (xmlAttribute3 != null)
		{
			HasValues.HasBonusDeliveryPrice = true;
			Values.BonusDeliveryPrice = (ObscuredLong)(xmlAttribute3.ParseLong(0L));
		}
		HasValues.HasType = true;
		Values.ItemType = LFLGCDNKNJI;
		XmlAttribute xmlAttribute4 = node.Attributes["Price"];
		if (xmlAttribute4 != null)
		{
			HasValues.HasPrice = true;
			Values.Price = (ObscuredLong)(xmlAttribute4.ParseLong(0L));
		}
		XmlAttribute xmlAttribute5 = node.Attributes["BonusPrice"];
		if (xmlAttribute5 != null)
		{
			HasValues.HasBonusPrice = true;
			Values.BonusPrice = (ObscuredLong)(xmlAttribute5.ParseLong(0L));
		}
		XmlAttribute xmlAttribute6 = node.Attributes["Level"];
		if (xmlAttribute6 != null)
		{
			HasValues.Level = true;
			Values.Level = xmlAttribute6.ParseInt();
		}
		XmlAttribute xmlAttribute7 = node.Attributes["UpgradeLevel"];
		if (xmlAttribute7 != null)
		{
			HasValues.HasUpgradeLevel = true;
			Values.UpgradeLevel = xmlAttribute7.ParseInt();
		}
		XmlAttribute xmlAttribute8 = node.Attributes["Milestone"];
		if (xmlAttribute8 != null)
		{
			HasValues.HasMilestone = true;
			Values.Milestone = xmlAttribute8.ParseInt();
		}
	}

	public UpgradeData(UpgradeData NOLFMPDGCOC)
	{
		HasValues.HasBonusDeliveryPrice = NOLFMPDGCOC.HasValues.HasBonusDeliveryPrice;
		HasValues.HasBonusPrice = NOLFMPDGCOC.HasValues.HasBonusPrice;
		HasValues.HasDeliveryTime = NOLFMPDGCOC.HasValues.HasDeliveryTime;
		HasValues.Level = NOLFMPDGCOC.HasValues.Level;
		HasValues.HasMilestone = NOLFMPDGCOC.HasValues.HasMilestone;
		HasValues.HasPrice = NOLFMPDGCOC.HasValues.HasPrice;
		HasValues.HasUpgradeLevel = NOLFMPDGCOC.HasValues.HasUpgradeLevel;
		HasValues.HasType = NOLFMPDGCOC.HasValues.HasType;
		Values.Attributes = NOLFMPDGCOC.Values.Attributes;
		Values.BonusDeliveryPrice = NOLFMPDGCOC.Values.BonusDeliveryPrice;
		Values.BonusPrice = NOLFMPDGCOC.Values.BonusPrice;
		Values.DeliveryTime = NOLFMPDGCOC.Values.DeliveryTime;
		Values.Level = NOLFMPDGCOC.Values.Level;
		Values.Milestone = NOLFMPDGCOC.Values.Milestone;
		Values.Price = NOLFMPDGCOC.Values.Price;
		Values.UpgradeLevel = NOLFMPDGCOC.Values.UpgradeLevel;
		Values.ItemType = NOLFMPDGCOC.Values.ItemType;
	}

	public int CompareTo(UpgradeData NOLFMPDGCOC)
	{
		return (Values.UpgradeLevel >= NOLFMPDGCOC.Values.UpgradeLevel) ? 1 : (-1);
	}

	public void RandomizeObscuredVars()
	{
		Values.BonusDeliveryPrice.RandomizeCryptoKey();
		Values.BonusPrice.RandomizeCryptoKey();
		Values.Price.RandomizeCryptoKey();
	}
}
