using System.Collections.Generic;
using Nekki.SF2.GUI;
using Eclipse.Input;
using UnityEngine;

namespace Nekki.SF2.Core.Fights.Controller
{
	public class GameController : SFMonoBehaviour<object>
	{
		public enum ControllerType
		{
			TYPE_NONE = 0,
			TYPE_STICK = 1,
			TYPE_KEYBOARD = 2
		}

		public enum ControlEventType
		{
			OnControlPressed = 0,
			OnControlReleased = 1
		}

		public struct ControllerData
		{
			private object data;

			private ControllerType controllerType;
		}

		private static GameController _Current;

		[SerializeField]
		private Stick _joystick;

		[SerializeField]
		private GameObject _leftContainer;

		private FightCID keyboardDirection;

		[SerializeField]
		private ActionButtons _actionButtons;

		private KeyboardInputPoller keyboardInput = new KeyboardInputPoller();

		private List<int> unusedIntList;

		private int unusedCounter;

		private bool punchEnabled = true;

		private bool kickEnabled = true;

		private bool stickEnabled = true;

		private bool _deviceInputEnabled; // best guess for name

			private FightGamepadInput _gamepadInput;
			private FightGamepadInput _localVersusPlayerTwoInput;
			private bool _localVersusInputEnabled;
			private bool _localVersusKeyboardPlayerOne;
			private bool _hasFocus = true;
			private byte _versusTouch;
            private byte _versusVisualInput;
            private FightCID _versusVisualDirection;

			/// <summary>Held on-screen stick and buttons as a versus input byte; sampled per tick by the versus driver.</summary>
			public byte VersusTouchInput => _versusTouch;
        private readonly Eclipse.Modding.ModUiControlGate<(FightCID, int)> _modUiControls =
            new Eclipse.Modding.ModUiControlGate<(FightCID, int)>();
        private readonly FightControlRuleGate _ruleControls = new FightControlRuleGate();
        private readonly FightControlRestrictions _controlRestrictions = new FightControlRestrictions();
        private static readonly FightCID[] RestrictedActions = { FightCID.Punch, FightCID.Kick, FightCID.MissileButton, FightCID.MagicButton, FightCID.RaidChargeButton };

        public void SetButtonRuleEnabled(FightCID control, bool enabled)
        {
            _controlRestrictions.SetNative(control, !enabled);
            RefreshControlRestriction(control);
        }

        public void SetScriptControlBlocked(object owner, FightCID control, bool blocked)
        {
            _controlRestrictions.SetScript(owner, control, blocked);
            RefreshControlRestriction(control);
        }

        public void ClearScriptControlBlocks()
        {
            _controlRestrictions.ClearScripts();
            foreach (var control in RestrictedActions) RefreshControlRestriction(control);
        }

        private void RefreshControlRestriction(FightCID control)
        {
            bool blocked = _controlRestrictions.IsBlocked(control);
            if (_ruleControls.SetBlocked(control, blocked))
                EmitAvailableControl(1, new FightControlEventData { Index = 0, Control = control });
            _actionButtons.SetRuleBlocked(control, blocked);
        }

		public static GameController CurrentController
		{
			get
			{
				return get_Current();
			}
		}

		public static GameController get_Current()
		{
			return _Current;
		}

		private void Awake()
		{
			_Current = this;
            // Hide the stick's container: the layout editor owns the Stick's own CanvasGroup
            // alpha, and the raid charge button lives beside the stick.
            Eclipse.UI.BattleTouchControls.ApplyPlatformVisibility(_leftContainer != null ? _leftContainer : _joystick.gameObject);
            Eclipse.UI.BattleTouchControls.ApplyPlatformVisibility(_actionButtons.gameObject);
            Eclipse.UI.BattleTouchControls.ApplyTouchLeniency(_joystick.gameObject);
            Eclipse.UI.BattleTouchControls.ApplyTouchLeniency(_actionButtons.gameObject);
            Eclipse.UI.BattleCooldownHud.Attach((RectTransform)transform, _actionButtons);
		}

		private void OnDestroy()
		{
			ReleaseLocalVersusInputs();
			_Current = null;
		}

