using System.Collections.Generic;
using UnityEngine;

public class ComboStatistic
{
	public FightStatistics.FightStyle MaxStyle;

	public string StatisticCrazyStyleToString = GetStyleAlias(FightStatistics.FightStyle.STYLE_TURTLE);

	public int PerfectCount;

	public int CriticalCount;

	public int FirstStrikeCount;

	public int OtherStrikeCount;

	public int HeadStrikeCount;

	public int MaxCombo;

	public int ShockCount;

	public ComboStatisticPrize Prize = new ComboStatisticPrize();

	public DetailedDamages Damages = new DetailedDamages();

	public void SetPrizes(long BLOOFMGLMHP, long GICNLBOICGP, long KNDKJANLIDI, float BHGNKHIKGOG, float FKHKEHICPAH, float IFCOPPPDOCD, float LMKJOMKPOAM, float OJIPBDBMLLO, List<float> LNDELINEHAL)
	{
		float num = 0.5f;
		Prize.BaseBonusValue = BLOOFMGLMHP;
		Prize.BaseGold = GameUtils.GetDenominatedValue(GICNLBOICGP);
		Prize.Experience = KNDKJANLIDI;
		Prize.PerfectGold = GameUtils.GetDenominatedValue((long)(Mathf.Ceil((float)BLOOFMGLMHP * BHGNKHIKGOG) * (float)PerfectCount + num));
		Prize.FirstStrikeGold = GameUtils.GetDenominatedValue((long)(Mathf.Ceil((float)BLOOFMGLMHP * FKHKEHICPAH) * (float)FirstStrikeCount + num));
		Prize.ComboGold = GameUtils.GetDenominatedValue((long)(Mathf.Ceil((float)BLOOFMGLMHP * LMKJOMKPOAM) * (float)MaxCombo + num));
		Prize.StyleGold = GameUtils.GetDenominatedValue((long)(Mathf.Ceil((float)BLOOFMGLMHP * LNDELINEHAL[(int)MaxStyle]) + num));
		Prize.ShockGold = GameUtils.GetDenominatedValue((long)(Mathf.Ceil((float)BLOOFMGLMHP * OJIPBDBMLLO) * (float)ShockCount + num));
		Prize.TotalGold = Prize.BaseGold + Prize.PerfectGold + Prize.FirstStrikeGold + Prize.ComboGold + Prize.StyleGold + Prize.ShockGold;
		Prize.TotalExperience = KNDKJANLIDI;
	}

	public void AddPrizes(long BLOOFMGLMHP, long GICNLBOICGP, long KNDKJANLIDI, float BHGNKHIKGOG, float FKHKEHICPAH, float IFCOPPPDOCD, float LMKJOMKPOAM, float OJIPBDBMLLO, List<float> LNDELINEHAL)
	{
		float num = 0.5f;
		Prize.BaseBonusValue += BLOOFMGLMHP;
		Prize.BaseGold += GameUtils.GetDenominatedValue(GICNLBOICGP);
		Prize.Experience += KNDKJANLIDI;
		Prize.PerfectGold += GameUtils.GetDenominatedValue((long)(Mathf.Ceil((float)BLOOFMGLMHP * BHGNKHIKGOG) * (float)PerfectCount + num));
		Prize.FirstStrikeGold += GameUtils.GetDenominatedValue((long)(Mathf.Ceil((float)BLOOFMGLMHP * FKHKEHICPAH) * (float)FirstStrikeCount + num));
		Prize.ComboGold += GameUtils.GetDenominatedValue((long)(Mathf.Ceil((float)BLOOFMGLMHP * LMKJOMKPOAM) + num) * MaxCombo);
		Prize.StyleGold += GameUtils.GetDenominatedValue((long)(Mathf.Ceil((float)BLOOFMGLMHP * LNDELINEHAL[(int)MaxStyle]) + num));
		Prize.ShockGold += GameUtils.GetDenominatedValue((long)(Mathf.Ceil((float)BLOOFMGLMHP * OJIPBDBMLLO) * (float)ShockCount + num));
		Prize.TotalGold = Prize.BaseGold + Prize.PerfectGold + Prize.FirstStrikeGold + Prize.ComboGold + Prize.StyleGold + Prize.ShockGold;
		Prize.TotalExperience = Prize.Experience;
	}

	public string GetCrazyStyleAlias()
	{
		return GetStyleAlias(MaxStyle);
	}

	public static string GetStyleAlias(FightStatistics.FightStyle KIGNIBIMLKK)
	{
		string result = string.Empty;
		switch (KIGNIBIMLKK)
		{
		case FightStatistics.FightStyle.STYLE_TURTLE:
			result = "goldTurtleStyle";
			break;
		case FightStatistics.FightStyle.STYLE_HARD:
			result = "goldHardStyle";
			break;
		case FightStatistics.FightStyle.STYLE_BRUTAL:
			result = "goldBrutalStyle";
			break;
		case FightStatistics.FightStyle.STYLE_AGGRESSIVE:
			result = "goldAgressiveStyle";
			break;
		case FightStatistics.FightStyle.STYLE_CRAZY:
			result = "goldCrazyStyle";
			break;
		case FightStatistics.FightStyle.STYLE_FANTASTIC:
			result = "goldFantasticStyle";
			break;
		}
		return result;
	}
}
