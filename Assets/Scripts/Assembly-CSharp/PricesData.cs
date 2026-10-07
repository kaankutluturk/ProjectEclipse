public class PricesData
{
	public long Amount;

	public long NewAmount;

	public long AddAmount;

	public long NewAddAmount;

	public long StartDate;

	public long EndDate;

	public int Currency;

	public int AddPercent;

	public string ProductId;

	public string NewProductId;

	public string Price;

	public string NewPrice;

	public string AddCurrency;

	public string name;

	public string Sign;

	public string SignCode;

	public string Label;

	public string GroupId;

	public string Locale;

	public string MobileOperator;

	public string SpenderTypeId;

	public bool Focus;

	public bool IsConsumable = true;

	public long CurrentAmount
	{
		get
		{
			return GetCurrentAmount();
		}
	}

	public long CurrentAddAmount
	{
		get
		{
			return GetCurrentAddAmount();
		}
	}

	public string CurrentPrice
	{
		get
		{
			return GetCurrentPrice();
		}
	}

	public bool IsDiscountActive
	{
		get
		{
			return GetIsDiscountActive();
		}
	}

	public long GetCurrentAmount()
	{
		if (NewAmount > 0 && GetIsDiscountActive())
		{
			return NewAmount;
		}
		return Amount;
	}

	public long GetCurrentAddAmount()
	{
		if (NewAddAmount > 0 && GetIsDiscountActive())
		{
			return NewAddAmount;
		}
		return AddAmount;
	}

	public string GetCurrentPrice()
	{
		if (!string.IsNullOrEmpty(NewPrice) && GetIsDiscountActive())
		{
			return NewPrice;
		}
		return Price;
	}

	public bool GetIsDiscountActive()
	{
		return false;
	}
}
