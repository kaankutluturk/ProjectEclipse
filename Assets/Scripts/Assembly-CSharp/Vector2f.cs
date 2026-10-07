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

	public Vector2f(Vector2f PALAIICCALN)
	{
		X = PALAIICCALN.GetX();
		Y = PALAIICCALN.GetY();
	}

	public Vector2f(float LHNJJFDIJKK = 0f, float FFFHIOALHGM = 0f)
	{
		X = LHNJJFDIJKK;
		Y = FFFHIOALHGM;
	}

	public Vector2f(Vector3f PMEFNGKHLII)
	{
		X = PMEFNGKHLII.GetX();
		Y = PMEFNGKHLII.GetY();
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
	public static Vector2f op_Subtraction(Vector2f GIIIFLBEONP, Vector2f OIFPGODHFGH)
	{
		return new Vector2f(GIIIFLBEONP.GetX() - OIFPGODHFGH.GetX(), GIIIFLBEONP.GetY() - OIFPGODHFGH.GetY());
	}

	[SpecialName]
	public static Vector2f op_Addition(Vector2f GIIIFLBEONP, Vector2f OIFPGODHFGH)
	{
		return new Vector2f(GIIIFLBEONP.GetX() + OIFPGODHFGH.GetX(), GIIIFLBEONP.GetY() + OIFPGODHFGH.GetY());
	}

	[SpecialName]
	public static bool op_Equality(Vector2f GIIIFLBEONP, Vector2f OIFPGODHFGH)
	{
		if (object.ReferenceEquals(GIIIFLBEONP, OIFPGODHFGH))
		{
			return true;
		}
		if (object.ReferenceEquals(GIIIFLBEONP, null) || object.ReferenceEquals(OIFPGODHFGH, null))
		{
			return false;
		}
		return GIIIFLBEONP.GetX() == OIFPGODHFGH.GetX() && GIIIFLBEONP.GetY() == OIFPGODHFGH.GetY();
	}

	[SpecialName]
	public static bool op_Inequality(Vector2f GIIIFLBEONP, Vector2f OIFPGODHFGH)
	{
		return !op_Equality(GIIIFLBEONP, OIFPGODHFGH);
	}

	[SpecialName]
	public static Vector3 op_Implicit(Vector2f BEHOPOPCJGB)
	{
		return new Vector3(BEHOPOPCJGB.GetX(), BEHOPOPCJGB.GetY(), 0f);
	}

	public Vector2f Add(Vector2f PALAIICCALN)
	{
		X += PALAIICCALN.GetX();
		Y += PALAIICCALN.GetY();
		return this;
	}

	public Vector2f Add(float LHNJJFDIJKK, float FFFHIOALHGM)
	{
		X += LHNJJFDIJKK;
		Y += FFFHIOALHGM;
		return this;
	}

	public Vector2f SubtractXY(Vector2f PALAIICCALN)
	{
		X -= PALAIICCALN.GetX();
		Y -= PALAIICCALN.GetY();
		return this;
	}

	public Vector2f Multiply(float LIAILCGJBDK)
	{
		X *= LIAILCGJBDK;
		Y *= LIAILCGJBDK;
		return this;
	}

	public void Set(Vector2f PALAIICCALN)
	{
		X = PALAIICCALN.GetX();
		Y = PALAIICCALN.GetY();
	}

	public void Round(int CFCPPNJKNAL)
	{
		X = Round(X, CFCPPNJKNAL);
		Y = Round(Y, CFCPPNJKNAL);
	}

	public void RoundToInteger()
	{
		X = Round(X, 1f);
		Y = Round(Y, 1f);
	}

	public float DotProduct(Vector2f PIFPKKNEAID)
	{
		return X * PIFPKKNEAID.GetX() + Y * PIFPKKNEAID.GetY();
	}

	public string GetFormattedValue()
	{
		return string.Format("[{0} {1}]", X, Y);
	}

	public override string ToString()
	{
		return string.Format("[Point: X={0}, Y={1}]", GetX(), GetY());
	}

	public static float Round(float Value, float JMLMHGAMNKN)
	{
		return Mathf.Floor(Value * JMLMHGAMNKN + 0.5f) / JMLMHGAMNKN;
	}

	public static float Distance2D(Vector3f AMNCLCPADOO, Vector3f IFIOLDFCLIE)
	{
		return Mathf.Sqrt((AMNCLCPADOO.GetX() - IFIOLDFCLIE.GetX()) * (AMNCLCPADOO.GetX() - IFIOLDFCLIE.GetX()) + (AMNCLCPADOO.GetY() - IFIOLDFCLIE.GetY()) * (AMNCLCPADOO.GetY() - IFIOLDFCLIE.GetY()));
	}

	public static float Distance2D(Vector3f AMNCLCPADOO, Vector2f IFIOLDFCLIE)
	{
		return Mathf.Sqrt((AMNCLCPADOO.GetX() - IFIOLDFCLIE.GetX()) * (AMNCLCPADOO.GetX() - IFIOLDFCLIE.GetX()) + (AMNCLCPADOO.GetY() - IFIOLDFCLIE.GetY()) * (AMNCLCPADOO.GetY() - IFIOLDFCLIE.GetY()));
	}

	public static bool TryGetSegmentIntersection(Vector3f IEKADOOKFKG, Vector3f LDKCOIHONPG, Vector3f AOOIPCJPALH, Vector3f IGMCGOFHHCJ, Vector3f DCJLKCFKCOM)
	{
		if ((IEKADOOKFKG.GetX() == LDKCOIHONPG.GetX() && IEKADOOKFKG.GetY() == LDKCOIHONPG.GetY()) || (AOOIPCJPALH.GetX() == IGMCGOFHHCJ.GetX() && AOOIPCJPALH.GetY() == IGMCGOFHHCJ.GetY()))
		{
			return false;
		}
		float num = LDKCOIHONPG.GetX() - IEKADOOKFKG.GetX();
		float num2 = LDKCOIHONPG.GetY() - IEKADOOKFKG.GetY();
		float num3 = IGMCGOFHHCJ.GetX() - AOOIPCJPALH.GetX();
		float num4 = IGMCGOFHHCJ.GetY() - AOOIPCJPALH.GetY();
		float num5 = IEKADOOKFKG.GetX() - AOOIPCJPALH.GetX();
		float num6 = IEKADOOKFKG.GetY() - AOOIPCJPALH.GetY();
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
			if (IEKADOOKFKG.GetX() < LDKCOIHONPG.GetX())
			{
				num10 = IEKADOOKFKG.GetX();
				num11 = LDKCOIHONPG.GetX();
			}
			else
			{
				num10 = LDKCOIHONPG.GetX();
				num11 = IEKADOOKFKG.GetX();
			}
			float num12;
			float num13;
			if (AOOIPCJPALH.GetX() < IGMCGOFHHCJ.GetX())
			{
				num12 = AOOIPCJPALH.GetX();
				num13 = IGMCGOFHHCJ.GetX();
			}
			else
			{
				num12 = IGMCGOFHHCJ.GetX();
				num13 = AOOIPCJPALH.GetX();
			}
			if (num10 > num13 || num12 > num11)
			{
				return false;
			}
			if (IEKADOOKFKG.GetY() < LDKCOIHONPG.GetY())
			{
				num10 = IEKADOOKFKG.GetY();
				num11 = LDKCOIHONPG.GetY();
			}
			else
			{
				num10 = LDKCOIHONPG.GetY();
				num11 = IEKADOOKFKG.GetY();
			}
			if (AOOIPCJPALH.GetY() < IGMCGOFHHCJ.GetY())
			{
				num12 = AOOIPCJPALH.GetY();
				num13 = IGMCGOFHHCJ.GetY();
			}
			else
			{
				num12 = IGMCGOFHHCJ.GetY();
				num13 = AOOIPCJPALH.GetY();
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
			DCJLKCFKCOM.SetX(IEKADOOKFKG.GetX() + num8 * (LDKCOIHONPG.GetX() - IEKADOOKFKG.GetX()));
			DCJLKCFKCOM.SetY(IEKADOOKFKG.GetY() + num8 * (LDKCOIHONPG.GetY() - IEKADOOKFKG.GetY()));
			return true;
		}
		return false;
	}

	public static void BuildEquationLine(Vector3f MLGFPMDKOHD, Vector3f DMMNCDKPCCI, EquationLine EGKHHBMCGMK)
	{
		float num = Distance2D(MLGFPMDKOHD, DMMNCDKPCCI);
		EGKHHBMCGMK.A = (MLGFPMDKOHD.GetY() - DMMNCDKPCCI.GetY()) / num;
		EGKHHBMCGMK.CoefficientB = (DMMNCDKPCCI.GetX() - MLGFPMDKOHD.GetX()) / num;
		EGKHHBMCGMK.ConstantC = 0f - (EGKHHBMCGMK.A * MLGFPMDKOHD.GetX() + EGKHHBMCGMK.CoefficientB * MLGFPMDKOHD.GetY());
	}

	public static EquationLine BuildEquationLine(Vector3f MLGFPMDKOHD, Vector3f DMMNCDKPCCI)
	{
		EquationLine kEDCEHBPOIM = new EquationLine();
		BuildEquationLine(MLGFPMDKOHD, DMMNCDKPCCI, kEDCEHBPOIM);
		return kEDCEHBPOIM;
	}

	public static bool TryIntersectSegments(Vector3f IEKADOOKFKG, Vector3f LDKCOIHONPG, Vector3f AOOIPCJPALH, Vector3f IGMCGOFHHCJ, Vector3f DCJLKCFKCOM)
	{
		if ((IEKADOOKFKG.GetX() == LDKCOIHONPG.GetX() && IEKADOOKFKG.GetY() == LDKCOIHONPG.GetY()) || (AOOIPCJPALH.GetX() == IGMCGOFHHCJ.GetX() && AOOIPCJPALH.GetY() == IGMCGOFHHCJ.GetY()))
		{
			return false;
		}
		float num = LDKCOIHONPG.GetX() - IEKADOOKFKG.GetX();
		float num2 = LDKCOIHONPG.GetY() - IEKADOOKFKG.GetY();
		float num3 = IGMCGOFHHCJ.GetX() - AOOIPCJPALH.GetX();
		float num4 = IGMCGOFHHCJ.GetY() - AOOIPCJPALH.GetY();
		float num5 = IEKADOOKFKG.GetX() - AOOIPCJPALH.GetX();
		float num6 = IEKADOOKFKG.GetY() - AOOIPCJPALH.GetY();
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
			if (IEKADOOKFKG.GetX() < LDKCOIHONPG.GetX())
			{
				num10 = IEKADOOKFKG.GetX();
				num11 = LDKCOIHONPG.GetX();
			}
			else
			{
				num10 = LDKCOIHONPG.GetX();
				num11 = IEKADOOKFKG.GetX();
			}
			float num12;
			float num13;
			if (AOOIPCJPALH.GetX() < IGMCGOFHHCJ.GetX())
			{
				num12 = AOOIPCJPALH.GetX();
				num13 = IGMCGOFHHCJ.GetX();
			}
			else
			{
				num12 = IGMCGOFHHCJ.GetX();
				num13 = AOOIPCJPALH.GetX();
			}
			if (num10 > num13 || num12 > num11)
			{
				return false;
			}
			if (IEKADOOKFKG.GetY() < LDKCOIHONPG.GetY())
			{
				num10 = IEKADOOKFKG.GetY();
				num11 = LDKCOIHONPG.GetY();
			}
			else
			{
				num10 = LDKCOIHONPG.GetY();
				num11 = IEKADOOKFKG.GetY();
			}
			if (AOOIPCJPALH.GetY() < IGMCGOFHHCJ.GetY())
			{
				num12 = AOOIPCJPALH.GetY();
				num13 = IGMCGOFHHCJ.GetY();
			}
			else
			{
				num12 = IGMCGOFHHCJ.GetY();
				num13 = AOOIPCJPALH.GetY();
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
			DCJLKCFKCOM.SetX(IEKADOOKFKG.GetX() + num8 * (LDKCOIHONPG.GetX() - IEKADOOKFKG.GetX()));
			DCJLKCFKCOM.SetY(IEKADOOKFKG.GetY() + num8 * (LDKCOIHONPG.GetY() - IEKADOOKFKG.GetY()));
			return true;
		}
		return false;
	}

	public static bool IsDistanceStrike(float OIOMNNFMDOO, float JBLFLFOGDFI, EquationLine EGKHHBMCGMK, Vector3f NAAPALOFBCI, Vector3f _base, Vector3f ILENLCMAMBH, Vector3f PCLFFOBJJFO)
	{
		if (Mathf.Abs(OIOMNNFMDOO) <= JBLFLFOGDFI)
		{
			_base.SetX(NAAPALOFBCI.GetX() - OIOMNNFMDOO * EGKHHBMCGMK.A);
			_base.SetY(NAAPALOFBCI.GetY() - OIOMNNFMDOO * EGKHHBMCGMK.CoefficientB);
			return (((PCLFFOBJJFO.GetX() <= _base.GetX() && _base.GetX() <= ILENLCMAMBH.GetX()) || (ILENLCMAMBH.GetX() <= _base.GetX() && _base.GetX() <= PCLFFOBJJFO.GetX())) && ((PCLFFOBJJFO.GetY() <= _base.GetY() && _base.GetY() <= ILENLCMAMBH.GetY()) || (ILENLCMAMBH.GetY() <= _base.GetY() && _base.GetY() <= PCLFFOBJJFO.GetY()))) || (NAAPALOFBCI.GetX() - ILENLCMAMBH.GetX()) * (NAAPALOFBCI.GetX() - ILENLCMAMBH.GetX()) + (NAAPALOFBCI.GetY() - ILENLCMAMBH.GetY()) * (NAAPALOFBCI.GetY() - ILENLCMAMBH.GetY()) <= JBLFLFOGDFI * JBLFLFOGDFI || (NAAPALOFBCI.GetX() - PCLFFOBJJFO.GetX()) * (NAAPALOFBCI.GetX() - PCLFFOBJJFO.GetX()) + (NAAPALOFBCI.GetY() - PCLFFOBJJFO.GetY()) * (NAAPALOFBCI.GetY() - PCLFFOBJJFO.GetY()) <= JBLFLFOGDFI * JBLFLFOGDFI;
		}
		return false;
	}

	public static bool TryIntersectThickSegments(Vector3f HICHONIJHKL, Vector3f LNPFHLPCLOP, float KLDFJGIKIHG, Vector3f NMAJNHKJJEM, Vector3f ONNJMGGPHEL, float MGCKDDGGCBI, Vector3f DNJCFHNICBH, Vector3f LOFMLNLKFLB, EquationLine JHMHDMOADMA, EquationLine GIMOMPLMEJH)
	{
		float num = KLDFJGIKIHG + MGCKDDGGCBI;
		if (num == 0f)
		{
			if (TryIntersectSegments(NMAJNHKJJEM, ONNJMGGPHEL, HICHONIJHKL, LNPFHLPCLOP, DNJCFHNICBH))
			{
				LOFMLNLKFLB.Set(DNJCFHNICBH);
				return true;
			}
			return false;
		}
		EquationLine kEDCEHBPOIM = ((GIMOMPLMEJH == null) ? BuildEquationLine(NMAJNHKJJEM, ONNJMGGPHEL) : GIMOMPLMEJH);
		float num2 = kEDCEHBPOIM.A * HICHONIJHKL.GetX() + kEDCEHBPOIM.CoefficientB * HICHONIJHKL.GetY() + kEDCEHBPOIM.ConstantC;
		float num3 = kEDCEHBPOIM.A * LNPFHLPCLOP.GetX() + kEDCEHBPOIM.CoefficientB * LNPFHLPCLOP.GetY() + kEDCEHBPOIM.ConstantC;
		if (0f <= num2 * num3 && num < Mathf.Abs(num2) && num < Mathf.Abs(num3))
		{
			return false;
		}
		EquationLine kEDCEHBPOIM2 = ((JHMHDMOADMA == null) ? BuildEquationLine(HICHONIJHKL, LNPFHLPCLOP) : JHMHDMOADMA);
		float num4 = kEDCEHBPOIM2.A * NMAJNHKJJEM.GetX() + kEDCEHBPOIM2.CoefficientB * NMAJNHKJJEM.GetY() + kEDCEHBPOIM2.ConstantC;
		float num5 = kEDCEHBPOIM2.A * ONNJMGGPHEL.GetX() + kEDCEHBPOIM2.CoefficientB * ONNJMGGPHEL.GetY() + kEDCEHBPOIM2.ConstantC;
		if (0f <= num4 * num5 && num < Mathf.Abs(num4) && num < Mathf.Abs(num5))
		{
			return false;
		}
		if (num4 * num5 < 0f && num2 * num3 < 0f)
		{
			float lIAILCGJBDK = num4 / (num4 - num5);
			DNJCFHNICBH.Set(Vector3f.op_Subtraction(ONNJMGGPHEL, NMAJNHKJJEM));
			DNJCFHNICBH.Multiply(lIAILCGJBDK);
			DNJCFHNICBH.Add(NMAJNHKJJEM);
			LOFMLNLKFLB.Set(DNJCFHNICBH);
			return true;
		}
		if (IsDistanceStrike(num2, num, kEDCEHBPOIM, HICHONIJHKL, LOFMLNLKFLB, NMAJNHKJJEM, ONNJMGGPHEL))
		{
			DNJCFHNICBH.Set(HICHONIJHKL);
			return true;
		}
		if (IsDistanceStrike(num3, num, kEDCEHBPOIM, LNPFHLPCLOP, LOFMLNLKFLB, NMAJNHKJJEM, ONNJMGGPHEL))
		{
			DNJCFHNICBH.Set(LNPFHLPCLOP);
			return true;
		}
		if (IsDistanceStrike(num4, num, kEDCEHBPOIM2, NMAJNHKJJEM, LOFMLNLKFLB, HICHONIJHKL, LNPFHLPCLOP))
		{
			DNJCFHNICBH.Set(NMAJNHKJJEM);
			LOFMLNLKFLB.Set(NMAJNHKJJEM);
			return true;
		}
		if (IsDistanceStrike(num5, num, kEDCEHBPOIM2, ONNJMGGPHEL, LOFMLNLKFLB, HICHONIJHKL, LNPFHLPCLOP))
		{
			DNJCFHNICBH.Set(ONNJMGGPHEL);
			LOFMLNLKFLB.Set(ONNJMGGPHEL);
			return true;
		}
		return false;
	}

	public static float DistanceSquared2D(Vector2f LHBNIMGFKIB, Vector2f AAOIAEJJINO)
	{
		return (AAOIAEJJINO.GetX() - LHBNIMGFKIB.GetX()) * (AAOIAEJJINO.GetX() - LHBNIMGFKIB.GetX()) + (AAOIAEJJINO.GetY() - LHBNIMGFKIB.GetY()) * (AAOIAEJJINO.GetY() - LHBNIMGFKIB.GetY());
	}

	public static float GetAngle2DDegreeSigned(Vector2f LHBNIMGFKIB, Vector2f AAOIAEJJINO)
	{
		return GetAngle2DRadianSigned(LHBNIMGFKIB, AAOIAEJJINO) * 57.29578f;
	}

	public static float GetAngle2DRadianSigned(Vector2f LHBNIMGFKIB, Vector2f AAOIAEJJINO)
	{
		float num = LHBNIMGFKIB.X * AAOIAEJJINO.Y - LHBNIMGFKIB.Y * AAOIAEJJINO.X;
		float num2 = LHBNIMGFKIB.X * AAOIAEJJINO.X + LHBNIMGFKIB.Y * AAOIAEJJINO.Y;
		float num3 = 1f / Mathf.Sqrt(num * num + num2 * num2);
		return Mathf.Atan2(num * num3, num2 * num3);
	}
}
