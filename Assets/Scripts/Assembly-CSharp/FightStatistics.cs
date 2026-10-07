using UnityEngine;

public class FightStatistics
{
	public enum FightStyle
	{
		STYLE_TURTLE = 0,
		STYLE_HARD = 1,
		STYLE_BRUTAL = 2,
		STYLE_AGGRESSIVE = 3,
		STYLE_CRAZY = 4,
		STYLE_FANTASTIC = 5
	}

	private const int MIN_COMBO = 3;

	private int winCount;

	private FightStyle style;

	private int unusedCounterA;

	private int comboCount;

	private int maxComboLength;

	private int unusedCounterB;

	private int unusedCounterC;

	private bool _noStrikes = true;

	private int comboFinishDelay;

	private int currentComboHits;

	public FightStyle Style
	{
		get
		{
			return GetStyle();
		}
		set
		{
			SetStyle(value);
		}
	}

	public FightStyle GetStyle()
	{
		return style;
	}

	public void SetStyle(FightStyle value)
	{
		style = (FightStyle)Mathf.Max((int)style, (int)value);
	}

	public void RegisterWinIfEqual(float left, float right)
	{
		if (left == right)
		{
			winCount++;
		}
	}

	public void RegisterWin()
	{
		winCount++;
	}

	public void Draw()
	{
		if (comboFinishDelay > 0)
		{
			comboFinishDelay--;
			if (comboFinishDelay == 0)
			{
				FinishCombo();
			}
		}
	}

	public void Reset()
	{
		_noStrikes = true;
		comboFinishDelay = 0;
		currentComboHits = 0;
	}

	private bool HasMinimumCombo()
	{
		return maxComboLength >= 3;
	}

	private void FinishCombo()
	{
		if (currentComboHits > 0)
		{
			comboCount++;
			maxComboLength = Mathf.Max(maxComboLength, currentComboHits);
			currentComboHits = 0;
		}
	}
}
