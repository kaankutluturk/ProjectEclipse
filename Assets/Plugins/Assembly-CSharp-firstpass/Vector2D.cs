using UnityEngine;

public class Vector2D
{
	public static void BuildLineEquation(Vector2 pointA, Vector2 pointB, EquationLine equation)
	{
		float num = Vector2.Distance(pointA, pointB);
		equation.A = (pointA.y - pointB.y) / num;
		equation.CoefficientB = (pointB.x - pointA.x) / num;
		equation.ConstantC = 0f - (equation.A * pointA.x + equation.CoefficientB * pointA.y);
	}

	public static EquationLine BuildLineEquation(Vector2 pointA, Vector2 pointB)
	{
		EquationLine equation = null;
		BuildLineEquation(pointA, pointB, equation);
		return equation;
	}

	private static float Abs(float value)
	{
		return (!(value < 0f)) ? value : (0f - value);
	}

	public static bool IntersectThickSegments(Vector2 segmentAStart, Vector2 segmentAEnd, float thicknessA, Vector2 segmentBStart, Vector2 segmentBEnd, float thicknessB, ref Vector2 intersectionStart, ref Vector2 intersectionEnd, EquationLine segmentALine, EquationLine segmentBLine)
	{
		Vector2 vector = segmentAStart;
		Vector2 vector2 = segmentAEnd;
		Vector2 vector3 = segmentBStart;
		Vector2 vector4 = segmentBEnd;
		float num = thicknessA + thicknessB;
		if (num == 0f)
		{
			if (SegmentIntersection(vector3, vector4, vector, vector2, intersectionStart))
			{
				intersectionEnd = intersectionStart;
				return true;
			}
			return false;
		}
		EquationLine segmentBEquation = ((segmentBLine == null) ? BuildLineEquation(segmentBStart, segmentBEnd) : segmentBLine);
		float num2 = segmentBEquation.A * vector.x + segmentBEquation.CoefficientB * vector.y + segmentBEquation.ConstantC;
		float num3 = segmentBEquation.A * vector2.x + segmentBEquation.CoefficientB * vector2.y + segmentBEquation.ConstantC;
		if (0f <= num2 * num3 && num < Abs(num2) && num < Abs(num3))
		{
			return false;
		}
		EquationLine kEDCEHBPOIM2 = ((segmentALine == null) ? BuildLineEquation(segmentAStart, segmentAEnd) : segmentALine);
		float num4 = kEDCEHBPOIM2.A * vector3.x + kEDCEHBPOIM2.CoefficientB * vector3.y + kEDCEHBPOIM2.ConstantC;
		float num5 = kEDCEHBPOIM2.A * vector4.x + kEDCEHBPOIM2.CoefficientB * vector4.y + kEDCEHBPOIM2.ConstantC;
		if (0f <= num4 * num5 && num < Abs(num4) && num < Abs(num5))
		{
			return false;
		}
		if (num4 * num5 < 0f && num2 * num3 < 0f)
		{
			float num6 = num4 / (num4 - num5);
			intersectionStart = vector4 - vector3;
			intersectionStart *= num6;
			intersectionStart += vector3;
			intersectionEnd = intersectionStart;
			return true;
		}
		if (IsPointNearSegment(num2, num, segmentBEquation, vector, ref intersectionEnd, vector3, vector4))
		{
			intersectionStart = vector;
			return true;
		}
		if (IsPointNearSegment(num3, num, segmentBEquation, vector2, ref intersectionEnd, vector3, vector4))
		{
			intersectionStart = vector2;
			return true;
		}
		if (IsPointNearSegment(num4, num, kEDCEHBPOIM2, vector3, ref intersectionEnd, vector, vector2))
		{
			intersectionStart = vector3;
			intersectionEnd = vector3;
			return true;
		}
		if (IsPointNearSegment(num5, num, kEDCEHBPOIM2, vector4, ref intersectionEnd, vector, vector2))
		{
			intersectionStart = vector4;
			intersectionEnd = vector4;
			return true;
		}
		return false;
	}

