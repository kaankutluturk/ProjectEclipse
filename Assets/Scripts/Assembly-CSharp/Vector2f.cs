using System.Runtime.CompilerServices;
using UnityEngine;

public class Vector2f
{
	public static readonly Vector2f ZeroVector = new Vector2f();

	protected float X;

	protected float Y;

	public float XValue
	{
		get
		{
			return GetX();
		}
		set
		{
			SetX(value);
		}
	}

	public float YValue
	{
		get
		{
			return GetY();
		}
		set
		{
			SetY(value);
		}
	}

	public string FormattedValue
	{
		get
		{
			return GetFormattedValue();
		}
	}

	public Vector2f(Vector2f source)
	{
		X = source.GetX();
		Y = source.GetY();
	}

	public Vector2f(float x = 0f, float y = 0f)
	{
		X = x;
		Y = y;
	}

	public Vector2f(Vector3f source)
	{
		X = source.GetX();
		Y = source.GetY();
	}

	public float GetX()
	{
		return X;
	}

	public void SetX(float value)
	{
		X = value;
	}

	public float GetY()
	{
		return Y;
	}

	public void SetY(float value)
	{
		Y = value;
	}

	[SpecialName]
	public static Vector2f op_Subtraction(Vector2f left, Vector2f right)
	{
		return new Vector2f(left.GetX() - right.GetX(), left.GetY() - right.GetY());
	}

	[SpecialName]
	public static Vector2f op_Addition(Vector2f left, Vector2f right)
	{
		return new Vector2f(left.GetX() + right.GetX(), left.GetY() + right.GetY());
	}

	[SpecialName]
	public static bool op_Equality(Vector2f left, Vector2f right)
	{
		if (object.ReferenceEquals(left, right))
		{
			return true;
		}
		if (object.ReferenceEquals(left, null) || object.ReferenceEquals(right, null))
		{
			return false;
		}
		return left.GetX() == right.GetX() && left.GetY() == right.GetY();
	}

	[SpecialName]
	public static bool op_Inequality(Vector2f left, Vector2f right)
	{
		return !op_Equality(left, right);
	}

	[SpecialName]
	public static Vector3 op_Implicit(Vector2f vector)
	{
		return new Vector3(vector.GetX(), vector.GetY(), 0f);
	}

	public Vector2f Add(Vector2f other)
	{
		X += other.GetX();
		Y += other.GetY();
		return this;
	}

	public Vector2f Add(float x, float y)
	{
		X += x;
		Y += y;
		return this;
	}

	public Vector2f SubtractXY(Vector2f other)
	{
		X -= other.GetX();
		Y -= other.GetY();
		return this;
	}

	public Vector2f Multiply(float scalar)
	{
		X *= scalar;
		Y *= scalar;
		return this;
	}

	public void Set(Vector2f source)
	{
		X = source.GetX();
		Y = source.GetY();
	}

	public void Round(int multiplier)
	{
		X = Round(X, multiplier);
		Y = Round(Y, multiplier);
	}

	public void RoundToInteger()
	{
		X = Round(X, 1f);
		Y = Round(Y, 1f);
	}

	public float DotProduct(Vector2f other)
	{
		return X * other.GetX() + Y * other.GetY();
	}

	public string GetFormattedValue()
	{
		return string.Format("[{0} {1}]", X, Y);
	}

	public override string ToString()
	{
		return string.Format("[Point: X={0}, Y={1}]", GetX(), GetY());
	}

	public static float Round(float Value, float multiplier)
	{
		return Mathf.Floor(Value * multiplier + 0.5f) / multiplier;
	}

	public static float Distance2D(Vector3f pointA, Vector3f pointB)
	{
		return Mathf.Sqrt((pointA.GetX() - pointB.GetX()) * (pointA.GetX() - pointB.GetX()) + (pointA.GetY() - pointB.GetY()) * (pointA.GetY() - pointB.GetY()));
	}

	public static float Distance2D(Vector3f pointA, Vector2f pointB)
	{
		return Mathf.Sqrt((pointA.GetX() - pointB.GetX()) * (pointA.GetX() - pointB.GetX()) + (pointA.GetY() - pointB.GetY()) * (pointA.GetY() - pointB.GetY()));
	}

