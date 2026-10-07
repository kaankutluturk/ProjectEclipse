using System;
using System.Xml;
using UnityEngine;

namespace Nekki.SF2.GUI.Dialogs
{
	public class SettingsDialog : BaseDialog
	{
		public enum SettingsButtonId
		{
			BTN_MUSIC = 0,
			BTN_SOUND = 1,
			BTN_CREDITS = 2,
			BTN_FACEBOOK = 3,
			BTN_LANGUAGE = 4,
			BTN_GAMECENTER = 5,
			BTN_SUPPORT = 6,
			BTN_ITUNES = 7,
			BTN_RESOLUTION_HIGHER = 8,
			BTN_RESOLUTION_LOWER = 9,
				BTN_RESOLUTION_SET = 10,
				BTN_GRAPHICS = 11,
				BTN_LOCATION_RESOLUTION = 12,
				BTN_CONTROLLER = 13,
				BTN_MUSIC_ADV = 14,
				BTN_SOUND_ADV = 15,
				BTN_RENDER_INTERPOLATION = 16,
				BTN_MAX_FRAME_RATE = 17,
				BTN_MOTION_BLUR = 18
			}

		protected const int ContentHeight = 1540;

		public const float USER_ID_Y = -516f;

		public const float USER_ID_Y_ANDROID = -356f;

		protected const float UserIdOffset = 37f;

		public const float STRIPES_TOP_OFFSET_ANDROID = 500f;

		public const float STRIPES_BOTTOM_OFFSET_ANDROID = -540f;

		private const float TopStripeY = 600f;

		private const float BottomStripeY = -600f;

		private const float TopStripeWidth = 1078f;

		private const float BottomStripeWidth = 1000.99994f;

		private const int ContentY = 100;

		private const int HeaderOffsetY = 110;

		private const int ButtonsBottomOffsetY = -70;

		private const int LanguageApplyDelayFrames = 60;

		private const int LabelFontSize = 101;

		private const int MusicButtonX = -670;

		private const int SoundButtonX = -670;

		private const int CreditsButtonX = -670;

		private const int SupportButtonX = -670;

		private const int LanguageButtonX = 0;

		private const int GameCenterButtonX = 0;

		private const int GameCenterButtonY = 0;

		private const int SupportButtonOffsetY = 0;

		private const int ButtonOriginOffset = 0;

		private const int MusicButtonY = 200;

		private const int SoundButtonY = 0;

		private const int CreditsButtonY = -200;

		private const int SupportButtonY = -400;

		private const int LanguageButtonY = 200;

		private const int GameCenterButtonOffsetY = 0;

		private const int ItunesButtonY = -196;

		private const int SupportButtonYTall = -400;

		private const int CreditsButtonYWindows = -620;

		private const int SupportButtonXCenter = 50;

		private const int SupportButtonXAmazon = 50;

		private const int SupportButtonXWindows = -200;

		private const int LanguageButtonYDefault = 200;

		private const int SupportButtonYCompact = 0;

		private const int SupportButtonYWindowsAlt = -200;

		private const int SupportButtonYWindows = -400;

		private const int SupportButtonOriginY = 0;

		private const int ResolutionButtonY = -300;

		private const string MusicSprite = "SettingsButtons.music";

		private const string MusicOffSprite = "SettingsButtons.music_off";

		private const string MusicSelectedSprite = "SettingsButtons.music_selected";

		private const string MusicOffSelectedSprite = "SettingsButtons.music_off_selected";

		private const string SoundSprite = "SettingsButtons.sound";

		private const string SoundOffSprite = "SettingsButtons.sound_off";

		private const string SoundSelectedSprite = "SettingsButtons.sound_selected";

		private const string SoundOffSelectedSprite = "SettingsButtons.sound_off_selected";

		private const string CreditsSprite = "SettingsButtons.credits";

		private const string CreditsSelectedSprite = "SettingsButtons.credits_selected";

		private const string FacebookSprite = "SettingsButtons.facebook";

		private const string FacebookOffSprite = "SettingsButtons.facebook_off";

		private const string FacebookSelectedSprite = "SettingsButtons.facebook_selected";

		private const string FacebookOffSelectedSprite = "SettingsButtons.facebook_off_selected";

		private const string FacebookDisabledSprite = "SettingsButtons.facebook_disable";

		private const string GameCenterSprite = "SettingsButtons.gamecenter";

		private const string GameCenterOffSprite = "SettingsButtons.gamecenter_off";

		private const string GameCenterDisabledSprite = "SettingsButtons.gamecenter_disable";

		private const string GooglePlaySprite = "SettingsButtons.googleplay_controller";

		private const string GooglePlayOffSprite = "SettingsButtons.googleplay_off_controller";

		private const string GooglePlayDisabledSprite = "SettingsButtons.googleplay_disable_controller";