			private void OnApplicationFocus(bool hasFocus)
			{
				_hasFocus = hasFocus;
				if (!hasFocus)
				{
					ReleaseLocalVersusInputs();
				}
			}

			private void OnDisable()
			{
				ReleaseLocalVersusInputs();
				_versusTouch = 0;
			}

		private void Start()
		{
		}

			private void Update()
			{
            Eclipse.UI.BattleTouchControls.KeepSixteenByNinePositions((RectTransform)transform,
                _joystick.transform.parent as RectTransform, (RectTransform)_actionButtons.transform);
            Eclipse.UI.ControlLayout.Apply((RectTransform)transform);
            SyncModUiCapture();
            if (_localVersusInputEnabled) RefreshVersusInputVisual();
				if (!_localVersusInputEnabled) keyboardInput.Render();
				if (_deviceInputEnabled)
			{
				// Versus devices are sampled per simulation tick by Eclipse.Multiplayer.VersusTickDriver.
				if (!_localVersusInputEnabled)
				{
					GetGamepadInput().Poll(!AssemblyController.GetMarket().GetIsSteamMarket());
				}
			}
		}

		public void ConfigureLocalVersusInput(bool enabled, bool keyboardPlayerOne)
		{
			if (_localVersusInputEnabled == enabled && _localVersusKeyboardPlayerOne == (enabled && keyboardPlayerOne))
			{
				return;
			}
			ReleaseLocalVersusInputs();
			_localVersusInputEnabled = enabled;
				_localVersusKeyboardPlayerOne = enabled && keyboardPlayerOne;
				_localVersusPlayerTwoInput = null;
				_versusTouch = 0;
				_hasFocus = Application.isFocused;
				// Local input owns complete keyboard snapshots, including key releases.
				// The campaign's legacy keyboard/debug dispatcher remains separate.
				keyboardInput.IsEnabled = _deviceInputEnabled && !enabled;
		}

		public void Init(bool enablePunch = true, bool enableKick = true, bool enableStick = true)
		{
			_actionButtons.Init();
			punchEnabled = enablePunch;
			kickEnabled = enableKick;
			stickEnabled = enableStick;
			InitController();
		}

		public void InitController()
		{
			if (!AssemblyController.GetMarket().GetIsSteamMarket())
			{
				AddActionButtonListeners(punchEnabled, kickEnabled);
				AddJoystickListeners();
			}
			InitKeyboardInput();
			ResetGamepadInput();
			unusedCounter = 0;
		}

		public Stick GetJoystick()
		{
			return _joystick;
		}

		public List<ProgressButton> GetButtons()
		{
			return _actionButtons.GetButtons();
		}

		public SFButton GetButtonKick()
		{
			return _actionButtons.GetButtonKick();
		}

		public SFButton GetButtonPunch()
		{
			return _actionButtons.GetButtonPunch();
		}

		public ActionButtons GetActionButtons()
		{
			return _actionButtons;
		}

		public void IsShowController(bool visible)
		{
			base.gameObject.SetActive(visible);
			_joystick.gameObject.SetActive(visible);
			_actionButtons.gameObject.SetActive(visible);
			if (!visible)
			{
				if (!stickEnabled)
				{
					_joystick.gameObject.SetActive(false);
				}
				if (!punchEnabled)
				{
					_actionButtons.GetButtonPunch().gameObject.SetActive(false);
				}
				if (!kickEnabled)
				{
					_actionButtons.GetButtonKick().gameObject.SetActive(false);
				}
			}
		}

		public void SetPunchEnabled(bool value)
		{
			_actionButtons.SetPunchEnabled(value);
			punchEnabled = value;
		}

		public void SetKickEnabled(bool value)
		{
			_actionButtons.SetKickEnabled(value);
			kickEnabled = value;
		}

		public void SetStickEnabled(bool value)
		{
			_joystick.gameObject.SetActive(value);
			stickEnabled = value;
		}

		public bool GetPunchEnabled()
		{
			return punchEnabled;
		}

		public bool GetKickEnabled()
		{
			return kickEnabled;
		}

		public bool GetStickEnabled()
		{
			return stickEnabled;
		}

