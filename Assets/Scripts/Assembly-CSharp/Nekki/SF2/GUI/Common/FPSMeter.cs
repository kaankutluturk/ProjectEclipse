using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

namespace Nekki.SF2.GUI.Common
{
	public class FPSMeter : UIModule
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static float fps;

		[SerializeField]
		private float _UpdateInterval = 0.2f;

		[SerializeField]
		private Text _Label;

		private float timeUntilUpdate;

		private int _LastFramesCount;

		private float lastSampleTime;

		public static float CurrentFps
		{
			get
			{
				return get_FPS();
			}
			private set
			{
				SetFps(value);
			}
		}

		public static float get_FPS()
		{
			return fps;
		}

		private static void SetFps(float value)
		{
			fps = value;
		}

		protected override void Init()
		{
			base.Init();
			SetTime();
		}

		protected override void OnModuleShutdown()
		{
			base.OnModuleShutdown();
		}

		private void Update()
		{
			timeUntilUpdate -= Time.deltaTime;
			if (timeUntilUpdate <= 1E-06f)
			{
				UpdateFpsLabel();
			}
		}

		private void SetTime()
		{
			timeUntilUpdate = _UpdateInterval;
			_LastFramesCount = Time.frameCount;
			lastSampleTime = Time.realtimeSinceStartup;
		}

		private void UpdateFpsLabel()
		{
			int num = Time.frameCount - _LastFramesCount;
			float num2 = Time.realtimeSinceStartup - lastSampleTime;
			SetTime();
			SetFps((float)num / num2);
			_Label.text = string.Format("FPS: {0:F1}", get_FPS());
		}
	}
}
