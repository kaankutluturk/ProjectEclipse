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

	public static float AspectToMultiplier(float FIJMPFHAKPB)
	{
		float lFIPMCAHODJ = GameUtils.GetAspectConstants().Antilimit;
		float jHJAFHLMOBJ = GameUtils.GetAspectConstants().DoublingRange;
		float gPEPDPOJJLM = GameUtils.GetAspectConstants().Limit;
		float num = 0f;
		if (FIJMPFHAKPB >= 0f)
		{
			return gPEPDPOJJLM - (gPEPDPOJJLM - 1f) * Mathf.Pow(2f, (0f - FIJMPFHAKPB) / jHJAFHLMOBJ);
		}
		return lFIPMCAHODJ + Mathf.Pow(2f, FIJMPFHAKPB / jHJAFHLMOBJ);
	}

	public string GetSetValue(string name)
	{
		return perkSetAttributes.GetValue(name);
	}

	public PerkInfoItem Clone(XmlNode HKCGPHLLOEA, XmlNode KKBODEIBPAK)
	{
		PerkInfoItem aCONCDFDNJH = GameUtils.PerkItemList.FindBasePerk(Name);
		XmlDocument xmlDocument = new XmlDocument();
		if (aCONCDFDNJH != null && aCONCDFDNJH.GetDefaultNode() != null)
		{
			xmlDocument.AppendImportedClone(aCONCDFDNJH.GetDefaultNode());
		}
		if (HKCGPHLLOEA != null)
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
			foreach (XmlAttribute attribute in HKCGPHLLOEA.Attributes)
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
		if (KKBODEIBPAK != null)
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
				xmlNode2 = xmlDocument["Perk"].AppendImportedClone(KKBODEIBPAK);
			}
		}
		PerkInfoItem aCONCDFDNJH2 = new PerkInfoItem();
		XmlNode xmlNode3 = xmlDocument["Perk"];
		if (xmlNode3 != null)
		{
			aCONCDFDNJH2.Parse(xmlNode3);
		}
		aCONCDFDNJH2.UpgradeLevel = UpgradeLevel;
		aCONCDFDNJH2.IsClone = true;
		aCONCDFDNJH2.ReleaseDefinitionXml();
		return aCONCDFDNJH2;
	}

	public void ReleaseDefinitionXml()
	{
		defaultNode = null;
		defaultDocument = null;
	}

	public void Parse(XmlNode node)
	{
		XmlDocument mEEAKLDGLDF = new XmlDocument();
		defaultNode = defaultDocument.AppendImportedClone(node);
		XmlNode xmlNode = mEEAKLDGLDF.AppendImportedClone(node);
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
		List<WarriorAttribute> iBLHIAHECLK = GameUtils.WarriorAttributeList.AttributeList;
		foreach (WarriorAttribute item in iBLHIAHECLK)
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
		mEEAKLDGLDF = null;
	}

	private void ParseAttributes(XmlNode node, PerkInfoItem AEFFHJGMNFI)
	{
		Name = node.Attributes["Name"].GetStringOrDefault((AEFFHJGMNFI == null) ? Name : AEFFHJGMNFI.Name);
		Id = node.Attributes["ID"].ParseInt((AEFFHJGMNFI == null) ? Id : AEFFHJGMNFI.Id);
		Level = node.Attributes["Level"].ParseInt((AEFFHJGMNFI == null) ? Level : AEFFHJGMNFI.Level);
		Alias = node.Attributes["Alias"].GetStringOrDefault((AEFFHJGMNFI == null) ? Alias : AEFFHJGMNFI.Alias);
		BarScale = node.Attributes["BarScale"].GetStringOrDefault((AEFFHJGMNFI == null) ? BarScale : AEFFHJGMNFI.BarScale);
		BarShift = node.Attributes["BarShift"].ParseInt((AEFFHJGMNFI == null) ? BarShift : AEFFHJGMNFI.BarShift);
		BarSetAttribute = node.Attributes["BarSetAttribute"].GetStringOrDefault((AEFFHJGMNFI == null) ? BarSetAttribute : AEFFHJGMNFI.BarSetAttribute);
		ImageName = node.Attributes["Image"].GetStringOrDefault((AEFFHJGMNFI == null) ? ImageName : AEFFHJGMNFI.ImageName);
		DescriptionKey = node.Attributes["Description"].GetStringOrDefault((AEFFHJGMNFI == null) ? DescriptionKey : AEFFHJGMNFI.DescriptionKey);
		MoveName = node.Attributes["Move"].GetStringOrDefault((AEFFHJGMNFI == null) ? MoveName : AEFFHJGMNFI.MoveName);
		IsHidden = node.Attributes["Hidden"].ParseBool((AEFFHJGMNFI == null) ? IsHidden : AEFFHJGMNFI.IsHidden);
		ItemSetName = node.Attributes["ItemSet"].GetStringOrDefault((AEFFHJGMNFI == null) ? ItemSetName : AEFFHJGMNFI.ItemSetName);
		string text = node.Attributes["PerkType"].GetStringOrDefault(string.Empty);
		Kind = ((!text.Equals("COMBO")) ? PerkKind.SINGLE : PerkKind.COMBO);
		if (AEFFHJGMNFI != null)
		{
			AttributeValues.AddRange(AEFFHJGMNFI.AttributeValues);
		}
		List<WarriorAttribute> iBLHIAHECLK = GameUtils.WarriorAttributeList.AttributeList;
		foreach (WarriorAttribute item in iBLHIAHECLK)
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
			PerkInfoItem aCONCDFDNJH = GameUtils.PerkItemList.FindBasePerk(item);
			if (aCONCDFDNJH != null)
			{
				list.AddRange(aCONCDFDNJH.inheritedTemplateNames);
			}
		}
		list.ForEach((string DHDMNHCIPEH) =>
		{
			inheritedTemplateNames.AddIfNotExist(DHDMNHCIPEH);
		});
	}

	private void ParseInheritedTriggers()
	{
		foreach (string item in inheritedTemplateNames)
		{
			PerkInfoItem aCONCDFDNJH = GameUtils.PerkItemList.FindBasePerk(item);
			if (aCONCDFDNJH == null)
			{
				continue;
			}
			XmlNode xmlNode = aCONCDFDNJH.GetDefaultNode().Clone();
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
			PerkInfoItem aCONCDFDNJH = GameUtils.PerkItemList.FindBasePerk(item);
			if (aCONCDFDNJH != null)
			{
				XmlNode xmlNode = aCONCDFDNJH.GetDefaultNode()["RatingEvaluation"];
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
			PerkInfoItem aCONCDFDNJH = GameUtils.PerkItemList.FindBasePerk(item);
			if (aCONCDFDNJH == null)
			{
				continue;
			}
			foreach (KeyValuePair<string, string> item2 in aCONCDFDNJH.GetPerkSet().Values)
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
		PerkTrigger eICIICPBDMC = new PerkTrigger();
		eICIICPBDMC.SetPerk(this);
		eICIICPBDMC.Parse(node);
		GetTriggers().Add(eICIICPBDMC);
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

	private void SetAttributes(XmlAttribute CJEPEDKEEGF)
	{
		string name = CJEPEDKEEGF.Name;
		string text = CJEPEDKEEGF.Value;
		FunctionExtension oPIFBDJNMKD = new FunctionExtension();
		oPIFBDJNMKD.Parse(text);
		List<FunctionExtension.FunctionObject> list = oPIFBDJNMKD.GetValueObjects();
		foreach (FunctionExtension.FunctionObject item in list)
		{
			if (item.body[0] == '_')
			{
				string gOHIIMFFFJI = item.body.Substring(1, item.body.Length - 1);
				string newValue = perkSetAttributes.GetValue(gOHIIMFFFJI);
				text = text.Replace(item.body, newValue);
			}
		}
		CJEPEDKEEGF.Value = text;
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

	private void ParseRatings(XmlNode MGOANJIJHGB, PerkSetAttributes CJILONFAJIK)
	{
		int count = MGOANJIJHGB.ChildNodes.Count;
		if (count > 0)
		{
			Ratings.Clear();
		}
		foreach (XmlNode childNode in MGOANJIJHGB.ChildNodes)
		{
			string name = childNode.Name;
			if (name.Equals("Rating"))
			{
				Rating cNLOJEAEGLG = new Rating();
				cNLOJEAEGLG.Parse(childNode, CJILONFAJIK);
				Ratings.Add(cNLOJEAEGLG);
			}
		}
	}

	public void CollectTriggersForEvent(List<PerkTrigger> DCJLKCFKCOM, PerkEvent.PerkEventType LFLGCDNKNJI)
	{
		foreach (PerkTrigger item in GetTriggers())
		{
			List<PerkEvent> list = item.GetEvents();
			foreach (PerkEvent item2 in list)
			{
				if (item2.get_Type() == LFLGCDNKNJI)
				{
					DCJLKCFKCOM.Add(item);
					break;
				}
			}
		}
	}

	public void OnFunctionPreCallback(FunctionExtension.CallbackResult DCJLKCFKCOM)
	{
	}

	public void EvaluateFunctionCallback(FunctionExtension.CallbackResult DCJLKCFKCOM)
	{
		FunctionExtension.FunctionCall gLBAFLLMOOH = DCJLKCFKCOM.data as FunctionExtension.FunctionCall;
		FunctionResult nAGGNMIFFGK = DCJLKCFKCOM.result;
		PerkObject iNCAIGLKDIE = DCJLKCFKCOM.target as PerkObject;
		Model fGCODGKLHED = ((GetOwnerModel() == null) ? null : GetOwnerModel());
		if (fGCODGKLHED != null && nAGGNMIFFGK.Value.Equals("Enemy"))
		{
			fGCODGKLHED = fGCODGKLHED.GetCombatTarget();
		}
		switch (gLBAFLLMOOH.functionName)
		{
		case "UniformFloatRandom":
			EvaluateUniformFloatRandom(fGCODGKLHED, gLBAFLLMOOH, iNCAIGLKDIE, nAGGNMIFFGK);
			return;
		case "PlayerAttribute":
			EvaluatePlayerAttribute(fGCODGKLHED, gLBAFLLMOOH, iNCAIGLKDIE, nAGGNMIFFGK);
			return;
		case "PlayerParameter":
			EvaluatePlayerParameter(fGCODGKLHED, gLBAFLLMOOH, iNCAIGLKDIE, nAGGNMIFFGK);
			return;
		case "RoundParameter":
			EvaluateRoundParameter(fGCODGKLHED, gLBAFLLMOOH, iNCAIGLKDIE, nAGGNMIFFGK);
			return;
		case "Hit":
			EvaluateHit(fGCODGKLHED, gLBAFLLMOOH, iNCAIGLKDIE, nAGGNMIFFGK);
			return;
		case "StringInArray":
			EvaluateStringInArray(fGCODGKLHED, gLBAFLLMOOH, iNCAIGLKDIE, nAGGNMIFFGK);
			return;
		case "CoordX":
			EvaluateCoordX(fGCODGKLHED, gLBAFLLMOOH, iNCAIGLKDIE, nAGGNMIFFGK);
			return;
		case "CoordY":
			EvaluateCoordY(fGCODGKLHED, gLBAFLLMOOH, iNCAIGLKDIE, nAGGNMIFFGK);
			return;
		case "CoordZ":
			EvaluateCoordZ(fGCODGKLHED, gLBAFLLMOOH, iNCAIGLKDIE, nAGGNMIFFGK);
			return;
		case "RandomAspect":
			EvaluateRandomAspect(fGCODGKLHED, gLBAFLLMOOH, iNCAIGLKDIE, nAGGNMIFFGK);
			return;
		case "Abs":
			EvaluateAbs(fGCODGKLHED, gLBAFLLMOOH, iNCAIGLKDIE, nAGGNMIFFGK);
			return;
		case "Aspect":
			EvaluateAspect(fGCODGKLHED, gLBAFLLMOOH, iNCAIGLKDIE, nAGGNMIFFGK);
			return;
		case "Round":
			EvaluateRound(fGCODGKLHED, gLBAFLLMOOH, iNCAIGLKDIE, nAGGNMIFFGK);
			return;
		case "Variable":
			EvaluateVariable(fGCODGKLHED, gLBAFLLMOOH, iNCAIGLKDIE, nAGGNMIFFGK);
			return;
		case "MovesVariable":
			EvaluateVariable(fGCODGKLHED, gLBAFLLMOOH, iNCAIGLKDIE, nAGGNMIFFGK);
			return;
		case "Player":
			EvaluatePlayer(fGCODGKLHED, gLBAFLLMOOH, iNCAIGLKDIE, nAGGNMIFFGK);
			return;
		case "CurrentFight":
			EvaluateCurrentFight(fGCODGKLHED, gLBAFLLMOOH, iNCAIGLKDIE, nAGGNMIFFGK);
			return;
		}
		FunctionExtension.CompareType dLLJOIFFBPL = FunctionExtension.ParseCompareType(gLBAFLLMOOH.functionName);
		bool flag = gLBAFLLMOOH.functionName.Equals("Compare");
		if (flag || dLLJOIFFBPL != FunctionExtension.CompareType.COMPARE_NONE)
		{
			EvaluateCompare(flag, fGCODGKLHED, gLBAFLLMOOH, iNCAIGLKDIE, nAGGNMIFFGK);
		}
	}

	private void EvaluateCurrentFight(Model ACENLMONNPA, FunctionExtension.FunctionCall KJFKPMCPIBH, PerkObject INCAIGLKDIE, FunctionResult DCJLKCFKCOM)
	{
		if (Fight.GetCurrentFight() != null && KJFKPMCPIBH.propertyName.Equals("isRaid"))
		{
			bool flag = Fight.GetCurrentFight().GetFightDefinition().get_Type() == BattleType.FightRaid;
			DCJLKCFKCOM.Value = ((!flag) ? "0" : "1");
		}
	}

	private void EvaluateVariable(Model ACENLMONNPA, FunctionExtension.FunctionCall KJFKPMCPIBH, PerkObject INCAIGLKDIE, FunctionResult DCJLKCFKCOM)
	{
		if (KJFKPMCPIBH.argumentValues.Count == 0)
		{
			DCJLKCFKCOM.Value = string.Empty;
		}
		string mJOCMMIBOGJ = KJFKPMCPIBH.argumentValues[0].body;
		Dictionary<string, float> cNOPDMEAODG = ACENLMONNPA.GetConditions().PerkVariables;
		Dictionary<string, string> stringVariables = ACENLMONNPA.GetConditions().PerkStringVariables;
		if (stringVariables.ContainsKey(mJOCMMIBOGJ))
		{
			DCJLKCFKCOM.Value = stringVariables[mJOCMMIBOGJ];
		}
		else if (cNOPDMEAODG.ContainsKey(mJOCMMIBOGJ))
		{
			DCJLKCFKCOM.Value = cNOPDMEAODG[mJOCMMIBOGJ].ToString();
		}
		else
		{
			DCJLKCFKCOM.Value = string.Empty;
		}
	}

	private void EvaluatePlayer(Model ACENLMONNPA, FunctionExtension.FunctionCall KJFKPMCPIBH, PerkObject INCAIGLKDIE, FunctionResult DCJLKCFKCOM)
	{
		if (KJFKPMCPIBH.propertyName.Equals("Level"))
		{
			DCJLKCFKCOM.Value = ListSF.GetRoster().GetLevel().ToString();
		}
	}

	private void EvaluateUniformFloatRandom(Model ACENLMONNPA, FunctionExtension.FunctionCall KJFKPMCPIBH, PerkObject INCAIGLKDIE, FunctionResult DCJLKCFKCOM)
	{
		DCJLKCFKCOM.Value = NekkiMath.randomFloat(1f).ToString();
	}

	private void EvaluatePlayerAttribute(Model ACENLMONNPA, FunctionExtension.FunctionCall KJFKPMCPIBH, PerkObject INCAIGLKDIE, FunctionResult DCJLKCFKCOM)
	{
		if (ACENLMONNPA != null)
		{
			ModelParameters kMMJCHDKBDO = ACENLMONNPA.Parameters;
			if (kMMJCHDKBDO != null)
			{
				int OEMALIFPGPO = 0;
				kMMJCHDKBDO.FinalAttributes.Get(KJFKPMCPIBH.propertyName, ref OEMALIFPGPO);
				DCJLKCFKCOM.Value = OEMALIFPGPO.ToString();
			}
		}
	}

	private void EvaluatePlayerParameter(Model ACENLMONNPA, FunctionExtension.FunctionCall KJFKPMCPIBH, PerkObject INCAIGLKDIE, FunctionResult DCJLKCFKCOM)
	{
		if (ACENLMONNPA != null)
		{
			switch (KJFKPMCPIBH.propertyName)
			{
			case "Health":
				DCJLKCFKCOM.Value = ACENLMONNPA.GetLife().ToString();
				break;
			case "Pain":
				DCJLKCFKCOM.Value = ACENLMONNPA.GetPain().ToString();
				break;
			case "Shock":
				DCJLKCFKCOM.Value = ((!ACENLMONNPA.IsInShock()) ? "0" : "1");
				break;
			case "Disarm":
				DCJLKCFKCOM.Value = ((!ACENLMONNPA.WasDisarmed()) ? "0" : "1");
				break;
			case "Style":
				DCJLKCFKCOM.Value = ACENLMONNPA.StyleName;
				break;
			case "StyleGain":
				DCJLKCFKCOM.Value = ACENLMONNPA.StyleProgress.ToString();
				break;
			case "Combo":
				DCJLKCFKCOM.Value = ACENLMONNPA.GetComboCount().ToString();
				break;
			case "MagicBullet":
				DCJLKCFKCOM.Value = ACENLMONNPA.GetMagicCharges().ToString();
				break;
			case "MagicCharge":
				DCJLKCFKCOM.Value = ACENLMONNPA.GetMagicChargeFraction().ToString();
				break;
			case "Magic":
				DCJLKCFKCOM.Value = (ACENLMONNPA.Parameters.Magic == null) ?
					string.Empty : ACENLMONNPA.Parameters.Magic.Name;
				break;
			case "Ranged":
				DCJLKCFKCOM.Value = (ACENLMONNPA.Parameters.Ranged == null) ?
					string.Empty : ACENLMONNPA.Parameters.Ranged.Name;
				break;
			case "Weapon":
				DCJLKCFKCOM.Value = (ACENLMONNPA.Parameters.Weapon == null) ?
					string.Empty : ACENLMONNPA.Parameters.Weapon.Name;
				break;
			case "Skeleton":
				DCJLKCFKCOM.Value = (ACENLMONNPA.Parameters.Skeleton == null) ?
					string.Empty : ACENLMONNPA.Parameters.Skeleton.Name;
				break;
			case "RaidChargeBullet":
				break;
			case "DamageConverter":
				DCJLKCFKCOM.Value = ACENLMONNPA.GetPowerMultiplier().ToString();
				break;
			case "DefaultPerksAspect":
				DCJLKCFKCOM.Value = ACENLMONNPA.GetBonusModifier().ToString();
				break;
			case "isPlayer":
				DCJLKCFKCOM.Value = ((!ACENLMONNPA.IsPlayerModel()) ? "0" : "1");
				break;
			}
		}
	}

	private void EvaluateRoundParameter(Model ACENLMONNPA, FunctionExtension.FunctionCall KJFKPMCPIBH, PerkObject INCAIGLKDIE, FunctionResult DCJLKCFKCOM)
	{
		if (Fight.GetCurrentFight() != null)
		{
			switch (KJFKPMCPIBH.propertyName)
			{
			case "Number":
				DCJLKCFKCOM.Value = Fight.GetCurrentFight().get_RoundNumber().ToString();
				break;
			case "TimeLeft":
				DCJLKCFKCOM.Value = Fight.GetCurrentFight().get_RoundTimeLeftFrames().ToString();
				break;
			case "TimePassed":
				DCJLKCFKCOM.Value = Fight.GetCurrentFight().get_RoundTimePassedFrames().ToString();
				break;
			case "RoundTime":
				DCJLKCFKCOM.Value = Fight.GetCurrentFight().get_RoundTimeTotalFrames().ToString();
				break;
			}
		}
	}

	private void EvaluateCompare(bool GOIOEAHAAIA, Model ACENLMONNPA, FunctionExtension.FunctionCall KJFKPMCPIBH, PerkObject INCAIGLKDIE, FunctionResult DCJLKCFKCOM)
	{
		if (DCJLKCFKCOM != null)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(DCJLKCFKCOM.Value);
			if (!GOIOEAHAAIA)
			{
				stringBuilder.Append(",");
				stringBuilder.Append(KJFKPMCPIBH.functionName);
			}
			bool flag = FunctionExtension.IsCompare(stringBuilder.ToString());
			DCJLKCFKCOM.Value = ((!flag) ? "0" : "1");
		}
	}

	private void EvaluateCoordX(Model ACENLMONNPA, FunctionExtension.FunctionCall KJFKPMCPIBH, PerkObject INCAIGLKDIE, FunctionResult DCJLKCFKCOM)
	{
		if (KJFKPMCPIBH.argumentValues.Count < 2)
		{
			DCJLKCFKCOM.Value = "-10000";
			return;
		}
		string mJOCMMIBOGJ = KJFKPMCPIBH.argumentValues[0].body;
		string mJOCMMIBOGJ2 = KJFKPMCPIBH.argumentValues[1].body;
		string text = string.Empty;
		if (KJFKPMCPIBH.argumentValues.Count >= 3)
		{
			text = KJFKPMCPIBH.argumentValues[2].body;
		}
		DistancePoint oGHICEHKFOL = new DistancePoint();
		oGHICEHKFOL.Create(mJOCMMIBOGJ, mJOCMMIBOGJ2, text);
		float num = 0f;
		if (oGHICEHKFOL.ObjectType != DistancePoint.Object.OBJECT_NODES)
		{
			num = oGHICEHKFOL.GetX(ACENLMONNPA.GetConditions());
		}
		else
		{
			if (oGHICEHKFOL.TargetModel == ModelType.ModelTargetType.MODEL_THIS)
			{
				ModelNode lCDGOCIAIDK = ACENLMONNPA.GetBodyObject().GetNodeByName(text);
				num = lCDGOCIAIDK.GetStart().GetX();
			}
			if (oGHICEHKFOL.TargetModel == ModelType.ModelTargetType.MODEL_OTHER)
			{
				ModelNode lCDGOCIAIDK2 = ACENLMONNPA.GetCombatTarget().GetBodyObject().GetNodeByName(text);
				num = lCDGOCIAIDK2.GetStart().GetX();
			}
		}
		DCJLKCFKCOM.Value = num.ToString();
	}

	private void EvaluateCoordY(Model ACENLMONNPA, FunctionExtension.FunctionCall KJFKPMCPIBH, PerkObject INCAIGLKDIE, FunctionResult DCJLKCFKCOM)
	{
		if (KJFKPMCPIBH.argumentValues.Count < 2)
		{
			DCJLKCFKCOM.Value = "-10000";
			return;
		}
		string mJOCMMIBOGJ = KJFKPMCPIBH.argumentValues[0].body;
		string mJOCMMIBOGJ2 = KJFKPMCPIBH.argumentValues[1].body;
		string text = string.Empty;
		if (KJFKPMCPIBH.argumentValues.Count >= 3)
		{
			text = KJFKPMCPIBH.argumentValues[2].body;
		}
		DistancePoint oGHICEHKFOL = new DistancePoint();
		oGHICEHKFOL.Create(mJOCMMIBOGJ, mJOCMMIBOGJ2, text);
		float num = 0f;
		if (oGHICEHKFOL.ObjectType != DistancePoint.Object.OBJECT_NODES)
		{
			num = oGHICEHKFOL.GetY(ACENLMONNPA.GetConditions());
		}
		else
		{
			if (oGHICEHKFOL.TargetModel == ModelType.ModelTargetType.MODEL_THIS)
			{
				ModelNode lCDGOCIAIDK = ACENLMONNPA.GetBodyObject().GetNodeByName(text);
				num = lCDGOCIAIDK.GetStart().GetY();
			}
			if (oGHICEHKFOL.TargetModel == ModelType.ModelTargetType.MODEL_OTHER)
			{
				ModelNode lCDGOCIAIDK2 = ACENLMONNPA.GetCombatTarget().GetBodyObject().GetNodeByName(text);
				num = lCDGOCIAIDK2.GetStart().GetY();
			}
		}
		DCJLKCFKCOM.Value = num.ToString();
	}

	private void EvaluateCoordZ(Model ACENLMONNPA, FunctionExtension.FunctionCall KJFKPMCPIBH, PerkObject INCAIGLKDIE, FunctionResult DCJLKCFKCOM)
	{
		if (KJFKPMCPIBH.argumentValues.Count < 2)
		{
			DCJLKCFKCOM.Value = "-10000";
			return;
		}
		string mJOCMMIBOGJ = KJFKPMCPIBH.argumentValues[0].body;
		string mJOCMMIBOGJ2 = KJFKPMCPIBH.argumentValues[1].body;
		string text = string.Empty;
		if (KJFKPMCPIBH.argumentValues.Count >= 3)
		{
			text = KJFKPMCPIBH.argumentValues[2].body;
		}
		DistancePoint oGHICEHKFOL = new DistancePoint();
		oGHICEHKFOL.Create(mJOCMMIBOGJ, mJOCMMIBOGJ2, text);
		float num = 0f;
		if (oGHICEHKFOL.ObjectType != DistancePoint.Object.OBJECT_NODES)
		{
			num = oGHICEHKFOL.GetZ(ACENLMONNPA.GetConditions());
		}
		else
		{
			if (oGHICEHKFOL.TargetModel == ModelType.ModelTargetType.MODEL_THIS)
			{
				ModelNode lCDGOCIAIDK = ACENLMONNPA.GetBodyObject().GetNodeByName(text);
				num = lCDGOCIAIDK.GetStart().GetZ();
			}
			if (oGHICEHKFOL.TargetModel == ModelType.ModelTargetType.MODEL_OTHER)
			{
				ModelNode lCDGOCIAIDK2 = ACENLMONNPA.GetCombatTarget().GetBodyObject().GetNodeByName(text);
				num = lCDGOCIAIDK2.GetStart().GetZ();
			}
		}
		DCJLKCFKCOM.Value = num.ToString();
	}

	private void EvaluateAbs(Model ACENLMONNPA, FunctionExtension.FunctionCall KJFKPMCPIBH, PerkObject INCAIGLKDIE, FunctionResult DCJLKCFKCOM)
	{
		string dCJLKCFKCOM = DCJLKCFKCOM.Value;
		double value = 0.0;
		Dictionary<string, RpnParser.VariableDelegate> pPEABEJMCPI = new Dictionary<string, RpnParser.VariableDelegate>();
		Dictionary<string, RpnParser.ParameterDelegate> gIOGAJGIGMO = new Dictionary<string, RpnParser.ParameterDelegate>();
		RpnParser.init(pPEABEJMCPI, gIOGAJGIGMO);
		RpnParser.Formula lANLKOHCGEJ = new RpnParser.Formula(dCJLKCFKCOM);
		if (lANLKOHCGEJ.GetVariableCount() == 0)
		{
			double result;
			if (double.TryParse(lANLKOHCGEJ.Calculate().ToString(), out result))
			{
				value = result;
			}
		}
		else
		{
			GameLog.Error("Abs function error! Argument is not valid expression: {0}", DCJLKCFKCOM.Value);
		}
		DCJLKCFKCOM.Value = Math.Abs(value).ToString();
	}

	private void EvaluateRandomAspect(Model ACENLMONNPA, FunctionExtension.FunctionCall KJFKPMCPIBH, PerkObject INCAIGLKDIE, FunctionResult DCJLKCFKCOM)
	{
		if (KJFKPMCPIBH.argumentValues.Count < 2)
		{
			DCJLKCFKCOM.Value = "0";
			return;
		}
		string mJOCMMIBOGJ = KJFKPMCPIBH.argumentValues[0].body;
		string mJOCMMIBOGJ2 = KJFKPMCPIBH.argumentValues[1].body;
		int lHNCHOAEGEA = mJOCMMIBOGJ.ToInt();
		int kAEPJHHLLPK = mJOCMMIBOGJ2.ToInt() + 1;
		NekkiMath.SetSeed();
		int num = NekkiMath.randomInt(lHNCHOAEGEA, kAEPJHHLLPK);
		int mHNCENBCECJ = ListSF.GetRoster().GetLevel();
		int num2 = ForgeManager.GetInstance().GetAspectValueByLevel(mHNCENBCECJ);
		DCJLKCFKCOM.Value = (num2 + num).ToString();
	}

	private void EvaluateAspect(Model ACENLMONNPA, FunctionExtension.FunctionCall KJFKPMCPIBH, PerkObject INCAIGLKDIE, FunctionResult DCJLKCFKCOM)
	{
		if (KJFKPMCPIBH.argumentValues.Count != 1)
		{
			DCJLKCFKCOM.Value = "0";
			GameLog.Error("Aspect function error! Number of argument is not 1: {0}", DCJLKCFKCOM.Value);
		}
		else
		{
			string mJOCMMIBOGJ = KJFKPMCPIBH.argumentValues[0].body;
			float fIJMPFHAKPB = mJOCMMIBOGJ.ToFloat();
			DCJLKCFKCOM.Value = AspectToMultiplier(fIJMPFHAKPB).ToString();
		}
	}

	private void EvaluateRound(Model ACENLMONNPA, FunctionExtension.FunctionCall KJFKPMCPIBH, PerkObject INCAIGLKDIE, FunctionResult DCJLKCFKCOM)
	{
		float num = 0f;
		int num2 = 0;
		string key = "trunc";
		RoundingMode jFOFGHPCIBE = RoundingMode.RT_TRUNC;
		if (KJFKPMCPIBH.argumentValues.Count < 1)
		{
			DCJLKCFKCOM.Value = "RoundingError";
			return;
		}
		if (KJFKPMCPIBH.argumentValues.Count > 3)
		{
			DCJLKCFKCOM.Value = "RoundingError";
			return;
		}
		if (KJFKPMCPIBH.argumentValues.Count >= 1)
		{
			string mJOCMMIBOGJ = KJFKPMCPIBH.argumentValues[0].body;
			num = mJOCMMIBOGJ.ToFloat();
		}
		if (KJFKPMCPIBH.argumentValues.Count >= 2)
		{
			string mJOCMMIBOGJ2 = KJFKPMCPIBH.argumentValues[1].body;
			num2 = mJOCMMIBOGJ2.ToInt();
		}
		if (KJFKPMCPIBH.argumentValues.Count == 3)
		{
			string mJOCMMIBOGJ3 = KJFKPMCPIBH.argumentValues[2].body;
			key = mJOCMMIBOGJ3;
		}
		if (!roundingModesByName.ContainsKey(key))
		{
			DCJLKCFKCOM.Value = "RoundingError";
			return;
		}
		jFOFGHPCIBE = roundingModesByName[key];
		float num3 = 0f;
		switch (jFOFGHPCIBE)
		{
		case RoundingMode.RT_FLOOR:
			DCJLKCFKCOM.Value = NekkiMath.FloorToDecimals(num, num2).ToString();
			break;
		case RoundingMode.RT_CEIL:
			DCJLKCFKCOM.Value = NekkiMath.CeilToDecimals(num, num2).ToString();
			break;
		case RoundingMode.RT_TRUNC:
			DCJLKCFKCOM.Value = NekkiMath.TruncateToDecimals(num, num2).ToString();
			break;
		case RoundingMode.RT_INF:
			DCJLKCFKCOM.Value = NekkiMath.RoundAwayFromZero(num, num2).ToString();
			break;
		default:
			DCJLKCFKCOM.Value = string.Empty;
			break;
		}
	}

	private void EvaluateStringInArray(Model ACENLMONNPA, FunctionExtension.FunctionCall KJFKPMCPIBH, PerkObject INCAIGLKDIE, FunctionResult DCJLKCFKCOM)
	{
		List<string> list = new List<string>(DCJLKCFKCOM.Value.Split(','));
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
					DCJLKCFKCOM.Value = "1";
					return;
				}
			}
		}
		DCJLKCFKCOM.Value = "0";
	}

	private void EvaluateHit(Model ACENLMONNPA, FunctionExtension.FunctionCall KJFKPMCPIBH, PerkObject INCAIGLKDIE, FunctionResult DCJLKCFKCOM)
	{
		if (Fight.GetCurrentFight() == null)
		{
			return;
		}
		Model.StrikeResult fKGAAFNNCNE = Fight.GetCurrentFight().LastStrikeResult;
		if (fKGAAFNNCNE == null)
		{
			return;
		}
		switch (KJFKPMCPIBH.propertyName)
		{
		case "Player":
			DCJLKCFKCOM.Value = ((fKGAAFNNCNE.Victim != ACENLMONNPA) ? "Enemy" : "Me");
			break;
		case "DefenseAttribute":
			DCJLKCFKCOM.Value = fKGAAFNNCNE.DefenceAttribute;
			break;
		case "Block":
			DCJLKCFKCOM.Value = ((!fKGAAFNNCNE.IsBlocked) ? "0" : "1");
			break;
		case "Critical":
			DCJLKCFKCOM.Value = ((!fKGAAFNNCNE.IsCritical) ? "0" : "1");
			break;
		case "Shock":
			DCJLKCFKCOM.Value = ((!fKGAAFNNCNE.IsShock) ? "0" : "1");
			break;
		case "Animations":
		{
			StringBuilder stringBuilder = new StringBuilder();
			List<string> list = fKGAAFNNCNE.AttackAnimation.GetTemplateNames();
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				stringBuilder.Append(list[i]);
				if (i < count - 1)
				{
					stringBuilder.Append("|");
				}
			}
			DCJLKCFKCOM.Value = stringBuilder.ToString();
			break;
		}
		case "Damage":
			DCJLKCFKCOM.Value = fKGAAFNNCNE.FinalDamage.ToString();
			break;
		case "BaseDamage":
		{
			float hMOLHIEDINK = fKGAAFNNCNE.BaseDamage;
			float num = 1f;
			if (fKGAAFNNCNE.IsBlocked)
			{
				int OEMALIFPGPO = 0;
				string nJFGLOECJEK = GameUtils.GetBlockDamageFactor().Attribute;
				fKGAAFNNCNE.Victim.Parameters.FinalAttributes.Get(nJFGLOECJEK, ref OEMALIFPGPO);
				float aMKPAGCFMIN = GameUtils.GetBlockDamageFactor().Base;
				num = Mathf.Pow(2f, (float)OEMALIFPGPO * aMKPAGCFMIN);
			}
			float num2 = 1f;
			if (fKGAAFNNCNE.IsCritical && fKGAAFNNCNE.AttackerModel != null)
			{
				int OEMALIFPGPO2 = 0;
				string nJFGLOECJEK2 = GameUtils.GetCriticalHitDamage().Attribute;
				fKGAAFNNCNE.AttackerModel.Parameters.FinalAttributes.Get(nJFGLOECJEK2, ref OEMALIFPGPO2);
				float aMKPAGCFMIN2 = GameUtils.GetCriticalHitDamage().Base;
				num2 = Mathf.Pow(2f, (float)OEMALIFPGPO2 * aMKPAGCFMIN2);
			}
			DCJLKCFKCOM.Value = (hMOLHIEDINK * num * num2).ToString();
			break;
		}
		}
	}

	public string ResolveDescriptionText(string PMDPPGNJAFE)
	{
		if (string.IsNullOrEmpty(PMDPPGNJAFE))
		{
			return string.Empty;
		}
		int num = PMDPPGNJAFE.IndexOf('{');
		if (num == -1)
		{
			return PMDPPGNJAFE;
		}
		int num2 = PMDPPGNJAFE.LastIndexOf('}');
		if (num2 == -1)
		{
			return PMDPPGNJAFE;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(PMDPPGNJAFE);
		QuestParameters jCICKLIMBEF = ListSF.GetInstance().GetQuestParameters();
		ConditionExtension.CompareResult lNIDLHOIHIM = new ConditionExtension.CompareResult();
		QuestCondition kKDGLNECFHA = new QuestCondition();
		kKDGLNECFHA.SetParameters(jCICKLIMBEF);
		while (num <= num2)
		{
			string newValue = string.Empty;
			if (PMDPPGNJAFE[num] == '{')
			{
				int num3 = PMDPPGNJAFE.IndexOf('}', num);
				string text = PMDPPGNJAFE.Substring(num + 1, num3 - num - 1);
				if (!string.IsNullOrEmpty(text))
				{
					FunctionExtension oPIFBDJNMKD = new FunctionExtension();
					oPIFBDJNMKD.SetVariableCallback(OnFunctionPreCallback);
					oPIFBDJNMKD.SetFunctionCallback(EvaluateFunctionCallback);
					foreach (KeyValuePair<string, string> item in perkSetAttributes.Values)
					{
						oPIFBDJNMKD.SetVariable(item.Key, item.Value);
					}
					oPIFBDJNMKD.Parse(text);
					FunctionResult dEIHAOLOPLC = oPIFBDJNMKD.Calculate();
					newValue = dEIHAOLOPLC.Value;
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
