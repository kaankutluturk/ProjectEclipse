using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class MapButtonController : global::EventDispatcher<MapButtonInfo>
{
	public enum MapButtonEvent
	{
		MAP_BUTTON_INFO_ADD = 0,
		MAP_BUTTON_INFO_REMOVE = 1
	}

	private static MapButtonController _instance;

	private XmlNode _node;

	private List<MapButtonInfo> buttons = new List<MapButtonInfo>();

	public static MapButtonController Instance
	{
		get
		{
			return GetInstance();
		}
	}

	public List<MapButtonInfo> StoryButtons
	{
		get
		{
			return GetStoryButtons();
		}
	}

	public static MapButtonController GetInstance()
	{
		if (_instance == null)
		{
			_instance = new MapButtonController();
		}
		return _instance;
	}

	public List<MapButtonInfo> GetStoryButtons()
	{
		return buttons.FindAll(IsStoryButton);
	}

	public bool IsStoryButton(MapButtonInfo KLNKEPMAGKF)
	{
		MapButtonInfo.MapButtonShowType hNEJAKIGDBA = KLNKEPMAGKF.GetShowType();
		return hNEJAKIGDBA == MapButtonInfo.MapButtonShowType.Story || hNEJAKIGDBA == MapButtonInfo.MapButtonShowType.Both;
	}

	public void AddButton(MapButtonInfo DJDNMAOEFBD)
	{
		if (DJDNMAOEFBD != null)
		{
			MapButtonInfo eBMMANKELOA = buttons.Find((MapButtonInfo DHDMNHCIPEH) => DHDMNHCIPEH.Name.Equals(DJDNMAOEFBD.Name));
			if (eBMMANKELOA != null)
			{
				if (SamePresentation(eBMMANKELOA, DJDNMAOEFBD)) return;
				// A quest can move a button across mod versions; replace the saved
				// presentation and refresh any map that is already open.
				RemoveButtonFromXml(eBMMANKELOA.Name);
				buttons.Remove(eBMMANKELOA);
				CallEvent(1, eBMMANKELOA);
			}
			SaveButtonToXml(DJDNMAOEFBD);
			buttons.Add(DJDNMAOEFBD);
			CallEvent(0, DJDNMAOEFBD);
		}
	}

	private static bool SamePresentation(MapButtonInfo current, MapButtonInfo next)
	{
		return current.ImageName == next.ImageName &&
			current.Timer == next.Timer && current.AtlasName == next.AtlasName &&
			(current.TypeName == next.TypeName ||
				current.TypeName == "Image" && string.IsNullOrEmpty(next.TypeName)) &&
			current.Position == next.Position &&
			current.AnchorMinX == next.AnchorMinX && current.AnchorMaxX == next.AnchorMaxX &&
			current.AutoPosition == next.AutoPosition &&
			current.Speed == next.Speed && current.Pause == next.Pause &&
			current.ShowTypeName == next.ShowTypeName;
	}

	public void RemoveButton(MapButtonInfo DJDNMAOEFBD)
	{
		if (DJDNMAOEFBD != null)
		{
			RemoveButton(DJDNMAOEFBD.Name);
		}
	}

	public void RemoveButton(string name)
	{
		MapButtonInfo eBMMANKELOA = buttons.Find((MapButtonInfo DHDMNHCIPEH) => DHDMNHCIPEH.Name.Equals(name));
		if (eBMMANKELOA != null)
		{
			RemoveButtonFromXml(name);
			buttons.Remove(eBMMANKELOA);
			CallEvent(1, eBMMANKELOA);
		}
	}

	public void Parse(XmlNode node)
	{
		EnsureButtonsNode(node);
		Clear();
		foreach (XmlNode childNode in _node.ChildNodes)
		{
			MapButtonInfo eBMMANKELOA = new MapButtonInfo();
			bool nEOIMNAHLAN = childNode.Attributes["X"].Empty() || childNode.Attributes["Y"].Empty();
			float x = childNode.Attributes["X"].ParseFloat();
			float y = childNode.Attributes["Y"].ParseFloat();
			eBMMANKELOA.Name = childNode.Attributes["Name"].GetStringOrDefault();
			eBMMANKELOA.ImageName = childNode.Attributes["Image"].GetStringOrDefault();
			eBMMANKELOA.Timer = childNode.Attributes["Timer"].GetStringOrDefault();
			eBMMANKELOA.TypeName = childNode.Attributes["Type"].GetStringOrDefault("Image");
			eBMMANKELOA.AtlasName = childNode.Attributes["Atlas"].GetStringOrDefault();
			eBMMANKELOA.Speed = childNode.Attributes["Speed"].ParseFloat();
			eBMMANKELOA.Pause = childNode.Attributes["Pause"].ParseFloat();
			eBMMANKELOA.Position = new Vector2(x, y);
			float defaultAnchorX = (eBMMANKELOA.Name == "EclipseModeOn" || eBMMANKELOA.Name == "EclipseModeOff") ? 1f : 0.5f;
			eBMMANKELOA.AnchorMinX = childNode.Attributes["AnchorMinX"].ParseFloat(defaultAnchorX);
			eBMMANKELOA.AnchorMaxX = childNode.Attributes["AnchorMaxX"].ParseFloat(eBMMANKELOA.AnchorMinX);
			eBMMANKELOA.AutoPosition = nEOIMNAHLAN;
			eBMMANKELOA.ShowTypeName = childNode.Attributes["ShowType"].GetStringOrDefault("Story");
			buttons.Add(eBMMANKELOA);
		}
	}

	private void Clear()
	{
		buttons.Clear();
	}

	private void EnsureButtonsNode(XmlNode node)
	{
		if (node != null)
		{
			_node = node["MapButtons"];
			if (_node == null)
			{
				node.AppendElement("MapButtons");
				_node = node["MapButtons"];
				ListSF.GetInstance().RequestSave();
			}
		}
	}

	private void SaveButtonToXml(MapButtonInfo DJDNMAOEFBD)
	{
		XmlNode mEEAKLDGLDF = _node.AppendElement("Button");
		mEEAKLDGLDF.AppendAttribute("Name").Value = DJDNMAOEFBD.Name;
		mEEAKLDGLDF.AppendAttribute("Image").Value = DJDNMAOEFBD.ImageName;
		mEEAKLDGLDF.AppendAttribute("Type").Value = DJDNMAOEFBD.TypeName;
		if (DJDNMAOEFBD.Speed > 0f)
		{
			mEEAKLDGLDF.AppendAttribute("Speed").Value = DJDNMAOEFBD.Speed.ToString();
		}
		if (DJDNMAOEFBD.Pause > 0f)
		{
			mEEAKLDGLDF.AppendAttribute("Pause").Value = DJDNMAOEFBD.Pause.ToString();
		}
		if (!DJDNMAOEFBD.AutoPosition)
		{
			mEEAKLDGLDF.AppendAttribute("X").Value = DJDNMAOEFBD.Position.x.ToString();
			mEEAKLDGLDF.AppendAttribute("Y").Value = DJDNMAOEFBD.Position.y.ToString();
			mEEAKLDGLDF.AppendAttribute("AnchorMinX").Value = DJDNMAOEFBD.AnchorMinX.ToString();
			mEEAKLDGLDF.AppendAttribute("AnchorMaxX").Value = DJDNMAOEFBD.AnchorMaxX.ToString();
		}
		if (!string.IsNullOrEmpty(DJDNMAOEFBD.AtlasName))
		{
			mEEAKLDGLDF.AppendAttribute("Atlas").Value = DJDNMAOEFBD.AtlasName;
		}
		if (!string.IsNullOrEmpty(DJDNMAOEFBD.Timer))
		{
			mEEAKLDGLDF.AppendAttribute("Timer").Value = DJDNMAOEFBD.Timer;
		}
		mEEAKLDGLDF.AppendAttribute("ShowType").Value = DJDNMAOEFBD.ShowTypeName;
		ListSF.GetInstance().RequestSave();
	}

	private void RemoveButtonFromXml(MapButtonInfo DJDNMAOEFBD)
	{
		RemoveButtonFromXml(DJDNMAOEFBD.Name);
	}

	private void RemoveButtonFromXml(string name)
	{
		XmlNode xmlNode = _node.FindChildWithAttribute("Button", "Name", name);
		if (xmlNode != null)
		{
			_node.RemoveChild(xmlNode);
			ListSF.GetInstance().RequestSave();
		}
	}
}
