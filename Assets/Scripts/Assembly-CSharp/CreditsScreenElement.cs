using Nekki.SF2.GUI;
using UnityEngine;
using UnityEngine.UI;

public class CreditsScreenElement : SFMonoBehaviour<object>
{
	[SerializeField]
	private LabelAlias lblRole;

	[SerializeField]
	private LabelAlias lblNames;

	public void Init(string role, string names)
	{
		lblRole.set_text(role);
		lblNames.set_text(names);
		lblRole.UpdateLabelFontSize();
		lblNames.UpdateLabelFontSize();
		int fontSize = lblRole.fontSize;
		int fontSize2 = lblNames.fontSize;
		lblNames.transform.SetLocalY(lblRole.transform.localPosition.y - (float)(fontSize - fontSize2));
		GetComponent<LayoutElement>().minHeight = (float)Mathf.Abs(fontSize - fontSize2) + lblNames.preferredHeight;
	}
}
