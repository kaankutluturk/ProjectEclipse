public class TacticFactors
{
	public float FactorsCount;

	public float Damage;

	public float Health;

	public float EnemyHealth;

	public int AnimationFrames;

	public int MissileBullets;

	public int MagicBullets;

	public int ExtraCount;

	public float Hits;

	public int ChildFrames;

	public float Distance;

	public InfoAnimation CurrentAnimation;

	public InfoAnimation EnemyCurrentAnimation;

	public ModelStatistics Statistics;

	public TacticFactors(ModelStatistics statistics, int missileBullets, int magicBullets)
	{
		MissileBullets = missileBullets;
		MagicBullets = magicBullets;
		Statistics = statistics;
	}

	public TacticFactors(TacticFactors source)
	{
		CopyFrom(source);
	}

	public void CopyFrom(TacticFactors source)
	{
		FactorsCount = source.FactorsCount;
		Damage = source.Damage;
		Health = source.Health;
		EnemyHealth = source.EnemyHealth;
		AnimationFrames = source.AnimationFrames;
		MissileBullets = source.MissileBullets;
		MagicBullets = source.MagicBullets;
		ExtraCount = source.ExtraCount;
		Hits = source.Hits;
		Statistics = source.Statistics;
		ChildFrames = source.ChildFrames;
		Distance = source.Distance;
		CurrentAnimation = source.CurrentAnimation;
		EnemyCurrentAnimation = source.EnemyCurrentAnimation;
	}
}