	public static bool TryGetSegmentIntersection(Vector3f segmentAStart, Vector3f segmentAEnd, Vector3f segmentBStart, Vector3f segmentBEnd, Vector3f intersection)
	{
		if ((segmentAStart.GetX() == segmentAEnd.GetX() && segmentAStart.GetY() == segmentAEnd.GetY()) || (segmentBStart.GetX() == segmentBEnd.GetX() && segmentBStart.GetY() == segmentBEnd.GetY()))
		{
			return false;
		}
		float num = segmentAEnd.GetX() - segmentAStart.GetX();
		float num2 = segmentAEnd.GetY() - segmentAStart.GetY();
		float num3 = segmentBEnd.GetX() - segmentBStart.GetX();
		float num4 = segmentBEnd.GetY() - segmentBStart.GetY();
		float num5 = segmentAStart.GetX() - segmentBStart.GetX();
		float num6 = segmentAStart.GetY() - segmentBStart.GetY();
		float num7 = num4 * num - num3 * num2;
		float num8 = num3 * num6 - num4 * num5;
		float num9 = num * num6 - num2 * num5;
		if (num7 == 0f)
		{
			if (num8 != 0f && num9 != 0f)
			{
				return false;
			}
			float num10;
			float num11;
			if (segmentAStart.GetX() < segmentAEnd.GetX())
			{
				num10 = segmentAStart.GetX();
				num11 = segmentAEnd.GetX();
			}
			else
			{
				num10 = segmentAEnd.GetX();
				num11 = segmentAStart.GetX();
			}
			float num12;
			float num13;
			if (segmentBStart.GetX() < segmentBEnd.GetX())
			{
				num12 = segmentBStart.GetX();
				num13 = segmentBEnd.GetX();
			}
			else
			{
				num12 = segmentBEnd.GetX();
				num13 = segmentBStart.GetX();
			}
			if (num10 > num13 || num12 > num11)
			{
				return false;
			}
			if (segmentAStart.GetY() < segmentAEnd.GetY())
			{
				num10 = segmentAStart.GetY();
				num11 = segmentAEnd.GetY();
			}
			else
			{
				num10 = segmentAEnd.GetY();
				num11 = segmentAStart.GetY();
			}
			if (segmentBStart.GetY() < segmentBEnd.GetY())
			{
				num12 = segmentBStart.GetY();
				num13 = segmentBEnd.GetY();
			}
			else
			{
				num12 = segmentBEnd.GetY();
				num13 = segmentBStart.GetY();
			}
			if (num10 > num13 || num12 > num11)
			{
				return false;
			}
			num7 = 1f;
		}
		num8 /= num7;
		num9 /= num7;
		if (num8 >= 0f && num8 <= 1f && num9 >= 0f && num9 <= 1f)
		{
			intersection.SetX(segmentAStart.GetX() + num8 * (segmentAEnd.GetX() - segmentAStart.GetX()));
			intersection.SetY(segmentAStart.GetY() + num8 * (segmentAEnd.GetY() - segmentAStart.GetY()));
			return true;
		}
		return false;
	}

	public static void BuildEquationLine(Vector3f start, Vector3f end, EquationLine equationLine)
	{
		float num = Distance2D(start, end);
		equationLine.A = (start.GetY() - end.GetY()) / num;
		equationLine.CoefficientB = (end.GetX() - start.GetX()) / num;
		equationLine.ConstantC = 0f - (equationLine.A * start.GetX() + equationLine.CoefficientB * start.GetY());
	}

	public static EquationLine BuildEquationLine(Vector3f start, Vector3f end)
	{
		EquationLine equationLine = new EquationLine();
		BuildEquationLine(start, end, equationLine);
		return equationLine;
	}

