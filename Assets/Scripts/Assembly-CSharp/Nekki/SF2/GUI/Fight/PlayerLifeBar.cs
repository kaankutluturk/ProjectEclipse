using CodeStage.AntiCheat.ObscuredTypes;
using Eclipse.Underworld.UI;
using UnityEngine;

namespace Nekki.SF2.GUI.Fight
{
	public class PlayerLifeBar : MonoBehaviour
	{
		[SerializeField]
		private ResolutionImageSkew _background;

		[SerializeField]
		private ResolutionImageSkew _healthBar;

		[SerializeField]
		private ResolutionImageSkew _hitBar;

		private ModelParameters fighterModel;

		private float targetHealthValue;

		private float targetHitValue;

		private float currentHealthValue;

		private float currentHitValue;

		private int healthBarFramesLeft;

		private int hitBarFramesLeft;

		private float hitBarDelay;

		private bool _lockLifeUpdate;

		private readonly UnderworldRaidLifeBarTransition _raidTransition =
			new UnderworldRaidLifeBarTransition();

		private readonly UnderworldRaidLifeBarStyle _raidStyle =
			new UnderworldRaidLifeBarStyle();

		public void SetRaidStyle(bool raidBoss)
		{
			_raidStyle.Apply(_healthBar, _background, raidBoss);
		}

		public bool IsLifeUpdateLocked
		{
			get
			{
				return get_LockLifeUpdate();
			}
			set
			{
				set_LockLifeUpdate(value);
			}
		}

		public bool get_LockLifeUpdate()
		{
			return _lockLifeUpdate;
		}

		public void set_LockLifeUpdate(bool value)
		{
			_lockLifeUpdate = value;
		}

		public RectTransform get_rectTransform()
		{
			return (RectTransform)base.transform;
		}

		public void Init(ModelParameters JCICKLIMBEF)
		{
			// Health and the delayed damage bar drain per tick; blend between ticks.
			Eclipse.Rendering.Interpolation.TickPresentationSmoother.AttachFill(_healthBar,
				Eclipse.Rendering.Interpolation.TickPresentationSmoother.Clock.Fight);
			Eclipse.Rendering.Interpolation.TickPresentationSmoother.AttachFill(_hitBar,
				Eclipse.Rendering.Interpolation.TickPresentationSmoother.Clock.Fight);
			fighterModel = JCICKLIMBEF;
			Eclipse.Multiplayer.PvpRecoverableBar.Attach(_healthBar, JCICKLIMBEF);
			ResetState();
		}

		private void ResetState()
		{
			hitBarDelay = 0f;
			_lockLifeUpdate = false;
			currentHealthValue = 0f;
			healthBarFramesLeft = 0;
			currentHitValue = 0f;
			hitBarFramesLeft = 0;
			targetHealthValue = 0f;
			targetHitValue = 0f;
			ResetLife();
		}

		public void ResetLife()
		{
			_raidTransition.Reset(fighterModel.RemainingHealthBars);
			hitBarDelay = 0f;
			SetValBarValue(GetHealthFraction());
			SetHitBarValue(GetHealthFraction());
		}

		public virtual void Render()
		{
			if (!_lockLifeUpdate)
			{
				UpdateLife();
			}
			AnimateHealthBar();
			AnimateHitBar();
			float num = 1f / (float)GameUtils.GetSlowMode();
			if (hitBarDelay > 0f)
			{
				hitBarDelay -= num;
				if (hitBarDelay <= 0f)
				{
					SetHitBarValue(targetHealthValue, 30);
				}
			}
		}

		private void AnimateHealthBar()
		{
			if (currentHealthValue != targetHealthValue)
			{
				float num = 0f;
				if (healthBarFramesLeft < 1)
				{
					num = targetHealthValue;
				}
				else
				{
					float num2 = currentHealthValue - targetHealthValue;
					float num3 = num2 / (float)healthBarFramesLeft;
					num = currentHealthValue - num3;
				}
				ApplyHealthBarValue(num);
				healthBarFramesLeft--;
			}
		}

		private void AnimateHitBar()
		{
			if (currentHitValue != targetHitValue)
			{
				float num = 0f;
				if (hitBarFramesLeft < 1)
				{
					num = targetHitValue;
				}
				else
				{
					float num2 = currentHitValue - targetHitValue;
					float num3 = num2 / (float)hitBarFramesLeft;
					num = currentHitValue - num3;
				}
				ApplyHitBarValue(num);
				hitBarFramesLeft--;
			}
		}

		private void UpdateLife()
		{
			float num = GetHealthFraction();
			UnderworldRaidLifeBarUpdate raidUpdate = _raidTransition.Update(
				num,
				fighterModel.RemainingHealthBars,
				fighterModel.HealthBarCount,
				currentHealthValue,
				currentHitValue,
				targetHealthValue);
			if (raidUpdate.Handled)
			{
				ApplyRaidLifeBarUpdate(raidUpdate);
				return;
			}
			float num2 = num - targetHealthValue;
			if (num2 != 0f)
			{
				if (num2 > 0f)
				{
					SetValBarValue(num, 10);
					SetHitBarValue(num, 30);
				}
				else
				{
					hitBarDelay = ((Mathf.Abs(num2) < 0.01f) ? 1 : 60);
					SetValBarValue(num, 10);
				}
			}
		}

		private void ApplyRaidLifeBarUpdate(UnderworldRaidLifeBarUpdate update)
		{
			if (update.ResetLife)
			{
				ResetLife();
				return;
			}
			if (update.SetHitDelay)
			{
				hitBarDelay = update.HitDelay;
			}
			if (update.SetHealthBar)
			{
				SetValBarValue(update.HealthBarValue, update.HealthBarFrames);
			}
			if (update.SetHitBar)
			{
				SetHitBarValue(update.HitBarValue, update.HitBarFrames);
			}
		}

		public void SetValBarValue(float value, int frames = 0)
		{
			targetHealthValue = value;
			healthBarFramesLeft = frames;
			if (healthBarFramesLeft == 0)
			{
				ApplyHealthBarValue(targetHealthValue);
			}
		}

		public void SetHitBarValue(float value, int frames = 0)
		{
			targetHitValue = value;
			hitBarFramesLeft = frames;
			if (hitBarFramesLeft == 0)
			{
				ApplyHitBarValue(targetHitValue);
			}
		}

		private void ApplyHealthBarValue(float value)
		{
			if (_healthBar != null && _healthBar.fillAmount != value)
			{
				_healthBar.fillAmount = value;
			}
			currentHealthValue = value;
		}

		private void ApplyHitBarValue(float value)
		{
			if (_hitBar != null && _hitBar.fillAmount != value)
			{
				_hitBar.fillAmount = value;
			}
			currentHitValue = value;
		}

		private float GetHealthFraction()
		{
			float num = fighterModel == null ? 0f : fighterModel.CurrentHealthBarFraction;
			if (num > 0f && num < FightGUI.GetLifeBarMin())
			{
				num = FightGUI.GetLifeBarMin();
			}
			return num;
		}
	}
}
