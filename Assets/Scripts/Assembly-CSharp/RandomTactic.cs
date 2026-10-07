using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class RandomTactic
{
	public class TacticDelay
	{
		public string animationName;

		public int minDelay;

		public int maxDelay;
	}

	public List<string> Intervals = new List<string>();

	public List<TacticDelay> delays = new List<TacticDelay>();

	public float BeginnerCheat;

	public void Parse(XmlNode node)
	{
		delays.Clear();
		XmlNode xmlNode = node["Intervals"];
		foreach (XmlNode childNode in xmlNode.ChildNodes)
		{
			Intervals.Add(childNode.Attributes["Name"].GetStringOrDefault(string.Empty));
		}
		XmlNode xmlNode3 = node["Delays"];
		foreach (XmlNode childNode2 in xmlNode3.ChildNodes)
		{
			TacticDelay eILEKOMGNOP = new TacticDelay();
			eILEKOMGNOP.animationName = childNode2.Attributes["Animation"].GetStringOrDefault(string.Empty);
			eILEKOMGNOP.minDelay = childNode2.Attributes["Min"].ParseInt();
			eILEKOMGNOP.maxDelay = childNode2.Attributes["Max"].ParseInt();
			delays.Add(eILEKOMGNOP);
		}
		XmlNode xmlNode5 = node["BeginnerCheat"];
		BeginnerCheat = xmlNode5.Attributes["Treshold"].ParseFloat();
	}

	public bool IsIntervalByName(string name)
	{
		return Intervals.Contains(name);
	}

	public int GetDelayByName(List<string> NIKHAICFGNM)
	{
		foreach (TacticDelay item in delays)
		{
			foreach (string item2 in NIKHAICFGNM)
			{
				if (item.animationName == item2)
				{
					return Eclipse.Multiplayer.VersusDeterminism.Range(item.minDelay, item.maxDelay) + 1;
				}
			}
		}
		return 0;
	}
}
