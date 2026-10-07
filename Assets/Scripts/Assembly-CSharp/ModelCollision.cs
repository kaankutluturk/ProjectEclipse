using System.Collections.Generic;
using UnityEngine;

public class ModelCollision
{
	public class StrikeHit
	{
		public ModelEdge VictimEdge;

		public ModelEdge AttackerEdge;

		private Vector3f point = new Vector3f();

		private Vector3f secondPoint = new Vector3f();

		public Vector3f Point
		{
			get
			{
				return GetPoint();
			}
			set
			{
				SetPoint(value);
			}
		}

		public Vector3f SecondPoint
		{
			get
			{
				return GetSecondPoint();
			}
			set
			{
				SetSecondPoint(value);
			}
		}

		public Vector3f GetPoint()
		{
			return point;
		}

		public void SetPoint(Vector3f value)
		{
			point.Set(value);
		}

		public Vector3f GetSecondPoint()
		{
			return secondPoint;
		}

		public void SetSecondPoint(Vector3f value)
		{
			secondPoint.Set(value);
		}
	}

	private IntervalAnimation lastStrikeInterval;

	private object _LastStrikePhase;

	private ModelObject _ModelObject;

	private bool _Render;

	public StrikeHit Strike;

	public object StrikePhase
	{
		get
		{
			return GetLastStrikePhase();
		}
		set
		{
			set_LastStrikePhase(value);
		}
	}

	public bool RenderEnabled
	{
		get
		{
			return GetRenderEnabled();
		}
		set
		{
			set_render(value);
		}
	}

	public ModelCollision(ModelObject ACENLMONNPA)
	{
		Strike = new StrikeHit();
		_ModelObject = ACENLMONNPA;
		_LastStrikePhase = null;
		lastStrikeInterval = null;
		_Render = true;
	}

	public object GetLastStrikePhase()
	{
		return _LastStrikePhase;
	}

	public void set_LastStrikePhase(object value)
	{
		_LastStrikePhase = value;
	}

	public bool GetRenderEnabled()
	{
		return _Render;
	}

	public void set_render(bool value)
	{
		_Render = value;
	}

	public bool Render(ModelObject HFGPAELCNMF, List<ModelEdge> BLJEFDAPKBH, object HIJDANGMJDM)
	{
		if (_Render && IsPhase(HIJDANGMJDM))
		{
			return false;
		}
		List<ModelEdge> lONAJAHCJGH = HFGPAELCNMF.GetCollisionEdges();
		foreach (ModelEdge item in BLJEFDAPKBH)
		{
			if (CrossModel(lONAJAHCJGH, item))
			{
				_LastStrikePhase = HIJDANGMJDM;
				return true;
			}
		}
		return false;
	}

	public bool Render(ModelObject HFGPAELCNMF, List<ModelEdge> BLJEFDAPKBH, IntervalAnimation NOJNPFMOFLM)
	{
		if (lastStrikeInterval == NOJNPFMOFLM)
		{
			return false;
		}
		IntervalAttack hFIIPNLCIEE = NOJNPFMOFLM as IntervalAttack;
		if (!hFIIPNLCIEE.GetHasAttackingParts())
		{
			lastStrikeInterval = NOJNPFMOFLM;
			Strike.AttackerEdge = null;
			Strike.VictimEdge = null;
			Strike.GetPoint().Reset();
			Strike.GetSecondPoint().Reset();
			return true;
		}
		List<ModelEdge> lONAJAHCJGH = HFGPAELCNMF.GetCollisionEdges();
		foreach (ModelEdge item in BLJEFDAPKBH)
		{
			if (CrossModel(lONAJAHCJGH, item))
			{
				lastStrikeInterval = NOJNPFMOFLM;
				return true;
			}
		}
		return false;
	}

	public void ResetLastStrike()
	{
		_LastStrikePhase = null;
	}

	public bool IsPhase(object HIJDANGMJDM)
	{
		return _LastStrikePhase == HIJDANGMJDM;
	}

	public bool CrossModelByEdge(ModelObject HFGPAELCNMF, ModelEdge ADFIIAJCBHA)
	{
		List<ModelEdge> lONAJAHCJGH = HFGPAELCNMF.GetCollisionEdges();
		return CrossModel(lONAJAHCJGH, ADFIIAJCBHA);
	}