		private const string WindowsSprite = "SettingsButtons.windows";

		private const string WindowsOffSprite = "SettingsButtons.windows_off";

		private const string SupportSprite = "SettingsButtons.support";

		private const string SupportSelectedSprite = "SettingsButtons.support_selected";

		private const string ItunesSprite = "SettingsButtons.itunes";

		private const string GooglePlayMusicSprite = "SettingsButtons.googleplay_music";

		private const string AmazonMp3Sprite = "SettingsButtons.amazon_mp3";

		[SerializeField]
		private ResolutionButton btnMusic;

		[SerializeField]
		private ResolutionButton btnSound;

		[SerializeField]
		private ResolutionButton btnCredits;

		[SerializeField]
		private ResolutionButton btnSupport;

		[SerializeField]
		private ResolutionButton btnLanguage;

		[SerializeField]
		private ResolutionButton btnGameCenter;

		[SerializeField]
		private ResolutionButton btnItunes;

		[SerializeField]
		private Sprite resolutionChanger;

		[SerializeField]
		private LabelAlias lblMusic;

		[SerializeField]
		private LabelAlias lblSound;

		[SerializeField]
		private LabelAlias lblCredits;

		[SerializeField]
		private LabelAlias lblSupport;

		[SerializeField]
		private LabelAlias lblLanguage;

		[SerializeField]
		private LabelAlias lblGameCenter;

		[SerializeField]
		private LabelAlias lblItunes;

		[SerializeField]
		protected LabelAlias lblUserId;

		[SerializeField]
		private LabelAlias lblResolution;

		[SerializeField]
		private LabelAlias lblResolutionValue;

		protected bool GameCenterSignedIn;

		protected int selectedResolutionIndex = -1;

		protected bool languageChangePending;

		protected int languageChangeFrames;

		protected LocalizationManager.Language pendingLanguage;

		private BaseDialog childDialog;

		public override void Init(object data)
		{
			GameCenterSignedIn = GameCenterController.GetIsAuthenticated();
			if (AssemblyController.GetMarket().GetIsAmazonMarket() || AssemblyController.GetMarket().GetIsChinaMarket() || AssemblyController.GetMarket().GetIsAndroidTvMarket())
			{
				base.Init("Settings_Title", "Settings_Advanced", "Settings_Back", FooterType.FOOTER_CANCEL);
			}
			else
			{
				base.Init("Settings_Title", "Settings_Advanced", "Settings_Back", FooterType.FOOTER_BOTH);
			}
			GameCenterAbstract.OnAuthenticate = (Action<bool>)Delegate.Combine(GameCenterAbstract.OnAuthenticate, new Action<bool>(OnAuthenticate));
			// Credits/support were mobile storefront/service entries in this build.
			// Their labels are absent in the migrated desktop localization, so do
			// not expose two anonymous buttons.
			if (btnCredits != null) btnCredits.gameObject.SetActive(false);
			if (lblCredits != null) lblCredits.gameObject.SetActive(false);
			if (btnSupport != null) btnSupport.gameObject.SetActive(false);
			if (lblSupport != null) lblSupport.gameObject.SetActive(false);
			if (btnGameCenter != null) btnGameCenter.gameObject.SetActive(false);
			if (lblGameCenter != null) lblGameCenter.gameObject.SetActive(false);
			if (btnItunes != null) btnItunes.gameObject.SetActive(false);
			if (lblItunes != null) lblItunes.gameObject.SetActive(false);
		}

		private void OnDestroy()
		{
			GameCenterAbstract.OnAuthenticate = (Action<bool>)Delegate.Remove(GameCenterAbstract.OnAuthenticate, new Action<bool>(OnAuthenticate));
		}

		public virtual void UpdateFacebookBtnImg()
		{
			if (!AssemblyController.GetMarket().GetIsChinaMarket() && !AssemblyController.GetMarket().GetIsAmazonMarket() && AssemblyController.GetMarket().GetIsSteamMarket())
			{
			}
		}

		public void UpdateLabels()
		{
			SetupLabel(lblMusic, "Settings_Music");
			SetupLabel(lblSound, "Settings_Sound");
			SetupLabel(lblCredits, "Settings_Credits");
			SetupLabel(lblLanguage, LocalizationManager.CurrentLanguage.Alias);
			if (AssemblyController.GetMarket().GetIsAmazonMarket())
			{
				HideUserIdLabel();
			}
			else if (AssemblyController.GetMarket().GetIsAmazonMobileMarket())
			{
				SetupAmazonLabels();
			}
			else if (AssemblyController.GetMarket().GetIsChinaMarket())
			{
				SetupSupportAndGameServiceLabels();
			}
			else if (AssemblyController.GetMarket().GetIsSteamMarket())
			{
				SetupResolutionLabels();
			}
			else
			{
				SetupDefaultLabels();
			}
		}

