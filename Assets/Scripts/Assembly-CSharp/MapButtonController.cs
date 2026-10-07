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

	public bool IsStoryButton(MapButtonInfo button)
	{
		MapButtonInfo.MapButtonShowType showType = button.GetShowType();
		return showType == MapButtonInfo.MapButtonShowType.Story || showType == MapButtonInfo.MapButtonShowType.Both;
	}

	public void AddButton(MapButtonInfo button)
	{
		if (button != null)
		{
			MapButtonInfo existingButton = buttons.Find((MapButtonInfo candidate) => candidate.Name.Equals(button.Name));
			if (existingButton != null)
			{
				if (SamePresentation(existingButton, button)) return;
				// A quest can move a button across mod versions; replace the saved
				// presentation and refresh any map that is already open.
				RemoveButtonFromXml(existingButton.Name);
				buttons.Remove(existingButton);
				CallEvent(1, existingButton);
			}
			SaveButtonToXml(button);
			buttons.Add(button);
			CallEvent(0, button);
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

	public void RemoveButton(MapButtonInfo button)
	{
		if (button != null)
		{
			RemoveButton(button.Name);
		}
	}

	public void RemoveButton(string name)
	{
		MapButtonInfo existingButton = buttons.Find((MapButtonInfo candidate) => candidate.Name.Equals(name));
		if (existingButton != null)
		{
			RemoveButtonFromXml(name);
			buttons.Remove(existingButton);
			CallEvent(1, existingButton);
		}
	}

	public void Parse(XmlNode node)
	{
		EnsureButtonsNode(node);
		Clear();
		foreach (XmlNode childNode in _node.ChildNodes)
		{
			MapButtonInfo button = new MapButtonInfo();
			bool autoPosition = childNode.Attributes["X"].Empty() || childNode.Attributes["Y"].Empty();
			float x = childNode.Attributes["X"].ParseFloat();
			float y = childNode.Attributes["Y"].ParseFloat();
			button.Name = childNode.Attributes["Name"].GetStringOrDefault();
			button.ImageName = childNode.Attributes["Image"].GetStringOrDefault();
			button.Timer = childNode.Attributes["Timer"].GetStringOrDefault();
			button.TypeName = childNode.Attributes["Type"].GetStringOrDefault("Image");
			button.AtlasName = childNode.Attributes["Atlas"].GetStringOrDefault();
			button.Speed = childNode.Attributes["Speed"].ParseFloat();
			button.Pause = childNode.Attributes["Pause"].ParseFloat();
			button.Position = new Vector2(x, y);
			float defaultAnchorX = (button.Name == "EclipseModeOn" || button.Name == "EclipseModeOff") ? 1f : 0.5f;
			button.AnchorMinX = childNode.Attributes["AnchorMinX"].ParseFloat(defaultAnchorX);
			button.AnchorMaxX = childNode.Attributes["AnchorMaxX"].ParseFloat(button.AnchorMinX);
			button.AutoPosition = autoPosition;
			button.ShowTypeName = childNode.Attributes["ShowType"].GetStringOrDefault("Story");
			buttons.Add(button);
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

	private void SaveButtonToXml(MapButtonInfo button)
	{
		XmlNode buttonNode = _node.AppendElement("Button");
		buttonNode.AppendAttribute("Name").Value = button.Name;
		buttonNode.AppendAttribute("Image").Value = button.ImageName;
		buttonNode.AppendAttribute("Type").Value = button.TypeName;
		if (button.Speed > 0f)
		{
			buttonNode.AppendAttribute("Speed").Value = button.Speed.ToString();
		}
		if (button.Pause > 0f)
		{
			buttonNode.AppendAttribute("Pause").Value = button.Pause.ToString();
		}
		if (!button.AutoPosition)
		{
			buttonNode.AppendAttribute("X").Value = button.Position.x.ToString();
			buttonNode.AppendAttribute("Y").Value = button.Position.y.ToString();
			buttonNode.AppendAttribute("AnchorMinX").Value = button.AnchorMinX.ToString();
			buttonNode.AppendAttribute("AnchorMaxX").Value = button.AnchorMaxX.ToString();
		}
		if (!string.IsNullOrEmpty(button.AtlasName))
		{
			buttonNode.AppendAttribute("Atlas").Value = button.AtlasName;
		}
		if (!string.IsNullOrEmpty(button.Timer))
		{
			buttonNode.AppendAttribute("Timer").Value = button.Timer;
		}
		buttonNode.AppendAttribute("ShowType").Value = button.ShowTypeName;
		ListSF.GetInstance().RequestSave();
	}

	private void RemoveButtonFromXml(MapButtonInfo button)
	{
		RemoveButtonFromXml(button.Name);
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
