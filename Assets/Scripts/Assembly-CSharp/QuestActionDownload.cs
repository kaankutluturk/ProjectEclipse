using System.IO;
using System.Xml;
using Nekki.SF2.GUI.Dialogs;

public class QuestActionDownload : QuestAction
{
	private QuestActionsSequence successSequence = new QuestActionsSequence();

	private QuestActionsSequence errorSequence = new QuestActionsSequence();

	private string name;

	private string progressBarTitle;

	private DownloadPack packInfo;

	private bool isRewriteHashes;

	private DownloadingScreen downloadingScreen;

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		name = node.Attributes["Pack"].GetStringOrDefault();
		progressBarTitle = node.Attributes["ProgressBarTitle"].GetStringOrDefault();
		packInfo = null;
		isRewriteHashes = node.Attributes["RewriteHashes"].ParseInt() > 0;
		XmlNode successNode = node["Success"];
		XmlNode ePKLCPOEELO2 = node["Error"];
		ParseSequenceWithUnlock(successNode, successSequence, OnActionComplete);
		ParseSequenceWithUnlock(ePKLCPOEELO2, errorSequence, OnActionComplete);
	}

	private void OnActionComplete(object data)
	{
		FinishAction();
		if (packInfo != null && packInfo.Reload)
		{
			CompleteQuestStage();
			GameUtils.ResetScenes();
		}
	}

	public override void ResetSequences()
	{
		base.ResetSequences();
		successSequence.Reset();
		errorSequence.Reset();
	}

	public override void Execute(QuestParameters parameters)
	{
		ResetSequences();
		base.Execute(parameters);
		ConditionExtension.CompareResult result = new ConditionExtension.CompareResult();
		QuestCondition condition = new QuestCondition();
		condition.SetParameters(parameters);
		condition.SetValue(name, result);
		packInfo = GeneralConfig.DownloadPacks.FindPack(result.resultSTR);
		if (packInfo == null)
		{
			GameLog.Error("QuestActionDownload noName: {0}", result.resultSTR);
			errorSequence.Run(parameters);
			return;
		}
		string text = NekkiMath.randomInt(1000000).ToString();
		string text2 = ((packInfo == null) ? string.Empty : packInfo.Url);
		text2 += "?";
		text2 += text;
		if (AssemblyController.GetSkipContentDownload() && (SystemProperties.IsWindowsEditorPlatform() || SystemProperties.IsWindowsPlatform() || SystemProperties.IsMacPlatform()))
		{
			Complete();
			return;
		}
		downloadingScreen = DownloadingScreen.get_Instance();
		downloadingScreen.set_TitleAlias(progressBarTitle);
		downloadingScreen.set_Progress(0f);
		FileDownloader.GetInstance().Download(text2, packInfo.Name, SF2Paths.GetBundlesPath(), OnLoadContent, OnProgressContent, packInfo.SizeBytes);
	}

	private void OnProgressContent(float progress)
	{
		if (downloadingScreen != null)
		{
			downloadingScreen.set_Progress(progress);
		}
	}

	private void OnLoadContent(bool isSuccess)
	{
		bool flag = false;
		string text = string.Format("{0}/{1}", SF2Paths.GetBundlesPath(), packInfo.Name);
		if (isSuccess && File.Exists(text))
		{
			string text2 = MD5Utils.MD5HashFile(text);
			flag = text2.Equals(packInfo.Checksum.ToUpper());
		}
		if (flag)
		{
			Complete();
		}
		else
		{
			errorSequence.Run(Parameters);
		}
		if (downloadingScreen != null)
		{
			DownloadingScreen.Destroy();
			downloadingScreen = null;
		}
	}

	private void Complete()
	{
		string version = SystemProperties.GetVersion().ToString();
		PacksController.GetInstance().AddPack(packInfo.Name, packInfo.Url, version, -1L, packInfo.Attach);
		if (isRewriteHashes)
		{
			ListSF.GetInstance().OnPacksChanged();
		}
		successSequence.Run(Parameters);
	}
}
