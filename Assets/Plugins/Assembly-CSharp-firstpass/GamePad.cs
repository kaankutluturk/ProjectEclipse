using System;
using UnityEngine;

public static class GamePad
{
	public enum Button // best guess for name
	{
		A = 0,
		B = 1,
		Y = 2,
		X = 3,
		RightShoulder = 4,
		LeftShoulder = 5,
		RightStick = 6,
		LeftStick = 7,
		Back = 8,
		Start = 9
	}

	public enum Trigger // best guess for name
	{
		LeftTrigger = 0,
		RightTrigger = 1
	}

	public enum Stick // best guess for name
	{
		LeftStick = 0,
		RightStick = 1,
		Dpad = 2
	}

	public enum Player // best guess for name
	{
		Any = 0,
		One = 1,
		Two = 2,
		Three = 3,
		Four = 4
	}

	public static bool GetButtonDown(Button button, Player player) // best guess for name
	{
		return Eclipse.Input.EclipseInput.GetGamepadButton((int)button, (int)player, 1);
	}

	public static bool GetButtonUp(Button button, Player player) // best guess for name
	{
		return Eclipse.Input.EclipseInput.GetGamepadButton((int)button, (int)player, 2);
	}

	public static bool GetButton(Button button, Player player) // best guess for name
	{
		return Eclipse.Input.EclipseInput.GetGamepadButton((int)button, (int)player);
	}

	public static Vector2 GetStick(Stick stick, Player player, bool raw = false) // best guess for name
	{
		return Eclipse.Input.EclipseInput.GetGamepadStick((int)stick, (int)player, raw);
	}

	public static float GetTrigger(Trigger trigger, Player player, bool raw = false) // best guess for name
	{
		return Eclipse.Input.EclipseInput.GetGamepadTrigger((int)trigger, (int)player, raw);
	}

	private static KeyCode GetKeyCode(Button button, Player player)
	{
		switch (player)
		{
		case Player.One:
			switch (button)
			{
			case Button.A:
				return KeyCode.Joystick1Button0;
			case Button.B:
				return KeyCode.Joystick1Button1;
			case Button.X:
				return KeyCode.Joystick1Button2;
			case Button.Y:
				return KeyCode.Joystick1Button3;
			case Button.RightShoulder:
				return KeyCode.Joystick1Button5;
			case Button.LeftShoulder:
				return KeyCode.Joystick1Button4;
			case Button.Back:
				return KeyCode.Joystick1Button6;
			case Button.Start:
				return KeyCode.Joystick1Button7;
			case Button.LeftStick:
				return KeyCode.Joystick1Button8;
			case Button.RightStick:
				return KeyCode.Joystick1Button9;
			}
			break;
		case Player.Two:
			switch (button)
			{
			case Button.A:
				return KeyCode.Joystick2Button0;
			case Button.B:
				return KeyCode.Joystick2Button1;
			case Button.X:
				return KeyCode.Joystick2Button2;
			case Button.Y:
				return KeyCode.Joystick2Button3;
			case Button.RightShoulder:
				return KeyCode.Joystick2Button5;
			case Button.LeftShoulder:
				return KeyCode.Joystick2Button4;
			case Button.Back:
				return KeyCode.Joystick2Button6;
			case Button.Start:
				return KeyCode.Joystick2Button7;
			case Button.LeftStick:
				return KeyCode.Joystick2Button8;
			case Button.RightStick:
				return KeyCode.Joystick2Button9;
			}
			break;
		case Player.Three:
			switch (button)
			{
			case Button.A:
				return KeyCode.Joystick3Button0;
			case Button.B:
				return KeyCode.Joystick3Button1;
			case Button.X:
				return KeyCode.Joystick3Button2;
			case Button.Y:
				return KeyCode.Joystick3Button3;
			case Button.RightShoulder:
				return KeyCode.Joystick3Button5;
			case Button.LeftShoulder:
				return KeyCode.Joystick3Button4;
			case Button.Back:
				return KeyCode.Joystick3Button6;
			case Button.Start:
				return KeyCode.Joystick3Button7;
			case Button.LeftStick:
				return KeyCode.Joystick3Button8;
			case Button.RightStick:
				return KeyCode.Joystick3Button9;
			}
			break;
		case Player.Four:
			switch (button)
			{
			case Button.A:
				return KeyCode.Joystick4Button0;
			case Button.B:
				return KeyCode.Joystick4Button1;
			case Button.X:
				return KeyCode.Joystick4Button2;
			case Button.Y:
				return KeyCode.Joystick4Button3;
			case Button.RightShoulder:
				return KeyCode.Joystick4Button5;
			case Button.LeftShoulder:
				return KeyCode.Joystick4Button4;
			case Button.Back:
				return KeyCode.Joystick4Button6;
			case Button.Start:
				return KeyCode.Joystick4Button7;
			case Button.LeftStick:
				return KeyCode.Joystick4Button8;
			case Button.RightStick:
				return KeyCode.Joystick4Button9;
			}
			break;
		case Player.Any:
			switch (button)
			{
			case Button.A:
				return KeyCode.JoystickButton0;
			case Button.B:
				return KeyCode.JoystickButton1;
			case Button.X:
				return KeyCode.JoystickButton2;
			case Button.Y:
				return KeyCode.JoystickButton3;
			case Button.RightShoulder:
				return KeyCode.JoystickButton5;
			case Button.LeftShoulder:
				return KeyCode.JoystickButton4;
			case Button.Back:
				return KeyCode.JoystickButton6;
			case Button.Start:
				return KeyCode.JoystickButton7;
			case Button.LeftStick:
				return KeyCode.JoystickButton8;
			case Button.RightStick:
				return KeyCode.JoystickButton9;
			}
			break;
		}
		return KeyCode.None;
	}

	public static GamepadState GetState(Player player, bool raw = false)
	{
		GamepadState state = new GamepadState();
		state.A = GetButton(Button.A, player);
		state.IsButtonBPressed = GetButton(Button.B, player);
		state.Y = GetButton(Button.Y, player);
		state.X = GetButton(Button.X, player);
		state.RightShoulder = GetButton(Button.RightShoulder, player);
		state.LeftShoulder = GetButton(Button.LeftShoulder, player);
		state.RightStick = GetButton(Button.RightStick, player);
		state.LeftStick = GetButton(Button.LeftStick, player);
		state.Start = GetButton(Button.Start, player);
		state.Back = GetButton(Button.Back, player);
		state.LeftStickAxis = GetStick(Stick.LeftStick, player, raw);
		state.RightStickAxis = GetStick(Stick.RightStick, player, raw);
		state.DpadAxis = GetStick(Stick.Dpad, player, raw);
		state.Left = state.DpadAxis.x < 0f;
		state.Right = state.DpadAxis.x > 0f;
		state.Up = state.DpadAxis.y > 0f;
		state.Down = state.DpadAxis.y < 0f;
		state.LeftTrigger = GetTrigger(Trigger.LeftTrigger, player, raw);
		state.RightTrigger = GetTrigger(Trigger.RightTrigger, player, raw);
		return state;
	}
}
