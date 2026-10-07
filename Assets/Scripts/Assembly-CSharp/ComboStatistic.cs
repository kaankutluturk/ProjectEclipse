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

	public void SetPrizes(long baseBonus, long baseGold, long experience, float perfectFactor, float firstStrikeFactor, float headShotFactor, float comboFactor, float shockFactor, List<float> styleFactors)
	{
		float num = 0.5f;
		Prize.BaseBonusValue = baseBonus;
		Prize.BaseGold = GameUtils.GetDenominatedValue(baseGold);
		Prize.Experience = experience;
		Prize.PerfectGold = GameUtils.GetDenominatedValue((long)(Mathf.Ceil((float)baseBonus * perfectFactor) * (float)PerfectCount + num));
		Prize.FirstStrikeGold = GameUtils.GetDenominatedValue((long)(Mathf.Ceil((float)baseBonus * firstStrikeFactor) * (float)FirstStrikeCount + num));
		Prize.ComboGold = GameUtils.GetDenominatedValue((long)(Mathf.Ceil((float)baseBonus * comboFactor) * (float)MaxCombo + num));
		Prize.StyleGold = GameUtils.GetDenominatedValue((long)(Mathf.Ceil((float)baseBonus * styleFactors[(int)MaxStyle]) + num));
		Prize.ShockGold = GameUtils.GetDenominatedValue((long)(Mathf.Ceil((float)baseBonus * shockFactor) * (float)ShockCount + num));
		Prize.TotalGold = Prize.BaseGold + Prize.PerfectGold + Prize.FirstStrikeGold + Prize.ComboGold + Prize.StyleGold + Prize.ShockGold;
		Prize.TotalExperience = experience;
	}

	public void AddPrizes(long baseBonus, long baseGold, long experience, float perfectFactor, float firstStrikeFactor, float headShotFactor, float comboFactor, float shockFactor, List<float> styleFactors)
	{
		float num = 0.5f;
		Prize.BaseBonusValue += baseBonus;
		Prize.BaseGold += GameUtils.GetDenominatedValue(baseGold);
		Prize.Experience += experience;
		Prize.PerfectGold += GameUtils.GetDenominatedValue((long)(Mathf.Ceil((float)baseBonus * perfectFactor) * (float)PerfectCount + num));
		Prize.FirstStrikeGold += GameUtils.GetDenominatedValue((long)(Mathf.Ceil((float)baseBonus * firstStrikeFactor) * (float)FirstStrikeCount + num));
		Prize.ComboGold += GameUtils.GetDenominatedValue((long)(Mathf.Ceil((float)baseBonus * comboFactor) + num) * MaxCombo);
		Prize.StyleGold += GameUtils.GetDenominatedValue((long)(Mathf.Ceil((float)baseBonus * styleFactors[(int)MaxStyle]) + num));
		Prize.ShockGold += GameUtils.GetDenominatedValue((long)(Mathf.Ceil((float)baseBonus * shockFactor) * (float)ShockCount + num));
		Prize.TotalGold = Prize.BaseGold + Prize.PerfectGold + Prize.FirstStrikeGold + Prize.ComboGold + Prize.StyleGold + Prize.ShockGold;
		Prize.TotalExperience = Prize.Experience;
	}

	public string GetCrazyStyleAlias()
	{
		return GetStyleAlias(MaxStyle);
	}

	public static string GetStyleAlias(FightStatistics.FightStyle style)
	{
		string result = string.Empty;
		switch (style)
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
