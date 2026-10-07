using System.Collections.Generic;

public class News
{
	private List<NewsItem> _items = new List<NewsItem>();

	public List<NewsItem> Items
	{
		get
		{
			return GetItems();
		}
	}

	public List<NewsItem> GetItems()
	{
		return _items;
	}

	public void Reset()
	{
		_items.Clear();
	}

	public void AddOrReplaceItem(string name, string BEPKJNKCKPH, string MDDOAGNHAHE, int OKNNNLIPODI, bool HNJDHGDLLPD, long NKKKMPPEMKE, List<NewsButton> HJNAHNICGMH, string PEMOECLNECD = "", bool EIKKPDKMMHK = false, string KINPMPFPFHD = "", string EJENJNPEDOH = "")
	{
		NewsItem pONDDFBMFOO = _items.Find((NewsItem DHDMNHCIPEH) => DHDMNHCIPEH.Id == OKNNNLIPODI);
		if (pONDDFBMFOO != null)
		{
			_items.Remove(pONDDFBMFOO);
		}
		pONDDFBMFOO = new NewsItem();
		pONDDFBMFOO.Title = PEMOECLNECD;
		pONDDFBMFOO.Name = name;
		pONDDFBMFOO.Url = BEPKJNKCKPH;
		pONDDFBMFOO.ImageUrl = MDDOAGNHAHE;
		pONDDFBMFOO.Id = OKNNNLIPODI;
		pONDDFBMFOO.IsActive = HNJDHGDLLPD;
		pONDDFBMFOO.EndDate = NKKKMPPEMKE;
		pONDDFBMFOO.WasShown = false;
		pONDDFBMFOO.IsImageReady = false;
		pONDDFBMFOO.GoShop = EIKKPDKMMHK;
		pONDDFBMFOO.RedirectShop = KINPMPFPFHD;
		pONDDFBMFOO.Buttons = HJNAHNICGMH;
		pONDDFBMFOO.SpenderTypeId = EJENJNPEDOH;
		GetItems().Add(pONDDFBMFOO);
	}
}
