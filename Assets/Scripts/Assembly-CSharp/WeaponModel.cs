public class WeaponModel : Model
{
	private bool hasPlayedAnimation;

	public override bool IsWeaponModel
	{
		get
		{
			return IsWeapon();
		}
	}

	public WeaponModel(ModelParameters data)
		: base(data)
	{
	}

	public override bool IsWeapon()
	{
		return true;
	}

	public override bool PlayAnimation(InfoAnimation CMGIPKIPIPA, int AOJJBKLCHJO = 0, bool HHJGACBCGBP = false, int BADKABIKMBD = -1)
	{
		bool flag = !hasPlayedAnimation;
		if (!hasPlayedAnimation)
		{
			GetParentModel().NotifyRangedAttack(this);
			hasPlayedAnimation = true;
		}
		bool result = base.PlayAnimation(CMGIPKIPIPA, AOJJBKLCHJO, HHJGACBCGBP, BADKABIKMBD);
		if (flag)
		{
			GetAnimationModule().Render();
		}
		return result;
	}
}
