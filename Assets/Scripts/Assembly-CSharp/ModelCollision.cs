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

	public ModelCollision(ModelObject modelObject)
	{
		Strike = new StrikeHit();
		_ModelObject = modelObject;
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

	public bool Render(ModelObject victimModel, List<ModelEdge> attackerEdges, object strikePhase)
	{
		if (_Render && IsPhase(strikePhase))
		{
			return false;
		}
		List<ModelEdge> victimEdges = victimModel.GetCollisionEdges();
		foreach (ModelEdge item in attackerEdges)
		{
			if (CrossModel(victimEdges, item))
			{
				_LastStrikePhase = strikePhase;
				return true;
			}
		}
		return false;
	}

	public bool Render(ModelObject victimModel, List<ModelEdge> attackerEdges, IntervalAnimation interval)
	{
		if (lastStrikeInterval == interval)
		{
			return false;
		}
		IntervalAttack attackInterval = interval as IntervalAttack;
		if (!attackInterval.GetHasAttackingParts())
		{
			lastStrikeInterval = interval;
			Strike.AttackerEdge = null;
			Strike.VictimEdge = null;
			Strike.GetPoint().Reset();
			Strike.GetSecondPoint().Reset();
			return true;
		}
		List<ModelEdge> victimEdges = victimModel.GetCollisionEdges();
		foreach (ModelEdge item in attackerEdges)
		{
			if (CrossModel(victimEdges, item))
			{
				lastStrikeInterval = interval;
				return true;
			}
		}
		return false;
	}

	public void ResetLastStrike()
	{
		_LastStrikePhase = null;
	}

	public bool IsPhase(object strikePhase)
	{
		return _LastStrikePhase == strikePhase;
	}

	public bool CrossModelByEdge(ModelObject victimModel, ModelEdge attackerEdge)
	{
		List<ModelEdge> victimEdges = victimModel.GetCollisionEdges();
		return CrossModel(victimEdges, attackerEdge);
	}

	public void ResetInterval()
	{
		lastStrikeInterval = null;
	}

	private bool CrossModel(List<ModelEdge> victimEdges, ModelEdge attackerEdge)
	{
		Vector3f firstIntersection = new Vector3f();
		Vector3f secondIntersection = new Vector3f();
		float attackerRadius = attackerEdge.GetCollisionRadius();
		Vector3f attackerStart = attackerEdge.GetCollisionStart();
		Vector3f attackerEnd = attackerEdge.GetCollisionEnd();
		EquationLine attackerLine = attackerEdge.LineEquation;
		foreach (ModelEdge item in victimEdges)
		{
			float victimRadius = item.GetCollisionRadius();
			Vector3f victimStart = item.GetCollisionStart();
			Vector3f victimEnd = item.GetCollisionEnd();
			EquationLine victimLine = item.LineEquation;
			if (Vector2f.TryIntersectThickSegments(attackerStart, attackerEnd, attackerRadius, victimStart, victimEnd, victimRadius, firstIntersection, secondIntersection, attackerLine, victimLine))
			{
				AddStrike(attackerEdge, item, firstIntersection, secondIntersection);
				return true;
			}
		}
		return false;
	}

	private void AddStrike(ModelEdge attackerEdge, ModelEdge victimEdge, Vector3f firstPoint, Vector3f secondPoint)
	{
		Strike.AttackerEdge = attackerEdge;
		Strike.VictimEdge = victimEdge;
		Strike.SetPoint(firstPoint);
		Strike.SetSecondPoint(secondPoint);
	}

	private static bool IsDistanceStrike(float distance, float radius, EquationLine line, Vector3f hitPoint, Vector3f _base, Vector3f segmentStart, Vector3f segmentEnd)
	{
		if (distance < radius)
		{
			_base.SetX(hitPoint.GetX() - distance * line.A);
			_base.SetY(hitPoint.GetY() - distance * line.CoefficientB);
			if (((_base.GetX() <= segmentStart.GetX() && _base.GetX() >= segmentEnd.GetX()) || (_base.GetX() <= segmentEnd.GetX() && _base.GetX() >= segmentStart.GetX())) && ((_base.GetY() <= segmentStart.GetY() && _base.GetY() >= segmentEnd.GetY()) || (_base.GetY() <= segmentEnd.GetY() && _base.GetY() >= segmentStart.GetY())))
			{
				return true;
			}
			if (Mathf.Pow(hitPoint.GetX() - segmentStart.GetX(), 2f) + Mathf.Pow(hitPoint.GetY() - segmentStart.GetY(), 2f) <= Mathf.Pow(radius, 2f))
			{
				return true;
			}
			if (Mathf.Pow(hitPoint.GetX() - segmentEnd.GetX(), 2f) + Mathf.Pow(hitPoint.GetY() - segmentEnd.GetY(), 2f) <= Mathf.Pow(radius, 2f))
			{
				return true;
			}
		}
		return false;
	}
}
