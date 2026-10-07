using System.Collections.Generic;
using System.Xml;

public class TemplateAnimation
{
	private string _Name;

	private List<InfoAnimation> _animations = new List<InfoAnimation>();

	public List<InfoAnimation> Animations
	{
		get
		{
			return GetAnimations();
		}
	}

	public TemplateAnimation(XmlNode node)
	{
		_Name = XmlUtils.ParseString(node.Attributes["Name"]);
	}

	public TemplateAnimation(InfoAnimation animation)
	{
		_Name = animation.Name;
		AddAnimation(animation);
	}

	public string get_Name()
	{
		return _Name;
	}

	public List<InfoAnimation> GetAnimations()
	{
		return _animations;
	}

	public void AddAnimation(InfoAnimation animation)
	{
		_animations.AddIfNotExist(animation);
		animation.AddTemplateName(_Name);
	}
}
