using System;
using System.Xml;

public class Trick : IComparable<Trick>
{
	public string Icon;

	public string Name;

	public string DisplayName { get; private set; }

	public string KeysDescription;

	public string EffectDescription;

	public int Rank;

	public bool IsNew;

	public InfoAnimation Animation;

	public Trick(XmlNode node, InfoAnimation animation)
	{
		Icon = node.Attributes["Icon"].GetStringOrDefault(string.Empty);
		Rank = node.Attributes["Rank"].ParseInt();
		KeysDescription = node.Attributes["KeysDescription"].GetStringOrDefault(string.Empty);
		EffectDescription = node.Attributes["EffectDescription"].GetStringOrDefault(string.Empty);
		Name = animation.Name;
		DisplayName = node.Attributes["DisplayName"].GetStringOrDefault(Name);
		Animation = animation;
		IsNew = false;
	}

	public Trick(string icon, string _name, InfoAnimation animation, int rank, string keysDescription, string effectDescription)
	{
		Icon = icon;
		Name = _name;
		DisplayName = _name;
		Animation = animation;
		Rank = rank;
		KeysDescription = keysDescription;
		EffectDescription = effectDescription;
		IsNew = false;
	}

	public static bool Compare(Trick left, Trick right)
	{
		return left.Rank < right.Rank;
	}

	public int CompareTo(Trick other)
	{
		return Rank.CompareTo(other.Rank);
	}
}