		public void ResetController()
		{
			keyboardInput.Clear();
			keyboardInput.RemoveEventListener(0, HandleKeyPressed);
			keyboardInput.RemoveEventListener(1, HandleKeyReleased);
			_joystick.RemoveEventListener(0, HandleControlPressed);
			_joystick.RemoveEventListener(1, HandleControlPressed);
			_joystick.RemoveEventListener(2, HandleControlReleased);
			_actionButtons.RemoveEventListener(1, HandleControlPressed);
			_actionButtons.RemoveEventListener(2, HandleControlReleased);
			StopController();
		}

		public void AddKeysModels()
		{
			keyboardInput.AddKey(Eclipse.Input.FightKeyBindings.Get(KeyCode.O), FightCID.Punch);
			keyboardInput.AddKey(Eclipse.Input.FightKeyBindings.Get(KeyCode.P), FightCID.Kick);
			keyboardInput.AddKey(Eclipse.Input.FightKeyBindings.Get(KeyCode.K), FightCID.MissileButton);
			keyboardInput.AddKey(Eclipse.Input.FightKeyBindings.Get(KeyCode.L), FightCID.MagicButton);
			keyboardInput.AddKey(Eclipse.Input.FightKeyBindings.Get(KeyCode.J), FightCID.RaidChargeButton);
		}

		public bool IsQuadrantEnabled(FightCID control)
		{
			if (!punchEnabled && control == FightCID.Punch)
			{
				return false;
			}
			if (!kickEnabled && control == FightCID.Kick)
			{
				return false;
			}
			if (!stickEnabled && IsDirectionQuadrantNonZero(control))
			{
				return false;
			}
			return true;
		}

		public void StartController()
		{
			SetDeviceInputEnabled(true);
		}

		public void StopController()
		{
			SetDeviceInputEnabled(false);
		}

		public void ClearButtonsAppearance()
		{
            ClearScriptControlBlocks();
            foreach (var control in RestrictedActions)
                SetButtonRuleEnabled(control, true);
			if (AssemblyController.GetShowController())
			{
				SetKickEnabled(true);
				SetPunchEnabled(true);
			}
		}

		public static bool IsDirectionQuadrant(FightCID control)
		{
			return control >= FightCID.QuadrantZero && control <= FightCID.QuadrantUpBack;
		}

		private void AddJoystickListeners()
		{
			_joystick.RemoveEventListener(0, HandleControlPressed);
			_joystick.AddEventListener(0, HandleControlPressed);
			_joystick.RemoveEventListener(1, HandleControlPressed);
			_joystick.AddEventListener(1, HandleControlPressed);
			_joystick.RemoveEventListener(2, HandleControlReleased);
			_joystick.AddEventListener(2, HandleControlReleased);
		}

		private void AddActionButtonListeners(bool enablePunch = true, bool enableKick = true)
		{
			_actionButtons.RemoveEventListener(1, HandleControlPressed);
			_actionButtons.AddEventListener(1, HandleControlPressed);
			_actionButtons.RemoveEventListener(2, HandleControlReleased);
			_actionButtons.AddEventListener(2, HandleControlReleased);
			_actionButtons.SetPunchEnabled(enablePunch);
			_actionButtons.SetKickEnabled(enableKick);
		}

