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

	public TemplateAnimation(InfoAnimation EDMCLHEOJGD)
	{
		_Name = EDMCLHEOJGD.Name;
		AddAnimation(EDMCLHEOJGD);
	}

	public string get_Name()
	{
		return _Name;
	}

	public List<InfoAnimation> GetAnimations()
	{
		return _animations;
	}

	public void AddAnimation(InfoAnimation DBOLBEOCEME)
	{
		_animations.AddIfNotExist(DBOLBEOCEME);
		DBOLBEOCEME.AddTemplateName(_Name);
	}
}
