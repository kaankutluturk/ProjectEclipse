using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Xml;
using UnityEngine;

public class Vector3f : Vector2f
{
	private float Z;

	public float ZValue
	{
		get
		{
			return GetZ();
		}
		set
		{
			SetZ(value);
		}
	}

	public float Magnitude
	{
		get
		{
			return GetMagnitude();
		}
	}

	public float Length2D
	{
		get
		{
			return GetLength2D();
		}
	}

	public static Vector3f UnitX
	{
		get
		{
			return GetUnitX();
		}
	}

	public static Vector3f UnitY
	{
		get
		{
			return GetUnitY();
		}
	}

	public static Vector3f Up
	{
		get
		{
			return GetUp();
		}
	}

	public static Vector3f Zero
	{
		get
		{
			return GetZero();
		}
	}

	public static Vector3f One
	{
		get
		{
			return GetOne();
		}
	}

	public Vector3f()
	{
		X = (Y = (Z = 0f));
	}

	public Vector3f(float x, float y = 0f, float z = 0f)
	{
		X = x;
		Y = y;
		Z = z;
	}

	public Vector3f(Vector3f source)
	{
		X = source.GetX();
		Y = source.GetY();
		Z = source.GetZ();
	}

	public Vector3f(Vector3 source)
	{
		X = source.x;
		Y = source.y;
		Z = source.z;
	}

	public float GetZ()
	{
		return Z;
	}

	public void SetZ(float value)
	{
		Z = value;
	}

	public float GetMagnitude()
	{
		return Mathf.Sqrt(X * X + Y * Y + Z * Z);
	}

	public float GetLength2D()
	{
		return Mathf.Sqrt(X * X + Y * Y);
	}

	public static Vector3f GetUnitX()
	{
		return new Vector3f(1f);
	}

	public static Vector3f GetUnitY()
	{
		return new Vector3f(0f, 1f);
	}

	public static Vector3f GetUp()
	{
		return new Vector3f(0f, 1f);
	}

	public static Vector3f GetZero()
	{
		return new Vector3f(0f);
	}

	public static Vector3f GetOne()
	{
		return new Vector3f(1f, 1f, 1f);
	}

	[SpecialName]
	public static Vector3f op_Addition(Vector3f vector, float amount)
	{
		return new Vector3f(vector.GetX() + amount, vector.GetY() + amount, vector.GetZ() + amount);
	}

	[SpecialName]
	public static Vector3f op_Addition(Vector3f left, Vector3f right)
	{
		return new Vector3f(left.GetX() + right.GetX(), left.GetY() + right.GetY(), left.GetZ() + right.GetZ());
	}

	[SpecialName]
	public static Vector3f op_Subtraction(Vector3f vector, float amount)
	{
		return new Vector3f(vector.GetX() - amount, vector.GetY() - amount, vector.GetZ() - amount);
	}

	[SpecialName]
	public static Vector3f op_Subtraction(Vector3f left, Vector3f right)
	{
		return new Vector3f(left.GetX() - right.GetX(), left.GetY() - right.GetY(), left.GetZ() - right.GetZ());
	}

	[SpecialName]
	public static Vector3f op_Multiply(Vector3f vector, float scalar)
	{
		return new Vector3f(vector.GetX() * scalar, vector.GetY() * scalar, vector.GetZ() * scalar);
	}

	[SpecialName]
	public static float op_Multiply(Vector3f left, Vector3f right)
	{
		return left.GetX() * right.GetX() + left.GetY() * right.GetY() + left.GetZ() * right.GetZ();
	}

	[SpecialName]
	public static Vector3 op_Implicit(Vector3f vector)
	{
		return new Vector3(vector.GetX(), vector.GetY(), vector.GetZ());
	}

	[SpecialName]
	public static Vector3f op_Implicit(Vector3 vector)
	{
		return new Vector3f(vector.x, vector.y, vector.z);
	}

	public static float Distance(Vector3f fromPoint, Vector3f toPoint)
	{
		float num = toPoint.X - fromPoint.X;
		float num2 = toPoint.Y - fromPoint.Y;
		float num3 = toPoint.Z - fromPoint.Z;
		return Mathf.Sqrt(num * num + num2 * num2 + num3 * num3);
	}

