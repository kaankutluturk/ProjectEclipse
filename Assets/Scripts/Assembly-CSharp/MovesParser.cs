using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public static class MovesParser
{
	private const string MovesFileName = "/moves.xml";

	private static Dictionary<string, XmlNode> _TemplateTemp;

	private static Dictionary<string, XmlNode> _LegacyTemplateTemp;

	private static Dictionary<string, XmlNode> _BaseTemplateNodes = new Dictionary<string, XmlNode>();

	private static Dictionary<string, XmlNode> _BaseLegacyTemplateNodes = new Dictionary<string, XmlNode>();

	private static Dictionary<string, XmlNode> _baseMoveLockSources;

	public static void Parse(string path, List<InfoAnimation> animations, Dictionary<string, TemplateAnimation> CBNKICJENCB, List<Trick> tricks, List<Trigger> triggers, bool flag)
	{
		_baseMoveLockSources = null;
		MovesMaps.Init();
		XmlDocument xmlDocument = XmlUtils.OpenXMLDocument(path + "/moves.xml", string.Empty);
		var moveLockSources = CaptureBaseMoveLockSources(xmlDocument["Movesxml"]?["Moves"]);
#if UNITY_EDITOR
		Eclipse.Content.LocalAnimationPreview.Apply(xmlDocument);
#endif
		XmlNode templatesNode = xmlDocument["Movesxml"]["Templates"];
		XmlNode movesNode = xmlDocument["Movesxml"]["Moves"];
		XmlNode triggersNode = xmlDocument["Movesxml"]["Triggers"];
		ParseTemplates(templatesNode, CBNKICJENCB);
		_BaseTemplateNodes.Clear();
		foreach (KeyValuePair<string, XmlNode> pair in _TemplateTemp)
			_BaseTemplateNodes.Add(pair.Key, pair.Value.CloneNode(true));
		_BaseLegacyTemplateNodes.Clear();
		_LegacyTemplateTemp = new Dictionary<string, XmlNode>();
		foreach (XmlNode template in xmlDocument.SelectNodes("/Movesxml/LegacyTemplates/Template"))
		{
			_LegacyTemplateTemp.Add(template.Attributes["Name"].Value, template);
			_BaseLegacyTemplateNodes[template.Attributes["Name"].Value] = template.CloneNode(true);
		}
		ParseMoves(movesNode, CBNKICJENCB, animations, tricks);
		ParseTriggers(triggersNode, triggers);
		_TemplateTemp.Clear();
		_TemplateTemp = null;
		_LegacyTemplateTemp.Clear();
		_LegacyTemplateTemp = null;
		_baseMoveLockSources = moveLockSources;
	}

	private static Dictionary<string, XmlNode> CaptureBaseMoveLockSources(XmlNode moves)
	{
		if (moves == null) return null;
		var sources = new Dictionary<string, XmlNode>(System.StringComparer.Ordinal);
		// The parser expands templates in place. Keep just the direct base Locks
		// before that expansion, in a separate document so we do not retain all XML.
		var snapshot = new XmlDocument { XmlResolver = null };
		foreach (XmlNode move in moves.ChildNodes)
		{
			if (move.NodeType != XmlNodeType.Element || move.Name != "Move") continue;
			string name = move.Attributes?["Name"]?.Value;
			if (string.IsNullOrEmpty(name)) continue;
			XmlElement source = snapshot.CreateElement("Move");
			source.SetAttribute("Name", name);
			if (move["Locks"] != null) source.AppendChild(snapshot.ImportNode(move["Locks"], true));
			sources[name] = source; // Match the recovered reader's last duplicate wins.
		}
		return sources;
	}

	internal static bool TryReadBaseMoveLockSources(HashSet<string> wanted, out Dictionary<string, XmlNode> sources)
	{
		sources = null;
		if (_baseMoveLockSources == null) return false;
		sources = new Dictionary<string, XmlNode>(System.StringComparer.Ordinal);
		foreach (string name in wanted)
			if (_baseMoveLockSources.TryGetValue(name, out XmlNode source)) sources.Add(name, source.CloneNode(true));
		return true;
	}

	internal static int ParseAdditional(XmlDocument xmlDocument, List<InfoAnimation> moves,
		Dictionary<string, TemplateAnimation> templates, List<Trick> tricks, List<Trigger> triggers)
	{
		if (xmlDocument == null || xmlDocument["Movesxml"] == null)
			throw new System.ArgumentException("External moves document must have a Movesxml root.", "xmlDocument");

		XmlNode root = xmlDocument["Movesxml"];
		XmlNode templateNodes = root["Templates"];
		XmlNode moveNodes = root["Moves"];
		XmlNode triggerNodes = root["Triggers"];
		if (moveNodes == null) throw new System.ArgumentException("External moves document is missing Moves.", "xmlDocument");

		_TemplateTemp = CloneNodeMap(_BaseTemplateNodes);
		_LegacyTemplateTemp = CloneNodeMap(_BaseLegacyTemplateNodes);
		try
		{
			HashSet<string> pendingTemplates = new HashSet<string>();
			if (templateNodes != null)
			{
				foreach (XmlNode node in templateNodes.ChildNodes)
				{
					if (node.Name != "Template") continue;
					string name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
					if (string.IsNullOrEmpty(name) || templates.ContainsKey(name) || _TemplateTemp.ContainsKey(name) || !pendingTemplates.Add(name))
						throw new System.InvalidOperationException("External move template collides with existing template '" + name + "'.");
				}
			}

			HashSet<string> pendingMoves = new HashSet<string>();
			foreach (XmlNode node in moveNodes.ChildNodes)
			{
				if (node.NodeType != XmlNodeType.Element) continue;
				string name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
				if (string.IsNullOrEmpty(name) || !pendingMoves.Add(name))
					throw new System.InvalidOperationException("External moves document contains invalid or duplicate move '" + name + "'.");
				for (int i = 0; i < moves.Count; i++)
					if (moves[i].Name == name)
						throw new System.InvalidOperationException("External move collides with existing move '" + name + "'.");
			}

			HashSet<string> pendingTriggers = new HashSet<string>();
			if (triggerNodes != null)
			{
				foreach (XmlNode node in triggerNodes.ChildNodes)
				{
					if (node.NodeType != XmlNodeType.Element) continue;
					string name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
					if (string.IsNullOrEmpty(name) || !pendingTriggers.Add(name))
						throw new System.InvalidOperationException("External moves document contains invalid or duplicate trigger '" + name + "'.");
					for (int i = 0; i < triggers.Count; i++)
						if (triggers[i].Name == name)
							throw new System.InvalidOperationException("External trigger collides with existing trigger '" + name + "'.");
				}
			}

			if (templateNodes != null)
			{
				foreach (XmlNode node in templateNodes.ChildNodes)
				{
					if (node.Name != "Template") continue;
					string name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
					TemplateAnimation template = new TemplateAnimation(node);
					templates.Add(name, template);
					_TemplateTemp.Add(name, node);
					_BaseTemplateNodes.Add(name, node.CloneNode(true));
				}
			}

			int before = moves.Count;
			ParseMoves(moveNodes, templates, moves, tricks);
			if (triggerNodes != null) ParseTriggers(triggerNodes, triggers);
			return moves.Count - before;
		}
		finally
		{
			_TemplateTemp.Clear();
			_TemplateTemp = null;
			_LegacyTemplateTemp.Clear();
			_LegacyTemplateTemp = null;
		}
	}

	private static Dictionary<string, XmlNode> CloneNodeMap(Dictionary<string, XmlNode> source)
	{
		Dictionary<string, XmlNode> result = new Dictionary<string, XmlNode>();
		foreach (KeyValuePair<string, XmlNode> pair in source)
			result.Add(pair.Key, pair.Value.CloneNode(true));
		return result;
	}

	private static void SetMoveTemplate(XmlNode moveNode, XmlNode templateNode, List<XmlNode> appliedTemplates, Dictionary<string, XmlNode> templates)
	{
		XmlAttribute xmlAttribute = templateNode.Attributes["Template"];
		if (xmlAttribute == null)
		{
			return;
		}
		bool flag = false;
		string[] array = xmlAttribute.Value.Split('|');
		for (int i = 0; i < array.Length; i++)
		{
			flag = false;
			for (int j = 0; j < appliedTemplates.Count; j++)
			{
				if (array[i] == appliedTemplates[j].Attributes["Name"].Value)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				XmlNode value = null;
				if (templates.TryGetValue(array[i], out value))
				{
					appliedTemplates.Add(value);
					AddAttributes(moveNode, value);
					SetMoveTemplate(moveNode, value, appliedTemplates, templates);
				}
				else
				{
					GameLog.Write("Move template don't find: " + array[i]);
				}
			}
		}
	}

	private static void AddAttributes(XmlNode targetNode, XmlNode sourceNode)
	{
		foreach (XmlAttribute attribute in sourceNode.Attributes)
		{
			string name = attribute.Name;
			XmlAttribute xmlAttribute2 = targetNode.Attributes[name];
			if (xmlAttribute2 == null)
			{
				targetNode.CopyAttribute(attribute);
			}
		}
	}

	private static List<InfoAnimation> ParseMoves(XmlNode nodes, Dictionary<string, TemplateAnimation> JIGEFEPNCIN, List<InfoAnimation> animations, List<Trick> tricks)
	{
		List<global::Pair<InfoAnimation, string>> list = new List<global::Pair<InfoAnimation, string>>();
		list.Capacity = 100;
		List<XmlNode> list2 = new List<XmlNode>();
		foreach (XmlNode childNode in nodes.ChildNodes)
		{
			list2.Clear();
			// Imported legacy moves retain their own inheritance graph. Template
			// names remain unchanged for CurrentAnimation and AI membership checks.
			Dictionary<string, XmlNode> templates = childNode.Attributes["UseLegacyTemplates"].ParseBool()
				? _LegacyTemplateTemp : _TemplateTemp;
			SetMoveTemplate(childNode, childNode, list2, templates);
			InfoAnimation animation = new InfoAnimation();
			animation.Name = childNode.Attributes["Name"].GetStringOrDefault(string.Empty);
			animation.Id = childNode.Attributes["ID"].ParseInt();
			animation.FileName = childNode.Attributes["FileName"].GetStringOrDefault(string.Empty);
			animation.MidFrames = childNode.Attributes["MidFrames"].ParseInt();
			animation.FirstFrame = childNode.Attributes["FirstFrame"].ParseInt();
			animation.AnimationEndFrame = childNode.Attributes["EndFrame"].ParseInt();
			animation.Priority = childNode.Attributes["Priority"].ParseInt();
			animation.SetNoMagicRecharge(childNode.Attributes["NoMagicRecharge"].ParseBool());
			animation.Type = InfoAnimation.AnimationKind.AnimationNone;
			animation.TutorialType = InfoAnimation.TutorialKind.TutorialNone;
			animation.NoWallRepulsion = childNode.Attributes["NoWallRepulsion"].ParseBool();
			animation.StyleFactor = childNode.Attributes["StyleFactor"].ParseFloat(1f);
			animation.HasPhysics = childNode.Attributes["Physics"].ParseBool();
			animation.EndsStage = childNode.Attributes["EndsStage"].ParseBool();
			animation.SetIsLooped(childNode.Attributes["Looped"].ParseBool());
			animation.NoInterpolationFrames = childNode.Attributes["NoInterpolationFrames"].ParseBool();
			animation.AlignOnParentWallCollision = childNode.Attributes["AlignOnParentWallCollision"].ParseBool();
			animation.LoadAnimationClip();
			animation.AddTemplateName(animation.Name);
			XmlAttribute xmlAttribute = childNode.Attributes["MirrorNode"];
			if (xmlAttribute != null)
			{
				animation.GetMirrorNode().SetNodeName(xmlAttribute.GetStringOrDefault(string.Empty));
			}
			xmlAttribute = childNode.Attributes["CameraCOMAlignStage"];
			if (xmlAttribute != null)
			{
				StageType.Stage cameraStage = StageType.GetStageByName(xmlAttribute.GetStringOrDefault(string.Empty));
				animation.SetCameraStage(cameraStage);
			}
			animation.SetTacticWeapons(childNode.Attributes["TacticWeapon"].GetStringOrDefault(string.Empty));
			string text = childNode.Attributes["TacticEquivalent"].GetStringOrDefault(string.Empty);
			if (!string.IsNullOrEmpty(text))
			{
				list.Add(new global::Pair<InfoAnimation, string>(animation, text));
			}
			animation.Priority = childNode.Attributes["Priority"].ParseInt();
			xmlAttribute = childNode.Attributes["Type"];
			if (xmlAttribute != null)
			{
				string text2 = xmlAttribute.GetStringOrDefault(string.Empty);
				if (text2 == "MOVE")
				{
					animation.Type = InfoAnimation.AnimationKind.AnimationMove;
				}
				else if (text2 == "ATTACK")
				{
					animation.Type = InfoAnimation.AnimationKind.AnimationAttack;
				}
			}
			xmlAttribute = childNode.Attributes["Delay"];
			if (xmlAttribute != null)
			{
				animation.AddDelay(xmlAttribute.ParseInt());
			}
			ApplyTemplates(list2, animation, JIGEFEPNCIN);
			ParseMoveInside(childNode, list2, animation);
			ParseVelocity(animation, childNode, list2);
			ParseRotation(animation, childNode["Rotation"]);
			animation.Init();
			animations.Add(animation);
			XmlNode xmlNode2 = childNode["Profile"];
			if (xmlNode2 != null && xmlNode2.Attributes["Show"].ParseBool())
			{
				animation.Rank = xmlNode2.Attributes["Rank"].ParseInt();
				Trick item = new Trick(xmlNode2, animation);
				tricks.Add(item);
			}
		}
		foreach (global::Pair<InfoAnimation, string> item2 in list)
		{
			InfoAnimation sourceAnimation = item2.First;
			string equivalentName = item2.Second;
			InfoAnimation pJAHIOELGGD2 = null;
			foreach (InfoAnimation item3 in animations)
			{
				if (item3.Name == equivalentName)
				{
					pJAHIOELGGD2 = item3;
					break;
				}
			}
			if (pJAHIOELGGD2 != null)
			{
				sourceAnimation.set_TacticEquivalent(pJAHIOELGGD2);
				continue;
			}
			GameLog.Error("{0} tactic equivalent {1} not found", sourceAnimation.Name, equivalentName);
		}
		return animations;
	}

	private static void ParseTriggers(XmlNode node, List<Trigger> triggers)
	{
		foreach (XmlNode childNode in node.ChildNodes)
		{
			Trigger trigger = new Trigger();
			trigger.Name = childNode.Attributes["Name"].GetStringOrDefault(string.Empty);
			ParseTriggerInside(childNode, trigger);
			trigger.Init();
			triggers.Add(trigger);
		}
	}

	private static void ParseTemplates(XmlNode templatesNode, Dictionary<string, TemplateAnimation> JIGEFEPNCIN)
	{
		JIGEFEPNCIN.Clear();
		_TemplateTemp = new Dictionary<string, XmlNode>();
		if (templatesNode == null)
		{
			return;
		}
		foreach (XmlNode childNode in templatesNode.ChildNodes)
		{
			if (childNode.Name == "Template")
			{
				TemplateAnimation template = new TemplateAnimation(childNode);
				JIGEFEPNCIN.Add(template.get_Name(), template);
				_TemplateTemp.Add(template.get_Name(), childNode);
			}
		}
	}

	private static void ApplyTemplates(List<XmlNode> templateNodes, InfoAnimation animation, Dictionary<string, TemplateAnimation> JIGEFEPNCIN)
	{
		string text = null;
		for (int i = 0; i < templateNodes.Count; i++)
		{
			text = templateNodes[i].Attributes["Name"].Value;
			if (JIGEFEPNCIN.ContainsKey(text))
			{
				JIGEFEPNCIN[text].AddAnimation(animation);
			}
		}
		if (!JIGEFEPNCIN.ContainsKey(animation.Name))
		{
			TemplateAnimation template = new TemplateAnimation(animation);
			JIGEFEPNCIN.Add(template.get_Name(), template);
		}
	}

	private static bool ContainsTemplate(string name, List<TemplateAnimation> OEMALIFPGPO)
	{
		foreach (TemplateAnimation item in OEMALIFPGPO)
		{
			if (item.get_Name() == name)
			{
				return true;
			}
		}
		return false;
	}

	private static TemplateAnimation FindTemplate(string name, List<TemplateAnimation> JIGEFEPNCIN)
	{
		int i = 0;
		for (int count = JIGEFEPNCIN.Count; i < count; i++)
		{
			if (JIGEFEPNCIN[i].get_Name() == name)
			{
				return JIGEFEPNCIN[i];
			}
		}
		return null;
	}

	private static void ParseMoveInside(XmlNode node, List<XmlNode> templateNodes, InfoAnimation animation)
	{
		InfoAnimation.MoveInside moveInside = new InfoAnimation.MoveInside();
		moveInside.Events = ParseEvents(node, templateNodes);
		moveInside.Conditions = ParseConditions("Conditions", node, templateNodes);
		moveInside.Locks = ParseConditions("Locks", node, templateNodes);
		moveInside.Intervals = ParseIntervals(node, templateNodes);
		moveInside.Actions = ParseActions(node, templateNodes);
		moveInside.TacticsConditions = ParseTacticConditions(node, templateNodes);
		XmlNode xmlNode = node["Transitions"];
		if (xmlNode != null)
		{
			moveInside.Transitions = ParseTransitions(xmlNode);
		}
		xmlNode = node["Shop"];
		if (xmlNode != null)
		{
			moveInside.ShopData = ParseShop(xmlNode);
		}
		ParseAlign(moveInside, node, templateNodes);
		ParseSetDirection(moveInside, node, templateNodes);
		animation.MergeMoveData(moveInside);
	}

	private static void ParseTriggerInside(XmlNode node, Trigger trigger)
	{
		Trigger.TriggerInside triggerInside = new Trigger.TriggerInside();
		triggerInside.Events = ParseEvents(node);
		triggerInside.Conditions = ParseConditions("Conditions", node);
		triggerInside.ExtraConditions = ParseConditions("Locks", node);
		triggerInside.Actions = ParseActions(node);
		trigger.MergeDefinition(triggerInside);
	}

	private static List<EventAnimation> ParseEvents(XmlNode nodes, List<XmlNode> templateNodes = null)
	{
		List<EventAnimation> list = new List<EventAnimation>();
		ParseEvents(nodes["Events"], list);
		if (templateNodes != null)
		{
			for (int i = 0; i < templateNodes.Count; i++)
			{
				ParseEvents(templateNodes[i]["Events"], list);
			}
		}
		return list;
	}

	private static void ParseEvents(XmlNode eventsNode, List<EventAnimation> events)
	{
		if (eventsNode == null)
		{
			return;
		}
		events.Capacity = events.Count + eventsNode.ChildNodes.Count;
		EventAnimation eventAnimation = null;
		foreach (XmlNode childNode in eventsNode.ChildNodes)
		{
			eventAnimation = EventParser.Create(childNode);
			if (eventAnimation != null)
			{
				eventAnimation.Init(childNode);
				events.Add(eventAnimation);
			}
		}
	}

	private static List<ConditionAnimation> ParseConditions(string sectionName, XmlNode nodes, List<XmlNode> templateNodes = null)
	{
		List<ConditionAnimation> list = new List<ConditionAnimation>();
		ParseConditions(nodes[sectionName], list);
		if (templateNodes != null)
		{
			for (int i = 0; i < templateNodes.Count; i++)
			{
				ParseConditions(templateNodes[i][sectionName], list);
			}
		}
		return list;
	}

	private static void ParseConditions(XmlNode conditionsNode, List<ConditionAnimation> conditions)
	{
		if (conditionsNode == null)
		{
			return;
		}
		conditions.Capacity = conditions.Count + conditionsNode.ChildNodes.Count;
		foreach (XmlNode childNode in conditionsNode.ChildNodes)
		{
			ConditionAnimation condition = ConditionsParser.Create(childNode);
			if (condition != null)
			{
				condition.Parse(childNode);
				conditions.Add(condition);
			}
		}
	}

	private static List<ConditionAnimation> ParseTacticConditions(XmlNode node, List<XmlNode> templateNodes)
	{
		List<ConditionAnimation> list = new List<ConditionAnimation>();
		XmlNode xmlNode = node["Tactics"];
		if (xmlNode != null)
		{
			ParseConditions(xmlNode["Conditions"], list);
		}
		for (int i = 0; i < templateNodes.Count; i++)
		{
			xmlNode = node["Tactics"];
			if (xmlNode != null)
			{
				ParseConditions(xmlNode["Conditions"], list);
			}
		}
		return list;
	}

	private static InfoAnimation.MoveInside.ShopAnimation ParseShop(XmlNode nodes)
	{
		InfoAnimation.MoveInside.ShopAnimation shopAnimation = new InfoAnimation.MoveInside.ShopAnimation();
		shopAnimation.RunOnStart = nodes["RunOnStart"] != null;
		shopAnimation.AnimationName = nodes["NextAnimation"].Attributes["Name"].GetStringOrDefault(string.Empty);
		shopAnimation.IsExists = true;
		return shopAnimation;
	}

	private static List<TransitionAnimation> ParseTransitions(XmlNode nodes)
	{
		List<TransitionAnimation> list = new List<TransitionAnimation>(nodes.ChildNodes.Count);
		foreach (XmlNode childNode in nodes.ChildNodes)
		{
			TransitionAnimation transition = new TransitionAnimation();
			transition.SetConditions(ParseConditions("Conditions", childNode));
			if (childNode.Attributes["FirstFrame"] != null)
			{
				transition.IsFrameShift = false;
				transition.FrameShift = childNode.Attributes["FirstFrame"].ParseInt();
			}
			if (childNode.Attributes["FrameShift"] != null)
			{
				transition.IsFrameShift = true;
				transition.FrameShift = childNode.Attributes["FrameShift"].ParseInt();
			}
			list.Add(transition);
		}
		return list;
	}

	private static List<IntervalAnimation> ParseIntervals(XmlNode nodes, List<XmlNode> templateNodes = null)
	{
		List<IntervalAnimation> list = new List<IntervalAnimation>();
		ParseIntervals(nodes["Intervals"], list);
		if (templateNodes != null)
		{
			for (int i = 0; i < templateNodes.Count; i++)
			{
				ParseIntervals(templateNodes[i]["Intervals"], list);
			}
		}
		return list;
	}

	private static void ParseIntervals(XmlNode intervalsNode, List<IntervalAnimation> intervals)
	{
		if (intervalsNode == null)
		{
			return;
		}
		intervals.Capacity = intervals.Count + intervalsNode.ChildNodes.Count;
		foreach (XmlNode childNode in intervalsNode.ChildNodes)
		{
			string typeName = childNode.Attributes["Type"].GetStringOrDefault(string.Empty);
			IntervalAnimation.IntervalType intervalType = IntervalAnimation.ParseIntervalType(typeName);
			IntervalAnimation interval = ((intervalType != IntervalAnimation.IntervalType.INTERVAL_ATTACK) ? new IntervalAnimation(intervalType) : new IntervalAttack());
			interval.Parse(childNode);
			intervals.Add(interval);
		}
	}

	private static List<ActionAnimation> ParseActions(XmlNode nodes, List<XmlNode> templateNodes = null)
	{
		List<ActionAnimation> list = new List<ActionAnimation>();
		ParseActions(nodes["Actions"], list);
		if (templateNodes != null)
		{
			for (int i = 0; i < templateNodes.Count; i++)
			{
				ParseActions(templateNodes[i]["Actions"], list);
			}
		}
		return list;
	}

	private static void ParseActions(XmlNode actionsNode, List<ActionAnimation> actions)
	{
		if (actionsNode == null)
		{
			return;
		}
		actions.Capacity = actions.Count + actionsNode.ChildNodes.Count;
		foreach (XmlNode childNode in actionsNode.ChildNodes)
		{
			ActionAnimation action = ActionsParser.Create(childNode);
			if (action != null)
			{
				actions.Add(action);
			}
		}
	}

	private static void ParseAlign(InfoAnimation.MoveInside moveInside, XmlNode node, List<XmlNode> templateNodes)
	{
		moveInside.AlignData.IsExists = false;
		XmlNode xmlNode = node["Align"];
		if (xmlNode != null)
		{
			moveInside.AlignData = ParseAlignPivot(xmlNode);
			moveInside.AlignData.IsExists = true;
			return;
		}
		for (int i = 0; i < templateNodes.Count; i++)
		{
			xmlNode = templateNodes[i]["Align"];
			if (xmlNode != null)
			{
				moveInside.AlignData = ParseAlignPivot(xmlNode);
				moveInside.AlignData.IsExists = true;
				break;
			}
		}
	}

	private static InfoAnimation.MovePivot ParseAlignPivot(XmlNode node)
	{
		InfoAnimation.MovePivot alignPivot = new InfoAnimation.MovePivot();
		XmlNode xmlNode = node["Pivot"];
		XmlNode xmlNode2 = node["Position"];
		string text = xmlNode.Attributes["Object"].GetStringOrDefault(string.Empty);
		string text2 = xmlNode2.Attributes["Object"].GetStringOrDefault(string.Empty);
		XmlAttribute targetAttribute = xmlNode.Attributes["Player"];
		string targetTypeName = targetAttribute.GetStringOrDefault("Me");
		XmlAttribute cJBEMNNNHDM2 = xmlNode2.Attributes["Player"];
		string lFLGCDNKNJI2 = cJBEMNNNHDM2.GetStringOrDefault("Me");
		XmlAttribute xmlAttribute = node.Attributes["Axis"];
		alignPivot.AlignX = (alignPivot.AlignY = (alignPivot.AlignZ = false));
		if (xmlAttribute == null)
		{
			alignPivot.AlignX = true;
			alignPivot.AlignY = true;
			alignPivot.AlignZ = true;
		}
		else
		{
			string text3 = xmlAttribute.GetStringOrDefault(string.Empty);
			string[] array = text3.Split('|');
			string[] array2 = array;
			foreach (string text4 in array2)
			{
				switch (text4)
				{
				case "X":
					alignPivot.AlignX = true;
					continue;
				case "Y":
					alignPivot.AlignY = true;
					continue;
				case "Z":
					alignPivot.AlignZ = true;
					continue;
				}
				GameLog.Error("ERROR: alignParse - wrong axis \"{0}\" in \"{1}\"", text4, text3);
			}
		}
		XmlAttribute xmlAttribute2 = node.Attributes["ShiftModelNode"];
		if (xmlAttribute2 != null)
		{
			alignPivot.ShiftModelNode = xmlAttribute2.GetStringOrDefault(string.Empty);
		}
		alignPivot.PivotModelType = ModelType.ParseTargetType(targetTypeName);
		alignPivot.PositionModelType = ModelType.ParseTargetType(lFLGCDNKNJI2);
		alignPivot.PivotPart = xmlNode.Attributes["Part"].GetStringOrDefault(string.Empty);
		alignPivot.PositionPart = xmlNode2.Attributes["Part"].GetStringOrDefault(string.Empty);
		alignPivot.PositionShift.SetX(xmlNode2.Attributes["ShiftX"].ParseFloat());
		alignPivot.PositionShift.SetY(xmlNode2.Attributes["ShiftY"].ParseFloat());
		switch (text)
		{
		case "Nodes":
			alignPivot.PivotObjectType = InfoAnimation.AlignObjectType.ObjectNodes;
			break;
		case "Animation":
			alignPivot.PivotObjectType = InfoAnimation.AlignObjectType.ObjectAnimation;
			break;
		case "Wall":
			alignPivot.PivotObjectType = InfoAnimation.AlignObjectType.ObjectWall;
			break;
		case "Pivot":
			alignPivot.PivotObjectType = InfoAnimation.AlignObjectType.ObjectPivot;
			break;
		}
		switch (text2)
		{
		case "Nodes":
			alignPivot.PositionObjectType = InfoAnimation.AlignObjectType.ObjectNodes;
			break;
		case "Animation":
			alignPivot.PositionObjectType = InfoAnimation.AlignObjectType.ObjectAnimation;
			break;
		case "Wall":
			alignPivot.PositionObjectType = InfoAnimation.AlignObjectType.ObjectWall;
			break;
		case "Pivot":
			alignPivot.PositionObjectType = InfoAnimation.AlignObjectType.ObjectPivot;
			break;
		}
		return alignPivot;
	}

	private static void ParseSetDirection(InfoAnimation.MoveInside moveInside, XmlNode node, List<XmlNode> templateNodes)
	{
		moveInside.SetDirectionData.IsExists = false;
		XmlNode xmlNode = node["SetDirection"];
		if (xmlNode != null)
		{
			moveInside.SetDirectionData = ParseDirection(xmlNode);
			moveInside.SetDirectionData.IsExists = true;
			return;
		}
		for (int i = 0; i < templateNodes.Count; i++)
		{
			xmlNode = templateNodes[i]["SetDirection"];
			if (xmlNode != null)
			{
				moveInside.SetDirectionData = ParseDirection(xmlNode);
				moveInside.SetDirectionData.IsExists = true;
				break;
			}
		}
	}

	public static InfoAnimation.MoveInside.Direction ParseDirection(XmlNode node)
	{
		InfoAnimation.MoveInside.Direction direction = new InfoAnimation.MoveInside.Direction();
		XmlNode pointNode = node["From"];
		direction.FromPoint.Create(pointNode);
		pointNode = node["To"];
		direction.ToPoint.Create(pointNode);
		pointNode = node["Impulse"];
		direction.ImpulseMode = InfoAnimation.MoveInside.Direction.ParseImpulseMode(pointNode);
		return direction;
	}

	public static void ClearCaches()
	{
		_baseMoveLockSources = null;
		MovesMaps.Clear();
		_BaseTemplateNodes.Clear();
		_BaseLegacyTemplateNodes.Clear();
	}

	private static void ParseVelocity(InfoAnimation animation, XmlNode moveNode, List<XmlNode> templateNodes)
	{
		XmlNode xmlNode = moveNode["Velocity"];
		if (xmlNode == null)
		{
			for (int i = 0; i < templateNodes.Count; i++)
			{
				xmlNode = moveNode["Velocity"];
				if (xmlNode != null)
				{
					break;
				}
			}
		}
		Vector3 velocity = new Vector3(0f, 0f, 0f);
		Vector3 bEHOPOPCJGB2 = new Vector3(0f, 0f, 0f);
		if (xmlNode != null)
		{
			velocity.x = xmlNode.Attributes["X"].ParseFloat();
			velocity.y = xmlNode.Attributes["Y"].ParseFloat();
			velocity.z = xmlNode.Attributes["Z"].ParseFloat();
			bEHOPOPCJGB2.x = xmlNode.Attributes["Ax"].ParseFloat();
			bEHOPOPCJGB2.y = xmlNode.Attributes["Ay"].ParseFloat();
			bEHOPOPCJGB2.z = xmlNode.Attributes["Az"].ParseFloat();
			animation.SetSaveVelocity(xmlNode.Attributes["SaveVelocity"].ParseBool());
		}
		animation.SetVelocity(Vector3f.op_Implicit(velocity));
		animation.SetAcceleration(Vector3f.op_Implicit(bEHOPOPCJGB2));
	}

	private static void ParseRotation(InfoAnimation animation, XmlNode node)
	{
		float angle = 0f;
		if (node != null)
		{
			angle = node.Attributes["Angle"].ParseFloat();
			XmlNode xmlNode = node["Position"];
			if (xmlNode != null)
			{
				DistancePoint bAINMLLIKOL2 = new DistancePoint(xmlNode);
				animation.SetRotationPosition(bAINMLLIKOL2);
			}
		}
		animation.set_RotationAngle(angle);
	}
}
