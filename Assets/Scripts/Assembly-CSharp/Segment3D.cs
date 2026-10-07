public class Segment3D
{
	private Vector3f start;

	private Vector3f end;

	public Vector3f Start
	{
		get
		{
			return GetStart();
		}
	}

	public Vector3f End
	{
		get
		{
			return GetEnd();
		}
	}

	public float Length
	{
		get
		{
			return GetLength();
		}
	}

	public float Length2D
	{
		get
		{
			return GetLength2D();
		}
	}

	public Vector3f Normal
	{
		get
		{
			return GetNormal();
		}
	}

	public Vector2f Direction2D
	{
		get
		{
			return GetDirection2D();
		}
	}

	public Vector3f Direction3D
	{
		get
		{
			return GetDirection3D();
		}
	}

	public Vector2f Midpoint2D
	{
		get
		{
			return GetMidpoint2D();
		}
	}

	public Vector3f Midpoint3D
	{
		get
		{
			return GetMidpoint3D();
		}
	}

	public Segment3D()
	{
	}

	public Segment3D(Vector3f startPoint, Vector3f endPoint)
	{
		start.Set(startPoint);
		end.Set(endPoint);
	}

	public Vector3f GetStart()
	{
		return start;
	}

	public Vector3f GetEnd()
	{
		return end;
	}

	public void SetSegment3D(Segment3D source)
	{
		SetStart(source.start);
		SetEnd(source.end);
	}

	public float GetLength()
	{
		return Vector3f.Distance(start, end);
	}

	public float GetLength2D()
	{
		return Vector2f.Distance2D(start, end);
	}

	public Vector3f GetNormal()
	{
		return Vector3f.GetPerpendicularDirection(start, end);
	}

	public Vector2f GetDirection2D()
	{
		return new Vector2f(end.GetX() - start.GetX(), end.GetY() - start.GetY());
	}

	public Vector3f GetDirection3D()
	{
		return Vector3f.op_Subtraction(end, start);
	}

	public Vector2f GetMidpoint2D()
	{
		return GetDivisionPoint2D(0.5f);
	}

	public Vector3f GetMidpoint3D()
	{
		return GetDivisionPoint3D(0.5f);
	}

	public Vector2f GetDivisionPoint2D(float ratio)
	{
		return new Vector2f(start.GetX() + (end.GetX() - start.GetX()) * ratio, start.GetY() + (end.GetY() - start.GetY()) * ratio);
	}

	public Vector3f GetDivisionPoint3D(float ratio)
	{
		return Vector3f.GetDivisionPoint3D(start, end, ratio);
	}

	public void GetDivisionPoint3D(Vector3f resultPoint, float ratio)
	{
		resultPoint.Set(start.GetX() + (end.GetX() - start.GetX()) * ratio, start.GetY() + (end.GetY() - start.GetY()) * ratio, start.GetZ() + (end.GetZ() - start.GetZ()) * ratio);
	}

	public Vector2f GetClosestPointOnLine2D(Vector2f point)
	{
		Vector2f offset = new Vector2f(point);
		Vector2f closestPoint = new Vector2f(start);
		Vector2f lineDirection = GetDirection2D();
		offset.SubtractXY(closestPoint);
		float num = offset.DotProduct(lineDirection);
		float num2 = lineDirection.DotProduct(lineDirection);
		float projectionRatio = ((num2 == 0f) ? 0f : (num / num2));
		lineDirection.Multiply(projectionRatio);
		closestPoint.Add(lineDirection);
		return closestPoint;
	}

	public float GetRatioFromEnd(Vector2f point)
	{
		float num = Vector2f.Distance2D(start, end);
		if (num != 0f)
		{
			return Vector2f.Distance2D(end, point) / num;
		}
		return 0f;
	}

	public static bool TryGetIntersection2D(Segment3D segmentA, Segment3D segmentB, Vector3f intersection)
	{
		return Vector2f.TryGetSegmentIntersection(segmentA.start, segmentA.end, segmentB.start, segmentB.end, intersection);
	}

	public void SetStart(Vector3f value)
	{
		start.Set(value);
	}

	public void SetStartReference(Vector3f value)
	{
		start = value;
	}

	public void SetEnd(Vector3f value)
	{
		end.Set(value);
	}

	public void SetEndReference(Vector3f value)
	{
		end = value;
	}
}
