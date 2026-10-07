using System.Xml;

public class ActionSound : ActionAnimation
{
	private string _Name;

	private bool _AnyGender;

	private string _Gender;

	private bool _Looped;

	private float _Volume;

	public bool IsLooped
	{
		get
		{
			return GetIsLooped();
		}
	}

	public float Volume
	{
		get
		{
			return GetVolume();
		}
	}

	public ActionSound(XmlNode node)
		: base(ActionType.SOUND)
	{
		Parse(node);
	}

	public string get_Name()
	{
		return _Name;
	}

	public bool GetIsLooped()
	{
		return _Looped;
	}

	public float GetVolume()
	{
		return _Volume;
	}

	public override void Visit(Model model)
	{
		model.StartAction(this);
	}

	public bool SameGender(string gender)
	{
		return _AnyGender || gender == _Gender;
	}

	protected override void Parse(XmlNode node)
	{
		base.Parse(node);
		_Name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		_Volume = node.Attributes["Volume"].ParseFloat(1f);
		_Looped = node.Attributes["Looped"].ParseBool();
		XmlAttribute xmlAttribute = node.Attributes["Voice"];
		_AnyGender = xmlAttribute == null;
		_Gender = xmlAttribute.GetStringOrDefault(string.Empty);
	}
}
