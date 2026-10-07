using System.Collections.Generic;

public class RatingEvaluation
{
	public string nodeName;

	public string evaluationName;

	public string cancellingItem;

	public float averageQuantity;

	public float averageBaseDamage;

	public float rechargeRate;

	public float magicRechargeRate;

	public List<Evaluation> evaluations = new List<Evaluation>();

	public List<Defense> defenses = new List<Defense>();
}