		private void InitKeyboardInput()
		{
			keyboardInput.Init();
			keyboardInput.RemoveEventListener(0, HandleKeyPressed);
			keyboardInput.RemoveEventListener(1, HandleKeyReleased);
			keyboardInput.AddEventListener(0, HandleKeyPressed);
			keyboardInput.AddEventListener(1, HandleKeyReleased);
			keyboardInput.AddKey(KeyCode.Keypad0, FightCID.QuadrantZero);
			AddKeysModels();
			keyboardInput.AddKey(KeyCode.RightArrow, FightCID.NextFrameButton);
			keyboardInput.AddKey(KeyCode.Keypad1, FightCID.WinRoundButton);
			keyboardInput.AddKey(KeyCode.Keypad2, FightCID.WinFightButton);
			keyboardInput.AddKey(KeyCode.Keypad3, FightCID.ResetRoundButton);
			keyboardInput.AddKey(KeyCode.Keypad4, FightCID.ResetFightButton);
			keyboardInput.AddKey(KeyCode.Keypad5, FightCID.LossRoundButton);
			keyboardInput.AddKey(KeyCode.Keypad6, FightCID.LossFightButton);
			keyboardInput.AddKey(KeyCode.Keypad7, FightCID.RechargeMagic);
			keyboardInput.AddKey(KeyCode.Keypad8, FightCID.IncreaseComboHit);
			keyboardInput.AddKey(KeyCode.Keypad9, FightCID.IncreaseStyle);
			keyboardInput.AddKey(KeyCode.Keypad0, FightCID.SetPlayerAllHitsCritical);
			keyboardInput.AddKey(KeyCode.Alpha1, FightCID.WinRoundButton);
			keyboardInput.AddKey(KeyCode.Alpha2, FightCID.WinFightButton);
			keyboardInput.AddKey(KeyCode.Alpha3, FightCID.ResetRoundButton);
			keyboardInput.AddKey(KeyCode.Alpha4, FightCID.ResetFightButton);
			keyboardInput.AddKey(KeyCode.Alpha5, FightCID.LossRoundButton);
			keyboardInput.AddKey(KeyCode.Alpha6, FightCID.LossFightButton);
			keyboardInput.AddKey(KeyCode.Alpha7, FightCID.RechargeMagic);
			keyboardInput.AddKey(KeyCode.Alpha8, FightCID.IncreaseComboHit);
			keyboardInput.AddKey(KeyCode.Alpha9, FightCID.IncreaseStyle);
			keyboardInput.AddKey(KeyCode.Alpha0, FightCID.SetPlayerAllHitsCritical);
			keyboardInput.AddKey(KeyCode.F1, FightCID.SlowModeKey);
			keyboardInput.AddKey(KeyCode.F3, FightCID.ShowEdgesButton);
			keyboardInput.AddKey(KeyCode.F4, FightCID.PauseButton);
			keyboardInput.AddKey(KeyCode.F5, FightCID.ShowDebugPerksButton);
			keyboardInput.AddKey(KeyCode.F6, FightCID.SetPlayerImmortality);
			keyboardInput.AddKey(KeyCode.F7, FightCID.SetBotImmortality);
			keyboardInput.AddKey(KeyCode.F10, FightCID.FullscreenMode);
			keyboardInput.AddKey(KeyCode.F11, FightCID.EnableMinScale);
			keyboardInput.AddKey(KeyCode.F12, FightCID.TestTactic);
			keyboardInput.AddKey(KeyCode.M, FightCID.SoundMuteButton);
			keyboardInput.AddKey(KeyCode.B, FightCID.StartBenchmarkKey);
			keyboardInput.AddKey(KeyCode.U, FightCID.StartSuper);
		}

		private void ResetGamepadInput()
		{
			GetGamepadInput().Reset();
		}

		private void SetDeviceInputEnabled(bool value)
		{
			if (!value && _deviceInputEnabled)
			{
				GetGamepadInput().ReleaseAll();
			}
			_deviceInputEnabled = value;
				keyboardInput.IsEnabled = value && !_localVersusInputEnabled;
			if (!value)
			{
				ReleaseLocalVersusInputs();
			}
		}

			private FightGamepadInput GetGamepadInput()
			{
				if (_gamepadInput == null)
					_gamepadInput = new FightGamepadInput(IsQuadrantEnabled, SendGamepadControlEvent);
				return _gamepadInput;
			}

			private FightGamepadInput GetLocalVersusPlayerTwoInput()
			{
				if (_localVersusPlayerTwoInput == null)
					_localVersusPlayerTwoInput = new FightGamepadInput(control => true,
						(eventType, control) => EmitControl(eventType, new FightControlEventData { Index = 1, Control = control }),
						_localVersusKeyboardPlayerOne ? GamePad.Player.One : GamePad.Player.Two);
				return _localVersusPlayerTwoInput;
			}

			private void ReleaseLocalVersusInputs()
			{
				_gamepadInput?.ReleaseAll();
				_localVersusPlayerTwoInput?.ReleaseAll();
                SetVersusInputVisual(Eclipse.Multiplayer.Online.NetInput.Neutral);
			}

