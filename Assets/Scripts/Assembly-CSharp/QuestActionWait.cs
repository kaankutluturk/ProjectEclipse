using System.Xml;
using Nekki.SF2.Core;

public class QuestActionWait : QuestAction
{
	private int frameCounter;

	private int frames;

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		frames = node.Attributes["Frames"].ParseInt();
	}

	public override void Execute(QuestParameters parameters)
	{
		ResetSequences();
		base.Execute(parameters);
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
