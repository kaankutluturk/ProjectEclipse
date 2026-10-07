using SF2.Offline;

public static class ProductExtensions
{
	public static PurchaseReceipt ToReceipt(this Product product)
	{
		return string.IsNullOrEmpty(product.receipt) ? null : new PurchaseReceipt(product);
	}

	public static string Log(this ProductDefinition definition)
	{
		return string.Format("[ProductDefinition: id={0}, storeSpecificId={1}, type={2}]", definition.id, definition.storeSpecificId, definition.type);
	}
}