        // Versus applies controls directly to Fight on its tick timeline. The HUD
        // observes the local device snapshot without emitting another combat event.
        internal void SetVersusInputVisual(byte input)
        {
            _versusVisualInput = input;
            RefreshVersusInputVisual();
        }

        private void RefreshVersusInputVisual()
        {
            byte input = _localVersusInputEnabled && _deviceInputEnabled && _hasFocus
                && !Eclipse.Multiplayer.LocalVersusMenu.BlocksFightInput
                && !Eclipse.UI.Modding.ModUiGameBridge.BlocksGameplayInput
                ? _versusVisualInput : Eclipse.Multiplayer.Online.NetInput.Neutral;
            var direction = (FightCID)Eclipse.Multiplayer.Online.NetInput.Direction(input);
            if (!IsQuadrantEnabled(direction)) direction = FightCID.QuadrantZero;
            if (_joystick != null)
            {
                if (direction != _versusVisualDirection && _versusVisualDirection != FightCID.QuadrantZero)
                    _joystick.SetInputDirectionVisual(_versusVisualDirection, false);
                if (direction != FightCID.QuadrantZero) _joystick.SetInputDirectionVisual(direction, true);
            }
            _versusVisualDirection = direction;
            SetVersusButtonVisual(input, Eclipse.Multiplayer.Online.NetInput.Punch, FightCID.Punch);
            SetVersusButtonVisual(input, Eclipse.Multiplayer.Online.NetInput.Kick, FightCID.Kick);
            SetVersusButtonVisual(input, Eclipse.Multiplayer.Online.NetInput.Ranged, FightCID.MissileButton);
            SetVersusButtonVisual(input, Eclipse.Multiplayer.Online.NetInput.Magic, FightCID.MagicButton);
        }

        private void SetVersusButtonVisual(byte input, byte bit, FightCID control)
        {
            if (_actionButtons != null)
                _actionButtons.SetInputPressedVisual(control, (input & bit) != 0
                    && IsQuadrantEnabled(control) && !_controlRestrictions.IsBlocked(control));
        }

		private void SendGamepadControlEvent(int eventType, FightCID control)
        {
            EmitControl(eventType, new FightControlEventData { Index = 0, Control = control });
        }

        private void SyncModUiCapture()
        {
            foreach (var control in _modUiControls.SetCaptured(Eclipse.UI.Modding.ModUiGameBridge.BlocksGameplayInput))
            {
                if (control.Item2 == 0)
                {
                    _actionButtons.SetInputPressedVisual(control.Item1, false);
                    _joystick.SetInputDirectionVisual(control.Item1, false);
                }
                CallEvent(1, new FightControlEventData { Index = control.Item2, Control = control.Item1 });
            }
        }

        private void EmitControl(int eventType, FightControlEventData data)
        {
            if (data.Index == 0 && !(eventType == 0 ? _ruleControls.Press(data.Control) : _ruleControls.Release(data.Control))) return;
            EmitAvailableControl(eventType, data);
        }

        private void EmitAvailableControl(int eventType, FightControlEventData data)
        {
            SyncModUiCapture();
            var key = (data.Control, data.Index);
            if (eventType == 0 ? _modUiControls.Press(key) : _modUiControls.Release(key))
            {
                if (data.Index == 0)
                {
                    _actionButtons.SetInputPressedVisual(data.Control, eventType == 0);
                    _joystick.SetInputDirectionVisual(data.Control, eventType == 0);
                }
                CallEvent(eventType, data);
            }
        }

		private void HandleControlPressed(object data)
		{
			FightControlEventData eventData = (FightControlEventData)data;
			// Touch presses reach a versus fight through the tick driver, not as events.
			if (_localVersusInputEnabled) _versusTouch = Eclipse.Multiplayer.VersusInputSampler.ApplyControl(_versusTouch, 0, eventData.Control);
			if (eventData.Control != FightCID.QuadrantZero && IsQuadrantEnabled(eventData.Control))
			{
				EmitControl(0, eventData);
			}
		}

		private void HandleControlReleased(object data)
		{
			FightControlEventData eventData = (FightControlEventData)data;
			if (_localVersusInputEnabled) _versusTouch = Eclipse.Multiplayer.VersusInputSampler.ApplyControl(_versusTouch, 1, eventData.Control);
			if (IsQuadrantEnabled(eventData.Control))
			{
				EmitControl(1, eventData);
			}
		}

