using System.Collections.Generic;
using System.Xml;

public class QuestActionAct : QuestAction
{
	private string _text = string.Empty;

	private List<KeyValuePair<string, int>> textFrames = new List<KeyValuePair<string, int>>();

	public override void Parse(XmlNode EPKLCPOEELO)
	{
		base.Parse(EPKLCPOEELO);
		_text = EPKLCPOEELO.Attributes["Text"].GetStringOrDefault(string.Empty);
		ParseTextEntries(EPKLCPOEELO);
	}

	public void ParseTextEntries(XmlNode EPKLCPOEELO)
	{
		foreach (XmlNode childNode in EPKLCPOEELO.ChildNodes)
		{
			string key = childNode.Attributes["Text"].GetStringOrDefault(string.Empty);
			int value = childNode.Attributes["Frames"].ParseInt();
			textFrames.Add(new KeyValuePair<string, int>(key, value));
		}
	}

	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		base.Execute(GFIHPBCEEOB);
		if (textFrames.Count > 0)
		{
			ShowMultipleTexts(GFIHPBCEEOB);
		}
		else
		{
			ShowSingleText(GFIHPBCEEOB);
		}
	}

	public void ShowMultipleTexts(QuestParameters GFIHPBCEEOB)
	{
		List<KeyValuePair<string, int>> KPKPFFGEFGI = new List<KeyValuePair<string, int>>();
		textFrames.ForEach((KeyValuePair<string, int> DHDMNHCIPEH) =>
		{
			string key = QuestTextResolver.ResolveText(DHDMNHCIPEH.Key, GFIHPBCEEOB);
			KPKPFFGEFGI.Add(new KeyValuePair<string, int>(key, DHDMNHCIPEH.Value));
		});
		GameUtils.ShowEnterScreen(KPKPFFGEFGI, OnEnterScreenFinished);
	}

	public void ShowSingleText(QuestParameters GFIHPBCEEOB)
	{
		string hCPNFPMHFCM = QuestTextResolver.ResolveText(_text, GFIHPBCEEOB);
		GameUtils.ShowEnterScreen(hCPNFPMHFCM, OnEnterScreenFinished);
		string text = QuestTextResolver.ResolveText(_text, GFIHPBCEEOB);
	}

	private void OnEnterScreenFinished()
	{
		GameUtils.TrackEvent("Chapter completed");
		FinishAction();
	}
}
