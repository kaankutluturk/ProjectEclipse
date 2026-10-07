using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Nekki.SF2.GUI.Dialogs
{
	public class NewsDialog : BaseDialog
	{
		public const int HEADER_OFFSET_Y = -25;

		public const int BUTTON_Y = -530;

		public const int BUTTON_OFFSET_X = -90;

		public const int SCROLL_WIDTH = 1680;

		private const int TopStripeY = 600;

		private const int BottomStripeY = -640;

		protected string imagePath = string.Empty;

		protected string externalUrl = string.Empty;

		[SerializeField]
		protected ResolutionImage _picture;

		[SerializeField]
		protected ResolutionImage _loadingPicture;

		private bool isLoading;

		protected List<NewsItem> newsItems = new List<NewsItem>();

		public bool GoShopAfterClose;

		public string RedirectShopAfterClose = string.Empty;

		public bool BuyItemAfterClose;

		protected NewsButtonMaker buttonMaker = new NewsButtonMaker();

		public override void Init(object data)
		{
			NewsDialogInfo newsInfo = (NewsDialogInfo)data;
			if (newsInfo != null)
			{
				newsItems = newsInfo.Items;
			}
			IsQuestDialog = true;
			_picture.GetComponent<SFButton>().AddEventListener(0, OnPictureClicked);
		}

		protected override void Start()
		{
			LayoutStripes();
			SetupHeader(titleAlias);
			SetupFooter(footerType);
			SetupContent();
			if (AssemblyController.GetGamepadEnabled())
			{
				ApplyPlatformLayout();
			}
		}

		protected override void SetupContent()
		{
			_picture.gameObject.SetActive(newsItems.Count > 0);
			if (newsItems.Count > 0)
			{
				NewsItem newsItem = newsItems[0];
				SetupHeader(newsItem.Title);
				imagePath = newsItem.LocalImagePath;
				externalUrl = newsItem.Url;
				if (newsItem.GoShop)
				{
					GoShopAfterClose = true;
					RedirectShopAfterClose = newsItem.RedirectShop;
				}
				Texture2D texture2D = null;
				if (File.Exists(newsItem.LocalImagePath))
				{
					byte[] data = File.ReadAllBytes(newsItem.LocalImagePath);
					texture2D = new Texture2D(2, 2);
					texture2D.LoadImage(data);
				}
				newsItems.Remove(newsItem);
				if (texture2D != null)
				{
					_picture.sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
				}
				else
				{
					EnableLoading(true);
				}
			}
		}

		protected override void LayoutStripes()
		{
			base.LayoutStripes();
			_topStripe.transform.SetLocalY(600f);
			_bottomStripe.transform.SetLocalY(-640f);
		}

		protected override void SetupFooter(FooterType footer)
		{
			base.SetupFooter(footer);
			SetupNewsButtons();
		}

		public override void OnClose(object data)
		{
			buttonMaker.ClearButtons();
			if (newsItems.Count == 0)
			{
				if ((!BuyItemAfterClose || !(RedirectShopAfterClose != string.Empty)) && GoShopAfterClose)
				{
					QuestParameters questParameters = ListSF.GetInstance().GetQuestParameters();
					GameUtils.NotifyTabChanged(GameUtils.GetSliderTypeByName(questParameters.currentTabName), SliderType.SliderRuby);
					DelayedStrike delayedStrike = new DelayedStrike(SliderType.SliderRuby);
					if (RedirectShopAfterClose != string.Empty)
					{
						delayedStrike.Item = ListSF.FindAvailableItemByGroup(RedirectShopAfterClose, 0L);
					}
					Module.OpenScreen(ScreenType.ModuleShop, delayedStrike);
				}
				base.OnClose(data);
			}
			else
			{
				SetupNewsButtons();
				SetupContent();
			}
		}

		protected virtual void EnableLoading(bool value)
		{
			_loadingPicture.gameObject.SetActive(value);
			isLoading = value;
		}

		private void Update()
		{
			if (isLoading)
			{
				_loadingPicture.transform.Rotate(0f, 20f * Time.deltaTime, 0f);
			}
		}

		protected virtual void SetupNewsButtons()
		{
			if (newsItems.Count == 0)
			{
				return;
			}
			NewsItem newsItem = newsItems[0];
			List<NewsButton> buttons = newsItem.Buttons;
			int count = buttons.Count;
			_btnOK.gameObject.SetActive(true);
			_btnOK.RemoveAllEventListener();
			_btnOK.AddEventListener(2, (object result) =>
			{
				OnClose(result);
				_btnOK.RemoveAllEventListener();
			});
			if (count <= 0)
			{
				return;
			}
			_btnOK.gameObject.SetActive(false);
			float buttonY = -530f;
			float num = 1680 / (buttons.Count + 1);
			float buttonX = -840f + num;
			if (count == 1)
			{
				buttonX = _btnOK.transform.localPosition.x;
			}
			buttonMaker.Init(buttonX, buttonY, num, base.gameObject, OnClose, this);
			foreach (NewsButton item in buttons)
			{
				buttonMaker.AddButton(item);
			}
		}

		protected virtual void OnPictureClicked(object data)
		{
			if (externalUrl != string.Empty)
			{
				OfflineServices.OpenExternalUrl(externalUrl);
			}
		}

		protected override void SetupHeader(string headerText)
		{
			_header.set_text((!(headerText == string.Empty)) ? headerText : LocalizationManager.GetString("dlgNewsTitle"));
			_header.transform.SetLocalY(-25f + _topStripe.transform.localPosition.y);
		}
	}
}
