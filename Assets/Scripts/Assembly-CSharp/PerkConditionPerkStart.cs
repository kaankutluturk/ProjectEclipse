using System.Collections.Generic;

public class PerkConditionPerkStart : PerkCondition
{
	private string ParentPerkName;

	private bool IsPlayer;

	public PerkConditionPerkStart(string perkName, bool isPlayer)
	{
		set_Type(PerkConditionType.CONDITION_PERK_START);
		ParentPerkName = perkName;
		IsPlayer = isPlayer;
	}

	public override bool IsEqual(Model model, List<string> args)
	{
		if (!GetPerk().GetOwnerModel().IsPlayerModel())
		{
			return true;
		}
		RaidModelParameters raidParameters = GetPerk().GetOwnerModel().Parameters as RaidModelParameters;
		if (raidParameters == null)
		{
			return true;
		}
		return PerksStage.CanUsePerk(ParentPerkName);
	}
}
