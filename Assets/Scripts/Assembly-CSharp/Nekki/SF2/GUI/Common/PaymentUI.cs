using System;
using Nekki.SF2.GUI.Scripts;
using UnityEngine;
using UnityEngine.Events;
using SF2.Offline;

namespace Nekki.SF2.GUI.Common
{
	public class PaymentUI : UIModule
	{
		[SerializeField]
		private GameObject _Blocker;

		[SerializeField]
		private LoadingCircle _LoadingCircle;

		[SerializeField]
		private UnityEvent _OnProductsUpdateEvent;

		public UnityEvent ProductsUpdateEvent
		{
			get
			{
				return get_OnProductsUpdateEvent();
			}
		}

		public UnityEvent get_OnProductsUpdateEvent()
		{
			return _OnProductsUpdateEvent;
		}

		protected override void Init()
		{
			base.Init();
			PaymentStore store = PaymentManager.GetStore();
			store.OnPurchaseCancelled = (Action<string>)Delegate.Combine(store.OnPurchaseCancelled, new Action<string>(OnPurchaseDismissed));
			PaymentStore aDEKACKLIJG2 = PaymentManager.GetStore();
			aDEKACKLIJG2.OnPurchaseSucceeded = (Action<string>)Delegate.Combine(aDEKACKLIJG2.OnPurchaseSucceeded, new Action<string>(OnPurchaseSucceeded));
			PaymentStore aDEKACKLIJG3 = PaymentManager.GetStore();
			aDEKACKLIJG3.OnPurchaseFailed = (Action<string, PurchaseFailureReason>)Delegate.Combine(aDEKACKLIJG3.OnPurchaseFailed, new Action<string, PurchaseFailureReason>(OnPurchaseFailed));
			PaymentStore aDEKACKLIJG4 = PaymentManager.GetStore();
			aDEKACKLIJG4.OnPurchaseRejected = (Action<string>)Delegate.Combine(aDEKACKLIJG4.OnPurchaseRejected, new Action<string>(OnPurchaseUnsuccessful));
			PaymentStore aDEKACKLIJG5 = PaymentManager.GetStore();
			aDEKACKLIJG5.OnVerificationNoResponse = (Action<string>)Delegate.Combine(aDEKACKLIJG5.OnVerificationNoResponse, new Action<string>(OnServerNoResponse));
			PaymentStore aDEKACKLIJG6 = PaymentManager.GetStore();
			aDEKACKLIJG6.OnRestoreCompleted = (Action)Delegate.Combine(aDEKACKLIJG6.OnRestoreCompleted, new Action(OnPurchaseFlowFinished));
			PaymentStore aDEKACKLIJG7 = PaymentManager.GetStore();
			aDEKACKLIJG7.OnRestoreFailed = (Action)Delegate.Combine(aDEKACKLIJG7.OnRestoreFailed, new Action(OnConnectionFailed));
			PaymentStore aDEKACKLIJG8 = PaymentManager.GetStore();
			aDEKACKLIJG8.OnInitialized = (Action)Delegate.Combine(aDEKACKLIJG8.OnInitialized, new Action(OnProductsUpdated));
			PaymentManager.GetStore().LoadProducts();
			PaymentManager.ProcessPendingPayments();
		}

