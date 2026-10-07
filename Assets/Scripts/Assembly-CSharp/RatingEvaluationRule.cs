using System.Xml;

public class RatingEvaluationRule : Rule
{
	protected float playerRating;

	protected float enemyRating;

	protected float playerRatingMagic;

	protected float enemyRatingMagic;

	protected float playerRatingRanged;

	protected float enemyRatingRanged;

	protected float ratingCorrection;

	public RatingEvaluationRule(XmlNode node)
		: base(RuleType.RuleRatingEvaluation, node)
	{
		playerRating = node.Attributes["PlayerRating"].ParseFloat();
		enemyRating = node.Attributes["EnemyRating"].ParseFloat();
		playerRatingMagic = node.Attributes["PlayerRatingMagic"].ParseFloat();
		enemyRatingMagic = node.Attributes["EnemyRatingMagic"].ParseFloat();
		playerRatingRanged = node.Attributes["PlayerRatingRanged"].ParseFloat();
		enemyRatingRanged = node.Attributes["EnemyRatingRanged"].ParseFloat();
		ratingCorrection = node.Attributes["RatingCorrection"].ParseFloat();
	}

	public float GetPlayerRating()
	{
		return playerRating;
	}

	public float GetEnemyRating()
	{
		return enemyRating;
	}

	public float GetPlayerRatingMagic()
	{
		return playerRatingMagic;
	}

	public float GetEnemyRatingMagic()
	{
		return enemyRatingMagic;
	}

	public float GetPlayerRatingRanged()
	{
		return playerRatingRanged;
	}

	public float GetEnemyRatingRanged()
	{
		return enemyRatingRanged;
	}

	public float GetRatingCorrection()
	{
		return ratingCorrection;
	}
}
