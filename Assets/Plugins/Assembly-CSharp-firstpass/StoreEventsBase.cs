using System;
using SF2.Offline;

public abstract class StoreEventsBase
{
	public Action OnInitialized = () =>
	{
	};

	public Action<InitializationFailureReason> OnInitializeFailed = delegate
	{
	};

	public Action<string> OnPurchaseStarted = delegate
	{
	};

	public Action<string> OnPurchaseSucceeded = delegate
	{
	};

	public Action<string, PurchaseFailureReason> OnPurchaseFailed = delegate
	{
	};

	public Action<string, string> OnPurchaseFinished = delegate
	{
	};

	public Action<string, string> OnVerificationStarted = delegate
	{
	};

	public Action<string, string> OnVerificationFinished = delegate
	{
	};

	public Action<string, string> OnConfirmationStarted = delegate
	{
	};

	public Action<string, string> OnConfirmationFinished = delegate
	{
	};

	public Action<string> OnPurchaseRejected = delegate
	{
	};

	public Action<string> OnVerificationNoResponse = delegate
	{
	};

	public Action<string> OnPurchaseCancelled = delegate
	{
	};

	public Action OnRestoreCompleted = () =>
	{
	};

	public Action OnRestoreFailed = () =>
	{
	};

	public Action<string> OnProductNotification = delegate
	{
	};

	public Action OnStoreNotification = () =>
	{
	};
}
