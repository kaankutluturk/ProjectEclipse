using System;
using UnityEngine;

namespace Nekki.SF2.GUI.Dialogs
{
	public class ExitDialog : BaseDialog
	{
		public const float CONTENT_PADDING = 40f;

		private bool isFightExit;

		private Action<object> exitCallback;

		private static bool _isOpened;

		[SerializeField]
		protected LabelAlias _text;

		public static bool IsDialogOpen
		{
			get
			{
				return get_IsOpened();
			}
		}

		public static bool get_IsOpened()
		{
			return _isOpened;
		}

		public override void Init(object data)
		{
			if (data != null)
			{
				ExitDialogData exitData = (ExitDialogData)data;
				isFightExit = exitData.IsInFight;
				exitCallback = exitData.Dlg;
				if (isFightExit)
				{
					IsPausing = false;
				}
			}
			base.Init((!isFightExit) ? "dlgExitTitle" : "dlgExitFightTitle", "dlgExitButton", "CANCEL", FooterType.FOOTER_BOTH);
			_isOpened = true;
		}

		private void OnDestroy()
		{
			_isOpened = false;
		}

		protected override void SetupContent()
		{
			_text.alignment = TextAnchor.MiddleCenter;
			_text.transform.SetLocalX(0f);
			_text.transform.SetLocalY(0f);
			_text.set_LabelFontSize(103);
			_text.color = Constants.DialogTextColor;
			_text.set_Alias((!isFightExit) ? "dlgExitMessage" : "dlgExitFightMessage");
		}

		protected override void SetupFooter(FooterType footer)
		{
			base.SetupFooter(footer);
			_btnOK.RemoveAllEventListener();
			_btnOK.AddEventListener(2, OnExitConfirmed);
		}

		private void OnExitConfirmed(object data)
		{
			if (isFightExit)
			{
				exitCallback(0);
			}
			else
			{
				GameUtils.ExitApplication();
			}
			base.OnClose(data);
		}
	}
}
