using System;
using System.Collections;
using System.Collections.Generic;
using Nekki.SF2.GUI.Fight;
using Nekki.SF2.GUI.Menu;
using UnityEngine;

namespace Nekki.SF2.GUI.Dialogs
{
	public class NotificationsGame : SFMonoBehaviour<object>, BackKeyController
	{
		public enum NotificationsGameEvent
		{
			ON_CLOSE = 0
		}

		public const float SCROLL_SECONDS = 0.5f;

		private static bool isOpen;

		private bool isDismissable;

		private bool reopenAfterClose;

		private bool buttonClicked;

		[SerializeField]
		private MenuScroll _scroll;

		[SerializeField]
		private ResolutionImageAvatar _image;

		[SerializeField]
		private LabelAlias _label;

		[SerializeField]
		private LabelButton _button;

		private string imagePath = string.Empty;

		private string messageText = string.Empty;

		private string buttonAlias = string.Empty;

		private LabelButton.ButtonColor buttonColor = LabelButton.ButtonColor.BUTTON_WHITE;

		private float dismissDelay;

		private Action<object> callback;

		private IEnumerator dismissDelayRoutine;

		private static NotificationsGame _instance;

		public static bool IsNotificationOpen
		{
			get
			{
				return get_IsOpen();
			}
		}

		public static NotificationsGame SharedInstance
		{
			get
			{
				return get_Instance();
			}
		}

		public static bool get_IsOpen()
		{
			return isOpen;
		}

		public static NotificationsGame get_Instance()
		{
			if (_instance == null)
			{
				GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Dialogs/NotificationGame"));
				gameObject.name = "[NotificationsGame]";
				_instance = gameObject.GetComponent<NotificationsGame>();
				_instance.Init();
				UnityEngine.Object.DontDestroyOnLoad(gameObject);
			}
			return _instance;
		}

		public void Init()
		{
			InitScroll();
		}

		public static void CloseNotifications()
		{
			if (_instance != null)
			{
				_instance.Close();
			}
		}

		private void OnDestroy()
		{
			_instance = null;
		}

		public void OpenNotification(string image, List<StoryDialogContent> contents, Action<object> onClose, string alias, LabelButton.ButtonColor color, float delay)
		{
			if (CanShowNotification())
			{
				imagePath = image;
				BuildMessageText(contents);
				buttonAlias = alias;
				buttonColor = color;
				callback = onClose;
				dismissDelay = delay;
				ShowNotification();
				if (dismissDelayRoutine != null)
				{
					CoroutineManager.get_Current().StopRoutine(dismissDelayRoutine);
				}
				dismissDelayRoutine = DismissDelayRoutine();
				CoroutineManager.get_Current().StartRoutine(dismissDelayRoutine);
			}
		}

		private IEnumerator DismissDelayRoutine()
		{
			isDismissable = false;
			yield return new WaitForSeconds(dismissDelay);
			isDismissable = true;
		}

		private void Update()
		{
			if ((Eclipse.Input.EclipseInput.GetMouseButtonDown(0) || Eclipse.Input.EclipseInput.touchCount > 0) && isDismissable)
			{
				_scroll.OnBackgroundClick();
			}
		}

		public void OnBackKeyClicked(object data)
		{
			if (isDismissable)
			{
				Close();
			}
		}

		private void BuildMessageText(List<StoryDialogContent> contents)
		{
			messageText = string.Empty;
			for (int i = 0; i < contents.Count; i++)
			{
				messageText += LocalizationManager.GetString(contents[i].Text);
				if (i + 1 < contents.Count)
				{
					messageText += "\n";
				}
			}
		}

		private void ShowNotification()
		{
			if (isOpen)
			{
				CloseAndReopen();
				return;
			}
			_scroll.SetOutsideTouchProperties(false);
			string[] array = imagePath.Split('|');
			string[] array2 = imagePath.Split('/');
			string[] array3 = array2[array2.Length - 1].Split('.');
			_image.set_TexturePath(SF2Paths.GetUsersUiPath());
			_image.set_SpriteName(array3[0]);
			_label.set_text(messageText);
			_button.gameObject.SetActive(buttonAlias == string.Empty);
			_button.interactable = buttonAlias == string.Empty;
			if (buttonAlias == string.Empty)
			{
				_button.SetColor(buttonColor);
				_button.SetAlias(buttonAlias);
				_button.AddEventListener(2, OnButtonClick);
			}
			ExpandNotification();
		}

		private void OnScrollStateChanged(object data)
		{
			isOpen = (bool)data;
			if (!isOpen)
			{
				_scroll.gameObject.SetActive(false);
				InvokeCallback(buttonClicked ? 1 : 0);
				buttonClicked = false;
				_scroll.SetOutsideTouchProperties(false);
			}
			else
			{
				_scroll.SetOutsideTouchProperties(true);
			}
		}

		private void OnScrollAnimationFinished(object data)
		{
			CallEvent(0, 0);
			if (reopenAfterClose)
			{
				reopenAfterClose = false;
				ShowNotification();
			}
		}

		private void OnButtonClick(object data)
		{
			if (isOpen)
			{
				buttonClicked = true;
				Close();
			}
		}

		private void InitScroll()
		{
			_scroll.Init(MenuScroll.ScrollOrientation.Horizontal);
			_scroll.SetOutsideTouchProperties(false);
			_scroll.Collapse(0f);
			_scroll.AddEventListener(2, OnScrollStateChanged);
			_scroll.AddEventListener(1, OnScrollAnimationFinished);
			_scroll.GetButton().interactable = false;
			_scroll.gameObject.SetActive(false);
		}

		private void ExpandNotification()
		{
			BackKeyManager.get_Instance().AddBackKeyController(this);
			_scroll.gameObject.SetActive(true);
			_scroll.Expand(0.5f);
			Eclipse.UI.NotificationReveal.Play(base.gameObject, _image != null ? _image.gameObject : null, _label != null ? _label.gameObject : null);
		}

		private void Close()
		{
			BackKeyManager.get_Instance().RemoveBackKeyController(this);
			if (_button != null)
			{
				_button.RemoveEventListener(2, OnButtonClick);
			}
			_scroll.Collapse(0.5f);
		}

		private void CloseAndReopen()
		{
			reopenAfterClose = true;
			Close();
		}

		private void InvokeCallback(int value)
		{
			callback(value);
		}

		private void OnBackgroundClicked(object data)
		{
			Close();
		}

		private bool CanShowNotification()
		{
			switch (Module.GetInstance().GetCurrentScreenType())
			{
			case ScreenType.ModulePreloader:
			case ScreenType.ModuleCreditsScreen:
				return false;
			case ScreenType.ModuleFight:
			{
				FightScene current = Scene<FightScene>.get_Current();
				if (current != null && current.Fight != null && current.Fight.GetFightDefinition() != null && current.Fight.GetFightDefinition().get_Type() != BattleType.FightNone)
				{
					return false;
				}
				break;
			}
			}
			return true;
		}
	}
}
