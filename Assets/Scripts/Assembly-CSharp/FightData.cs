public class FightData
{
	public InfoAnimation CurrentAnimation;

	public FightEvent FightEventType;

	public FightStatistics.FightStyle Style;

	public int currentComboLevel;

	public float DamageDealt;

	public float DamageReceived;

	public bool IsBlocked;

	public bool IsUsingItem;

	public bool IsOpponentUsingItem;

	public bool IsShocked;

	public bool IsCritical;

	public bool IsOpponentShocked;

	public bool IsHeadHit;

	public bool IsAttacker;

	public FightData()
	{
		Reset();
	}

	public void Reset()
	{
		CurrentAnimation = null;
		FightEventType = FightEvent.NoneEvent;
		Style = FightStatistics.FightStyle.STYLE_TURTLE;
		currentComboLevel = 0;
		DamageDealt = 0f;
		DamageReceived = 0f;
		IsBlocked = false;
		IsUsingItem = false;
		IsOpponentUsingItem = false;
		IsShocked = false;
		IsCritical = false;
		IsOpponentShocked = false;
		IsHeadHit = false;
		IsAttacker = false;
	}
}