		public override void Close(object data)
		{
			OnClose(DialogCloseEvent.OnPopupClose);
		}

		public override void OnClose(object data)
		{
			if (languageChangePending)
			{
				ApplyLanguageChange();
			}
			if (childDialog != null)
			{
				childDialog.RemoveAllEventListener();
				childDialog = null;
			}
			base.OnClose(data);
		}

		protected override void SetupContent()
		{
			bool flag = SoundController.GetMusicMuted();
			SetupButton(btnMusic, (!flag) ? "SettingsButtons.music" : "SettingsButtons.music_off", (!flag) ? "SettingsButtons.music_selected" : "SettingsButtons.music_off_selected", -670f, 200f, SettingsButtonId.BTN_MUSIC);
			bool flag2 = SoundController.GetSoundMuted();
			SetupButton(btnSound, (!flag2) ? "SettingsButtons.sound" : "SettingsButtons.sound_off", (!flag2) ? "SettingsButtons.sound_selected" : "SettingsButtons.sound_off_selected", -670f, 0f, SettingsButtonId.BTN_SOUND);
			SetupButton(btnCredits, "SettingsButtons.credits", "SettingsButtons.credits_selected", -670f, -200f, SettingsButtonId.BTN_CREDITS);
			string mMBELNEBNBM = LocalizationManager.CurrentLanguage.IconSprite;
			string oKGJAMBPDGO = LocalizationManager.CurrentLanguage.SelectedIconSprite;
			oKGJAMBPDGO = ((!(oKGJAMBPDGO == string.Empty)) ? oKGJAMBPDGO : mMBELNEBNBM);
			SetupButton(btnLanguage, mMBELNEBNBM, oKGJAMBPDGO, 0f, 200f, SettingsButtonId.BTN_LANGUAGE);
			Eclipse.UI.LanguageGlobeIcon.Apply(btnLanguage);
			SetupLabel(lblLanguage, LocalizationManager.CurrentLanguage.Alias);
			if (AssemblyController.GetMarket().GetIsAmazonMarket())
			{
				btnMusic.transform.SetLocalY(0f);
				btnSound.transform.SetLocalY(-200f);
				if (btnCredits != null) btnCredits.transform.SetLocalX(0f);
				btnLanguage.transform.SetLocalY(0f);
			}
			else if (AssemblyController.GetMarket().GetIsAmazonMobileMarket())
			{
				float bAINMLLIKOL = ((!AssemblyController.GetMarket().GetIsChinaMarket()) ? 200 : 200);
				btnLanguage.transform.SetLocalY(bAINMLLIKOL);
				SetupAmazonButtons();
			}
			else if (AssemblyController.GetMarket().GetIsChinaMarket())
			{
				float bAINMLLIKOL2 = ((!AssemblyController.GetMarket().GetIsChinaMarket()) ? 200 : 200);
				btnLanguage.transform.SetLocalY(bAINMLLIKOL2);
				SetupSupportButtonCompact();
			}
			else if (AssemblyController.GetMarket().GetIsSteamMarket())
			{
				SetupSupportButtonOnly();
			}
			else if (SystemProperties.IsWp8Platform())
			{
				if (btnCredits != null) btnCredits.transform.SetLocalX(-620f);
				SetupWindowsButtons();
			}
			else
			{
				float bAINMLLIKOL3 = ((!AssemblyController.GetMarket().GetIsChinaMarket()) ? 200 : 200);
				btnLanguage.transform.SetLocalY(bAINMLLIKOL3);
				SetupDefaultPlatformButtons();
			}
			UpdateLabels();
			PositionContent();
		}

		protected override void LayoutStripes()
		{
			base.LayoutStripes();
			_topStripe.rectTransform.sizeDelta = new Vector2(1078f, _topStripe.rectTransform.rect.height);
			_bottomStripe.rectTransform.sizeDelta = new Vector2(1000.99994f, _bottomStripe.rectTransform.rect.height);
			if (AssemblyController.GetMarket().GetIsAmazonMarket())
			{
				PositionStripesCompact();
			}
			else if (AssemblyController.GetMarket().GetIsAmazonMobileMarket())
			{
				PositionStripesWithUserId();
			}
			else if (AssemblyController.GetMarket().GetIsChinaMarket())
			{
				PositionStripesAlternate();
			}
			else
			{
				PositionStripesDefault();
			}
		}

