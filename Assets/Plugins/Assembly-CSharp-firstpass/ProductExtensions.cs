using SF2.Offline;

public static class ProductExtensions
{
	public static PurchaseReceipt ToReceipt(this Product KDOEGOIJKLG)
	{
		return string.IsNullOrEmpty(KDOEGOIJKLG.receipt) ? null : new PurchaseReceipt(KDOEGOIJKLG);
	}

	public static string Log(this ProductDefinition PANEMFIIOGB)
	{
		return string.Format("[ProductDefinition: id={0}, storeSpecificId={1}, type={2}]", PANEMFIIOGB.id, PANEMFIIOGB.storeSpecificId, PANEMFIIOGB.type);
	}
}
