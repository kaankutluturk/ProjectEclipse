using System.Collections.Generic;
using Nekki.SF2.GUI;
using UnityEngine;
using UnityEngine.UI;

namespace Nekki.SF2.Core.Fights.Controller
{
	public class ActionButtons : SFMonoBehaviour<object>
	{
		public enum ButtonEventType
		{
			OnButtonClick = 0,
			OnButtonPress = 1,
			OnButtonRelease = 2
		}

		[SerializeField]
		private Text _lblRaidChargeCount;

		[SerializeField]
		private ProgressButton _btnKick;

		[SerializeField]
		private ProgressButton _btnPunch;

		[SerializeField]
		private ProgressButton _btnMissile;

		[SerializeField]
		private ProgressButton _btnMagic;

		[SerializeField]
		private ProgressButton _btnRaidCharge;

        private readonly HashSet<FightCID> _ruleHidden = new HashSet<FightCID>();
        private readonly Dictionary<FightCID, bool> _requestedVisibility = new Dictionary<FightCID, bool>();

        private ProgressButton RuleButton(FightCID control)
        {
            switch (control)
            {
                case FightCID.Punch: return _btnPunch;
                case FightCID.Kick: return _btnKick;
                case FightCID.MissileButton: return _btnMissile;
                case FightCID.MagicButton: return _btnMagic;
                case FightCID.RaidChargeButton: return _btnRaidCharge;
                default: return null;
            }
        }

        public void SetRuleBlocked(FightCID control, bool blocked)
        {
            var button = RuleButton(control);
            if (button == null) return;
            if (!_requestedVisibility.ContainsKey(control)) _requestedVisibility[control] = button.gameObject.activeSelf;
            if (blocked) _ruleHidden.Add(control); else _ruleHidden.Remove(control);
            SetRequestedVisibility(control, _requestedVisibility[control]);
        }

        private void SetRequestedVisibility(FightCID control, bool visible)
        {
            _requestedVisibility[control] = visible;
            visible = visible && !_ruleHidden.Contains(control);
            var button = RuleButton(control);
            if (button != null) button.gameObject.SetActive(visible);
            if (control == FightCID.RaidChargeButton && _lblRaidChargeCount != null)
                _lblRaidChargeCount.gameObject.SetActive(visible);
        }

		private void Start()
		{
		}

		private void Update()
		{
		}

		public void Init()
		{
			_btnKick.Init();
			_btnKick.ButtonId = 10;
			_btnKick.AddEventListener(2, ButtonClick);
			_btnKick.AddEventListener(0, ButtonPress);
			_btnKick.AddEventListener(1, ButtonRelease);
			_btnPunch.Init();
			_btnPunch.ButtonId = 9;
			_btnPunch.AddEventListener(2, ButtonClick);
			_btnPunch.AddEventListener(0, ButtonPress);
			_btnPunch.AddEventListener(1, ButtonRelease);
			_btnMissile.Init();
			_btnMissile.ButtonId = 11;
			_btnMissile.AddEventListener(2, ButtonClick);
			_btnMissile.AddEventListener(0, ButtonPress);
			_btnMissile.AddEventListener(1, ButtonRelease);
			_btnMagic.Init();
			_btnMagic.FillSmoothingRate = 10f; // Eclipse: ease the magic charge ring (presentation only)
			_btnMagic.ButtonId = 12;
			_btnMagic.AddEventListener(2, ButtonClick);
			_btnMagic.AddEventListener(0, ButtonPress);
			_btnMagic.AddEventListener(1, ButtonRelease);
			_btnRaidCharge.Init();
			_btnRaidCharge.ButtonId = 13;
			_btnRaidCharge.AddEventListener(2, ButtonClick);
			_btnRaidCharge.AddEventListener(0, ButtonPress);
			_btnRaidCharge.AddEventListener(1, ButtonRelease);
			if (!AssemblyController.GetShowController())
			{
				_btnMissile.transform.position = _btnKick.transform.position;
				_btnMagic.transform.position = _btnPunch.transform.position;
				_btnKick.gameObject.SetActive(false);
				_btnPunch.gameObject.SetActive(false);
				_btnRaidCharge.gameObject.SetActive(false);
			}
			_btnMagic.SetPercentage(100f, 1f);
		}

