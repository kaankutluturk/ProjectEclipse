using Nekki.SF2.Core.Fights.Renders.Model;
using UnityEngine;

public class Capsule : Segment3D
{
	private class AdditionalData
	{
		public string Name;

		public float Radius1;

		public float Radius2;

		public float Margin1;

		public float Margin2;

		public float Thickness;
	}

	public const float DefaultRadius1 = 1f;

	public const float DefaultRadius2 = 1f;

	public const float DefaultMargin1 = 1f;

	public const float DefaultMargin2 = 1f;

	private AdditionalData _data = new AdditionalData();

	private CapsuleRender _CapsuleRender;

	public float Thickness
	{
		get
		{
			return GetThickness();
		}
		set
		{
			SetThickness(value);
		}
	}

	public float Radius1
	{
		get
		{
			return GetRadius1();
		}
	}

	public float Radius2
	{
		get
		{
			return GetRadius2();
		}
	}

	public float Margin1
	{
		get
		{
			return GetMargin1();
		}
	}

	public float Margin2
	{
		get
		{
			return GetMargin2();
		}
	}

	public Capsule(Segment3D segment)
	{
		SetStartReference(segment.GetStart());
		SetEndReference(segment.GetEnd());
	}

	public CapsuleRender CreateUI(Transform parent)
	{
		GameObject gameObject = new GameObject(_data.Name);
		CapsuleRender capsuleRender = gameObject.AddComponent<CapsuleRender>();
		capsuleRender.set_Base(this);
		gameObject.transform.SetParent(parent, false);
		return capsuleRender;
	}

	public string get_Name()
	{
		return _data.Name;
	}

	public void set_Name(string value)
	{
		_data.Name = value;
	}

	public float GetThickness()
	{
		return _data.Thickness;
	}

	public void SetThickness(float value)
	{
		_data.Thickness = value;
	}

	public float GetRadius1()
	{
		return _data.Radius1;
	}

	public float GetRadius2()
	{
		return _data.Radius2;
	}

	public float GetMargin1()
	{
		return _data.Margin1;
	}

	public float GetMargin2()
	{
		return _data.Margin2;
	}

	public void SetRadius1(float value = 1f)
	{
		_data.Radius1 = value;
	}

	public void SetRadius2(float value = 1f)
	{
		_data.Radius2 = value;
	}

	public void SetMargin1(float value = 1f)
	{
		_data.Margin1 = value;
	}

	public void SetMargin2(float value = 1f)
	{
		_data.Margin2 = value;
	}

	private void ApplyMarginsToSegment(Segment3D segment)
	{
		segment.SetStart(GetDivisionPoint3D(_data.Margin1));
		segment.SetEnd(GetDivisionPoint3D(1f - _data.Margin2));
	}
}
