using System.Collections.Generic;
using System.Xml;

public class QuestActionAct : QuestAction
{
	private string _text = string.Empty;

	private List<KeyValuePair<string, int>> textFrames = new List<KeyValuePair<string, int>>();

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		_text = node.Attributes["Text"].GetStringOrDefault(string.Empty);
		ParseTextEntries(node);
	}

	public void ParseTextEntries(XmlNode node)
	{
		foreach (XmlNode childNode in node.ChildNodes)
		{
			string key = childNode.Attributes["Text"].GetStringOrDefault(string.Empty);
			int value = childNode.Attributes["Frames"].ParseInt();
			textFrames.Add(new KeyValuePair<string, int>(key, value));
		}
	}

	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		if (textFrames.Count > 0)
		{
			ShowMultipleTexts(parameters);
		}
		else
		{
			ShowSingleText(parameters);
		}
	}

	public void ShowMultipleTexts(QuestParameters parameters)
	{
		List<KeyValuePair<string, int>> textEntries = new List<KeyValuePair<string, int>>();
		textFrames.ForEach((KeyValuePair<string, int> entry) =>
		{
			string key = QuestTextResolver.ResolveText(entry.Key, parameters);
			textEntries.Add(new KeyValuePair<string, int>(key, entry.Value));
		});
		GameUtils.ShowEnterScreen(textEntries, OnEnterScreenFinished);
	}

	public void ShowSingleText(QuestParameters parameters)
	{
		string resolvedText = QuestTextResolver.ResolveText(_text, parameters);
		GameUtils.ShowEnterScreen(resolvedText, OnEnterScreenFinished);
		string text = QuestTextResolver.ResolveText(_text, parameters);
	}

	private void OnEnterScreenFinished()
	{
		GameUtils.TrackEvent("Chapter completed");
		FinishAction();
	}
}
