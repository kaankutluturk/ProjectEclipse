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

	private void ComputeCurvePoints(Vector3f startPoint, Vector3f controlPoint, Vector3f endPoint, int count, List<Vector3f> points)
	{
		float num = _secondDifference;
		if (points.Count != count)
		{
			points.Resize(count);
			for (int i = 0; i < count; i++)
			{
				if (Vector2f.op_Equality(points[i], null))
				{
					points[i] = new Vector3f();
				}
			}
		}
		float num2 = 1f;
		float num3 = 0f;
		float num4 = 0f;
		foreach (Vector3f item in points)
		{
			num += _differenceStep;
			num2 -= _stepFactor - num;
			num3 += _stepFactor - num - num;
			num4 += num;
			item.SetX(num2 * startPoint.GetX() + num3 * controlPoint.GetX() + num4 * endPoint.GetX());
			item.SetY(num2 * startPoint.GetY() + num3 * controlPoint.GetY() + num4 * endPoint.GetY());
			item.SetZ(num2 * startPoint.GetZ() + num3 * controlPoint.GetZ() + num4 * endPoint.GetZ());
		}
	}

	public void BuildCurve(Vector3f startPoint, Vector3f controlPoint, Vector3f endPoint, List<Vector3f> points)
	{
		_firstMidPoint.SetMiddlePoint3D(startPoint, controlPoint);
		_secondMidPoint.SetMiddlePoint3D(controlPoint, endPoint);
		ComputeCurvePoints(_firstMidPoint, controlPoint, _secondMidPoint, _count, points);
	}
}
