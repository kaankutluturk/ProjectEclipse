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
			NewsDialogInfo oFOJGCFHJKD = (NewsDialogInfo)data;
			if (oFOJGCFHJKD != null)
			{
				newsItems = oFOJGCFHJKD.Items;
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
				NewsItem pONDDFBMFOO = newsItems[0];
				SetupHeader(pONDDFBMFOO.Title);
				imagePath = pONDDFBMFOO.LocalImagePath;
				externalUrl = pONDDFBMFOO.Url;
				if (pONDDFBMFOO.GoShop)
				{
					GoShopAfterClose = true;
					RedirectShopAfterClose = pONDDFBMFOO.RedirectShop;
				}
				Texture2D texture2D = null;
				if (File.Exists(pONDDFBMFOO.LocalImagePath))
				{
					byte[] data = File.ReadAllBytes(pONDDFBMFOO.LocalImagePath);
					texture2D = new Texture2D(2, 2);
					texture2D.LoadImage(data);
				}
				newsItems.Remove(pONDDFBMFOO);
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

		protected override void SetupFooter(FooterType HJNAHNICGMH)
		{
			base.SetupFooter(HJNAHNICGMH);
			SetupNewsButtons();
		}

		public override void OnClose(object data)
		{
			buttonMaker.ClearButtons();
			if (newsItems.Count == 0)
			{
				if ((!BuyItemAfterClose || !(RedirectShopAfterClose != string.Empty)) && GoShopAfterClose)
				{
					QuestParameters hHKLFIIBIFF = ListSF.GetInstance().GetQuestParameters();
					GameUtils.NotifyTabChanged(GameUtils.GetSliderTypeByName(hHKLFIIBIFF.currentTabName), SliderType.SliderRuby);
					DelayedStrike dDFFCNPELBC = new DelayedStrike(SliderType.SliderRuby);
					if (RedirectShopAfterClose != string.Empty)
					{
						dDFFCNPELBC.Item = ListSF.FindAvailableItemByGroup(RedirectShopAfterClose, 0L);
					}
					Module.OpenScreen(ScreenType.ModuleShop, dDFFCNPELBC);
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
			NewsItem pONDDFBMFOO = newsItems[0];
			List<NewsButton> dHKDOHFKOOJ = pONDDFBMFOO.Buttons;
			int count = dHKDOHFKOOJ.Count;
			_btnOK.gameObject.SetActive(true);
			_btnOK.RemoveAllEventListener();
			_btnOK.AddEventListener(2, (object NPKMJMCLDAH) =>
			{
				OnClose(NPKMJMCLDAH);
				_btnOK.RemoveAllEventListener();
			});
			if (count <= 0)
			{
				return;
			}
			_btnOK.gameObject.SetActive(false);
			float gBCONNBABLL = -530f;
			float num = 1680 / (dHKDOHFKOOJ.Count + 1);
			float fNDOOJNDJDC = -840f + num;
			if (count == 1)
			{
				fNDOOJNDJDC = _btnOK.transform.localPosition.x;
			}
			buttonMaker.Init(fNDOOJNDJDC, gBCONNBABLL, num, base.gameObject, OnClose, this);
			foreach (NewsButton item in dHKDOHFKOOJ)
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

		protected override void SetupHeader(string HCPNFPMHFCM)
		{
			_header.set_text((!(HCPNFPMHFCM == string.Empty)) ? HCPNFPMHFCM : LocalizationManager.GetString("dlgNewsTitle"));
			_header.transform.SetLocalY(-25f + _topStripe.transform.localPosition.y);
		}
	}
}
