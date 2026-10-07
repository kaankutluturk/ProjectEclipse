using System.Xml;
using Nekki.SF2.Core;

public class QuestActionWait : QuestAction
{
	private int frameCounter;

	private int frames;

	public override void Parse(XmlNode EPKLCPOEELO)
	{
		base.Parse(EPKLCPOEELO);
		frames = EPKLCPOEELO.Attributes["Frames"].ParseInt();
	}

	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		ResetSequences();
		base.Execute(GFIHPBCEEOB);
		ApplicationController.add_OnUpdate(OnEveryFrame);
	}

	private void OnEveryFrame()
	{
		frameCounter++;
		if (frameCounter >= frames)
		{
			Stop();
		}
	}

	private void Stop()
	{
		ApplicationController.remove_OnUpdate(OnEveryFrame);
		FinishAction();
	}

	public override void ResetSequences()
	{
		base.ResetSequences();
		frameCounter = 0;
	}
}