		protected override void SetupFooter(FooterType HJNAHNICGMH)
		{
			base.SetupFooter(HJNAHNICGMH);
			float num = _bottomStripe.transform.localPosition.y - -70f;
			if (_btnOK.gameObject.activeSelf)
			{
				_btnOK.transform.SetLocalY(num + _btnOK.get_rect().height / 2f);
				_btnOK.RemoveEvent(2);
				_btnOK.AddEventListener(2, OnAdvancedClicked);
			}
			if (_btnCancel.gameObject.activeSelf)
			{
				_btnCancel.transform.SetLocalY(num + _btnCancel.get_rect().height / 2f);
				_btnCancel.RemoveEventListener(2, OnClose);
				_btnCancel.AddEventListener(2, OnClose);
			}
		}

		protected override void SetupHeader(string HCPNFPMHFCM)
		{
			base.SetupHeader(HCPNFPMHFCM);
			float bAINMLLIKOL = _topStripe.transform.localPosition.y - 110f;
			_header.transform.SetLocalY(bAINMLLIKOL);
		}

		private void Update()
		{
			if (languageChangePending)
			{
				languageChangeFrames++;
				if (languageChangeFrames >= 60)
				{
					ApplyLanguageChange();
					languageChangePending = false;
					languageChangeFrames = 0;
				}
			}
			bool flag = GameCenterController.GetIsAuthenticated();
			if (GameCenterSignedIn != flag)
			{
				RefreshGameCenterButton();
				GameCenterSignedIn = flag;
			}
		}

		protected virtual void SetupUserIdLabel()
		{
			lblUserId.set_Alias(string.Empty);
			lblUserId.set_text(string.Empty);
			lblUserId.set_LabelFontSize(101);
			float bAINMLLIKOL = ((!AssemblyController.GetMarket().GetIsChinaMarket()) ? (-516f) : (-356f));
			lblUserId.transform.SetLocalY(bAINMLLIKOL);
			lblUserId.color = Constants.DialogTextColor;
			string text = ListSF.GetRoster().GetServerUserId();
			lblUserId.set_text(LocalizationManager.GetString("Settings_UserID") + ": " + text);
			lblUserId.gameObject.SetActive(text != string.Empty);
		}

		protected virtual void PositionContent()
		{
			if (AssemblyController.GetMarket().GetIsAmazonMarket())
			{
				_content.transform.SetLocalY(100f);
			}
			else if (AssemblyController.GetMarket().GetIsAmazonMobileMarket())
			{
				float num = 100f;
				num += ((!lblUserId.gameObject.activeSelf) ? 0f : 37f);
				_content.transform.SetLocalY(num);
			}
			else if (AssemblyController.GetMarket().GetIsChinaMarket())
			{
				float num2 = ((!AssemblyController.GetMarket().GetIsChinaMarket()) ? 100 : 0);
				num2 += ((!lblUserId.gameObject.activeSelf) ? 0f : 37f);
				_content.transform.SetLocalY(num2);
			}
			else if (AssemblyController.GetMarket().GetIsSteamMarket())
			{
				float num3 = 100f;
				num3 += ((!lblUserId.gameObject.activeSelf) ? 0f : 37f);
				_content.transform.SetLocalY(num3);
			}
			else
			{
				float num4 = ((!AssemblyController.GetMarket().GetIsChinaMarket()) ? 100 : 0);
				num4 += ((!lblUserId.gameObject.activeSelf) ? 0f : 37f);
				_content.transform.SetLocalY(num4);
			}
		}

		protected void SetupButton(ResolutionButton GAMILDJHFDB, string CGNJEDIFEKJ, string AOFLEGLGGAC, float DHDMNHCIPEH, float BGEEALIPKCC, SettingsButtonId OKNNNLIPODI)
		{
			// Desktop prefabs can omit the legacy credits/support controls.
			if (GAMILDJHFDB == null)
			{
				return;
			}
			if (OKNNNLIPODI == SettingsButtonId.BTN_GAMECENTER || OKNNNLIPODI == SettingsButtonId.BTN_ITUNES)
			{
				if (GAMILDJHFDB != null) GAMILDJHFDB.gameObject.SetActive(false);
				return;
			}
			GAMILDJHFDB.SetNormalSprite("UI/Atlases/", CGNJEDIFEKJ);
			GAMILDJHFDB.SetPressedSprite("UI/Atlases/", (!UsesSelectedSprites()) ? CGNJEDIFEKJ : AOFLEGLGGAC);
			GAMILDJHFDB.ButtonId = (int)OKNNNLIPODI;
			GAMILDJHFDB.RemoveEventListener(2, OnClickButton);
			GAMILDJHFDB.AddEventListener(2, OnClickButton);
			GAMILDJHFDB.transform.localPosition = new Vector2(DHDMNHCIPEH, BGEEALIPKCC);
			GAMILDJHFDB.gameObject.SetActive(true);
		}

