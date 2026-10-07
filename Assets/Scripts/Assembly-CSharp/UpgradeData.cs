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

	public UpgradeData(XmlNode node, string itemType)
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
		Values.ItemType = itemType;
		Values.Attributes = new Attributes();
		List<WarriorAttribute> attributes = GameUtils.WarriorAttributeList.AttributeList;
		foreach (WarriorAttribute item in attributes)
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
		Values.ItemType = itemType;
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

	public UpgradeData(UpgradeData source)
	{
		HasValues.HasBonusDeliveryPrice = source.HasValues.HasBonusDeliveryPrice;
		HasValues.HasBonusPrice = source.HasValues.HasBonusPrice;
		HasValues.HasDeliveryTime = source.HasValues.HasDeliveryTime;
		HasValues.Level = source.HasValues.Level;
		HasValues.HasMilestone = source.HasValues.HasMilestone;
		HasValues.HasPrice = source.HasValues.HasPrice;
		HasValues.HasUpgradeLevel = source.HasValues.HasUpgradeLevel;
		HasValues.HasType = source.HasValues.HasType;
		Values.Attributes = source.Values.Attributes;
		Values.BonusDeliveryPrice = source.Values.BonusDeliveryPrice;
		Values.BonusPrice = source.Values.BonusPrice;
		Values.DeliveryTime = source.Values.DeliveryTime;
		Values.Level = source.Values.Level;
		Values.Milestone = source.Values.Milestone;
		Values.Price = source.Values.Price;
		Values.UpgradeLevel = source.Values.UpgradeLevel;
		Values.ItemType = source.Values.ItemType;
	}

	public int CompareTo(UpgradeData other)
	{
		return (Values.UpgradeLevel >= other.Values.UpgradeLevel) ? 1 : (-1);
	}

	public void RandomizeObscuredVars()
	{
		Values.BonusDeliveryPrice.RandomizeCryptoKey();
		Values.BonusPrice.RandomizeCryptoKey();
		Values.Price.RandomizeCryptoKey();
	}
}
