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

	public override bool PlayAnimation(InfoAnimation animation, int direction = 0, bool isRelativeStart = false, int startFrame = -1)
	{
		bool flag = !hasPlayedAnimation;
		if (!hasPlayedAnimation)
		{
			GetParentModel().NotifyRangedAttack(this);
			hasPlayedAnimation = true;
		}
		bool result = base.PlayAnimation(animation, direction, isRelativeStart, startFrame);
		if (flag)
		{
			GetAnimationModule().Render();
		}
		return result;
	}
}