	public void ResetInterval()
	{
		lastStrikeInterval = null;
	}

	private bool CrossModel(List<ModelEdge> LONAJAHCJGH, ModelEdge PJMKFHFECLK)
	{
		Vector3f eMAFACPEPDK = new Vector3f();
		Vector3f eMAFACPEPDK2 = new Vector3f();
		float kLDFJGIKIHG = PJMKFHFECLK.GetCollisionRadius();
		Vector3f hICHONIJHKL = PJMKFHFECLK.GetCollisionStart();
		Vector3f lNPFHLPCLOP = PJMKFHFECLK.GetCollisionEnd();
		EquationLine hENNAFMBEAG = PJMKFHFECLK.LineEquation;
		foreach (ModelEdge item in LONAJAHCJGH)
		{
			float mGCKDDGGCBI = item.GetCollisionRadius();
			Vector3f nMAJNHKJJEM = item.GetCollisionStart();
			Vector3f oNNJMGGPHEL = item.GetCollisionEnd();
			EquationLine hENNAFMBEAG2 = item.LineEquation;
			if (Vector2f.TryIntersectThickSegments(hICHONIJHKL, lNPFHLPCLOP, kLDFJGIKIHG, nMAJNHKJJEM, oNNJMGGPHEL, mGCKDDGGCBI, eMAFACPEPDK, eMAFACPEPDK2, hENNAFMBEAG, hENNAFMBEAG2))
			{
				AddStrike(PJMKFHFECLK, item, eMAFACPEPDK, eMAFACPEPDK2);
				return true;
			}
		}
		return false;
	}

	private void AddStrike(ModelEdge PJMKFHFECLK, ModelEdge KPEGNDLGKFB, Vector3f NAAPALOFBCI, Vector3f GKCGDDBMHNJ)
	{
		Strike.AttackerEdge = PJMKFHFECLK;
		Strike.VictimEdge = KPEGNDLGKFB;
		Strike.SetPoint(NAAPALOFBCI);
		Strike.SetSecondPoint(GKCGDDBMHNJ);
	}

	private static bool IsDistanceStrike(float OIOMNNFMDOO, float JBLFLFOGDFI, EquationLine EGKHHBMCGMK, Vector3f NAAPALOFBCI, Vector3f _base, Vector3f ILENLCMAMBH, Vector3f PCLFFOBJJFO)
	{
		if (OIOMNNFMDOO < JBLFLFOGDFI)
		{
			_base.SetX(NAAPALOFBCI.GetX() - OIOMNNFMDOO * EGKHHBMCGMK.A);
			_base.SetY(NAAPALOFBCI.GetY() - OIOMNNFMDOO * EGKHHBMCGMK.CoefficientB);
			if (((_base.GetX() <= ILENLCMAMBH.GetX() && _base.GetX() >= PCLFFOBJJFO.GetX()) || (_base.GetX() <= PCLFFOBJJFO.GetX() && _base.GetX() >= ILENLCMAMBH.GetX())) && ((_base.GetY() <= ILENLCMAMBH.GetY() && _base.GetY() >= PCLFFOBJJFO.GetY()) || (_base.GetY() <= PCLFFOBJJFO.GetY() && _base.GetY() >= ILENLCMAMBH.GetY())))
			{
				return true;
			}
			if (Mathf.Pow(NAAPALOFBCI.GetX() - ILENLCMAMBH.GetX(), 2f) + Mathf.Pow(NAAPALOFBCI.GetY() - ILENLCMAMBH.GetY(), 2f) <= Mathf.Pow(JBLFLFOGDFI, 2f))
			{
				return true;
			}
			if (Mathf.Pow(NAAPALOFBCI.GetX() - PCLFFOBJJFO.GetX(), 2f) + Mathf.Pow(NAAPALOFBCI.GetY() - PCLFFOBJJFO.GetY(), 2f) <= Mathf.Pow(JBLFLFOGDFI, 2f))
			{
				return true;
			}
		}
		return false;
	}
}