	public static bool TryIntersectSegments(Vector3f segmentAStart, Vector3f segmentAEnd, Vector3f segmentBStart, Vector3f segmentBEnd, Vector3f intersection)
	{
		if ((segmentAStart.GetX() == segmentAEnd.GetX() && segmentAStart.GetY() == segmentAEnd.GetY()) || (segmentBStart.GetX() == segmentBEnd.GetX() && segmentBStart.GetY() == segmentBEnd.GetY()))
		{
			return false;
		}
		float num = segmentAEnd.GetX() - segmentAStart.GetX();
		float num2 = segmentAEnd.GetY() - segmentAStart.GetY();
		float num3 = segmentBEnd.GetX() - segmentBStart.GetX();
		float num4 = segmentBEnd.GetY() - segmentBStart.GetY();
		float num5 = segmentAStart.GetX() - segmentBStart.GetX();
		float num6 = segmentAStart.GetY() - segmentBStart.GetY();
		float num7 = num4 * num - num3 * num2;
		float num8 = num3 * num6 - num4 * num5;
		float num9 = num * num6 - num2 * num5;
		if (num7 == 0f)
		{
			if (num8 != 0f && num9 != 0f)
			{
				return false;
			}
			float num10;
			float num11;
			if (segmentAStart.GetX() < segmentAEnd.GetX())
			{
				num10 = segmentAStart.GetX();
				num11 = segmentAEnd.GetX();
			}
			else
			{
				num10 = segmentAEnd.GetX();
				num11 = segmentAStart.GetX();
			}
			float num12;
			float num13;
			if (segmentBStart.GetX() < segmentBEnd.GetX())
			{
				num12 = segmentBStart.GetX();
				num13 = segmentBEnd.GetX();
			}
			else
			{
				num12 = segmentBEnd.GetX();
				num13 = segmentBStart.GetX();
			}
			if (num10 > num13 || num12 > num11)
			{
				return false;
			}
			if (segmentAStart.GetY() < segmentAEnd.GetY())
			{
				num10 = segmentAStart.GetY();
				num11 = segmentAEnd.GetY();
			}
			else
			{
				num10 = segmentAEnd.GetY();
				num11 = segmentAStart.GetY();
			}
			if (segmentBStart.GetY() < segmentBEnd.GetY())
			{
				num12 = segmentBStart.GetY();
				num13 = segmentBEnd.GetY();
			}
			else
			{
				num12 = segmentBEnd.GetY();
				num13 = segmentBStart.GetY();
			}
			if (num10 > num13 || num12 > num11)
			{
				return false;
			}
			num7 = 1f;
		}
		num8 /= num7;
		num9 /= num7;
		if (num8 >= 0f && num8 <= 1f && num9 >= 0f && num9 <= 1f)
		{
			intersection.SetX(segmentAStart.GetX() + num8 * (segmentAEnd.GetX() - segmentAStart.GetX()));
			intersection.SetY(segmentAStart.GetY() + num8 * (segmentAEnd.GetY() - segmentAStart.GetY()));
			return true;
		}
		return false;
	}

	public static bool IsDistanceStrike(float signedDistance, float radius, EquationLine equationLine, Vector3f point, Vector3f _base, Vector3f segmentStart, Vector3f segmentEnd)
	{
		if (Mathf.Abs(signedDistance) <= radius)
		{
			_base.SetX(point.GetX() - signedDistance * equationLine.A);
			_base.SetY(point.GetY() - signedDistance * equationLine.CoefficientB);
			return (((segmentEnd.GetX() <= _base.GetX() && _base.GetX() <= segmentStart.GetX()) || (segmentStart.GetX() <= _base.GetX() && _base.GetX() <= segmentEnd.GetX())) && ((segmentEnd.GetY() <= _base.GetY() && _base.GetY() <= segmentStart.GetY()) || (segmentStart.GetY() <= _base.GetY() && _base.GetY() <= segmentEnd.GetY()))) || (point.GetX() - segmentStart.GetX()) * (point.GetX() - segmentStart.GetX()) + (point.GetY() - segmentStart.GetY()) * (point.GetY() - segmentStart.GetY()) <= radius * radius || (point.GetX() - segmentEnd.GetX()) * (point.GetX() - segmentEnd.GetX()) + (point.GetY() - segmentEnd.GetY()) * (point.GetY() - segmentEnd.GetY()) <= radius * radius;
		}
		return false;
	}

