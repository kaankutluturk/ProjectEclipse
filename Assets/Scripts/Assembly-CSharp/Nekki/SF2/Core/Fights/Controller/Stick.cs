using System;
using System.Collections.Generic;
using Nekki.SF2.GUI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Nekki.SF2.Core.Fights.Controller
{
	public class Stick : SFMonoBehaviour<object>, IEventSystemHandler, IDragHandler, IPointerDownHandler, IPointerUpHandler
	{
		public enum StickEventType
		{
			OnStickBegan = 0,
			OnStickChange = 1,
			OnStickEnd = 2
		}

		[SerializeField]
		private Image _normalTexture;

		[SerializeField]
		private Image _selectedTexture;

		[SerializeField]
		private Image _normalController;

		[SerializeField]
		private Image _selectedController;

		[SerializeField]
		private Image _flashing;

		private FightCID currentDirection;

		private float safeRadius;

		private float safeRadiusSquared;

		private float movementRadius;

		private float movementRadiusSquared;

		private float stopRadius;

		private float joystickRadius;

		private float joystickRadiusSquared;

		private int opacityCounter;

		private int flashingSpeed = 10;

		private bool isFlashing;

		private bool isRising;

		private float axisSectorDegrees;

		private float diagonalSectorDegrees;

		private float axisSectorRadians;

		private float diagonalSectorRadians;

		private float halfAxisSectorCos;

		private float halfAxisSectorSin;

		private float diagonalSectorTan;

		private List<global::Pair<float, float>> quadrantAngles = new List<global::Pair<float, float>>();

		private bool isRelativeTouch;

		private Vector2 touchOrigin = default(Vector2);

		private void Start()
		{
			Init();
		}

		private void Update()
		{
			EaseVisualKnob();
			if (!isFlashing || !_flashing)
			{
				return;
			}
			_flashing.color = new Color(_flashing.color.r, _flashing.color.g, _flashing.color.b, (float)opacityCounter / 255f);
			if (isRising)
			{
				if (opacityCounter < 250)
				{
					opacityCounter += flashingSpeed;
					return;
				}
				isRising = false;
				if (opacityCounter > 250)
				{
					opacityCounter = 250;
				}
			}
			else if (opacityCounter > 0)
			{
				opacityCounter -= flashingSpeed;
			}
			else
			{
				isRising = true;
				if (opacityCounter < 0)
				{
					opacityCounter = 0;
				}
			}
		}

		public void Init()
		{
			axisSectorDegrees = AssemblyController.GetControllerPrimaryAngle();
			axisSectorDegrees = 55f;
			if (axisSectorDegrees < 0f)
			{
				axisSectorDegrees = 0f;
			}
			if (axisSectorDegrees > 90f)
			{
				axisSectorDegrees = 90f;
			}
			diagonalSectorDegrees = 90f - axisSectorDegrees;
			axisSectorRadians = axisSectorDegrees * (float)Math.PI / 180f;
			diagonalSectorRadians = diagonalSectorDegrees * (float)Math.PI / 180f;
			halfAxisSectorCos = Mathf.Cos(axisSectorRadians / 2f);
			halfAxisSectorSin = Mathf.Sin(axisSectorRadians / 2f);
			diagonalSectorTan = Mathf.Tan(diagonalSectorRadians);
			BuildQuadrantAngles();
			SetJoystickRadius(_selectedTexture.rectTransform.rect.width / 2f);
			SetStopRadius(joystickRadius);
			SetSafeRadius(joystickRadius / 2f);
			SetMovementRadius(joystickRadius * AssemblyController.GetControllerGripRelativeRadius());
			SetMovementRadius(joystickRadius * 0.5f);
			_selectedTexture.gameObject.SetActive(false);
			SetPressedVisual(false);
			if (_flashing != null)
			{
				_flashing.color = new Color(_flashing.color.r, _flashing.color.g, _flashing.color.b, 0f);
				_flashing.gameObject.SetActive(isFlashing);
			}
		}

		public void SetSafeRadius(float value)
		{
			safeRadius = value;
			safeRadiusSquared = safeRadius * safeRadius;
		}

		public float GetSafeRadius()
		{
			return safeRadius;
		}

		public void SetMovementRadius(float value)
		{
			movementRadius = value;
			movementRadiusSquared = movementRadius * movementRadius;
		}

		public float GetMovementRadius()
		{
			return movementRadius;
		}

		public void SetStopRadius(float value)
		{
			stopRadius = value;
		}

		public float GetStopRadius()
		{
			return stopRadius;
		}

		public void SetJoystickRadius(float value)
		{
			joystickRadius = value;
			joystickRadiusSquared = joystickRadius * joystickRadius;
		}

		public float GetJoystickRadius()
		{
			return joystickRadius;
		}

		public bool GetIsFlashing()
		{
			return isFlashing;
		}

		public void SetIsFlashing(bool value)
		{
			if (isFlashing != value)
			{
				isFlashing = value;
				if ((bool)_flashing)
				{
					_flashing.gameObject.SetActive(value);
				}
				opacityCounter = 0;
				isRising = true;
			}
		}

		public bool GetIsRising()
		{
			return isRising;
		}

		public void SetIsRising(bool value)
		{
			isRising = value;
		}

		public int GetOpacityCounter()
		{
			return opacityCounter;
		}

		public void SetOpacityCounter(int value)
		{
			opacityCounter = value;
		}

		public int GetFlashingSpeed()
		{
			return flashingSpeed;
		}

		public void SetFlashingSpeed(int value)
		{
			flashingSpeed = value;
		}

		public List<global::Pair<float, float>> GetQuadrantsAngles()
		{
			return quadrantAngles;
		}

		private float GetSquaredMagnitude(Vector2 vector)
		{
			return vector.x * vector.x + vector.y * vector.y;
		}

		public void TT()
		{
			if (currentDirection != FightCID.QuadrantZero)
			{
				DispatchStickEvent(StickEventType.OnStickChange, currentDirection);
			}
		}

        private FightCID visualDirection;
        // While a finger holds the stick the knob follows it; the 8-way visual is for keys and pads.
        private bool touching;
        public void SetInputDirectionVisual(FightCID direction, bool pressed)
        {
            if (touching || direction < FightCID.QuadrantUp || direction > FightCID.QuadrantUpBack) return;
            if (pressed && visualDirection == direction && _selectedController.gameObject.activeSelf) return;
            if (!pressed && visualDirection != direction) return;
            if (pressed) visualDirection = direction;
            else if (visualDirection == direction) visualDirection = FightCID.QuadrantZero;
            bool active = visualDirection != FightCID.QuadrantZero;
            if (active)
            {
                // Coming from rest, the knob leaves the centre rather than a stale touch position.
                if (!visualKnobEasing && !_selectedController.gameObject.activeSelf)
                    _selectedController.transform.localPosition = Vector3.zero;
                SetPressedVisual(true);
                float angle = (int)visualDirection * Mathf.PI / 4f - Mathf.PI / 4f;
                visualKnobTarget = new Vector3(Mathf.Sin(angle), Mathf.Cos(angle), 0) * stopRadius;
            }
            else visualKnobTarget = Vector3.zero;
            visualKnobEasing = true;
        }

        // Keyboard/pad knob: glide toward the 8-way target and back to centre on release
        // (unscaled, so it still settles while the fight is paused). Touch stays direct.
        private Vector3 visualKnobTarget;
        private bool visualKnobEasing;
        private const float VisualKnobRate = 26f;
        private void EaseVisualKnob()
        {
            if (!visualKnobEasing) return;
            if (touching) { visualKnobEasing = false; return; }
            Transform knob = _selectedController.transform;
            Vector3 position = Vector3.Lerp(knob.localPosition, visualKnobTarget,
                1f - Mathf.Exp(-VisualKnobRate * Time.unscaledDeltaTime));
            if ((position - visualKnobTarget).sqrMagnitude < 0.25f) position = visualKnobTarget;
            knob.localPosition = position;
            if (position != visualKnobTarget) return;
            visualKnobEasing = false;
            if (visualDirection == FightCID.QuadrantZero)
            {
                knob.localPosition = Vector3.zero;
                SetPressedVisual(false);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
		{
			Vector2 localPoint;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out localPoint);
			float num = GetSquaredMagnitude(localPoint);
			float accept = 1f + 2f * Eclipse.UI.BattleTouchControls.TouchLeniency;
			if (num <= joystickRadiusSquared * accept * accept)
			{
				touching = true;
				visualDirection = FightCID.QuadrantZero;
				if (num <= movementRadiusSquared)
				{
					isRelativeTouch = true;
					touchOrigin = localPoint;
					currentDirection = FightCID.QuadrantZero;
					SetKnobPosition(default(Vector2));
				}
				else
				{
					currentDirection = GetDirectionForPoint(localPoint);
					SetKnobPosition(localPoint);
				}
				_normalTexture.gameObject.SetActive(false);
				_selectedTexture.gameObject.SetActive(true);
				if ((bool)_flashing)
				{
					_flashing.gameObject.SetActive(false);
				}
				SetPressedVisual(true);
				DispatchStickEvent(StickEventType.OnStickBegan, currentDirection);
			}
		}

		public void OnDrag(PointerEventData eventData)
		{
			Vector2 localPoint;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out localPoint);
			if (isRelativeTouch)
			{
				localPoint.x -= touchOrigin.x;
				localPoint.y -= touchOrigin.y;
			}
			SetKnobPosition(localPoint);
			FightCID direction = GetDirectionForPoint(localPoint);
			if (currentDirection != direction)
			{
				DispatchStickEvent(StickEventType.OnStickEnd, currentDirection);
			}
			currentDirection = direction;
			DispatchStickEvent(StickEventType.OnStickChange, currentDirection);
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			touching = false;
			isRelativeTouch = false;
			SetPressedVisual(false);
			if ((bool)_flashing)
			{
				_flashing.gameObject.SetActive(isFlashing);
			}
			DispatchStickEvent(StickEventType.OnStickEnd, currentDirection);
			currentDirection = FightCID.QuadrantZero;
		}

		private void SetKnobPosition(Vector2 knobPosition)
		{
			knobPosition = Vector2.ClampMagnitude(knobPosition, stopRadius);
			_selectedController.transform.localPosition = knobPosition;
		}

		private FightCID GetDirectionForPoint(Vector2 point)
		{
			FightCID direction = FightCID.QuadrantZero;
			float num = point.x * halfAxisSectorCos + point.y * halfAxisSectorSin;
			float num2 = point.y * halfAxisSectorCos - point.x * halfAxisSectorSin;
			if (GetSquaredMagnitude(point) < safeRadiusSquared)
			{
				return FightCID.QuadrantZero;
			}
			bool flag = num >= 0f;
			bool flag2 = num2 >= 0f;
			float num3 = 0f;
			float num4 = 0f;
			if (flag && flag2)
			{
				direction = FightCID.QuadrantUp;
				num3 = Mathf.Abs(num);
				num4 = Mathf.Abs(num2);
			}
			else if (flag && !flag2)
			{
				direction = FightCID.QuadrantUpForward;
				num3 = Mathf.Abs(num2);
				num4 = Mathf.Abs(num);
			}
			else if (!flag && !flag2)
			{
				direction = FightCID.QuadrantForward;
				num3 = Mathf.Abs(num);
				num4 = Mathf.Abs(num2);
			}
			else if (!flag && flag2)
			{
				direction = FightCID.QuadrantDownForward;
				num3 = Mathf.Abs(num2);
				num4 = Mathf.Abs(num);
			}
			bool flag3 = false;
			if (diagonalSectorDegrees != 90f)
			{
				float num5 = num3 * diagonalSectorTan;
				if (num4 <= num5)
				{
					flag3 = true;
				}
			}
			else
			{
				flag3 = true;
			}
			switch (direction)
			{
			case FightCID.QuadrantUp:
				return (!flag3) ? FightCID.QuadrantUp : FightCID.QuadrantUpForward;
			case FightCID.QuadrantUpForward:
				return flag3 ? FightCID.QuadrantDownForward : FightCID.QuadrantForward;
			case FightCID.QuadrantForward:
				return flag3 ? FightCID.QuadrantDownBack : FightCID.QuadrantDown;
			case FightCID.QuadrantDownForward:
				return flag3 ? FightCID.QuadrantUpBack : FightCID.QuadrantBack;
			default:
				return FightCID.QuadrantZero;
			}
		}

		private void SetPressedVisual(bool pressed)
		{
			_normalController.gameObject.SetActive(!pressed);
			_selectedController.gameObject.SetActive(pressed);
			_normalTexture.gameObject.SetActive(!pressed);
			_selectedTexture.gameObject.SetActive(pressed);
		}

		private void DispatchStickEvent(StickEventType eventType, FightCID control)
		{
			FightControlEventData eventData = new FightControlEventData();
			eventData.Index = 0;
			eventData.Control = control;
			CallEvent((int)eventType, eventData);
		}

		private void BuildQuadrantAngles()
		{
			float num = axisSectorRadians / 2f + diagonalSectorRadians + axisSectorRadians;
			quadrantAngles.Clear();
			for (int i = 0; i < 8; i++)
			{
				float num2;
				if (i == 0)
				{
					num2 = num;
				}
				else
				{
					global::Pair<float, float> anglePair = quadrantAngles[i - 1];
					num2 = anglePair.Second;
				}
				float endAngle = ((i % 2 != 0) ? (num2 + axisSectorRadians) : (num2 + diagonalSectorRadians));
				global::Pair<float, float> item = new global::Pair<float, float>(num2, endAngle);
				quadrantAngles.Add(item);
			}
		}
	}
}
