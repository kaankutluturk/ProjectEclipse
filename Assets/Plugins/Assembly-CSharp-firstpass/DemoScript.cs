using System;
using UnityEngine;

public class DemoScript : MonoBehaviour
{
	private void LogGamepadOneInput()
	{
		GamePad.GetButtonDown(GamePad.Button.A, GamePad.Player.One);
		GamePad.GetStick(GamePad.Stick.LeftStick, GamePad.Player.One);
		GamePad.GetTrigger(GamePad.Trigger.RightTrigger, GamePad.Player.One);
		GamepadState iOIGCCPIJPN = GamePad.GetState(GamePad.Player.One);
		MonoBehaviour.print("A: " + iOIGCCPIJPN.A);
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

	private void DrawGamepadState(GamePad.Player OJINMMFLEEB)
	{
		GUILayout.Space(45f);
		GUILayout.BeginVertical();
		GamepadState iOIGCCPIJPN = GamePad.GetState(OJINMMFLEEB);
		GUILayout.Label("Gamepad " + OJINMMFLEEB);
		GUILayout.Label(string.Empty + iOIGCCPIJPN.A);
		GUILayout.Label(string.Empty + iOIGCCPIJPN.IsButtonBPressed);
		GUILayout.Label(string.Empty + iOIGCCPIJPN.X);
		GUILayout.Label(string.Empty + iOIGCCPIJPN.Y);
		GUILayout.Label(string.Empty + iOIGCCPIJPN.Start);
		GUILayout.Label(string.Empty + iOIGCCPIJPN.Back);
		GUILayout.Label(string.Empty + iOIGCCPIJPN.LeftShoulder);
		GUILayout.Label(string.Empty + iOIGCCPIJPN.RightShoulder);
		GUILayout.Label(string.Empty + iOIGCCPIJPN.Left);
		GUILayout.Label(string.Empty + iOIGCCPIJPN.Right);
		GUILayout.Label(string.Empty + iOIGCCPIJPN.Up);
		GUILayout.Label(string.Empty + iOIGCCPIJPN.Down);
		GUILayout.Label(string.Empty + iOIGCCPIJPN.LeftStick);
		GUILayout.Label(string.Empty + iOIGCCPIJPN.RightStick);
		GUILayout.Label(string.Empty);
		GUILayout.Label(string.Empty + Math.Round(iOIGCCPIJPN.LeftTrigger, 2));
		GUILayout.Label(string.Empty + Math.Round(iOIGCCPIJPN.RightTrigger, 2));
		GUILayout.Label(string.Empty);
		GUILayout.Label(string.Empty + iOIGCCPIJPN.LeftStickAxis);
		GUILayout.Label(string.Empty + iOIGCCPIJPN.RightStickAxis);
		GUILayout.Label(string.Empty + iOIGCCPIJPN.DpadAxis);
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
