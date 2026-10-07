using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;
using UnityEngine;

public class PerkInfoItem
{
	public enum PerkKind
	{
		COMBO = 0,
		SINGLE = 1
	}

	public enum RoundingMode
	{
		RT_FLOOR = 0,
		RT_CEIL = 1,
		RT_TRUNC = 2,
		RT_INF = 3
	}

	private bool isWeaponPerk;

	private bool isEnabled;

	private XmlDocument defaultDocument = new XmlDocument();

	private XmlNode defaultNode;

	public string ImageName;

	public string Name;

	public string DescriptionKey;

	public string Alias;

	public string BarScale;

	public string BarSetAttribute;

	public string MoveName;

	public string ItemSetName;

	public int UpgradeLevel;

	public int Id;

	public int Level;

	public int BarShift;

	public int BarValue;

	public bool IsHidden;

	public bool IsClone;

	public PerkKind Kind;

	private PerkSetAttributes perkSetAttributes;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private Model ownerModel;

	public Attributes AttributeValues;

	public List<Rating> Ratings;

	private List<string> Names;

	private List<string> inheritedTemplateNames;

	private List<PerkTrigger> triggers = new List<PerkTrigger>();

	private static Dictionary<string, RoundingMode> roundingModesByName = new Dictionary<string, RoundingMode>
	{
		{
			"floor",
			RoundingMode.RT_FLOOR
		},
		{
			"ceil",
			RoundingMode.RT_CEIL
		},
		{
			"trunc",
			RoundingMode.RT_TRUNC
		},
		{
			"inf",
			RoundingMode.RT_INF
		}
	};

	public bool IsWeaponPerk
	{
		get
		{
			return GetIsWeaponPerk();
		}
		set
		{
			SetIsWeaponPerk(value);
		}
	}

	public bool IsEnabled
	{
		get
		{
			return GetIsEnabled();
		}
		set
		{
			SetIsEnabled(value);
		}
	}

	public XmlNode DefaultNode
	{
		get
		{
			return GetDefaultNode();
		}
	}

	public PerkSetAttributes PerkSet
	{
		get
		{
			return GetPerkSet();
		}
	}

	public Model OwnerModel
	{
		get
		{
			return GetOwnerModel();
		}
		set
		{
			SetOwnerModel(value);
		}
	}

	public List<PerkTrigger> Triggers
	{
		get
		{
			return GetTriggers();
		}
	}

	public PerkInfoItem()
	{
		isWeaponPerk = false;
		isEnabled = false;
		Name = string.Empty;
		DescriptionKey = string.Empty;
		Alias = string.Empty;
		BarScale = string.Empty;
		BarSetAttribute = string.Empty;
		MoveName = string.Empty;
		ItemSetName = string.Empty;
		Id = -1;
		Level = 0;
		BarShift = 0;
		BarValue = 0;
		UpgradeLevel = 0;
		IsHidden = false;
		IsClone = false;
		Kind = PerkKind.SINGLE;
		perkSetAttributes = new PerkSetAttributes();
		AttributeValues = new Attributes();
		Ratings = new List<Rating>();
		Names = new List<string>();
		inheritedTemplateNames = new List<string>();
	}

	public bool GetIsWeaponPerk()
	{
		return isWeaponPerk;
	}

	public void SetIsWeaponPerk(bool value)
	{
		isWeaponPerk = value;
	}

	public bool GetIsEnabled()
	{
		return isEnabled;
	}

	public void SetIsEnabled(bool value)
	{
		isEnabled = value;
	}

	public XmlNode GetDefaultNode()
	{
		return defaultNode;
	}

	public PerkSetAttributes GetPerkSet()
	{
		return perkSetAttributes;
	}

	public Model GetOwnerModel()
	{
		return ownerModel;
	}

	public void SetOwnerModel(Model value)
	{
		ownerModel = value;
	}

	public List<PerkTrigger> GetTriggers()
	{
		return triggers;
	}

