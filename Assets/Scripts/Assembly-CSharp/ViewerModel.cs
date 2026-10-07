using System.Collections.Generic;
using UnityEngine;

public class ViewerModel
{
	private GameObject _UnityObject;

	private List<ModelObject> models = new List<ModelObject>();

	private ModelObject firstFighter;

	private ModelObject secondFighter;

	public GameObject RootObject
	{
		get
		{
			return GetRootObject();
		}
	}

	public ModelObject FirstFighter
	{
		get
		{
			return GetFirstFighter();
		}
	}

	public ModelObject SecondFighter
	{
		get
		{
			return GetSecondFighter();
		}
	}

	public float FighterDistance
	{
		get
		{
			return GetFighterDistance();
		}
	}

	public ViewerModel()
	{
		_UnityObject = new GameObject("ViewerModel");
	}

	public GameObject GetRootObject()
	{
		return _UnityObject;
	}

	public ModelObject GetFirstFighter()
	{
		return firstFighter;
	}

	public ModelObject GetSecondFighter()
	{
		return secondFighter;
	}

	public void Clear()
	{
		firstFighter = null;
		secondFighter = null;
		models.Clear();
	}

	public void Init(float GBNPHCHGKDO)
	{
	}

	public int AddModel(ModelObject ACENLMONNPA, Color color, bool IGGHECALMMP)
	{
		if (IGGHECALMMP)
		{
			if (firstFighter == null)
			{
				firstFighter = ACENLMONNPA;
			}
			else
			{
				secondFighter = ACENLMONNPA;
			}
		}
		ACENLMONNPA.GetModel().GetGameObject().transform.SetParent(_UnityObject.transform, false);
		ACENLMONNPA.GetModel().set_color(color);
		models.Add(ACENLMONNPA);
		return 0;
	}

	public void RemoveModel(int index)
	{
		ModelObject oIEODIEHJMH = models[index];
		if (oIEODIEHJMH == firstFighter)
		{
			firstFighter = null;
		}
		else if (oIEODIEHJMH == secondFighter)
		{
			secondFighter = null;
		}
		models.RemoveAt(index);
	}

    internal bool ReplaceModel(int index, ModelObject expected, ModelObject replacement, Color color)
    {
        if (expected == null || replacement == null || index < 0 || index >= models.Count ||
            models[index] != expected || models.Contains(replacement)) return false;
        // Prepare render parenting before changing either primary fighter reference.
        replacement.GetModel().GetGameObject().transform.SetParent(_UnityObject.transform, false);
        replacement.GetModel().set_color(color);
        models[index] = replacement;
        if (firstFighter == expected) firstFighter = replacement;
        if (secondFighter == expected) secondFighter = replacement;
        return true;
    }

	public void SetModelActive(ModelObject ACENLMONNPA, bool value)
	{
		foreach (ModelObject item in models)
		{
			if (item == ACENLMONNPA)
			{
				// Rollback remembers the previous state, so an undone vanish is shown again.
				Eclipse.Multiplayer.Rollback.RollbackObjects.SetActive(item.GetModel().GetGameObject(), value);
				break;
			}
		}
	}

	// Distance between the two fighters' interpolated pivots, for presentation.
	public float InterpolatedFighterDistance()
	{
		Model first = firstFighter != null ? firstFighter.GetModel() : null;
		Model second = secondFighter != null ? secondFighter.GetModel() : null;
		if (first == null || second == null) return GetFighterDistance();
		return Vector2f.Distance2D(first.InterpolatedPivot(), second.InterpolatedPivot());
	}

	public float GetFighterDistance()
	{
		if (firstFighter != null && secondFighter != null)
		{
			return Vector2f.Distance2D(firstFighter.GetCenterOfMassPosition(), secondFighter.GetCenterOfMassPosition());
		}
		return 0f;
	}
}