		protected void SetupLabel(LabelAlias NCJDCOLEFHG, string LOKLDPLAPOL)
		{
			if (NCJDCOLEFHG == null)
			{
				return;
			}
			if (NCJDCOLEFHG == lblGameCenter || NCJDCOLEFHG == lblItunes)
			{
				if (NCJDCOLEFHG != null) NCJDCOLEFHG.gameObject.SetActive(false);
				return;
			}
			NCJDCOLEFHG.gameObject.SetActive(true);
			NCJDCOLEFHG.set_Alias(LOKLDPLAPOL);
			NCJDCOLEFHG.alignment = TextAnchor.MiddleLeft;
			NCJDCOLEFHG.set_LabelFontSize(101);
			NCJDCOLEFHG.color = Constants.DialogTextColor;
		}

		protected void SetupPlatformButtonsNoOp()
		{
		}

		protected virtual void SetupDefaultPlatformButtons()
		{
			if (!SystemProperties.IsWindowsEditorPlatform() && !SystemProperties.IsWindowsPlatform() && !SystemProperties.IsEditorPlatform())
			{
				string empty = string.Empty;
				empty = ((!SystemProperties.IsAndroidPlatform()) ? ((!SFSocial.GetInstance().IsAuthorized()) ? "SettingsButtons.gamecenter_off" : "SettingsButtons.gamecenter") : ((!SFSocial.GetInstance().IsAuthorized()) ? "SettingsButtons.googleplay_off_controller" : "SettingsButtons.googleplay_controller"));
				SetupButton(btnGameCenter, empty, empty, 0f, 0f, SettingsButtonId.BTN_GAMECENTER);
				if (SystemProperties.IsAndroidPlatform())
				{
					btnGameCenter.SetDisabledSprite("UI/Atlases/", "SettingsButtons.googleplay_disable_controller");
				}
				else
				{
					btnGameCenter.SetDisabledSprite("UI/Atlases/", "SettingsButtons.gamecenter_disable");
				}
			}
			SetupButton(btnSupport, "SettingsButtons.support", "SettingsButtons.support", -670f, -400f, SettingsButtonId.BTN_SUPPORT);
			if (SystemProperties.IsIosPlatform())
			{
				SetupButton(btnItunes, "SettingsButtons.itunes", "SettingsButtons.itunes", 0f, -196f, SettingsButtonId.BTN_ITUNES);
			}
			else if (SystemProperties.IsAndroidPlatform())
			{
				SetupButton(btnItunes, "SettingsButtons.googleplay_music", "SettingsButtons.googleplay_music", 0f, -196f, SettingsButtonId.BTN_ITUNES);
			}
		}

		private void SetupSupportButtonCompact()
		{
			float dHDMNHCIPEH = ((!AssemblyController.GetMarket().GetIsChinaMarket()) ? (-670) : 50);
			float bGEEALIPKCC = 0f;
			SetupButton(btnSupport, "SettingsButtons.support", "SettingsButtons.support", dHDMNHCIPEH, bGEEALIPKCC, SettingsButtonId.BTN_SUPPORT);
		}

		private void SetupAmazonButtons()
		{
			float dHDMNHCIPEH = 50f;
			float bGEEALIPKCC = 0f;
			SetupButton(btnSupport, "SettingsButtons.support", "SettingsButtons.support", dHDMNHCIPEH, bGEEALIPKCC, SettingsButtonId.BTN_SUPPORT);
			SetupButton(btnItunes, "SettingsButtons.amazon_mp3", "SettingsButtons.amazon_mp3", -670f, -196f, SettingsButtonId.BTN_ITUNES);
		}

		private void SetupSupportButtonOnly()
		{
			SetupButton(btnSupport, "SettingsButtons.support", "SettingsButtons.support", -670f, -400f, SettingsButtonId.BTN_SUPPORT);
			SetupPlatformButtonsNoOp();
		}

		private void SetupWindowsButtons()
		{
			if (SystemProperties.IsWp8Platform())
			{
				SetupButton(btnGameCenter, (!GameCenterController.GetIsAuthenticated()) ? "SettingsButtons.windows_off" : "SettingsButtons.windows", (!GameCenterController.GetIsAuthenticated()) ? "SettingsButtons.windows_off" : "SettingsButtons.windows", 0f, 0f, SettingsButtonId.BTN_GAMECENTER);
			}
			float num = 0f;
			float num2 = 0f;
			if (SystemProperties.IsWp8Platform())
			{
				num = -200f;
				num2 = -400f;
			}
			else
			{
				num = 50f;
				num2 = -200f;
			}
			SetupButton(btnSupport, "SettingsButtons.support", "SettingsButtons.support_selected", num, num2, SettingsButtonId.BTN_SUPPORT);
		}

