using System.Collections.Generic;
using System.Globalization;
using System.Xml;

public class RewardItem : Rewardable
{
	public string Name;

	public uint UpgradeNumber;

	internal string UpgradeLevelExpression { get; private set; }

	internal string EclipseRewardId { get; private set; }

	internal int EclipseGrantIndex { get; private set; } = -1;

	internal bool HasEclipseGrantConfiguration => !string.IsNullOrEmpty(EclipseRewardId);

	private XmlElement _sourceNode;

	internal sealed class ConfiguredGrantEnchantment
	{
		internal string Name { get; }

		internal string Aspect { get; }

		internal string EclipseKind { get; }
		internal string ChanceFactor { get; }
		internal string Chance { get; }
		internal string Frames { get; }
		internal IReadOnlyDictionary<string, string> Parameters { get; }

		internal ConfiguredGrantEnchantment(string name, string aspect, string eclipseKind,
			string chanceFactor = null, string chance = null, string frames = null,
			IReadOnlyDictionary<string, string> parameters = null)
		{
			Name = name;
			Aspect = aspect;
			EclipseKind = eclipseKind;
			ChanceFactor = chanceFactor;
			Chance = chance;
			Frames = frames;
			Parameters = parameters;
		}
	}

	protected string levelExpression;

	public List<PerkStruct> enchantments = new List<PerkStruct>();

	public RewardItem(XmlNode node)
	{
		Parse(node);
		Kind = RewardKind.REWARD_ITEM;
		Name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		levelExpression = node.Attributes["Level"].GetStringOrDefault(string.Empty);
		UpgradeNumber = node.Attributes["UpgradeNumber"].ParseUint();
		UpgradeLevelExpression = node.Attributes["UpgradeLevel"].GetStringOrDefault(string.Empty);
		EclipseRewardId = node.Attributes["EclipseReward"].GetStringOrDefault(string.Empty);
		int grantIndex;
		if (!string.IsNullOrEmpty(EclipseRewardId) &&
			int.TryParse(node.Attributes["EclipseGrant"].GetStringOrDefault(string.Empty), NumberStyles.None,
				CultureInfo.InvariantCulture, out grantIndex)) EclipseGrantIndex = grantIndex;
		if (HasEclipseGrantConfiguration && node is XmlElement sourceElement)
			_sourceNode = (XmlElement)sourceElement.CloneNode(true);
		if (UpgradeLevelExpression.Length != 0 && node.Attributes["UpgradeNumber"] != null)
		{
			throw new System.FormatException("Reward item cannot specify both UpgradeLevel and UpgradeNumber: " + Name);
		}
		XmlNode xmlNode = node["Enchantments"];
		if (xmlNode == null)
		{
			return;
		}
		foreach (XmlNode childNode in xmlNode.ChildNodes)
		{
			PerkStruct item = new PerkStruct(childNode);
			enchantments.Add(item);
		}
	}

	internal RewardItem CloneForConfiguredGrant(int level, IReadOnlyList<ConfiguredGrantEnchantment> enchantments)
	{
		if (_sourceNode == null) throw new System.InvalidOperationException("Reward item source XML is unavailable: " + Name);
		XmlElement item = (XmlElement)_sourceNode.CloneNode(true);
		item.SetAttribute("Level", level.ToString(CultureInfo.InvariantCulture));
		XmlNode oldEnchantments = item["Enchantments"];
		if (oldEnchantments != null) item.RemoveChild(oldEnchantments);
		if (enchantments != null && enchantments.Count != 0)
		{
			XmlElement enchantmentsNode = item.OwnerDocument.CreateElement("Enchantments");
			for (int i = 0; i < enchantments.Count; i++)
			{
				XmlElement perk = item.OwnerDocument.CreateElement("Perk");
				perk.SetAttribute("Name", enchantments[i].Name);
				if (!string.IsNullOrEmpty(enchantments[i].EclipseKind))
					perk.SetAttribute(PerkStruct.EclipseKindAttribute, enchantments[i].EclipseKind);
				var enchantment = enchantments[i];
				if (!string.IsNullOrEmpty(enchantment.Aspect) ||
					!string.IsNullOrEmpty(enchantment.ChanceFactor) || !string.IsNullOrEmpty(enchantment.Chance) ||
					!string.IsNullOrEmpty(enchantment.Frames) || (enchantment.Parameters != null && enchantment.Parameters.Count != 0))
				{
					XmlElement set = item.OwnerDocument.CreateElement("Set");
					if (!string.IsNullOrEmpty(enchantment.Aspect)) set.SetAttribute("Aspect", enchantment.Aspect);
					if (!string.IsNullOrEmpty(enchantment.ChanceFactor)) set.SetAttribute("ChanceFactor", enchantment.ChanceFactor);
					if (!string.IsNullOrEmpty(enchantment.Chance)) set.SetAttribute("Chance", enchantment.Chance);
					if (!string.IsNullOrEmpty(enchantment.Frames)) set.SetAttribute("Frames", enchantment.Frames);
					if (enchantment.Parameters != null)
						foreach (var parameter in enchantment.Parameters)
							set.SetAttribute(parameter.Key, parameter.Value);
					perk.AppendChild(set);
				}
				enchantmentsNode.AppendChild(perk);
			}
			item.AppendChild(enchantmentsNode);
		}
		return new RewardItem(item);
	}

	public int EvaluateLevel()
	{
		return EvaluateLevelExpression(levelExpression).ToInt();
	}

	internal int EvaluateUpgradeLevel()
	{
		int value;
		if (!int.TryParse(EvaluateLevelExpression(UpgradeLevelExpression), out value))
			throw new System.FormatException("Reward upgrade level must evaluate to an integer: " + Name);
		return value;
	}

	private string EvaluateLevelExpression(string expression)
	{
		FunctionExtension oPIFBDJNMKD = new FunctionExtension();
		oPIFBDJNMKD.Parse(expression);
		oPIFBDJNMKD.SetFunctionCallback(OnFunctionCalled);
		oPIFBDJNMKD.SetVariableCallback(OnFunctionCompleted);
		FunctionResult dEIHAOLOPLC = oPIFBDJNMKD.Calculate();
		return dEIHAOLOPLC.Value;
	}

	public void OnFunctionCompleted(FunctionExtension.CallbackResult DCJLKCFKCOM)
	{
	}

	public void OnFunctionCalled(FunctionExtension.CallbackResult DCJLKCFKCOM)
	{
		FunctionExtension.FunctionCall gLBAFLLMOOH = DCJLKCFKCOM.data as FunctionExtension.FunctionCall;
		FunctionResult nAGGNMIFFGK = DCJLKCFKCOM.result;
		if (gLBAFLLMOOH.functionName.Equals("Player"))
		{
			ResolvePlayerFunction(gLBAFLLMOOH, nAGGNMIFFGK);
		}
	}

	private void ResolvePlayerFunction(FunctionExtension.FunctionCall KJFKPMCPIBH, FunctionResult DCJLKCFKCOM)
	{
		if (KJFKPMCPIBH.propertyName.Equals("Level"))
		{
			DCJLKCFKCOM.Value = ListSF.GetRoster().GetLevel().ToString();
		}
	}
}
