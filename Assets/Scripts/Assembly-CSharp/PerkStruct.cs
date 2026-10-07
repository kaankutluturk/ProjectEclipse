using System.Collections.Generic;
using System.Xml;

public class PerkStruct
{
	public const string EclipseEnchantmentAttribute = "EclipseEnchantment";

	public const string EclipseKindAttribute = "EclipseKind";

	private string _name = string.Empty;

	private List<string> _itemTypes = new List<string>();

	private List<KeyValuePair<string, string>> setPairs = new List<KeyValuePair<string, string>>();

	private string _eclipseEnchantment = string.Empty;

	private string _eclipseKind = string.Empty;

	private readonly Dictionary<string, string> _eclipseParameters =
		new Dictionary<string, string>(System.StringComparer.Ordinal);

	public List<string> ItemTypes
	{
		get
		{
			return GetItemTypes();
		}
	}

	public List<KeyValuePair<string, string>> Pairs
	{
		get
		{
			return GetPairs();
		}
	}

	public string EclipseEnchantment
	{
		get
		{
			return _eclipseEnchantment;
		}
	}

	public string EclipseKind
	{
		get
		{
			return _eclipseKind;
		}
	}

	public IReadOnlyDictionary<string, string> EclipseParameters => _eclipseParameters;

	public PerkStruct(PerkStruct NOLFMPDGCOC)
	{
		_name = string.Copy(NOLFMPDGCOC.get_Name());
		_eclipseEnchantment = string.Copy(NOLFMPDGCOC._eclipseEnchantment);
		_eclipseKind = string.Copy(NOLFMPDGCOC._eclipseKind);
		foreach (KeyValuePair<string, string> pair in NOLFMPDGCOC._eclipseParameters)
			_eclipseParameters.Add(string.Copy(pair.Key), string.Copy(pair.Value));
		_itemTypes = new List<string>();
		NOLFMPDGCOC._itemTypes.ForEach((string DHDMNHCIPEH) =>
		{
			_itemTypes.Add(string.Copy(DHDMNHCIPEH));
		});
		setPairs = new List<KeyValuePair<string, string>>();
		NOLFMPDGCOC.setPairs.ForEach((KeyValuePair<string, string> DHDMNHCIPEH) =>
		{
			string key = string.Copy(DHDMNHCIPEH.Key);
			string value = string.Copy(DHDMNHCIPEH.Value);
			setPairs.Add(new KeyValuePair<string, string>(key, value));
		});
	}

	public PerkStruct(XmlNode node)
	{
		_name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		_eclipseEnchantment = node.Attributes[EclipseEnchantmentAttribute].GetStringOrDefault(string.Empty);
		_eclipseKind = node.Attributes[EclipseKindAttribute].GetStringOrDefault(string.Empty);
		string text = node.Attributes["ItemType"].GetStringOrDefault(string.Empty);
		if (text != null)
		{
			string[] collection = text.Split('|');
			_itemTypes.AddRange(collection);
		}
		XmlNode parameters = node[Eclipse.Modding.ModEffectSaveData.NodeName];
		if (parameters != null && parameters.Attributes?["Format"]?.Value == Eclipse.Modding.ModEffectSaveData.Format)
		{
			foreach (XmlNode parameter in parameters.ChildNodes)
			{
				if (parameter.NodeType != XmlNodeType.Element ||
					parameter.Name != Eclipse.Modding.ModEffectSaveData.ParameterNodeName) continue;
				string key = parameter.Attributes?["Name"]?.Value;
				XmlAttribute value = parameter.Attributes?["Value"];
				if (string.IsNullOrEmpty(key) || value == null || _eclipseParameters.ContainsKey(key)) continue;
				_eclipseParameters.Add(key, value.Value);
			}
		}
		XmlNode xmlNode = node["Set"];
		if (xmlNode != null)
		{
			foreach (XmlAttribute attribute in xmlNode.Attributes)
			{
				KeyValuePair<string, string> item = new KeyValuePair<string, string>(attribute.Name, attribute.GetStringOrDefault(string.Empty));
				setPairs.Add(item);
			}
		}
	}

	public string get_Name()
	{
		return _name;
	}

	public List<string> GetItemTypes()
	{
		return _itemTypes;
	}

	public List<KeyValuePair<string, string>> GetPairs()
	{
		return setPairs;
	}

	private bool CompareItemType(string LMNNBBKHMEI)
	{
		foreach (string item in _itemTypes)
		{
			if (item == LMNNBBKHMEI)
			{
				return true;
			}
		}
		return false;
	}

	public void EvaluatePairValues()
	{
		FunctionExtension oPIFBDJNMKD = new FunctionExtension();
		PerkInfoItem aCONCDFDNJH = GameUtils.PerkItemList.FindBasePerk(_name);
		if (aCONCDFDNJH == null)
		{
			return;
		}
		List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
		foreach (KeyValuePair<string, string> item in setPairs)
		{
			oPIFBDJNMKD.Parse(item.Value);
			oPIFBDJNMKD.SetFunctionCallback(aCONCDFDNJH.EvaluateFunctionCallback);
			oPIFBDJNMKD.SetVariableCallback(aCONCDFDNJH.OnFunctionPreCallback);
			FunctionResult dEIHAOLOPLC = oPIFBDJNMKD.Calculate();
			list.Add(new KeyValuePair<string, string>(item.Key, dEIHAOLOPLC.Value));
		}
		setPairs = list;
	}
}