	public static bool TryIntersectThickSegments(Vector3f segmentAStart, Vector3f segmentAEnd, float radiusA, Vector3f segmentBStart, Vector3f segmentBEnd, float radiusB, Vector3f intersection, Vector3f basePoint, EquationLine cachedLineA, EquationLine cachedLineB)
	{
		float num = radiusA + radiusB;
		if (num == 0f)
		{
			if (TryIntersectSegments(segmentBStart, segmentBEnd, segmentAStart, segmentAEnd, intersection))
			{
				basePoint.Set(intersection);
				return true;
			}
			return false;
		}
		EquationLine lineB = ((cachedLineB == null) ? BuildEquationLine(segmentBStart, segmentBEnd) : cachedLineB);
		float num2 = lineB.A * segmentAStart.GetX() + lineB.CoefficientB * segmentAStart.GetY() + lineB.ConstantC;
		float num3 = lineB.A * segmentAEnd.GetX() + lineB.CoefficientB * segmentAEnd.GetY() + lineB.ConstantC;
		if (0f <= num2 * num3 && num < Mathf.Abs(num2) && num < Mathf.Abs(num3))
		{
			return false;
		}
		EquationLine kEDCEHBPOIM2 = ((cachedLineA == null) ? BuildEquationLine(segmentAStart, segmentAEnd) : cachedLineA);
		float num4 = kEDCEHBPOIM2.A * segmentBStart.GetX() + kEDCEHBPOIM2.CoefficientB * segmentBStart.GetY() + kEDCEHBPOIM2.ConstantC;
		float num5 = kEDCEHBPOIM2.A * segmentBEnd.GetX() + kEDCEHBPOIM2.CoefficientB * segmentBEnd.GetY() + kEDCEHBPOIM2.ConstantC;
		if (0f <= num4 * num5 && num < Mathf.Abs(num4) && num < Mathf.Abs(num5))
		{
			return false;
		}
		if (num4 * num5 < 0f && num2 * num3 < 0f)
		{
			float interpolation = num4 / (num4 - num5);
			intersection.Set(Vector3f.op_Subtraction(segmentBEnd, segmentBStart));
			intersection.Multiply(interpolation);
			intersection.Add(segmentBStart);
			basePoint.Set(intersection);
			return true;
		}
		if (IsDistanceStrike(num2, num, lineB, segmentAStart, basePoint, segmentBStart, segmentBEnd))
		{
			intersection.Set(segmentAStart);
			return true;
		}
		if (IsDistanceStrike(num3, num, lineB, segmentAEnd, basePoint, segmentBStart, segmentBEnd))
		{
			intersection.Set(segmentAEnd);
			return true;
		}
		if (IsDistanceStrike(num4, num, kEDCEHBPOIM2, segmentBStart, basePoint, segmentAStart, segmentAEnd))
		{
			intersection.Set(segmentBStart);
			basePoint.Set(segmentBStart);
			return true;
		}
		if (IsDistanceStrike(num5, num, kEDCEHBPOIM2, segmentBEnd, basePoint, segmentAStart, segmentAEnd))
		{
			intersection.Set(segmentBEnd);
			basePoint.Set(segmentBEnd);
			return true;
		}
		return false;
	}

	public static float DistanceSquared2D(Vector2f pointA, Vector2f pointB)
	{
		return (pointB.GetX() - pointA.GetX()) * (pointB.GetX() - pointA.GetX()) + (pointB.GetY() - pointA.GetY()) * (pointB.GetY() - pointA.GetY());
	}

	public static float GetAngle2DDegreeSigned(Vector2f first, Vector2f second)
	{
		return GetAngle2DRadianSigned(first, second) * 57.29578f;
	}

	public static float GetAngle2DRadianSigned(Vector2f first, Vector2f second)
	{
		float num = first.X * second.Y - first.Y * second.X;
		float num2 = first.X * second.X + first.Y * second.Y;
		float num3 = 1f / Mathf.Sqrt(num * num + num2 * num2);
		return Mathf.Atan2(num * num3, num2 * num3);
	}
}
