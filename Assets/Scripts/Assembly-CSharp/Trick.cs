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

	public Trick(XmlNode BHBHAOJHABE, InfoAnimation KJHGIKMFJOB)
	{
		Icon = BHBHAOJHABE.Attributes["Icon"].GetStringOrDefault(string.Empty);
		Rank = BHBHAOJHABE.Attributes["Rank"].ParseInt();
		KeysDescription = BHBHAOJHABE.Attributes["KeysDescription"].GetStringOrDefault(string.Empty);
		EffectDescription = BHBHAOJHABE.Attributes["EffectDescription"].GetStringOrDefault(string.Empty);
		Name = KJHGIKMFJOB.Name;
		DisplayName = BHBHAOJHABE.Attributes["DisplayName"].GetStringOrDefault(Name);
		Animation = KJHGIKMFJOB;
		IsNew = false;
	}

	public Trick(string NCKCDCODNHA, string _name, InfoAnimation KJHGIKMFJOB, int HEIBENBPNLN, string PHDCIEGEKBC, string LDKAELDNKGH)
	{
		Icon = NCKCDCODNHA;
		Name = _name;
		DisplayName = _name;
		Animation = KJHGIKMFJOB;
		Rank = HEIBENBPNLN;
		KeysDescription = PHDCIEGEKBC;
		EffectDescription = LDKAELDNKGH;
		IsNew = false;
	}

	public static bool Compare(Trick KOOLDHKJHNH, Trick MHFCMOONCHB)
	{
		return KOOLDHKJHNH.Rank < MHFCMOONCHB.Rank;
	}

	public int CompareTo(Trick NOLFMPDGCOC)
	{
		return Rank.CompareTo(NOLFMPDGCOC.Rank);
	}
}
