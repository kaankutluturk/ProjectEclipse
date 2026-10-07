using System.Xml;
using Eclipse.Rendering;

public class ActionEffect : ActionAnimation
{
	private string _Name;

	private string _Sequence;

	private float _Scale;

	private float _ScaleX;

	private float _ScaleY;

	private float _StartRotation;

	private int _Priority;

	private float _TimeScale;

	private bool _Looped;

	private bool _OnBackground;

	private DistancePoint _Position = new DistancePoint();

	private bool _IsFollowObject;

	private int _StopFollowFrame;

	private DistanceVector _Vector = new DistanceVector();

	public EffectAttachment Attachment { get; private set; }

	public string FileName
	{
		get
		{
			return GetSequence();
		}
	}

	public float Scale
	{
		get
		{
			return GetScale();
		}
	}

	public float TimeScale
	{
		get
		{
			return GetTimeScale();
		}
	}

	public bool IsLooped
	{
		get
		{
			return GetIsLooped();
		}
	}

	public bool IsOnBackground
	{
		get
		{
			return GetIsOnBackground();
		}
	}

	public DistancePoint Position
	{
		get
		{
			return GetPosition();
		}
	}

	public bool FollowObject
	{
		get
		{
			return GetIsFollowObject();
		}
		set
		{
			set_IsFollowObject(value);
		}
	}

	public DistanceVector DistanceInfo
	{
		get
		{
			return GetVector();
		}
	}

	public ActionEffect(XmlNode node)
		: base(ActionType.EFFECT)
	{
		Parse(node);
	}

	public string get_Name()
	{
		return _Name;
	}

	public string GetSequence()
	{
		return _Sequence;
	}

	public float GetScale()
	{
		return _Scale;
	}

	public float GetScaleX()
	{
		return _ScaleX;
	}

	public float GetScaleY()
	{
		return _ScaleY;
	}

	public float GetStartRotation()
	{
		return _StartRotation;
	}

	public int GetPriority()
	{
		return _Priority;
	}

	public float GetTimeScale()
	{
		return _TimeScale;
	}

	public bool GetIsLooped()
	{
		return _Looped;
	}

	public bool GetIsOnBackground()
	{
		return _OnBackground;
	}

	public DistancePoint GetPosition()
	{
		return _Position;
	}

	public bool GetIsFollowObject()
	{
		return _IsFollowObject;
	}

	public void set_IsFollowObject(bool value)
	{
		_IsFollowObject = value;
	}

	public DistanceVector GetVector()
	{
		return _Vector;
	}

	public override void Visit(Model model)
	{
		model.StartAction(this);
	}

	public void UpdateNodes(ModelObject modelObject, bool isPlayer, ModelNode pivotNode, bool isChild, ModelObject childOwner = null)
	{
		_Position.UpdateNode(modelObject, isPlayer, pivotNode, isChild, childOwner);
		_Vector.UpdateNodes(modelObject, isPlayer, pivotNode, isChild, childOwner);
	}

	public void ResetNodes()
	{
		_Position.ClearChildPoints();
		_Vector.ClearChildPoints();
	}

	protected override void Parse(XmlNode node)
	{
		base.Parse(node);
		_Name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		_Sequence = node.Attributes["Sequence"].GetStringOrDefault(string.Empty);
		_Scale = node.Attributes["Scale"].ParseFloat(1f);
		_ScaleX = node.Attributes["ScaleX"].ParseFloat(_Scale);
		_ScaleY = node.Attributes["ScaleY"].ParseFloat(_Scale);
		_StartRotation = node.Attributes["StartRotation"].ParseFloat();
		_TimeScale = node.Attributes["TimeScale"].ParseFloat(1f);
		_Looped = node.Attributes["Looped"].ParseBool();
		_OnBackground = node.Attributes["OnBackground"].ParseBool();
		_Priority = node.Attributes["Priority"].ParseInt(_OnBackground ? -10 : 0);
		XmlNode xmlNode = node["Position"];
		if (xmlNode != null)
		{
			_Position.Create(xmlNode);
			_IsFollowObject = xmlNode.Attributes["Follow"].ParseBool();
			_StopFollowFrame = xmlNode.Attributes["StopFollowframe"].ParseInt(-1);
		}
		else
		{
			// Newer screen-space and model-owned effects intentionally omit Position.
			// DistancePoint's default OBJECT_NULL is the legacy representation of
			// that behavior, so this is valid data rather than a parser error.
		}
		XmlNode xmlNode2 = node["Vector"];
		if (xmlNode2 != null)
		{
			_Vector.Parse(xmlNode2);
		}
		XmlNode attachment = node["Attach"];
		if (attachment != null)
		{
			Attachment = new EffectAttachment(attachment);
			_IsFollowObject = true;
		}
	}
}
