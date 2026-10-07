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

	public CurrentEffect(Model GIAMLEDNFJD, ActionEffect FNNOHPEMKMB, GameObject GHDAPMGLICD, CocosAnimation EDMCLHEOJGD)
	{
		Owner = GIAMLEDNFJD;
		Effect = FNNOHPEMKMB;
		EffectObject = GHDAPMGLICD;
		Animation = EDMCLHEOJGD;
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
		ModelConditions kDOGKKGDOBK = Owner.GetConditions();
		Vector3f eMAFACPEPDK = Vector3f.op_Implicit(Effect.GetPosition().GetPosition(kDOGKKGDOBK));
		Vector3 anchor = new Vector3(eMAFACPEPDK.GetX(), eMAFACPEPDK.GetY(), eMAFACPEPDK.GetZ());
		if (Effect.GetIsOnBackground()) anchor.z += 0.1f;
		_Diagnostics.Observe(Owner, Effect, anchor, num);
		Quaternion rotation = _Interpolation.CurrentRotation;
		Vector2f hEJKLMNOLLG = Effect.GetVector().GetVector(kDOGKKGDOBK);
		if (hEJKLMNOLLG.GetX() != 0f || hEJKLMNOLLG.GetY() != 0f)
		{
			hEJKLMNOLLG.SetX(hEJKLMNOLLG.GetX() * (float)num);
			hEJKLMNOLLG.SetY(hEJKLMNOLLG.GetY() * (float)num);
			float z = Vector2f.GetAngle2DDegreeSigned(hEJKLMNOLLG, new Vector2f(1f));
			rotation = Quaternion.Euler(0f, 0f, z);
		}
		_Interpolation.Push(anchor, rotation);
	}
}