	public static bool IsPointNearSegment(float signedDistance, float maxDistance, EquationLine line, Vector2 point, ref Vector2 projectedPoint, Vector2 segmentStart, Vector2 segmentEnd)
	{
		if (Abs(signedDistance) <= maxDistance)
		{
			projectedPoint.x = point.x - signedDistance * line.A;
			projectedPoint.y = point.y - signedDistance * line.CoefficientB;
			return (((segmentEnd.x <= projectedPoint.x && projectedPoint.x <= segmentStart.x) || (segmentStart.x <= projectedPoint.x && projectedPoint.x <= segmentEnd.x)) && ((segmentEnd.y <= projectedPoint.y && projectedPoint.y <= segmentStart.y) || (segmentStart.y <= projectedPoint.y && projectedPoint.y <= segmentEnd.y))) || (point.x - segmentStart.x) * (point.x - segmentStart.x) + (point.y - segmentStart.y) * (point.y - segmentStart.y) <= maxDistance * maxDistance || (point.x - segmentEnd.x) * (point.x - segmentEnd.x) + (point.y - segmentEnd.y) * (point.y - segmentEnd.y) <= maxDistance * maxDistance;
		}
		return false;
	}

	public static bool SegmentIntersection(Vector2 segmentAStart, Vector2 segmentAEnd, Vector2 segmentBStart, Vector2 segmentBEnd, Vector2 intersection)
	{
		if ((segmentAStart.x == segmentAEnd.x && segmentAStart.y == segmentAEnd.y) || (segmentBStart.x == segmentBEnd.x && segmentBStart.y == segmentBEnd.y))
		{
			return false;
		}
		float num = segmentAEnd.x - segmentAStart.x;
		float num2 = segmentAEnd.y - segmentAStart.y;
		float num3 = segmentBEnd.x - segmentBStart.x;
		float num4 = segmentBEnd.y - segmentBStart.y;
		float num5 = segmentAStart.x - segmentBStart.x;
		float num6 = segmentAStart.y - segmentBStart.y;
		float num7 = num4 * num - num3 * num2;
		float num8 = num3 * num6 - num4 * num5;
		float num9 = num * num6 - num2 * num5;
		if (num7 == 0f)
		{
			if (num8 != 0f && num9 != 0f)
			{
				return false;
			}
			float x;
			float x2;
			if (segmentAStart.x < segmentAEnd.x)
			{
				x = segmentAStart.x;
				x2 = segmentAEnd.x;
			}
			else
			{
				x = segmentAEnd.x;
				x2 = segmentAStart.x;
			}
			float x3;
			float x4;
			if (segmentBStart.x < segmentBEnd.x)
			{
				x3 = segmentBStart.x;
				x4 = segmentBEnd.x;
			}
			else
			{
				x3 = segmentBEnd.x;
				x4 = segmentBStart.x;
			}
			if (x > x4 || x3 > x2)
			{
				return false;
			}
			if (segmentAStart.y < segmentAEnd.y)
			{
				x = segmentAStart.y;
				x2 = segmentAEnd.y;
			}
			else
			{
				x = segmentAEnd.y;
				x2 = segmentAStart.y;
			}
			if (segmentBStart.y < segmentBEnd.y)
			{
				x3 = segmentBStart.y;
				x4 = segmentBEnd.y;
			}
			else
			{
				x3 = segmentBEnd.y;
				x4 = segmentBStart.y;
			}
			if (x > x4 || x3 > x2)
			{
				return false;
			}
			num7 = 1f;
		}
		num8 /= num7;
		num9 /= num7;
		if (num8 >= 0f && num8 <= 1f && num9 >= 0f && num9 <= 1f)
		{
			intersection.x = segmentAStart.x + num8 * (segmentAEnd.x - segmentAStart.x);
			intersection.y = segmentAStart.y + num8 * (segmentAEnd.y - segmentAStart.y);
			return true;
		}
		return false;
	}

	public static float GetAngle2DDegreeSigned(Vector2 from, Vector3 to)
	{
		return GetAngle2DRadianSigned(from, to) * 57.29578f;
	}

	public static float GetAngle2DRadianSigned(Vector2 from, Vector2 to)
	{
		float num = from.x * to.y - from.y * to.x;
		float num2 = from.x * to.x + from.y * to.y;
		float num3 = 1f / Mathf.Sqrt(num * num + num2 * num2);
		return Mathf.Atan2(num * num3, num2 * num3);
	}
}
