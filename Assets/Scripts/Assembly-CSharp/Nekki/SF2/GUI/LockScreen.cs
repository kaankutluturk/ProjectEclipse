using System.Diagnostics;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Nekki.SF2.GUI
{
	public class LockScreen : MonoBehaviour
	{
		[SerializeField]
		private Color visibleColor;

		[SerializeField]
		private Color invisibleColor;

		[SerializeField]
		private Vector3 rotationAngle;

		[SerializeField]
		private float rotationInterval;

		[SerializeField]
		private Image background;

		[SerializeField]
		private ResolutionImage rotateImg;

		private Tween tween;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static LockScreen instance;

		public static LockScreen BPCBBHAKFDM
		{
			get
			{
				return get_Instance();
			}
			private set
			{
				set_Instance(value);
			}
		}

		static LockScreen()
		{
			set_Instance(null);
		}

		public static LockScreen get_Instance()
		{
			return instance;
		}

		private static void set_Instance(LockScreen value)
		{
			instance = value;
		}

		private void Start()
		{
			Init();
		}

		public void Init()
		{
			set_Instance(this);
			SetLocked(false);
			Object.DontDestroyOnLoad(base.gameObject);
		}

		public static bool Lock(bool IJHFJPBBNEJ, bool KFIECNIMAOA = false)
		{
			if (get_Instance() != null)
			{
				get_Instance().SetLocked(IJHFJPBBNEJ, KFIECNIMAOA);
				return true;
			}
			return false;
		}

		private void SetLocked(bool IJHFJPBBNEJ, bool KFIECNIMAOA = false)
		{
			if (base.gameObject != null)
			{
				base.gameObject.SetActive(IJHFJPBBNEJ);
			}
			if (IJHFJPBBNEJ)
			{
				background.color = ((!KFIECNIMAOA) ? invisibleColor : visibleColor);
			}
			if (rotateImg != null)
			{
				rotateImg.gameObject.SetActive(IJHFJPBBNEJ && KFIECNIMAOA);
				SetSpinnerRotating(IJHFJPBBNEJ && KFIECNIMAOA);
			}
		}

		private void SetSpinnerRotating(bool IJHFJPBBNEJ)
		{
			if (tween != null)
			{
				tween.Kill();
				tween = null;
			}
			if (IJHFJPBBNEJ)
			{
				tween = DOTween.Sequence().AppendInterval(rotationInterval).AppendCallback(() =>
				{
					rotateImg.transform.Rotate(rotationAngle);
				})
					.SetLoops(-1, LoopType.Restart);
			}
		}
	}
}