		protected virtual void SetupDefaultLabels()
		{
			SetupLabel(lblSupport, "Settings_Support");
			if (!AssemblyController.GetMarket().GetIsChinaMarket() && !AssemblyController.GetMarket().GetIsSteamMarket())
			{
				if (SystemProperties.IsAndroidPlatform())
				{
					SetupLabel(lblGameCenter, "Settings_GooglePlus");
				}
				else if (SystemProperties.IsIosPlatform())
				{
					SetupLabel(lblGameCenter, "Settings_GameCenter");
				}
				else if (SystemProperties.IsMetroArmPlatform() && SystemProperties.IsWp8Platform())
				{
					SetupLabel(lblGameCenter, "Settings_live_id");
				}
			}
			if (btnItunes.gameObject.activeSelf)
			{
				SetupLabel(lblItunes, "Settings_Soundtack");
			}
			SetupUserIdLabel();
		}

		protected void SetupSupportAndGameServiceLabels()
		{
			SetupLabel(lblSupport, "Settings_Support");
			if (!AssemblyController.GetMarket().GetIsChinaMarket())
			{
				if (SystemProperties.IsAndroidPlatform())
				{
					SetupLabel(lblGameCenter, "Settings_GooglePlus");
				}
				else
				{
					SetupLabel(lblGameCenter, "Settings_GameCenter");
				}
			}
			SetupUserIdLabel();
		}

		protected void HideUserIdLabel()
		{
			SetupUserIdLabel();
			lblUserId.gameObject.SetActive(false);
		}

		protected void SetupAmazonLabels()
		{
			SetupLabel(lblSupport, "Settings_Support");
			SetupLabel(lblItunes, "Settings_Soundtack");
			SetupUserIdLabel();
			if (lblUserId.gameObject.activeSelf)
			{
				lblUserId.transform.SetLocalY(-516f);
			}
		}

		protected void SetupResolutionLabels()
		{
			SetupLabel(lblSupport, "Settings_Support");
			lblResolution.gameObject.SetActive(true);
			lblResolution.set_Alias("Settings_Resolution");
			lblResolution.alignment = TextAnchor.MiddleLeft;
			lblResolution.set_LabelFontSize(101);
			lblResolution.transform.SetLocalX(0f);
			lblResolution.transform.SetLocalY(150f);
			lblResolution.color = Constants.DialogTextColor;
			lblResolutionValue.set_Alias(string.Empty);
			lblResolutionValue.alignment = TextAnchor.MiddleLeft;
			lblResolutionValue.set_LabelFontSize(96);
			lblResolutionValue.color = Constants.DialogTextColor;
			lblResolutionValue.transform.SetLocalX(100f);
			lblResolutionValue.transform.SetLocalY(0f);
			SetupUserIdLabel();
		}

		protected virtual void PositionStripesDefault()
		{
			float num = ((!AssemblyController.GetMarket().GetIsChinaMarket()) ? 600f : 500f);
			float num2 = ((!lblUserId.gameObject.activeSelf) ? 0f : 37f);
			_topStripe.transform.SetLocalY(num + num2);
			num = ((!AssemblyController.GetMarket().GetIsChinaMarket()) ? (-600f) : (-540f));
			_bottomStripe.transform.SetLocalY(num - num2);
		}

		protected void PositionStripesAlternate()
		{
			float num = ((!AssemblyController.GetMarket().GetIsChinaMarket()) ? 600f : 500f);
			float num2 = ((!lblUserId.gameObject.activeSelf) ? 0f : 37f);
			_topStripe.transform.SetLocalY(num + num2);
			num = ((!AssemblyController.GetMarket().GetIsChinaMarket()) ? (-600f) : (-540f));
			_bottomStripe.transform.SetLocalY(num - num2);
		}

		protected void PositionStripesCompact()
		{
			_topStripe.transform.SetLocalY(400f);
			_bottomStripe.transform.SetLocalY(-440f);
		}

		protected void PositionStripesWithUserId()
		{
			float num = 600f;
			float num2 = ((!lblUserId.gameObject.activeSelf) ? 0f : 37f);
			_topStripe.transform.SetLocalY(num + num2);
			num = -600f;
			_bottomStripe.transform.SetLocalY(num - num2);
		}

		protected void RefreshGameCenterButton()
		{
			string text = string.Empty;
			bool flag = GameCenterController.GetIsAuthenticated();
			if (SystemProperties.IsAndroidPlatform())
			{
				text = ((!flag) ? "SettingsButtons.googleplay_off_controller" : "SettingsButtons.googleplay_controller");
			}
			else if (SystemProperties.IsIosPlatform())
			{
				text = ((!flag) ? "SettingsButtons.gamecenter_off" : "SettingsButtons.gamecenter");
			}
			else if (SystemProperties.IsMetroArmPlatform())
			{
				text = ((!flag) ? "SettingsButtons.windows_off" : "SettingsButtons.windows");
			}
			SetupButton(btnGameCenter, text, text, 0f, 0f, SettingsButtonId.BTN_GAMECENTER);
			if (SystemProperties.IsAndroidPlatform())
			{
				btnGameCenter.SetDisabledSprite("UI/Atlases/", "SettingsButtons.googleplay_disable_controller");
			}
			else
			{
				btnGameCenter.SetDisabledSprite("UI/Atlases/", "SettingsButtons.gamecenter_disable");
			}
		}

