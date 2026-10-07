using Eclipse.Rendering.Diagnostics;
using Eclipse.Rendering.Interpolation;
using UnityEngine;

public class CurrentEffect
{
	public Model Owner;

	public ActionEffect Effect;

	public GameObject EffectObject;

	public CocosAnimation Animation;

	public bool stopFollowEffect;

	private FightTransformInterpolation _Interpolation;

	private readonly EffectFollowDiagnostics _Diagnostics = new EffectFollowDiagnostics();

	public CurrentEffect(Model owner, ActionEffect effect, GameObject effectObject, CocosAnimation animation)
	{
		Owner = owner;
		Effect = effect;
		EffectObject = effectObject;
		Animation = animation;
		stopFollowEffect = false;
		if (Effect.GetIsFollowObject())
		{
			_Interpolation = EffectObject.GetComponent<FightTransformInterpolation>();
			if (_Interpolation == null)
			{
				_Interpolation = EffectObject.AddComponent<FightTransformInterpolation>();
			}
			_Interpolation.Snap(EffectObject.transform.localPosition, EffectObject.transform.localRotation);
		}
	}

	public void UpdateFollow()
	{
		if (Effect.Attachment != null)
		{
			Vector3 position;
			Quaternion attachmentRotation;
			if (Effect.Attachment.TryGetTransform(Owner, out position, out attachmentRotation))
			{
				if (Effect.GetIsOnBackground()) position.z += 0.1f;
				_Interpolation.Push(position, attachmentRotation);
			}
			return;
		}
		int num = Owner.GetFacingSign();
		ModelConditions conditions = Owner.GetConditions();
		Vector3f followPosition = Vector3f.op_Implicit(Effect.GetPosition().GetPosition(conditions));
		Vector3 anchor = new Vector3(followPosition.GetX(), followPosition.GetY(), followPosition.GetZ());
		if (Effect.GetIsOnBackground()) anchor.z += 0.1f;
		_Diagnostics.Observe(Owner, Effect, anchor, num);
		Quaternion rotation = _Interpolation.CurrentRotation;
		Vector2f direction = Effect.GetVector().GetVector(conditions);
		if (direction.GetX() != 0f || direction.GetY() != 0f)
		{
			direction.SetX(direction.GetX() * (float)num);
			direction.SetY(direction.GetY() * (float)num);
			float z = Vector2f.GetAngle2DDegreeSigned(direction, new Vector2f(1f));
			rotation = Quaternion.Euler(0f, 0f, z);
		}
		_Interpolation.Push(anchor, rotation);
	}
}
