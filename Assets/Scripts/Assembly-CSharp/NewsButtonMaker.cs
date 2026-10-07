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

	public void Init(float nextButtonX, float buttonY, float buttonSpacing, GameObject parent, Action<object> callback, NewsDialog dialog)
	{
		_nextButtonX = nextButtonX;
		_buttonY = buttonY;
		_buttonSpacing = buttonSpacing;
		_parent = parent;
		_buttons.Clear();
		_dlg = callback;
		_dialog = dialog;
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

	public void AddButton(NewsButton newsButton)
	{
		if (_labelButtonPrefab == null)
		{
			_labelButtonPrefab = Resources.Load<LabelButton>("Prefabs/Buttons/LabelButton");
		}
		if (!(_labelButtonPrefab == null))
		{
			LabelButton labelButton = UnityEngine.Object.Instantiate(_labelButtonPrefab);
			labelButton.name = "LabelButton";
			labelButton.SetColor(newsButton.Color);
			labelButton.SetAlias(newsButton.LabelAliasName);
			labelButton.ButtonId = _id;
			labelButton.AddEventListener(2, OnClickButton);
			labelButton.transform.SetParent(_parent.transform);
			labelButton.transform.SetLocalX(_nextButtonX);
			labelButton.transform.SetLocalY(_buttonY);
			labelButton.transform.localScale = new Vector3(1f, 1f, 1f);
			_id++;
			_nextButtonX += _buttonSpacing;
			_buttons.Add(newsButton);
			_labelButtons.Add(labelButton);
		}
	}

	private void OnClickButton(object data)
	{
		int num = (int)data;
		NewsButton button = null;
		if (_buttons.Count > num)
		{
			button = _buttons[num];
		}
		if (button == null)
		{
			return;
		}
		if (button.Url != string.Empty)
		{
			OfflineServices.OpenExternalUrl(button.Url);
			return;
		}
		if (button.GoShop && _dialog != null)
		{
			_dialog.GoShopAfterClose = true;
			_dialog.RedirectShopAfterClose = button.RedirectShop;
		}
		if (button.BuyItem && _dialog != null)
		{
			_dialog.BuyItemAfterClose = true;
			_dialog.RedirectShopAfterClose = button.RedirectShop;
		}
		_dlg(data);
	}
}