		private void HandleKeyPressed(object data)
		{
			FightControlEventData eventData = (FightControlEventData)data;
			if (!AssemblyController.GetMarket().GetIsWinStoreMarket() || !IsDirectionQuadrant(eventData.Control))
			{
				if (eventData.Control != FightCID.QuadrantZero && IsQuadrantEnabled(eventData.Control))
				{
					EmitControl(0, eventData);
				}
			}
			else
			{
				UpdateKeyboardDirection(eventData);
			}
		}

		private void HandleKeyReleased(object data)
		{
			FightControlEventData eventData = (FightControlEventData)data;
			if (!AssemblyController.GetMarket().GetIsWinStoreMarket() || !IsDirectionQuadrant(eventData.Control))
			{
				if (IsQuadrantEnabled(eventData.Control))
				{
					EmitControl(1, eventData);
				}
			}
			else
			{
				UpdateKeyboardDirection(eventData);
			}
		}

		private bool IsDirectionQuadrantNonZero(FightCID control)
		{
			return control > FightCID.QuadrantZero && control <= FightCID.QuadrantUpBack;
		}

		private void UpdateKeyboardDirection(FightControlEventData eventData)
		{
			List<FightCID> list = new List<FightCID>();
			int i = 0;
			for (int count = keyboardInput.keyBindingsByPlayer.Count; i < count; i++)
			{
				foreach (FightControlEventData.KeyboardBinding item in keyboardInput.keyBindingsByPlayer[i])
				{
					if (Eclipse.Input.EclipseInput.GetKeyDown(item.Key) || Eclipse.Input.EclipseInput.GetKey(item.Key))
					{
						if (!item.isActive)
						{
							item.isActive = true;
						}
						list.Add(item.Index);
					}
					else if (item.isActive)
					{
						item.isActive = false;
						eventData.Control = item.Index;
						EmitControl(1, eventData);
					}
				}
			}
			FightCID direction = FightCID.QuadrantZero;
			if (list.Count == 1)
			{
				direction = list[0];
			}
			else if (list.Count > 1)
			{
				FightCID eCHINOPKGGI2 = list[0];
				FightCID eCHINOPKGGI3 = list[1];
				switch (eCHINOPKGGI2)
				{
				case FightCID.QuadrantUp:
					switch (eCHINOPKGGI3)
					{
					case FightCID.QuadrantForward:
						direction = FightCID.QuadrantUpForward;
						break;
					case FightCID.QuadrantBack:
						direction = FightCID.QuadrantUpBack;
						break;
					}
					break;
				case FightCID.QuadrantForward:
					switch (eCHINOPKGGI3)
					{
					case FightCID.QuadrantUp:
						direction = FightCID.QuadrantUpForward;
						break;
					case FightCID.QuadrantDown:
						direction = FightCID.QuadrantDownForward;
						break;
					}
					break;
				case FightCID.QuadrantDown:
					switch (eCHINOPKGGI3)
					{
					case FightCID.QuadrantForward:
						direction = FightCID.QuadrantDownForward;
						break;
					case FightCID.QuadrantBack:
						direction = FightCID.QuadrantDownBack;
						break;
					}
					break;
				case FightCID.QuadrantBack:
					switch (eCHINOPKGGI3)
					{
					case FightCID.QuadrantUp:
						direction = FightCID.QuadrantUpBack;
						break;
					case FightCID.QuadrantDown:
						direction = FightCID.QuadrantDownBack;
						break;
					}
					break;
				}
			}
			if (keyboardDirection != direction)
			{
				if (keyboardDirection != FightCID.QuadrantZero)
				{
					eventData.Control = keyboardDirection;
					EmitControl(1, eventData);
				}
				keyboardDirection = direction;
				if (direction != FightCID.QuadrantZero)
				{
					eventData.Control = direction;
					EmitControl(0, eventData);
				}
			}
		}

		public void SetScale(float scale)
		{
			_leftContainer.transform.localScale = new Vector3(scale, scale);
			_actionButtons.transform.localScale = new Vector3(scale, scale);
		}
	}
}
