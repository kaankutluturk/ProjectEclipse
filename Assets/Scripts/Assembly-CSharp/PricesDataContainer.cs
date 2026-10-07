using System.Collections.Generic;

public class PricesDataContainer
{
	private List<PricesData> prices = new List<PricesData>();

	public List<PricesData> Prices
	{
		get
		{
			return GetPrices();
		}
	}

	public List<PricesData> GetPrices()
	{
		return prices;
	}

	public PricesData FindByProductId(string JKKKGIOHNMH)
	{
		return prices.Find((PricesData DHDMNHCIPEH) => DHDMNHCIPEH.ProductId == JKKKGIOHNMH || DHDMNHCIPEH.NewProductId == JKKKGIOHNMH);
	}

	public PricesData FindByName(string BMCEHAPAJCA)
	{
		return prices.Find((PricesData DHDMNHCIPEH) => DHDMNHCIPEH.name == BMCEHAPAJCA);
	}

	public bool TryGetPriceValue(string BMCEHAPAJCA, out float HCHKFOJEEBK)
	{
		HCHKFOJEEBK = 0f;
		PricesData bEOLBLGJCKA = FindByName(BMCEHAPAJCA);
		if (bEOLBLGJCKA != null)
		{
			return float.TryParse(bEOLBLGJCKA.GetCurrentPrice(), out HCHKFOJEEBK);
		}
		return false;
	}
}