		protected override void OnModuleShutdown()
		{
			base.OnModuleShutdown();
			if (PaymentManager.GetStore() != null)
			{
				PaymentStore store = PaymentManager.GetStore();
				store.OnPurchaseCancelled = (Action<string>)Delegate.Remove(store.OnPurchaseCancelled, new Action<string>(OnPurchaseDismissed));
				PaymentStore aDEKACKLIJG2 = PaymentManager.GetStore();
				aDEKACKLIJG2.OnPurchaseSucceeded = (Action<string>)Delegate.Remove(aDEKACKLIJG2.OnPurchaseSucceeded, new Action<string>(OnPurchaseSucceeded));
				PaymentStore aDEKACKLIJG3 = PaymentManager.GetStore();
				aDEKACKLIJG3.OnPurchaseFailed = (Action<string, PurchaseFailureReason>)Delegate.Remove(aDEKACKLIJG3.OnPurchaseFailed, new Action<string, PurchaseFailureReason>(OnPurchaseFailed));
				PaymentStore aDEKACKLIJG4 = PaymentManager.GetStore();
				aDEKACKLIJG4.OnPurchaseRejected = (Action<string>)Delegate.Remove(aDEKACKLIJG4.OnPurchaseRejected, new Action<string>(OnPurchaseUnsuccessful));
				PaymentStore aDEKACKLIJG5 = PaymentManager.GetStore();
				aDEKACKLIJG5.OnVerificationNoResponse = (Action<string>)Delegate.Remove(aDEKACKLIJG5.OnVerificationNoResponse, new Action<string>(OnServerNoResponse));
				PaymentStore aDEKACKLIJG6 = PaymentManager.GetStore();
				aDEKACKLIJG6.OnRestoreCompleted = (Action)Delegate.Remove(aDEKACKLIJG6.OnRestoreCompleted, new Action(OnPurchaseFlowFinished));
				PaymentStore aDEKACKLIJG7 = PaymentManager.GetStore();
				aDEKACKLIJG7.OnRestoreFailed = (Action)Delegate.Remove(aDEKACKLIJG7.OnRestoreFailed, new Action(OnConnectionFailed));
				PaymentStore aDEKACKLIJG8 = PaymentManager.GetStore();
				aDEKACKLIJG8.OnInitialized = (Action)Delegate.Remove(aDEKACKLIJG8.OnInitialized, new Action(OnProductsUpdated));
			}
		}

		public void MakePurchase(ItemInfo item)
		{
			ShowBlocker();
			PaymentManager.GetStore().PurchaseProduct(item.GetMarketId());
		}

		public void RestorePurchases()
		{
			ShowBlocker();
			PaymentManager.GetStore().RestorePurchases();
		}

		private void OnPurchaseDismissed(string productId)
		{
			HideBlocker();
		}

		private void OnPurchaseSucceeded(string productId)
		{
			ItemInfo item = ListSF.FindItemByMarketId(productId);
			RaisePurchaseQuestEvent(QuestEvent.QuestEventType.QUEST_EVENT_PURCHASE, item);
			HideBlocker();
		}

		private void OnPurchaseFailed(string productId, PurchaseFailureReason reason)
		{
			if (reason != PurchaseFailureReason.UserCancelled)
			{
				RaisePurchaseQuestEvent(QuestEvent.QuestEventType.QUEST_EVENT_PURCHASE_UNSUCCESSFUL, null, "Connection");
			}
			HideBlocker();
		}

		private void OnPurchaseUnsuccessful(string productId)
		{
			RaisePurchaseQuestEvent(QuestEvent.QuestEventType.QUEST_EVENT_PURCHASE_UNSUCCESSFUL, null);
			HideBlocker();
		}

		private void OnServerNoResponse(string productId)
		{
			RaisePurchaseQuestEvent(QuestEvent.QuestEventType.QUEST_EVENT_PURCHASE_UNSUCCESSFUL, null, "ServerNoResponse");
			HideBlocker();
		}

		private void OnPurchaseFlowFinished()
		{
			HideBlocker();
		}

		private void OnConnectionFailed()
		{
			RaisePurchaseQuestEvent(QuestEvent.QuestEventType.QUEST_EVENT_PURCHASE_UNSUCCESSFUL, null, "Connection");
			HideBlocker();
		}

		private void OnProductsUpdated()
		{
			_OnProductsUpdateEvent.Invoke();
		}

		private void RaisePurchaseQuestEvent(QuestEvent.QuestEventType p_event, ItemInfo item, string failureReason = null)
		{
			QuestParameters questParameters = ListSF.GetInstance().GetQuestParameters();
			FightIDS savedFightIds = questParameters.fightIds;
			questParameters.fightIds = FightIDS.Empty();
			questParameters.fightResult = string.Empty;
			if (item != null)
			{
				questParameters.purchasedItem = item;
			}
			if (!string.IsNullOrEmpty(failureReason))
			{
				questParameters.purchaseFailureReason = failureReason;
			}
			if (ListSF.GetInstance().RaiseQuestEvent(p_event))
			{
				ListSF.GetInstance().RunQuestActions();
			}
			questParameters.fightIds = savedFightIds;
		}

		private void ShowBlocker()
		{
			_Blocker.SetActive(true);
			_LoadingCircle.Play();
		}

		private void HideBlocker()
		{
			_LoadingCircle.Stop();
			_Blocker.SetActive(false);
		}
	}
}
