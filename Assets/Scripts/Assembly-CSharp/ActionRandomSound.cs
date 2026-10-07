using System.Collections.Generic;
using System.Xml;

public class ActionRandomSound : ActionAnimation
{
	private List<string> _Names = new List<string>();

	private bool _AnyGender;

	private string _Gender;

	public ActionRandomSound(XmlNode node)
		: base(ActionType.RANDOM_SOUND)
	{
		Parse(node);
	}

	public string get_Name()
	{
		return _Names.GetRandomElement();
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
		XmlAttribute xmlAttribute = node.Attributes["Voice"];
		_AnyGender = xmlAttribute == null;
		_Gender = xmlAttribute.GetStringOrDefault(string.Empty);
		foreach (XmlNode childNode in node.ChildNodes)
		{
			_Names.Add(childNode.Attributes["Name"].GetStringOrDefault("ERR_RAND_SOUND_NO_NAME"));
		}
	}
}
