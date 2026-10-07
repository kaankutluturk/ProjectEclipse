using System;
using System.Collections.Generic;
using SF2.Offline;

// Offline facade preserves UI completion callbacks but cannot purchase or grant items.
public class PaymentStore : StoreEventsBase
{
    public PaymentStore(Dictionary<string, object> options = null) { }
    public virtual bool IsAvailable { get { return false; } }
    public Product[] Products { get { return GetProducts(); } }
    public Product[] AllProducts { get { return GetProducts(); } }
    public Product[] AvailableProducts { get { return GetProducts(); } }
    public T GetExtension<T>() where T : PaymentStore { return this as T; }
    public bool HasExtension<T>() where T : PaymentStore { return this is T; }
    public virtual bool CanMakePayments() { return false; }
    public Product[] GetProducts() { return new Product[0]; }
    public Product[] GetAllProducts() { return GetProducts(); }
    public Product[] GetAvailableProducts() { return GetProducts(); }
    public Product[] GetProductsWhere(Func<Product, bool> predicate) { return GetProducts(); }
    public Product GetProductById(string id) { return null; }
    public virtual void LoadProducts(params string[] ids)
    {
        OnInitializeFailed(InitializationFailureReason.PurchasingUnavailable);
    }
    public virtual void PurchaseProduct(string id)
    {
        OnPurchaseFailed(id, PurchaseFailureReason.PurchasingUnavailable);
        OnPurchaseFinished(id, string.Empty);
    }
    public virtual void RestorePurchases() { OnRestoreCompleted(); }
    public void ConfirmPendingPurchase(string id) { }
    public void FinishTransaction(PaymentInfo transaction) { }
    public void NotifyVerificationStarted(string id, string receipt) { }
    public void NotifyConfirmationStarted(string id, string receipt) { }
    public void NotifyVerificationFinished(string id, string receipt) { }
    public void NotifyConfirmationFinished(string id, string receipt) { }
    public void NotifyPurchaseRejected(string id) { OnPurchaseRejected(id); }
    public void NotifyVerificationFailed(string id) { OnVerificationNoResponse(id); }
}
