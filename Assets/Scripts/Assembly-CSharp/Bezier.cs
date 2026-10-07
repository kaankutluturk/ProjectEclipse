using System.Collections.Generic;

public class Bezier
{
	private Vector3f _firstMidPoint = new Vector3f();

	private Vector3f _secondMidPoint = new Vector3f();

	private float _stepFactor;

	private float _secondDifference;

	private float _differenceStep;

	private int _count;

	public Bezier(int count)
	{
		_stepFactor = 1f / (float)count;
		_secondDifference = (0f - _stepFactor) / (float)count;
		_differenceStep = 0f - _secondDifference - _secondDifference;
		_count = count;
		_stepFactor += _stepFactor;
	}

	private void ComputeCurvePoints(Vector3f HAEJICBDOKC, Vector3f MILMANCOCLK, Vector3f DMECFLFKOPA, int count, List<Vector3f> OEMALIFPGPO)
	{
		float num = _secondDifference;
		if (OEMALIFPGPO.Count != count)
		{
			OEMALIFPGPO.Resize(count);
			for (int i = 0; i < count; i++)
			{
				if (Vector2f.op_Equality(OEMALIFPGPO[i], null))
				{
					OEMALIFPGPO[i] = new Vector3f();
				}
			}
		}
		float num2 = 1f;
		float num3 = 0f;
		float num4 = 0f;
		foreach (Vector3f item in OEMALIFPGPO)
		{
			num += _differenceStep;
			num2 -= _stepFactor - num;
			num3 += _stepFactor - num - num;
			num4 += num;
			item.SetX(num2 * HAEJICBDOKC.GetX() + num3 * MILMANCOCLK.GetX() + num4 * DMECFLFKOPA.GetX());
			item.SetY(num2 * HAEJICBDOKC.GetY() + num3 * MILMANCOCLK.GetY() + num4 * DMECFLFKOPA.GetY());
			item.SetZ(num2 * HAEJICBDOKC.GetZ() + num3 * MILMANCOCLK.GetZ() + num4 * DMECFLFKOPA.GetZ());
		}
	}

	public void BuildCurve(Vector3f MLGFPMDKOHD, Vector3f DMMNCDKPCCI, Vector3f PIBOFKAMIDL, List<Vector3f> OEMALIFPGPO)
	{
		_firstMidPoint.SetMiddlePoint3D(MLGFPMDKOHD, DMMNCDKPCCI);
		_secondMidPoint.SetMiddlePoint3D(DMMNCDKPCCI, PIBOFKAMIDL);
		ComputeCurvePoints(_firstMidPoint, DMMNCDKPCCI, _secondMidPoint, _count, OEMALIFPGPO);
	}
}