	public float Distance(Vector3f other)
	{
		return Distance(other, this);
	}

	public Vector3f Add(float amount)
	{
		X += amount;
		Y += amount;
		Z += amount;
		return this;
	}

	public Vector3f Add(float x, float y, float z)
	{
		X += x;
		Y += y;
		Z += z;
		return this;
	}

	public Vector3f Add(Vector3f other)
	{
		X += other.GetX();
		Y += other.GetY();
		Z += other.GetZ();
		return this;
	}

	public void Add(Vector3f other, float scale)
	{
		X += other.X * scale;
		Y += other.Y * scale;
		Z += other.Z * scale;
	}

	public void AddScaledXY(Vector3f other, float scale)
	{
		X += other.X * scale;
		Y += other.Y * scale;
	}

	public new Vector3f Add(Vector2f other)
	{
		X += other.GetX();
		Y += other.GetY();
		return this;
	}

	public Vector3f Subtract(float amount)
	{
		X -= amount;
		Y -= amount;
		Z -= amount;
		return this;
	}

	public Vector3f Subtract(float x, float y, float z)
	{
		X -= x;
		Y -= y;
		Z -= z;
		return this;
	}

	public Vector3f Subtract(Vector3f other)
	{
		X -= other.GetX();
		Y -= other.GetY();
		Z -= other.GetZ();
		return this;
	}

	public new Vector3f SubtractXY(Vector2f other)
	{
		X -= other.GetX();
		Y -= other.GetY();
		return this;
	}

	public new Vector3f Multiply(float scalar)
	{
		X *= scalar;
		Y *= scalar;
		Z *= scalar;
		return this;
	}

	public Vector3f Multiply(float x, float y, float z)
	{
		X *= x;
		Y *= y;
		Z *= z;
		return this;
	}

	public Vector3f Cross(Vector3f other)
	{
		return new Vector3f(Y * other.GetZ() - Z * other.GetY(), Z * other.GetX() - X * other.GetZ(), X * other.GetY() - Y - other.GetX());
	}

	public bool IsEqual(float x, float y, float z)
	{
		return X == x && Y == y && Z == z;
	}

	public static Vector3f Cross(Vector3f p1, Vector3f p2, Vector3f p3, Vector3f p4)
	{
		float num = (p2.GetY() - p1.GetY()) * (p3.GetX() - p4.GetX()) - (p3.GetY() - p4.GetY()) * (p2.GetX() - p1.GetX());
		float num2 = (p2.GetY() - p1.GetY()) * (p3.GetX() - p1.GetX()) - (p3.GetY() - p1.GetY()) * (p2.GetX() - p1.GetX());
		float num3 = (p3.GetY() - p1.GetY()) * (p3.GetX() - p4.GetX()) - (p3.GetY() - p4.GetY()) * (p3.GetX() - p1.GetX());
		if ((double)num == 0.0 && (double)num2 == 0.0 && (double)num3 == 0.0)
		{
			return null;
		}
		if ((double)num == 0.0)
		{
			return null;
		}
		float num4 = num2 / num;
		float num5 = num3 / num;
		float intersectX = p1.GetX() + (p2.GetX() - p1.GetX()) * num5;
		float intersectY = p1.GetY() + (p2.GetY() - p1.GetY()) * num5;
		if (0f < num4 && num4 < 1f && ((0f < num5) & (num5 < 1f)))
		{
			return new Vector3f(intersectX, intersectY);
		}
		return null;
	}

	public static Vector3f Middle(Vector3f start, Vector3f end)
	{
		return op_Addition(start, op_Multiply(op_Subtraction(end, start), 0.5f));
	}

	public static void Middle(Vector3f start, Vector3f end, Vector3f result)
	{
		result.X = start.X + (end.X - start.X) * 0.5f;
		result.Y = start.Y + (end.Y - start.Y) * 0.5f;
		result.Z = start.Z + (end.Z - start.Z) * 0.5f;
	}