	public bool IsPerkByNames(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return true;
		}
		for (int i = 0; i < Names.Count; i++)
		{
			if (Names[i] == value)
			{
				return true;
			}
		}
		return false;
	}

	public static float AspectToMultiplier(float aspect)
	{
		float antilimit = GameUtils.GetAspectConstants().Antilimit;
		float doublingRange = GameUtils.GetAspectConstants().DoublingRange;
		float limit = GameUtils.GetAspectConstants().Limit;
		float num = 0f;
		if (aspect >= 0f)
		{
			return limit - (limit - 1f) * Mathf.Pow(2f, (0f - aspect) / doublingRange);
		}
		return antilimit + Mathf.Pow(2f, aspect / doublingRange);
	}

	public string GetSetValue(string name)
	{
		return perkSetAttributes.GetValue(name);
	}

	public PerkInfoItem Clone(XmlNode setNode, XmlNode ratingEvaluationNode)
	{
		PerkInfoItem basePerk = GameUtils.PerkItemList.FindBasePerk(Name);
		XmlDocument xmlDocument = new XmlDocument();
		if (basePerk != null && basePerk.GetDefaultNode() != null)
		{
			xmlDocument.AppendImportedClone(basePerk.GetDefaultNode());
		}
		if (setNode != null)
		{
			XmlNode xmlNode = null;
			if (xmlDocument["Perk"] != null)
			{
				xmlNode = xmlDocument["Perk"]["Set"];
			}
			if (xmlNode == null)
			{
				if (xmlDocument["Perk"] == null)
				{
					xmlDocument.AppendElement("Perk");
				}
				xmlNode = xmlDocument["Perk"].AppendElement("Set");
			}
			foreach (XmlAttribute attribute in setNode.Attributes)
			{
				string name = attribute.Name;
				string value = attribute.Value;
				XmlAttribute xmlAttribute2 = xmlNode.Attributes[name];
				if (xmlAttribute2 == null)
				{
					xmlAttribute2 = xmlNode.OwnerDocument.CreateAttribute(name);
					xmlNode.Attributes.Append(xmlAttribute2);
				}
				xmlAttribute2.Value = value;
			}
		}
		if (ratingEvaluationNode != null)
		{
			XmlNode xmlNode2 = null;
			if (xmlDocument["Perk"] != null)
			{
				xmlNode2 = xmlDocument["Perk"]["RatingEvaluation"];
			}
			if (xmlNode2 == null)
			{
				if (xmlDocument["Perk"] == null)
				{
					xmlDocument.AppendElement("Perk");
				}
				xmlNode2 = xmlDocument["Perk"].AppendImportedClone(ratingEvaluationNode);
			}
		}
		PerkInfoItem clonedPerk = new PerkInfoItem();
		XmlNode xmlNode3 = xmlDocument["Perk"];
		if (xmlNode3 != null)
		{
			clonedPerk.Parse(xmlNode3);
		}
		clonedPerk.UpgradeLevel = UpgradeLevel;
		clonedPerk.IsClone = true;
		clonedPerk.ReleaseDefinitionXml();
		return clonedPerk;
	}

	public void ReleaseDefinitionXml()
	{
		defaultNode = null;
		defaultDocument = null;
	}

	public void Parse(XmlNode node)
	{
		XmlDocument document = new XmlDocument();
		defaultNode = defaultDocument.AppendImportedClone(node);
		XmlNode xmlNode = document.AppendImportedClone(node);
		Id = node.Attributes["ID"].ParseInt(-1);
		Level = node.Attributes["Level"].ParseInt();
		Name = node.Attributes["Name"].GetStringOrDefault();
		Alias = node.Attributes["Alias"].GetStringOrDefault();
		BarScale = node.Attributes["BarScale"].GetStringOrDefault();
		BarShift = node.Attributes["BarShift"].ParseInt();
		BarSetAttribute = node.Attributes["BarSetAttribute"].GetStringOrDefault();
		ImageName = node.Attributes["Image"].GetStringOrDefault();
		DescriptionKey = xmlNode.Attributes["Description"].GetStringOrDefault();
		MoveName = xmlNode.Attributes["Move"].GetStringOrDefault();
		IsHidden = xmlNode.Attributes["Hidden"].ParseBool();
		ItemSetName = xmlNode.Attributes["ItemSet"].GetStringOrDefault();
		string text = node.Attributes["Template"].GetStringOrDefault(string.Empty);
		Names.AddRange(text.Split('|'));
		Names.Add(Name);
		Kind = PerkKind.SINGLE;
		if (node.Attributes["PerkType"].GetStringOrDefault(string.Empty).Equals("Combo"))
		{
			Kind = PerkKind.COMBO;
		}
		List<WarriorAttribute> attributes = GameUtils.WarriorAttributeList.AttributeList;
		foreach (WarriorAttribute item in attributes)
		{
			XmlAttribute xmlAttribute = xmlNode.Attributes[item.get_Name()];
			if (xmlAttribute != null)
			{
				AttributeValues.Set(item.get_Name(), xmlAttribute.ParseInt());
			}
		}
		XmlNode xmlNode2 = xmlNode["Set"];
		if (xmlNode2 != null)
		{
			ReadSetAttributes(xmlNode2);
		}
		BarValue = 0;
		foreach (KeyValuePair<string, string> item2 in perkSetAttributes.Values)
		{
			if (item2.Key == BarSetAttribute)
			{
				float result;
				if (float.TryParse(item2.Value, out result))
				{
					BarValue = (int)result;
				}
				BarValue += BarShift;
			}
		}
		foreach (XmlNode childNode in xmlNode.ChildNodes)
		{
			string name = childNode.Name;
			if (name == "Trigger")
			{
				ResolveTriggerVariables(childNode);
				AddTrigger(childNode);
			}
		}
		XmlNode xmlNode4 = xmlNode["RatingEvaluation"];
		if (xmlNode4 != null)
		{
			ParseRatings(xmlNode4, perkSetAttributes);
		}
		document = null;
	}

	private void ParseAttributes(XmlNode node, PerkInfoItem basePerk)
	{
		Name = node.Attributes["Name"].GetStringOrDefault((basePerk == null) ? Name : basePerk.Name);
		Id = node.Attributes["ID"].ParseInt((basePerk == null) ? Id : basePerk.Id);
		Level = node.Attributes["Level"].ParseInt((basePerk == null) ? Level : basePerk.Level);
		Alias = node.Attributes["Alias"].GetStringOrDefault((basePerk == null) ? Alias : basePerk.Alias);
		BarScale = node.Attributes["BarScale"].GetStringOrDefault((basePerk == null) ? BarScale : basePerk.BarScale);
		BarShift = node.Attributes["BarShift"].ParseInt((basePerk == null) ? BarShift : basePerk.BarShift);
		BarSetAttribute = node.Attributes["BarSetAttribute"].GetStringOrDefault((basePerk == null) ? BarSetAttribute : basePerk.BarSetAttribute);
		ImageName = node.Attributes["Image"].GetStringOrDefault((basePerk == null) ? ImageName : basePerk.ImageName);
		DescriptionKey = node.Attributes["Description"].GetStringOrDefault((basePerk == null) ? DescriptionKey : basePerk.DescriptionKey);
		MoveName = node.Attributes["Move"].GetStringOrDefault((basePerk == null) ? MoveName : basePerk.MoveName);
		IsHidden = node.Attributes["Hidden"].ParseBool((basePerk == null) ? IsHidden : basePerk.IsHidden);
		ItemSetName = node.Attributes["ItemSet"].GetStringOrDefault((basePerk == null) ? ItemSetName : basePerk.ItemSetName);
		string text = node.Attributes["PerkType"].GetStringOrDefault(string.Empty);
		Kind = ((!text.Equals("COMBO")) ? PerkKind.SINGLE : PerkKind.COMBO);
		if (basePerk != null)
		{
			AttributeValues.AddRange(basePerk.AttributeValues);
		}
		List<WarriorAttribute> attributes = GameUtils.WarriorAttributeList.AttributeList;
		foreach (WarriorAttribute item in attributes)
		{
			XmlAttribute xmlAttribute = node.Attributes[item.get_Name()];
			if (xmlAttribute != null)
			{
				AttributeValues.Set(item.get_Name(), xmlAttribute.ParseInt());
			}
		}
	}

	private void BuildInheritedTemplateNames()
	{
		inheritedTemplateNames.Clear();
		inheritedTemplateNames.AddRange(Names);
		inheritedTemplateNames.Remove(Name);
		List<string> list = new List<string>();
		foreach (string item in inheritedTemplateNames)
		{
			PerkInfoItem basePerk = GameUtils.PerkItemList.FindBasePerk(item);
			if (basePerk != null)
			{
				list.AddRange(basePerk.inheritedTemplateNames);
			}
		}
		list.ForEach((string templateName) =>
		{
			inheritedTemplateNames.AddIfNotExist(templateName);
		});
	}

	private void ParseInheritedTriggers()
	{
		foreach (string item in inheritedTemplateNames)
		{
			PerkInfoItem basePerk = GameUtils.PerkItemList.FindBasePerk(item);
			if (basePerk == null)
			{
				continue;
			}
			XmlNode xmlNode = basePerk.GetDefaultNode().Clone();
			foreach (XmlNode childNode in xmlNode.ChildNodes)
			{
				string name = childNode.Name;
				if (name.Equals("Trigger"))
				{
					ResolveTriggerVariables(childNode);
					AddTrigger(childNode);
				}
			}
		}
	}

	private void ParseInheritedRatings(XmlNode node)
	{
		if (node != null)
		{
			ParseRatings(node, GetPerkSet());
			return;
		}
		foreach (string item in inheritedTemplateNames)
		{
			PerkInfoItem basePerk = GameUtils.PerkItemList.FindBasePerk(item);
			if (basePerk != null)
			{
				XmlNode xmlNode = basePerk.GetDefaultNode()["RatingEvaluation"];
				if (xmlNode != null)
				{
					ParseRatings(xmlNode, GetPerkSet());
					break;
				}
			}
		}
	}

	private void MergeTemplateSetAttributes(XmlNode node)
	{
		List<string> list = new List<string>(Names);
		list.Reverse();
		foreach (string item in list)
		{
			PerkInfoItem basePerk = GameUtils.PerkItemList.FindBasePerk(item);
			if (basePerk == null)
			{
				continue;
			}
			foreach (KeyValuePair<string, string> item2 in basePerk.GetPerkSet().Values)
			{
				perkSetAttributes.SetValue(item2.Key, item2.Value);
			}
		}
		if (node != null)
		{
			ReadSetAttributes(node);
		}
	}

	private void AddTrigger(XmlNode node)
	{
		PerkTrigger trigger = new PerkTrigger();
		trigger.SetPerk(this);
		trigger.Parse(node);
		GetTriggers().Add(trigger);
	}

	private void ResolveTriggerVariables(XmlNode node)
	{
		foreach (XmlNode childNode in node.ChildNodes)
		{
			foreach (XmlAttribute attribute in childNode.Attributes)
			{
				SetAttributes(attribute);
			}
			if (childNode.ChildNodes.Count > 0)
			{
				ResolveTriggerVariables(childNode);
			}
		}
	}

	private void SetAttributes(XmlAttribute attribute)
	{
		string name = attribute.Name;
		string text = attribute.Value;
		FunctionExtension functionExtension = new FunctionExtension();
		functionExtension.Parse(text);
		List<FunctionExtension.FunctionObject> list = functionExtension.GetValueObjects();
		foreach (FunctionExtension.FunctionObject item in list)
		{
			if (item.body[0] == '_')
			{
				string attributeName = item.body.Substring(1, item.body.Length - 1);
				string newValue = perkSetAttributes.GetValue(attributeName);
				text = text.Replace(item.body, newValue);
			}
		}
		attribute.Value = text;
	}

	private void ReadSetAttributes(XmlNode node)
	{
		foreach (XmlAttribute attribute in node.Attributes)
		{
			string name = attribute.Name;
			string value = attribute.Value;
			perkSetAttributes.SetValue(name, value);
		}
	}

	private void ParseRatings(XmlNode ratingsNode, PerkSetAttributes setAttributes)
	{
		int count = ratingsNode.ChildNodes.Count;
		if (count > 0)
		{
			Ratings.Clear();
		}
		foreach (XmlNode childNode in ratingsNode.ChildNodes)
		{
			string name = childNode.Name;
			if (name.Equals("Rating"))
			{
				Rating rating = new Rating();
				rating.Parse(childNode, setAttributes);
				Ratings.Add(rating);
			}
		}
	}

	public void CollectTriggersForEvent(List<PerkTrigger> triggers, PerkEvent.PerkEventType eventType)
	{
		foreach (PerkTrigger item in GetTriggers())
		{
			List<PerkEvent> list = item.GetEvents();
			foreach (PerkEvent item2 in list)
			{
				if (item2.get_Type() == eventType)
				{
					triggers.Add(item);
					break;
				}
			}
		}
	}

	public void OnFunctionPreCallback(FunctionExtension.CallbackResult callbackResult)
	{
	}

	public void EvaluateFunctionCallback(FunctionExtension.CallbackResult callbackResult)
	{
		FunctionExtension.FunctionCall functionCall = callbackResult.data as FunctionExtension.FunctionCall;
		FunctionResult functionResult = callbackResult.result;
		PerkObject perkObject = callbackResult.target as PerkObject;
		Model targetModel = ((GetOwnerModel() == null) ? null : GetOwnerModel());
		if (targetModel != null && functionResult.Value.Equals("Enemy"))
		{
			targetModel = targetModel.GetCombatTarget();
		}
		switch (functionCall.functionName)
		{
		case "UniformFloatRandom":
			EvaluateUniformFloatRandom(targetModel, functionCall, perkObject, functionResult);
			return;
		case "PlayerAttribute":
			EvaluatePlayerAttribute(targetModel, functionCall, perkObject, functionResult);
			return;
		case "PlayerParameter":
			EvaluatePlayerParameter(targetModel, functionCall, perkObject, functionResult);
			return;
		case "RoundParameter":
			EvaluateRoundParameter(targetModel, functionCall, perkObject, functionResult);
			return;
		case "Hit":
			EvaluateHit(targetModel, functionCall, perkObject, functionResult);
			return;
		case "StringInArray":
			EvaluateStringInArray(targetModel, functionCall, perkObject, functionResult);
			return;
		case "CoordX":
			EvaluateCoordX(targetModel, functionCall, perkObject, functionResult);
			return;
		case "CoordY":
			EvaluateCoordY(targetModel, functionCall, perkObject, functionResult);
			return;
		case "CoordZ":
			EvaluateCoordZ(targetModel, functionCall, perkObject, functionResult);
			return;
		case "RandomAspect":
			EvaluateRandomAspect(targetModel, functionCall, perkObject, functionResult);
			return;
		case "Abs":
			EvaluateAbs(targetModel, functionCall, perkObject, functionResult);
			return;
		case "Aspect":
			EvaluateAspect(targetModel, functionCall, perkObject, functionResult);
			return;
		case "Round":
			EvaluateRound(targetModel, functionCall, perkObject, functionResult);
			return;
		case "Variable":
			EvaluateVariable(targetModel, functionCall, perkObject, functionResult);
			return;
		case "MovesVariable":
			EvaluateVariable(targetModel, functionCall, perkObject, functionResult);
			return;
		case "Player":
			EvaluatePlayer(targetModel, functionCall, perkObject, functionResult);
			return;
		case "CurrentFight":
			EvaluateCurrentFight(targetModel, functionCall, perkObject, functionResult);
			return;
		}
		FunctionExtension.CompareType compareType = FunctionExtension.ParseCompareType(functionCall.functionName);
		bool flag = functionCall.functionName.Equals("Compare");
		if (flag || compareType != FunctionExtension.CompareType.COMPARE_NONE)
		{
			EvaluateCompare(flag, targetModel, functionCall, perkObject, functionResult);
		}
	}

	private void EvaluateCurrentFight(Model model, FunctionExtension.FunctionCall functionCall, PerkObject perkObject, FunctionResult functionResult)
	{
		if (Fight.GetCurrentFight() != null && functionCall.propertyName.Equals("isRaid"))
		{
			bool flag = Fight.GetCurrentFight().GetFightDefinition().get_Type() == BattleType.FightRaid;
			functionResult.Value = ((!flag) ? "0" : "1");
		}
	}

	private void EvaluateVariable(Model model, FunctionExtension.FunctionCall functionCall, PerkObject perkObject, FunctionResult functionResult)
	{
		if (functionCall.argumentValues.Count == 0)
		{
			functionResult.Value = string.Empty;
		}
		string variableName = functionCall.argumentValues[0].body;
		Dictionary<string, float> numericVariables = model.GetConditions().PerkVariables;
		Dictionary<string, string> stringVariables = model.GetConditions().PerkStringVariables;
		if (stringVariables.ContainsKey(variableName))
		{
			functionResult.Value = stringVariables[variableName];
		}
		else if (numericVariables.ContainsKey(variableName))
		{
			functionResult.Value = numericVariables[variableName].ToString();
		}
		else
		{
			functionResult.Value = string.Empty;
		}
	}

	private void EvaluatePlayer(Model model, FunctionExtension.FunctionCall functionCall, PerkObject perkObject, FunctionResult functionResult)
	{
		if (functionCall.propertyName.Equals("Level"))
		{
			functionResult.Value = ListSF.GetRoster().GetLevel().ToString();
		}
	}

	private void EvaluateUniformFloatRandom(Model model, FunctionExtension.FunctionCall functionCall, PerkObject perkObject, FunctionResult functionResult)
	{
		functionResult.Value = NekkiMath.randomFloat(1f).ToString();
	}

	private void EvaluatePlayerAttribute(Model model, FunctionExtension.FunctionCall functionCall, PerkObject perkObject, FunctionResult functionResult)
	{
		if (model != null)
		{
			ModelParameters modelParameters = model.Parameters;
			if (modelParameters != null)
			{
				int attributeValue = 0;
				modelParameters.FinalAttributes.Get(functionCall.propertyName, ref attributeValue);
				functionResult.Value = attributeValue.ToString();
			}
		}
	}

	private void EvaluatePlayerParameter(Model model, FunctionExtension.FunctionCall functionCall, PerkObject perkObject, FunctionResult functionResult)
	{
		if (model != null)
		{
			switch (functionCall.propertyName)
			{
			case "Health":
				functionResult.Value = model.GetLife().ToString();
				break;
			case "Pain":
				functionResult.Value = model.GetPain().ToString();
				break;
			case "Shock":
				functionResult.Value = ((!model.IsInShock()) ? "0" : "1");
				break;
			case "Disarm":
				functionResult.Value = ((!model.WasDisarmed()) ? "0" : "1");
				break;
			case "Style":
				functionResult.Value = model.StyleName;
				break;
			case "StyleGain":
				functionResult.Value = model.StyleProgress.ToString();
				break;
			case "Combo":
				functionResult.Value = model.GetComboCount().ToString();
				break;
			case "MagicBullet":
				functionResult.Value = model.GetMagicCharges().ToString();
				break;
			case "MagicCharge":
				functionResult.Value = model.GetMagicChargeFraction().ToString();
				break;
			case "Magic":
				functionResult.Value = (model.Parameters.Magic == null) ?
					string.Empty : model.Parameters.Magic.Name;
				break;
			case "Ranged":
				functionResult.Value = (model.Parameters.Ranged == null) ?
					string.Empty : model.Parameters.Ranged.Name;
				break;
			case "Weapon":
				functionResult.Value = (model.Parameters.Weapon == null) ?
					string.Empty : model.Parameters.Weapon.Name;
				break;
			case "Skeleton":
				functionResult.Value = (model.Parameters.Skeleton == null) ?
					string.Empty : model.Parameters.Skeleton.Name;
				break;
			case "RaidChargeBullet":
				break;
			case "DamageConverter":
				functionResult.Value = model.GetPowerMultiplier().ToString();
				break;
			case "DefaultPerksAspect":
				functionResult.Value = model.GetBonusModifier().ToString();
				break;
			case "isPlayer":
				functionResult.Value = ((!model.IsPlayerModel()) ? "0" : "1");
				break;
			}
		}
	}

	private void EvaluateRoundParameter(Model model, FunctionExtension.FunctionCall functionCall, PerkObject perkObject, FunctionResult functionResult)
	{
		if (Fight.GetCurrentFight() != null)
		{
			switch (functionCall.propertyName)
			{
			case "Number":
				functionResult.Value = Fight.GetCurrentFight().get_RoundNumber().ToString();
				break;
			case "TimeLeft":
				functionResult.Value = Fight.GetCurrentFight().get_RoundTimeLeftFrames().ToString();
				break;
			case "TimePassed":
				functionResult.Value = Fight.GetCurrentFight().get_RoundTimePassedFrames().ToString();
				break;
			case "RoundTime":
				functionResult.Value = Fight.GetCurrentFight().get_RoundTimeTotalFrames().ToString();
				break;
			}
		}
	}

	private void EvaluateCompare(bool isCompareOperatorIncluded, Model model, FunctionExtension.FunctionCall functionCall, PerkObject perkObject, FunctionResult functionResult)
	{
		if (functionResult != null)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(functionResult.Value);
			if (!isCompareOperatorIncluded)
			{
				stringBuilder.Append(",");
				stringBuilder.Append(functionCall.functionName);
			}
			bool flag = FunctionExtension.IsCompare(stringBuilder.ToString());
			functionResult.Value = ((!flag) ? "0" : "1");
		}
	}

	private void EvaluateCoordX(Model model, FunctionExtension.FunctionCall functionCall, PerkObject perkObject, FunctionResult functionResult)
	{
		if (functionCall.argumentValues.Count < 2)
		{
			functionResult.Value = "-10000";
			return;
		}
		string firstArgument = functionCall.argumentValues[0].body;
		string secondArgument = functionCall.argumentValues[1].body;
		string text = string.Empty;
		if (functionCall.argumentValues.Count >= 3)
		{
			text = functionCall.argumentValues[2].body;
		}
		DistancePoint distancePoint = new DistancePoint();
		distancePoint.Create(firstArgument, secondArgument, text);
		float num = 0f;
		if (distancePoint.ObjectType != DistancePoint.Object.OBJECT_NODES)
		{
			num = distancePoint.GetX(model.GetConditions());
		}
		else
		{
			if (distancePoint.TargetModel == ModelType.ModelTargetType.MODEL_THIS)
			{
				ModelNode bodyNode = model.GetBodyObject().GetNodeByName(text);
				num = bodyNode.GetStart().GetX();
			}
			if (distancePoint.TargetModel == ModelType.ModelTargetType.MODEL_OTHER)
			{
				ModelNode otherNode = model.GetCombatTarget().GetBodyObject().GetNodeByName(text);
				num = otherNode.GetStart().GetX();
			}
		}
		functionResult.Value = num.ToString();
	}

	private void EvaluateCoordY(Model model, FunctionExtension.FunctionCall functionCall, PerkObject perkObject, FunctionResult functionResult)
	{
		if (functionCall.argumentValues.Count < 2)
		{
			functionResult.Value = "-10000";
			return;
		}
		string firstArgument = functionCall.argumentValues[0].body;
		string secondArgument = functionCall.argumentValues[1].body;
		string text = string.Empty;
		if (functionCall.argumentValues.Count >= 3)
		{
			text = functionCall.argumentValues[2].body;
		}
		DistancePoint distancePoint = new DistancePoint();
		distancePoint.Create(firstArgument, secondArgument, text);
		float num = 0f;
		if (distancePoint.ObjectType != DistancePoint.Object.OBJECT_NODES)
		{
			num = distancePoint.GetY(model.GetConditions());
		}
		else
		{
			if (distancePoint.TargetModel == ModelType.ModelTargetType.MODEL_THIS)
			{
				ModelNode bodyNode = model.GetBodyObject().GetNodeByName(text);
				num = bodyNode.GetStart().GetY();
			}
			if (distancePoint.TargetModel == ModelType.ModelTargetType.MODEL_OTHER)
			{
				ModelNode otherNode = model.GetCombatTarget().GetBodyObject().GetNodeByName(text);
				num = otherNode.GetStart().GetY();
			}
		}
		functionResult.Value = num.ToString();
	}

	private void EvaluateCoordZ(Model model, FunctionExtension.FunctionCall functionCall, PerkObject perkObject, FunctionResult functionResult)
	{
		if (functionCall.argumentValues.Count < 2)
		{
			functionResult.Value = "-10000";
			return;
		}
		string firstArgument = functionCall.argumentValues[0].body;
		string secondArgument = functionCall.argumentValues[1].body;
		string text = string.Empty;
		if (functionCall.argumentValues.Count >= 3)
		{
			text = functionCall.argumentValues[2].body;
		}
		DistancePoint distancePoint = new DistancePoint();
		distancePoint.Create(firstArgument, secondArgument, text);
		float num = 0f;
		if (distancePoint.ObjectType != DistancePoint.Object.OBJECT_NODES)
		{
			num = distancePoint.GetZ(model.GetConditions());
		}
		else
		{
			if (distancePoint.TargetModel == ModelType.ModelTargetType.MODEL_THIS)
			{
				ModelNode bodyNode = model.GetBodyObject().GetNodeByName(text);
				num = bodyNode.GetStart().GetZ();
			}
			if (distancePoint.TargetModel == ModelType.ModelTargetType.MODEL_OTHER)
			{
				ModelNode otherNode = model.GetCombatTarget().GetBodyObject().GetNodeByName(text);
				num = otherNode.GetStart().GetZ();
			}
		}
		functionResult.Value = num.ToString();
	}

	private void EvaluateAbs(Model model, FunctionExtension.FunctionCall functionCall, PerkObject perkObject, FunctionResult functionResult)
	{
		string expression = functionResult.Value;
		double value = 0.0;
		Dictionary<string, RpnParser.VariableDelegate> variables = new Dictionary<string, RpnParser.VariableDelegate>();
		Dictionary<string, RpnParser.ParameterDelegate> parameters = new Dictionary<string, RpnParser.ParameterDelegate>();
		RpnParser.init(variables, parameters);
		RpnParser.Formula formula = new RpnParser.Formula(expression);
		if (formula.GetVariableCount() == 0)
		{
			double result;
			if (double.TryParse(formula.Calculate().ToString(), out result))
			{
				value = result;
			}
		}
		else
		{
			GameLog.Error("Abs function error! Argument is not valid expression: {0}", functionResult.Value);
		}
		functionResult.Value = Math.Abs(value).ToString();
	}

	private void EvaluateRandomAspect(Model model, FunctionExtension.FunctionCall functionCall, PerkObject perkObject, FunctionResult functionResult)
	{
		if (functionCall.argumentValues.Count < 2)
		{
			functionResult.Value = "0";
			return;
		}
		string minText = functionCall.argumentValues[0].body;
		string maxText = functionCall.argumentValues[1].body;
		int minValue = minText.ToInt();
		int maxValue = maxText.ToInt() + 1;
		NekkiMath.SetSeed();
		int num = NekkiMath.randomInt(minValue, maxValue);
		int aspectLevel = ListSF.GetRoster().GetLevel();
		int num2 = ForgeManager.GetInstance().GetAspectValueByLevel(aspectLevel);
		functionResult.Value = (num2 + num).ToString();
	}

	private void EvaluateAspect(Model model, FunctionExtension.FunctionCall functionCall, PerkObject perkObject, FunctionResult functionResult)
	{
		if (functionCall.argumentValues.Count != 1)
		{
			functionResult.Value = "0";
			GameLog.Error("Aspect function error! Number of argument is not 1: {0}", functionResult.Value);
		}
		else
		{
			string argumentText = functionCall.argumentValues[0].body;
			float aspect = argumentText.ToFloat();
			functionResult.Value = AspectToMultiplier(aspect).ToString();
		}
	}

	private void EvaluateRound(Model model, FunctionExtension.FunctionCall functionCall, PerkObject perkObject, FunctionResult functionResult)
	{
		float num = 0f;
		int num2 = 0;
		string key = "trunc";
		RoundingMode roundingMode = RoundingMode.RT_TRUNC;
		if (functionCall.argumentValues.Count < 1)
		{
			functionResult.Value = "RoundingError";
			return;
		}
		if (functionCall.argumentValues.Count > 3)
		{
			functionResult.Value = "RoundingError";
			return;
		}
		if (functionCall.argumentValues.Count >= 1)
		{
			string valueText = functionCall.argumentValues[0].body;
			num = valueText.ToFloat();
		}
		if (functionCall.argumentValues.Count >= 2)
		{
			string precisionText = functionCall.argumentValues[1].body;
			num2 = precisionText.ToInt();
		}
		if (functionCall.argumentValues.Count == 3)
		{
			string modeText = functionCall.argumentValues[2].body;
			key = modeText;
		}
		if (!roundingModesByName.ContainsKey(key))
		{
			functionResult.Value = "RoundingError";
			return;
		}
		roundingMode = roundingModesByName[key];
		float num3 = 0f;
		switch (roundingMode)
		{
		case RoundingMode.RT_FLOOR:
			functionResult.Value = NekkiMath.FloorToDecimals(num, num2).ToString();
			break;
		case RoundingMode.RT_CEIL:
			functionResult.Value = NekkiMath.CeilToDecimals(num, num2).ToString();
			break;
		case RoundingMode.RT_TRUNC:
			functionResult.Value = NekkiMath.TruncateToDecimals(num, num2).ToString();
			break;
		case RoundingMode.RT_INF:
			functionResult.Value = NekkiMath.RoundAwayFromZero(num, num2).ToString();
			break;
		default:
			functionResult.Value = string.Empty;
			break;
		}
	}

	private void EvaluateStringInArray(Model model, FunctionExtension.FunctionCall functionCall, PerkObject perkObject, FunctionResult functionResult)
	{
		List<string> list = new List<string>(functionResult.Value.Split(','));
		if (list.Count > 1)
		{
			string text = list[0];
			string value = list[1];
			List<string> list2 = new List<string>(text.Split('|'));
			int i = 0;
			for (int count = list2.Count; i < count; i++)
			{
				if (list2[i].Equals(value))
				{
					functionResult.Value = "1";
					return;
				}
			}
		}
		functionResult.Value = "0";
	}

	private void EvaluateHit(Model model, FunctionExtension.FunctionCall functionCall, PerkObject perkObject, FunctionResult functionResult)
	{
		if (Fight.GetCurrentFight() == null)
		{
			return;
		}
		Model.StrikeResult strikeResult = Fight.GetCurrentFight().LastStrikeResult;
		if (strikeResult == null)
		{
			return;
		}
		switch (functionCall.propertyName)
		{
		case "Player":
			functionResult.Value = ((strikeResult.Victim != model) ? "Enemy" : "Me");
			break;
		case "DefenseAttribute":
			functionResult.Value = strikeResult.DefenceAttribute;
			break;
		case "Block":
			functionResult.Value = ((!strikeResult.IsBlocked) ? "0" : "1");
			break;
		case "Critical":
			functionResult.Value = ((!strikeResult.IsCritical) ? "0" : "1");
			break;
		case "Shock":
			functionResult.Value = ((!strikeResult.IsShock) ? "0" : "1");
			break;
		case "Animations":
		{
			StringBuilder stringBuilder = new StringBuilder();
			List<string> list = strikeResult.AttackAnimation.GetTemplateNames();
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				stringBuilder.Append(list[i]);
				if (i < count - 1)
				{
					stringBuilder.Append("|");
				}
			}
			functionResult.Value = stringBuilder.ToString();
			break;
		}
		case "Damage":
			functionResult.Value = strikeResult.FinalDamage.ToString();
			break;
		case "BaseDamage":
		{
			float baseDamage = strikeResult.BaseDamage;
			float num = 1f;
			if (strikeResult.IsBlocked)
			{
				int attributeValue = 0;
				string attributeName = GameUtils.GetBlockDamageFactor().Attribute;
				strikeResult.Victim.Parameters.FinalAttributes.Get(attributeName, ref attributeValue);
				float blockDamageBase = GameUtils.GetBlockDamageFactor().Base;
				num = Mathf.Pow(2f, (float)attributeValue * blockDamageBase);
			}
			float num2 = 1f;
			if (strikeResult.IsCritical && strikeResult.AttackerModel != null)
			{
				int critAttributeValue = 0;
				string critAttributeName = GameUtils.GetCriticalHitDamage().Attribute;
				strikeResult.AttackerModel.Parameters.FinalAttributes.Get(critAttributeName, ref critAttributeValue);
				float critBase = GameUtils.GetCriticalHitDamage().Base;
				num2 = Mathf.Pow(2f, (float)critAttributeValue * critBase);
			}
			functionResult.Value = (baseDamage * num * num2).ToString();
			break;
		}
		}
	}

	public string ResolveDescriptionText(string description)
	{
		if (string.IsNullOrEmpty(description))
		{
			return string.Empty;
		}
		int num = description.IndexOf('{');
		if (num == -1)
		{
			return description;
		}
		int num2 = description.LastIndexOf('}');
		if (num2 == -1)
		{
			return description;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(description);
		QuestParameters questParameters = ListSF.GetInstance().GetQuestParameters();
		ConditionExtension.CompareResult compareResult = new ConditionExtension.CompareResult();
		QuestCondition questCondition = new QuestCondition();
		questCondition.SetParameters(questParameters);
		while (num <= num2)
		{
			string newValue = string.Empty;
			if (description[num] == '{')
			{
				int num3 = description.IndexOf('}', num);
				string text = description.Substring(num + 1, num3 - num - 1);
				if (!string.IsNullOrEmpty(text))
				{
					FunctionExtension functionExtension = new FunctionExtension();
					functionExtension.SetVariableCallback(OnFunctionPreCallback);
					functionExtension.SetFunctionCallback(EvaluateFunctionCallback);
					foreach (KeyValuePair<string, string> item in perkSetAttributes.Values)
					{
						functionExtension.SetVariable(item.Key, item.Value);
					}
					functionExtension.Parse(text);
					FunctionResult functionResult = functionExtension.Calculate();
					newValue = functionResult.Value;
				}
				stringBuilder.Replace(text, newValue);
				num = num3 + 1;
			}
			else
			{
				num++;
			}
		}
		return stringBuilder.ToString();
	}
}
