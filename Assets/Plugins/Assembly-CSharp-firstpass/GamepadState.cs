using UnityEngine;

public class GamepadState
{
	public bool A;

	public bool IsButtonBPressed;

	public bool X;

	public bool Y;

	public bool Start;

	public bool Back;

	public bool Left;

	public bool Right;

	public bool Up;

	public bool Down;

	public bool LeftStick;

	public bool RightStick;

	public bool RightShoulder;

	public bool LeftShoulder;

	public Vector2 LeftStickAxis = Vector2.zero;

	public Vector2 RightStickAxis = Vector2.zero;

	public Vector2 DpadAxis = Vector2.zero;

	public float LeftTrigger;

	public float RightTrigger;
}
