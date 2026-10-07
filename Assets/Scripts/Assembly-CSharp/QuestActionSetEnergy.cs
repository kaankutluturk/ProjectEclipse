using System;
using System.Xml;

public class QuestActionSetEnergy : QuestAction
{
	private int _value;

	public override void Parse(XmlNode EPKLCPOEELO)
	{
		base.Parse(EPKLCPOEELO);
		_value = EPKLCPOEELO.Attributes["Value"].ParseInt();
		if (_value < 0)
		{
			GameLog.Error("QuestActionSetEnergy::parse - wrong value: %i, setting to 0", _value);
			_value = 0;
		}
	}

	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		base.Execute(GFIHPBCEEOB);
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		int oGLHGFJKMCO = nKGLHEGIKKP.PowerMax;
		nKGLHEGIKKP.SetPower(Math.Min(_value, oGLHGFJKMCO));
		FinishAction();
	}
}
