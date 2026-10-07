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

	public PricesData FindByProductId(string productId)
	{
		return prices.Find((PricesData priceData) => priceData.ProductId == productId || priceData.NewProductId == productId);
	}

	public PricesData FindByName(string priceName)
	{
		return prices.Find((PricesData priceData) => priceData.name == priceName);
	}

	public bool TryGetPriceValue(string priceName, out float priceValue)
	{
		priceValue = 0f;
		PricesData priceData = FindByName(priceName);
		if (priceData != null)
		{
			return float.TryParse(priceData.GetCurrentPrice(), out priceValue);
		}
		return false;
	}
}