		protected virtual void OnClickButton(object data)
		{
			switch ((SettingsButtonId)data)
			{
			case SettingsButtonId.BTN_RESOLUTION_HIGHER:
				if (selectedResolutionIndex < SystemProperties.GetNumOfDisplayModes() - 1)
				{
					selectedResolutionIndex++;
				}
				else
				{
					selectedResolutionIndex = 0;
				}
				UpdateLabels();
				break;
			case SettingsButtonId.BTN_RESOLUTION_LOWER:
				if (selectedResolutionIndex > 0)
				{
					selectedResolutionIndex--;
				}
				else
				{
					selectedResolutionIndex = SystemProperties.GetNumOfDisplayModes() - 1;
				}
				UpdateLabels();
				break;
			case SettingsButtonId.BTN_RESOLUTION_SET:
			{
				XmlDocument jFJPKEONJIJ = XmlUtils.OpenXMLDocument(SF2Paths.GetGameDataPath(), "devices.xml");
				SystemProperties.LoadDevicesConfig(jFJPKEONJIJ);
				SystemProperties.ApplyResolution(selectedResolutionIndex);
				GameUtils.StartFight();
				break;
			}
			case SettingsButtonId.BTN_MUSIC:
				SoundController.SetMusicMuted(!SoundController.GetMusicMuted());
				SetupButton(btnMusic, (!SoundController.GetMusicMuted()) ? "SettingsButtons.music" : "SettingsButtons.music_off", (!SoundController.GetMusicMuted()) ? "SettingsButtons.music_selected" : "SettingsButtons.music_off_selected", btnMusic.transform.localPosition.x, btnMusic.transform.localPosition.y, SettingsButtonId.BTN_MUSIC);
				break;
			case SettingsButtonId.BTN_SOUND:
				SoundController.SetSoundMuted(!SoundController.GetSoundMuted());
				SetupButton(btnSound, (!SoundController.GetSoundMuted()) ? "SettingsButtons.sound" : "SettingsButtons.sound_off", (!SoundController.GetSoundMuted()) ? "SettingsButtons.sound_selected" : "SettingsButtons.sound_off_selected", btnSound.transform.localPosition.x, btnSound.transform.localPosition.y, SettingsButtonId.BTN_SOUND);
				break;
			case SettingsButtonId.BTN_CREDITS:
				CreditsScreen.Create();
				OnClose(0);
				break;
			case SettingsButtonId.BTN_FACEBOOK:
			{
				QuestParameters hHKLFIIBIFF = ListSF.GetInstance().GetQuestParameters();
				FightIDS jLGLBLDPAAF = hHKLFIIBIFF.fightIds;
				hHKLFIIBIFF.fightIds = FightIDS.Empty();
				hHKLFIIBIFF.fightResult = string.Empty;
				hHKLFIIBIFF.raidResult = string.Empty;
				hHKLFIIBIFF.purchasedItem = null;
				if (ListSF.GetInstance().RaiseQuestEvent(QuestEvent.QuestEventType.QUEST_EVENT_LOGIN_FB))
				{
					ListSF.GetInstance().RunQuestActions();
				}
				hHKLFIIBIFF.fightIds = jLGLBLDPAAF;
				break;
			}
			case SettingsButtonId.BTN_LANGUAGE:
			{
				LocalizationManager.Language pPNFBAFOOAH = LocalizationManager.GetNextLanguage(pendingLanguage);
				string mMBELNEBNBM = pPNFBAFOOAH.IconSprite;
				string oKGJAMBPDGO = pPNFBAFOOAH.SelectedIconSprite;
				oKGJAMBPDGO = ((!(oKGJAMBPDGO == string.Empty)) ? oKGJAMBPDGO : mMBELNEBNBM);
				SetupButton(btnLanguage, mMBELNEBNBM, oKGJAMBPDGO, btnLanguage.transform.localPosition.x, btnLanguage.transform.localPosition.y, SettingsButtonId.BTN_LANGUAGE);
				Eclipse.UI.LanguageGlobeIcon.Apply(btnLanguage);
				SetupLabel(lblLanguage, pPNFBAFOOAH.Alias);
				languageChangePending = true;
				pendingLanguage = pPNFBAFOOAH;
				languageChangeFrames = 0;
				break;
			}
			case SettingsButtonId.BTN_GAMECENTER:
				if (GameCenterController.GetIsAuthenticated())
				{
					if (SystemProperties.IsAndroidPlatform())
					{
						GameCenterController.SignOut();
						RefreshGameCenterButton();
						ListSF.GetRoster().SetGPlusAutoLogin(false);
					}
					else if (SystemProperties.IsIosPlatform())
					{
						GameCenterController.ShowAchievements();
					}
					else if (SystemProperties.IsWp8Platform())
					{
						GameCenterController.SignOut();
						RefreshGameCenterButton();
					}
				}
				else
				{
					if (SystemProperties.IsWp8Platform())
					{
						GameUtils.LockInput();
						SetMainButtonsHidden(true);
					}
					GameCenterController.SignIn();
					ListSF.GetRoster().SetGPlusAutoLogin(true);
					ListSF.GetRoster().ResetGPlusFailedLogins();
				}
				break;
			case SettingsButtonId.BTN_SUPPORT:
			{
				string url2 = GameUtils.SupportChoices.GetUrl(LocalizationManager.CurrentLanguage.name, LocalizationManager.DefaultLanguageName);
				OfflineServices.OpenExternalUrl(url2);
				break;
			}
			case SettingsButtonId.BTN_ITUNES:
			{
				string url = InternetController.GetMusicStoreUrl();
				OfflineServices.OpenExternalUrl(url);
				break;
			}
			}
		}

