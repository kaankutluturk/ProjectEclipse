using System;
using System.Collections.Generic;
using Nekki.SF2.GUI.Dialogs;
using UnityEngine;

public class NewsButtonMaker : global::EventDispatcher<object>
{
	private float _nextButtonX;

	private float _buttonY;

	private float _buttonSpacing;

	private int _id;

	private GameObject _parent;

	private List<NewsButton> _buttons = new List<NewsButton>();

	private List<LabelButton> _labelButtons = new List<LabelButton>();

	private Action<object> _dlg;

	private NewsDialog _dialog;

	private LabelButton _labelButtonPrefab;

	public void Init(float FNDOOJNDJDC, float GBCONNBABLL, float KDGOIIIHPCL, GameObject KPAICOOKACB, Action<object> ODDEOFKLIAG, NewsDialog MDOHPMBJFIL)
	{
		_nextButtonX = FNDOOJNDJDC;
		_buttonY = GBCONNBABLL;
		_buttonSpacing = KDGOIIIHPCL;
		_parent = KPAICOOKACB;
		_buttons.Clear();
		_dlg = ODDEOFKLIAG;
		_dialog = MDOHPMBJFIL;
		_id = 0;
	}

	public void ClearButtons()
	{
		foreach (LabelButton item in _labelButtons)
		{
			item.RemoveAllEventListener();
			UnityEngine.Object.Destroy(item.gameObject);
		}
		_labelButtons.Clear();
	}

	public void AddButton(NewsButton HJNAHNICGMH)
	{
		if (_labelButtonPrefab == null)
		{
			_labelButtonPrefab = Resources.Load<LabelButton>("Prefabs/Buttons/LabelButton");
		}
		if (!(_labelButtonPrefab == null))
		{
			LabelButton labelButton = UnityEngine.Object.Instantiate(_labelButtonPrefab);
			labelButton.name = "LabelButton";
			labelButton.SetColor(HJNAHNICGMH.Color);
			labelButton.SetAlias(HJNAHNICGMH.LabelAliasName);
			labelButton.ButtonId = _id;
			labelButton.AddEventListener(2, OnClickButton);
			labelButton.transform.SetParent(_parent.transform);
			labelButton.transform.SetLocalX(_nextButtonX);
			labelButton.transform.SetLocalY(_buttonY);
			labelButton.transform.localScale = new Vector3(1f, 1f, 1f);
			_id++;
			_nextButtonX += _buttonSpacing;
			_buttons.Add(HJNAHNICGMH);
			_labelButtons.Add(labelButton);
		}
	}

	private void OnClickButton(object data)
	{
		int num = (int)data;
		NewsButton fBKMFDJBJIB = null;
		if (_buttons.Count > num)
		{
			fBKMFDJBJIB = _buttons[num];
		}
		if (fBKMFDJBJIB == null)
		{
			return;
		}
		if (fBKMFDJBJIB.Url != string.Empty)
		{
			OfflineServices.OpenExternalUrl(fBKMFDJBJIB.Url);
			return;
		}
		if (fBKMFDJBJIB.GoShop && _dialog != null)
		{
			_dialog.GoShopAfterClose = true;
			_dialog.RedirectShopAfterClose = fBKMFDJBJIB.RedirectShop;
		}
		if (fBKMFDJBJIB.BuyItem && _dialog != null)
		{
			_dialog.BuyItemAfterClose = true;
			_dialog.RedirectShopAfterClose = fBKMFDJBJIB.RedirectShop;
		}
		_dlg(data);
	}
}
