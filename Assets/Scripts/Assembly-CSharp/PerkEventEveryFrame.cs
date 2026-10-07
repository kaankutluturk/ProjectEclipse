using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

public class PerkEventEveryFrame : PerkEvent
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int _step;

	public int StepFrames
	{
		get
		{
			return GetStep();
		}
		protected set
		{
			set_Step(value);
		}
	}

	public PerkEventEveryFrame()
	{
	}

	public PerkEventEveryFrame(PerkEventEveryFrame source)
		: base(source)
	{
		set_Step(source.GetStep());
	}

	public int GetStep()
	{
		return _step;
	}

	protected void set_Step(int value)
	{
		_step = value;
	}

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		set_Step(node.Attributes["Step"].ParseInt());
	}

	public override bool IsEqual(EventStruct eventData)
	{
		if (!base.IsEqual(eventData) || eventData == null || eventData.Info == null)
		{
			return false;
		}
		if (GetStep() != 0)
		{
			Dictionary<string, object> dictionary = (Dictionary<string, object>)eventData.Info;
			if (dictionary != null)
			{
				long num = 0L;
				if (dictionary.ContainsKey("StepFrame"))
				{
					num = Convert.ToInt64(dictionary["StepFrame"]);
				}
				return 0 == num % GetStep();
			}
		}
		return true;
	}
}
