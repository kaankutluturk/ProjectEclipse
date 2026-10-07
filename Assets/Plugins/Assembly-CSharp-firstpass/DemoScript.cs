using System;
using UnityEngine;

public class DemoScript : MonoBehaviour
{
	private void LogGamepadOneInput()
	{
		GamePad.GetButtonDown(GamePad.Button.A, GamePad.Player.One);
		GamePad.GetStick(GamePad.Stick.LeftStick, GamePad.Player.One);
		GamePad.GetTrigger(GamePad.Trigger.RightTrigger, GamePad.Player.One);
		GamepadState gamepadState = GamePad.GetState(GamePad.Player.One);
		MonoBehaviour.print("A: " + gamepadState.A);
	}

	private void OnGUI()
	{
		GUILayout.BeginArea(new Rect(0f, 20f, Screen.width, Screen.height));
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		DrawLabels();
		for (int i = 0; i < 5; i++)
		{
			DrawGamepadState((GamePad.Player)i);
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}

	private void DrawGamepadState(GamePad.Player player)
	{
		GUILayout.Space(45f);
		GUILayout.BeginVertical();
		GamepadState gamepadState = GamePad.GetState(player);
		GUILayout.Label("Gamepad " + player);
		GUILayout.Label(string.Empty + gamepadState.A);
		GUILayout.Label(string.Empty + gamepadState.IsButtonBPressed);
		GUILayout.Label(string.Empty + gamepadState.X);
		GUILayout.Label(string.Empty + gamepadState.Y);
		GUILayout.Label(string.Empty + gamepadState.Start);
		GUILayout.Label(string.Empty + gamepadState.Back);
		GUILayout.Label(string.Empty + gamepadState.LeftShoulder);
		GUILayout.Label(string.Empty + gamepadState.RightShoulder);
		GUILayout.Label(string.Empty + gamepadState.Left);
		GUILayout.Label(string.Empty + gamepadState.Right);
		GUILayout.Label(string.Empty + gamepadState.Up);
		GUILayout.Label(string.Empty + gamepadState.Down);
		GUILayout.Label(string.Empty + gamepadState.LeftStick);
		GUILayout.Label(string.Empty + gamepadState.RightStick);
		GUILayout.Label(string.Empty);
		GUILayout.Label(string.Empty + Math.Round(gamepadState.LeftTrigger, 2));
		GUILayout.Label(string.Empty + Math.Round(gamepadState.RightTrigger, 2));
		GUILayout.Label(string.Empty);
		GUILayout.Label(string.Empty + gamepadState.LeftStickAxis);
		GUILayout.Label(string.Empty + gamepadState.RightStickAxis);
		GUILayout.Label(string.Empty + gamepadState.DpadAxis);
		GUILayout.EndVertical();
	}

	private void DrawLabels()
	{
		GUILayout.BeginVertical();
		GUILayout.Label(" ", GUILayout.Width(80f));
		GUILayout.Label("A");
		GUILayout.Label("B");
		GUILayout.Label("X");
		GUILayout.Label("Y");
		GUILayout.Label("Start");
		GUILayout.Label("Back");
		GUILayout.Label("Left Shoulder");
		GUILayout.Label("Right Shoulder");
		GUILayout.Label("Left");
		GUILayout.Label("Right");
		GUILayout.Label("Up");
		GUILayout.Label("Down");
		GUILayout.Label("LeftStick");
		GUILayout.Label("RightStick");
		GUILayout.Label(string.Empty);
		GUILayout.Label("LeftTrigger");
		GUILayout.Label("RightTrigger");
		GUILayout.Label(string.Empty);
		GUILayout.Label("LeftStickAxis");
		GUILayout.Label("rightStickAxis");
		GUILayout.Label("dPadAxis");
		GUILayout.EndVertical();
	}
}
