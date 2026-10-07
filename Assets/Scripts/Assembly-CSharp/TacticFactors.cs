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

	public TacticFactors(ModelStatistics PNEPMGPIEIH, int IIAALGOFJJJ, int CPOOPPKHFHB)
	{
		MissileBullets = IIAALGOFJJJ;
		MagicBullets = CPOOPPKHFHB;
		Statistics = PNEPMGPIEIH;
	}

	public TacticFactors(TacticFactors FJCBLOKOBBD)
	{
		CopyFrom(FJCBLOKOBBD);
	}

	public void CopyFrom(TacticFactors JFMALLHPPMH)
	{
		FactorsCount = JFMALLHPPMH.FactorsCount;
		Damage = JFMALLHPPMH.Damage;
		Health = JFMALLHPPMH.Health;
		EnemyHealth = JFMALLHPPMH.EnemyHealth;
		AnimationFrames = JFMALLHPPMH.AnimationFrames;
		MissileBullets = JFMALLHPPMH.MissileBullets;
		MagicBullets = JFMALLHPPMH.MagicBullets;
		ExtraCount = JFMALLHPPMH.ExtraCount;
		Hits = JFMALLHPPMH.Hits;
		Statistics = JFMALLHPPMH.Statistics;
		ChildFrames = JFMALLHPPMH.ChildFrames;
		Distance = JFMALLHPPMH.Distance;
		CurrentAnimation = JFMALLHPPMH.CurrentAnimation;
		EnemyCurrentAnimation = JFMALLHPPMH.EnemyCurrentAnimation;
	}
}