	public static Vector3f Closest(Vector3f point, Vector3f lineOrigin, Vector3f lineDirection)
	{
		Vector3f offset = op_Subtraction(point, lineOrigin);
		float num = op_Multiply(offset, lineDirection);
		float num2 = op_Multiply(lineDirection, lineDirection);
		float projection = 0f;
		if (num2 != 0f)
		{
			projection = num / num2;
		}
		return op_Addition(lineOrigin, op_Multiply(lineDirection, projection));
	}

	public void Reset()
	{
		X = 0f;
		Y = 0f;
		Z = 0f;
	}

	public static Vector3f Round(Vector3f vector, float multiplier)
	{
		vector.SetX(Round(vector.GetX(), multiplier));
		vector.SetY(Round(vector.GetY(), multiplier));
		vector.SetZ(Round(vector.GetZ(), multiplier));
		return vector;
	}

	public Vector3f Round(float multiplier)
	{
		X = Round(X, multiplier);
		Y = Round(Y, multiplier);
		Z = Round(Z, multiplier);
		return this;
	}

	public Vector3f Normalize()
	{
		float num = GetMagnitude();
		if (num != 0f)
		{
			num = 1f / num;
		}
		X *= num;
		Y *= num;
		Z *= num;
		return this;
	}

	public static Vector3f GetPerpendicularDirection(Vector3f pointA, Vector3f pointB)
	{
		return op_Subtraction(pointA, pointB).Cross(GetUp()).Normalize();
	}

	public static float Factor(Vector3f targetPoint, Vector3f originPoint, Vector3f referencePoint)
	{
		originPoint.SetZ(0f);
		targetPoint.SetZ(0f);
		referencePoint.SetZ(0f);
		return Distance(originPoint, referencePoint) / Distance(originPoint, targetPoint);
	}

	public Vector3f Clone()
	{
		return new Vector3f(X, Y, Z);
	}

	public Vector3f Set(Vector3f source)
	{
		X = source.X;
		Y = source.Y;
		Z = source.Z;
		return this;
	}

	public Vector3f Set(Vector3 source)
	{
		X = source.x;
		Y = source.y;
		Z = source.z;
		return this;
	}

	public Vector3f Set(float x = 0f, float y = 0f, float z = 0f)
	{
		X = x;
		Y = y;
		Z = z;
		return this;
	}

	public void SetMiddlePoint3D(Vector3f start, Vector3f end)
	{
		X = start.GetX() + (end.GetX() - start.GetX()) * 0.5f;
		Y = start.GetY() + (end.GetY() - start.GetY()) * 0.5f;
		Z = start.GetZ() + (end.GetZ() - start.GetZ()) * 0.5f;
	}

	public void Truncate()
	{
		X = (int)X;
		Y = (int)Y;
		Z = (int)Z;
	}

	public static Vector3f Create(XmlNode node)
	{
		if (node == null)
		{
			return null;
		}
		Vector3f vector = new Vector3f();
		try
		{
			vector.X = float.Parse(node.Attributes["X"].Value, NumberStyles.Float, CultureInfo.InvariantCulture);
		}
		catch
		{
			throw new Exception("Error : parse X eeror type");
		}
		try
		{
			vector.Y = float.Parse(node.Attributes["Y"].Value, NumberStyles.Float, CultureInfo.InvariantCulture);
			return vector;
		}
		catch
		{
			throw new Exception("Error : parse Y eeror type");
		}
	}

	public override string ToString()
	{
		return "X=" + X.ToString("F4") + " Y=" + Y.ToString("F4") + " Z=" + Z.ToString("F4");
	}

	public new static float Round(float Value, float multiplier)
	{
		return Mathf.Floor(Value * multiplier + 0.5f) / multiplier;
	}

	public static Vector3f GetDivisionPoint3D(Vector3f start, Vector3f end, float ratio)
	{
		return new Vector3f(start.X + (end.X - start.X) * ratio, start.Y + (end.Y - start.Y) * ratio, start.Z + (end.Z - start.Z) * ratio);
	}

	public static void GetDivisionPoint3D(Vector3f start, Vector3f end, float ratio, Vector3f result)
	{
		result.Set(start.X + (end.X - start.X) * ratio, start.Y + (end.Y - start.Y) * ratio, start.Z + (end.Z - start.Z) * ratio);
	}
}