        public void SetInputPressedVisual(FightCID control, bool pressed)
        {
            ProgressButton button = null;
            switch (control)
            {
                case FightCID.Punch: button = _btnPunch; break;
                case FightCID.Kick: button = _btnKick; break;
                case FightCID.MissileButton: button = _btnMissile; break;
                case FightCID.MagicButton: button = _btnMagic; break;
                case FightCID.RaidChargeButton: button = _btnRaidCharge; break;
            }
            if (button != null) button.SetInputPressedVisual(pressed);
        }

        public List<ProgressButton> GetButtons()
		{
			List<ProgressButton> list = new List<ProgressButton>();
			list.Add(_btnKick);
			list.Add(_btnPunch);
			return list;
		}

		public ProgressButton GetButtonKick()
		{
			return _btnKick;
		}

		public ProgressButton GetButtonPunch()
		{
			return _btnPunch;
		}

		public ProgressButton GetButtonRaidCharge()
		{
			return _btnRaidCharge;
		}

        public ProgressButton GetButtonMagic()
        {
            return _btnMagic;
        }

        public ProgressButton GetButtonMissile()
        {
            return _btnMissile;
        }

		public void SetButtonRaidChargePos(float x, float y)
		{
			_btnRaidCharge.transform.position = new Vector2(x, y);
			_lblRaidChargeCount.transform.position = _btnRaidCharge.transform.position;
		}

		public void SetPunchEnabled(bool value)
		{
			SetRequestedVisibility(FightCID.Punch, value);
		}

		public void SetKickEnabled(bool value)
		{
			SetRequestedVisibility(FightCID.Kick, value);
		}

		public void ShowMagic(bool visible)
		{
			SetRequestedVisibility(FightCID.MagicButton, visible);
		}

		public void ShowRanged(bool visible)
		{
			SetRequestedVisibility(FightCID.MissileButton, visible);
		}

		public void ShowRaidCharge(bool visible)
		{
			SetRequestedVisibility(FightCID.RaidChargeButton, visible);
		}

        // Eclipse: an equipped ability on RaidCharge outside a raid. The raid charge
        // count belongs to raid items, so its label stays hidden.
        public void ShowRaidChargeAbility(bool visible)
        {
            SetRequestedVisibility(FightCID.RaidChargeButton, visible);
            if (_lblRaidChargeCount != null) _lblRaidChargeCount.gameObject.SetActive(false);
        }

		public void SetNeededPercentageToActBtn(FightCID button, float percentage, float _Duration = 0.5f)
		{
			switch (button)
			{
			case FightCID.MagicButton:
				_btnMagic.SetPercentage(percentage, _Duration);
				break;
			case FightCID.MissileButton:
				_btnMissile.SetPercentage(percentage, _Duration);
				break;
			case FightCID.RaidChargeButton:
				_btnRaidCharge.SetPercentage(percentage, _Duration);
				break;
			case FightCID.Punch:
				_btnPunch.SetPercentage(percentage, _Duration);
				break;
			case FightCID.Kick:
				_btnKick.SetPercentage(percentage, _Duration);
				break;
			}
		}

		public void SetBulletsCountToActBtn(FightCID button, int count)
		{
			if (button == FightCID.RaidChargeButton)
			{
				_lblRaidChargeCount.text = count.ToString();
			}
		}

		public void ResetMagicButton()
		{
			_btnMagic.ResetPercentage();
		}

		public void ResetRaidChargeButton()
		{
			_btnRaidCharge.ResetPercentage();
		}

		public virtual void SetVisible(bool visible)
		{
			base.gameObject.SetActive(visible);
		}

		public void ButtonClick(object data)
		{
			FightCID buttonId = (FightCID)data;
			DispatchButtonEvent(ButtonEventType.OnButtonClick, buttonId);
		}

		public void ButtonPress(object data)
		{
			FightCID buttonId = (FightCID)data;
			DispatchButtonEvent(ButtonEventType.OnButtonPress, buttonId);
		}

		public void ButtonRelease(object data)
		{
			FightCID buttonId = (FightCID)data;
			DispatchButtonEvent(ButtonEventType.OnButtonRelease, buttonId);
		}

		private void DispatchButtonEvent(ButtonEventType eventType, FightCID buttonId)
		{
			FightControlEventData eventData = new FightControlEventData();
			eventData.Index = 0;
			eventData.Control = buttonId;
			CallEvent((int)eventType, eventData);
		}

		private float GetSegmentAngle(int count)
		{
			float num = 0.125f;
			float num2 = num * 360f / (float)count;
			if (num2 < 1.7f)
			{
				num2 = 0f;
			}
			return num2;
		}
	}
}
