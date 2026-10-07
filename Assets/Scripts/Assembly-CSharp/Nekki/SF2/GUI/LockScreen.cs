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

		public static LockScreen Current
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

		public static bool Lock(bool locked, bool visible = false)
		{
			if (get_Instance() != null)
			{
				get_Instance().SetLocked(locked, visible);
				return true;
			}
			return false;
		}

		private void SetLocked(bool locked, bool visible = false)
		{
			if (base.gameObject != null)
			{
				base.gameObject.SetActive(locked);
			}
			if (locked)
			{
				background.color = ((!visible) ? invisibleColor : visibleColor);
			}
			if (rotateImg != null)
			{
				rotateImg.gameObject.SetActive(locked && visible);
				SetSpinnerRotating(locked && visible);
			}
		}

		private void SetSpinnerRotating(bool rotating)
		{
			if (tween != null)
			{
				tween.Kill();
				tween = null;
			}
			if (rotating)
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