		protected void OnAuthenticate(bool CELFBNLILMA)
		{
			if (CELFBNLILMA)
			{
				OnGameCenterSignedIn();
				ListSF.GetInstance().OnAuthenticateSucceeded();
			}
			else
			{
				OnGameCenterSignInFailed();
			}
		}

		protected void OnGameCenterSignedIn()
		{
			btnGameCenter.interactable = true;
			if (!ListSF.GetRoster().GetTrySocialLogin())
			{
				ListSF.GetRoster().SetTrySocialLogin(true);
			}
			if (SystemProperties.IsWp8Platform())
			{
				SetMainButtonsHidden(false);
			}
		}

		protected void OnGameCenterSignInFailed()
		{
			if (SystemProperties.IsWp8Platform())
			{
				RefreshGameCenterButton();
			}
		}

		protected void OnAdvancedClicked(object data)
		{
            base.gameObject.SetActive(false);
            Eclipse.UI.TitleScreen.ShowOptions(() =>
            {
                if (this != null) base.gameObject.SetActive(true);
            });
		}

		protected void OnChildDialogClosed(object data)
		{
			ApplyPlatformLayout();
			bool flag = SoundController.GetMusicMuted();
			SetupButton(btnMusic, (!flag) ? "SettingsButtons.music" : "SettingsButtons.music_off", (!flag) ? "SettingsButtons.music_selected" : "SettingsButtons.music_off_selected", btnMusic.transform.localPosition.x, btnMusic.transform.localPosition.y, SettingsButtonId.BTN_MUSIC);
			bool flag2 = SoundController.GetSoundMuted();
			SetupButton(btnSound, (!flag2) ? "SettingsButtons.sound" : "SettingsButtons.sound_off", (!flag2) ? "SettingsButtons.sound_selected" : "SettingsButtons.sound_off_selected", btnSound.transform.localPosition.x, btnSound.transform.localPosition.y, SettingsButtonId.BTN_SOUND);
			base.gameObject.SetActive(true);
			childDialog.RemoveAllEventListener();
			childDialog = null;
		}

		protected override void ApplyPlatformLayout()
		{
		}

		protected void ApplyLanguageChange()
		{
			QuestParameters hHKLFIIBIFF = ListSF.GetInstance().GetQuestParameters();
			hHKLFIIBIFF.chosenLanguage = pendingLanguage;
			if (ListSF.GetInstance().RaiseQuestEvent(QuestEvent.QuestEventType.QUEST_EVENT_LANGUAGE_SWITCH))
			{
				ListSF.GetInstance().RunQuestActions();
				return;
			}
			LocalizationManager.ChangeLanguage(pendingLanguage);
			UpdateLabels();
		}

		protected bool UsesSelectedSprites()
		{
			return AssemblyController.GetGamepadEnabled();
		}

		protected void SetMainButtonsHidden(bool JILGHDDEMPE)
		{
			btnSound.gameObject.SetActive(!JILGHDDEMPE);
			btnMusic.gameObject.SetActive(!JILGHDDEMPE);
			if (btnCredits != null) btnCredits.gameObject.SetActive(!JILGHDDEMPE);
			btnLanguage.gameObject.SetActive(!JILGHDDEMPE);
			if (SystemProperties.IsWp8Platform())
			{
				btnGameCenter.interactable = !JILGHDDEMPE;
			}
			if (btnSupport != null) btnSupport.gameObject.SetActive(!JILGHDDEMPE);
		}
	}
}
