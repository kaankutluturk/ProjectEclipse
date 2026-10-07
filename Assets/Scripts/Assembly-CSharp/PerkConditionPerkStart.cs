using System.Collections.Generic;

public class PerkConditionPerkStart : PerkCondition
{
	private string ParentPerkName;

	private bool IsPlayer;

	public PerkConditionPerkStart(string OCIEELPKJKL, bool EKBOGDKIHIH)
	{
		set_Type(PerkConditionType.CONDITION_PERK_START);
		ParentPerkName = OCIEELPKJKL;
		IsPlayer = EKBOGDKIHIH;
	}

	public override bool IsEqual(Model ACENLMONNPA, List<string> NIKHAICFGNM)
	{
		if (!GetPerk().GetOwnerModel().IsPlayerModel())
		{
			return true;
		}
		RaidModelParameters kAOPLEPILDH = GetPerk().GetOwnerModel().Parameters as RaidModelParameters;
		if (kAOPLEPILDH == null)
		{
			return true;
		}
		return PerksStage.CanUsePerk(ParentPerkName);
	}
}
