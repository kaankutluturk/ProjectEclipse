using System;
using Nekki.SF2.GUI.Scenes;
using Eclipse.UI.Settings;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Nekki.SF2.GUI.Dialogs
{
	public class SettingsAdvancedDialog : SettingsDialog
	{
		private const int GraphicsButtonX = -620;

		private const int ControllerButtonX = -620;

		private const int LocationButtonX = -620;

		private const int SoundButtonX = -620;

		private const int MusicButtonX = -620;

		private const int LegacyButtonYCenter = 0;

		private const int GraphicsButtonY = 300;

		private const int ControllerButtonY = 100;

		private const int LegacyButtonYUpper = 100;

		private const int LegacyButtonYMiddle = -100;

		private const int LegacyButtonYLower = -300;

		private const int TopStripeY = 650;

		private const int BottomStripeY = -680;

		private const int TopStripeWidth = 1077;

		private const int BottomStripeWidth = 1000;

		private const int UserIdLabelOffsetY = -100;

		private const string SoundSpriteName = "SettingsButtons.sound";

		private const string SoundOffSpriteName = "SettingsButtons.sound_off";

		private const string SoundSelectedSpriteName = "SettingsButtons.sound_selected";

		private const string SoundOffSelectedSpriteName = "SettingsButtons.sound_off_selected";

		private const string MusicSpriteName = "SettingsButtons.music";

		private const string MusicOffSpriteName = "SettingsButtons.music_off";

		private const string MusicSelectedSpriteName = "SettingsButtons.music_selected";

		private const string MusicOffSelectedSpriteName = "SettingsButtons.music_off_selected";

		private const string GraphicsSpriteName = "SettingsButtons.graphics";

		private const string GraphicsSelectedSpriteName = "SettingsButtons.graphics_selected";

		private const string LocationSpriteName = "SettingsButtons.location";

		private const string LocationSelectedSpriteName = "SettingsButtons.location_selected";

		private const string ControllerSpriteName = "SettingsButtons.controller";

		private const string ControllerSelectedSpriteName = "SettingsButtons.controller_selected";

		protected string initialQualityName;

		[SerializeField]
		private ResolutionButton btnGraphics;

		[SerializeField]
		private ResolutionButton btnController;

		[SerializeField]
		private ResolutionButton btnSoundAdv;

		[SerializeField]
		private ResolutionButton btnMusicAdv;

		[SerializeField]
		private Slider soundTrackBar;

		[SerializeField]
		private Slider musicTrackBar;

		[SerializeField]
		private LabelAlias lblGraphics;

		[SerializeField]
		private LabelAlias lblController;

		[SerializeField]
		private LabelAlias lblSoundAdv;

		[SerializeField]
		private LabelAlias lblMusicAdv;

		private DesktopRenderSettingsControls _desktopRenderSettings;

		public override void Init(object data)
		{
			initialQualityName = GraphicsController.GetEffectiveQualityCondition();
			IsPausing = false;
			Init("Settings_Advanced_Title", "Settings_Advanced", "Settings_Back", FooterType.FOOTER_CANCEL);
		}

		protected override void SetupContent()
		{
			SetupDefaultPlatformButtons();
			SetupDefaultLabels();
			GetDesktopRenderSettings().Setup();
			SetupVolumeSliders();
			SetupUserIdLabel();
			PositionContent();
			ExpandButtonTouchZones();
		}

		protected override void LayoutStripes()
		{
			base.LayoutStripes();
			PositionStripesDefault();
		}

		protected override void OnClickButton(object data)
		{
			SettingsButtonId buttonId = (SettingsButtonId)data;
			if (GetDesktopRenderSettings().HandleClick(buttonId))
			{
				return;
			}
			switch (buttonId)
			{
			case SettingsButtonId.BTN_SOUND_ADV:
				SoundController.SetSoundMuted(!SoundController.GetSoundMuted());
				if (SoundController.GetSoundMuted())
				{
					soundTrackBar.value = 0f;
				}
				else
				{
					soundTrackBar.value = SoundController.GetSoundVolume();
				}
				RefreshSoundButton();
				break;
			case SettingsButtonId.BTN_MUSIC_ADV:
				SoundController.SetMusicMuted(!SoundController.GetMusicMuted());
				if (SoundController.GetMusicMuted())
				{
					musicTrackBar.value = 0f;
				}
				else
				{
					musicTrackBar.value = SoundController.GetMusicVolume();
				}
				RefreshMusicButton();
				break;
			case SettingsButtonId.BTN_CONTROLLER:
				GraphicsController.ToggleControlSize();
				RefreshControllerLabel();
				RefreshDojoControllerLayout();
				break;
			case SettingsButtonId.BTN_GRAPHICS:
				if (GraphicsController.CycleQualityCondition())
				{
					RefreshGraphicsLabel();
				}
				break;
			case SettingsButtonId.BTN_LOCATION_RESOLUTION:
				GraphicsController.ToggleLocationResolution();
				RefreshLocationLabel();
				break;
			}
		}

		public override void OnClose(object data)
		{
			if (initialQualityName != GraphicsController.GetEffectiveQualityCondition())
			{
				ShowRestartDialog();
			}
			else
			{
				base.OnClose(data);
			}
		}

		private void CloseWithCascade()
		{
			CallEvent(2, null);
			base.OnClose((object)DialogCloseEvent.OnPopupCloseCascade);
		}

		private void Update()
		{
		}

		protected override void SetupDefaultPlatformButtons()
		{
			SetupButton(btnGraphics, "SettingsButtons.graphics", "SettingsButtons.graphics_selected", -620f, 300f, SettingsButtonId.BTN_GRAPHICS);
			SetupButton(btnController, "SettingsButtons.controller", "SettingsButtons.controller_selected", -620f, 100f, SettingsButtonId.BTN_CONTROLLER);
			RefreshMusicButton();
			RefreshSoundButton();
		}

		protected override void SetupDefaultLabels()
		{
			RefreshGraphicsLabel();
			RefreshLocationLabel();
			RefreshControllerLabel();
			SetupLabel(lblMusicAdv, "Settings_Music");
			SetupLabel(lblSoundAdv, "Settings_Sound");
		}

		protected void SetupVolumeSliders()
		{
			SetupSlider(soundTrackBar, SoundController.GetSoundVolume(), new Vector2(50f, btnSoundAdv.transform.localPosition.y), OnSoundVolumeChanged);
			SetupSlider(musicTrackBar, SoundController.GetMusicVolume(), new Vector2(50f, btnMusicAdv.transform.localPosition.y), OnMusicVolumeChanged);
		}

		protected override void PositionStripesDefault()
		{
			float num = 650f;
			float num2 = ((!lblUserId.gameObject.activeSelf) ? 0f : 37f);
			_topStripe.transform.SetLocalY(num + num2);
			_topStripe.rectTransform.sizeDelta = new Vector2(1077f, _topStripe.rectTransform.rect.height);
			num = -680f;
			_bottomStripe.transform.SetLocalY(num - num2);
			_bottomStripe.rectTransform.sizeDelta = new Vector2(1000f, _bottomStripe.rectTransform.rect.height);
		}

		protected override void SetupUserIdLabel()
		{
			base.SetupUserIdLabel();
			float y = lblUserId.transform.localPosition.y;
			y += -100f;
			lblUserId.transform.SetLocalY(y);
			lblUserId.gameObject.SetActive(false);
		}

		protected override void PositionContent()
		{
			float num = 0f;
			num += ((!lblUserId.gameObject.activeSelf) ? 0f : 37f);
			_content.transform.SetLocalY(num);
		}

		protected void OnSoundVolumeChanged(float volume)
		{
			bool flag = SoundController.GetSoundMuted();
			SoundController.SetSoundVolume(volume);
			if (flag != SoundController.GetSoundMuted())
			{
				RefreshSoundButton();
			}
		}

		protected void OnMusicVolumeChanged(float volume)
		{
			bool flag = SoundController.GetMusicMuted();
			SoundController.SetMusicVolume(volume);
			if (flag != SoundController.GetMusicMuted())
			{
				RefreshMusicButton();
			}
		}

		protected void OnRestartDialogClosed(object data)
		{
			if (data != null && ((DialogCloseEvent)Enum.Parse(typeof(DialogCloseEvent), Convert.ToString(data))/*cast due to constrained. prefix*/).Equals(DialogCloseEvent.OnPopupCloseOK))
			{
				CloseWithCascade();
				GameUtils.ResetScenes();
			}
		}

			protected void RefreshSoundButton()
			{
				bool flag = SoundController.GetSoundMuted();
				SetupButton(btnSoundAdv, (!flag) ? "SettingsButtons.sound" : "SettingsButtons.sound_off", (!flag) ? "SettingsButtons.sound_selected" : "SettingsButtons.sound_off_selected", -620f, -375f, SettingsButtonId.BTN_SOUND_ADV);
			}

			protected void RefreshMusicButton()
			{
				bool flag = SoundController.GetMusicMuted();
				SetupButton(btnMusicAdv, (!flag) ? "SettingsButtons.music" : "SettingsButtons.music_off", (!flag) ? "SettingsButtons.music_selected" : "SettingsButtons.music_off_selected", -620f, -235f, SettingsButtonId.BTN_MUSIC_ADV);
			}

		protected void RefreshControllerLabel()
		{
			SetupLabel(lblController, string.Empty);
			string text = LocalizationManager.GetString("Settings_Controller_Scale") + LocalizationManager.GetString(GetControllerSizeAlias());
			lblController.set_text(text);
		}

		protected void RefreshGraphicsLabel()
		{
			SetupLabel(lblGraphics, string.Empty);
			lblGraphics.set_text(LocalizationManager.GetString("Settings_Graphics_Quality") + LocalizationManager.GetString(GetGraphicsQualityAlias()));
		}

		protected void RefreshLocationLabel()
		{
		}

		protected void ExpandButtonTouchZones()
		{
			float touchZoneSize = 1500f;
			ChangeButtonTouchZone(btnGraphics, touchZoneSize);
			ChangeButtonTouchZone(GetDesktopRenderSettings().FrameRateButton, touchZoneSize);
			ChangeButtonTouchZone(GetDesktopRenderSettings().MotionBlurButton, touchZoneSize);
			ChangeButtonTouchZone(btnController, touchZoneSize);
		}

		private DesktopRenderSettingsControls GetDesktopRenderSettings()
		{
			if (_desktopRenderSettings == null)
			{
				_desktopRenderSettings = new DesktopRenderSettingsControls(
					(_content == null) ? null : _content.transform,
					btnGraphics, lblGraphics, btnController, btnMusicAdv, btnSoundAdv,
					SetupButton, RefreshMusicButton, RefreshSoundButton);
			}
			return _desktopRenderSettings;
		}

		protected void ShowRestartDialog()
		{
			DialogsOpener.OpenSimpleDialog("dlgAlertTitle", "dlgSettingsRestart", "dlgServiceRestart", "dlgServiceBtnLater", OnRestartDialogClosed, LabelButton.ButtonColor.BUTTON_WHITE, LabelButton.ButtonColor.BUTTON_DARK, false, false, string.Empty);
		}

		protected string GetControllerSizeAlias()
		{
			return (!GraphicsController.LargeControlsEnabled()) ? "Settings_Controller_Small" : "Settings_Controller_Large";
		}

		protected string GetGraphicsQualityAlias()
		{
			string qualityName = GraphicsController.GetEffectiveQualityCondition();
			QualityOption.QualityLevel qualityLevel = QualityOption.ParseQualityLevel(qualityName);
			string empty = string.Empty;
			switch (qualityLevel)
			{
			case QualityOption.QualityLevel.QUALITY_LOW:
				return "Settings_Graphics_Low";
			case QualityOption.QualityLevel.QUALITY_MEDIUM:
				return "Settings_Graphics_Medium";
			case QualityOption.QualityLevel.QUALITY_HIGH:
				return "Settings_Graphics_High";
			default:
				return string.Empty;
			}
		}

		protected string GetResolutionPathAlias()
		{
			string empty = string.Empty;
			switch (GraphicsController.GetLocationResolution())
			{
			case SystemProperties.PathType.PATH_SMALL:
				return "Settings_Graphics_Low";
			case SystemProperties.PathType.PATH_BIG:
				return "Settings_Graphics_High";
			default:
				return string.Empty;
			}
		}

		protected void SetupSlider(Slider slider, float value, Vector2 MGMMDGFPBLP, UnityAction<float> onValueChanged)
		{
			slider.gameObject.SetActive(true);
			slider.onValueChanged.AddListener(onValueChanged);
			slider.minValue = 0f;
			slider.maxValue = 1f;
			slider.value = value;
			slider.transform.SetLocalX(MGMMDGFPBLP.x);
			slider.transform.SetLocalY(MGMMDGFPBLP.y);
		}

		protected void ChangeButtonTouchZone(Button button, LabelAlias label)
		{
		}

		protected void ChangeButtonTouchZone(Button button, float size)
		{
		}

		protected void RefreshDojoControllerLayout()
		{
			DojoScene current = Scene<DojoScene>.get_Current();
			if (current != null)
			{
				current.fight.RefreshControllerLayout();
			}
		}
	}
}
