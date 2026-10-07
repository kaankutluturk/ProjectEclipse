using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Nekki.SF2.GUI
{
	[AddComponentMenu("UI/Scroll Rect", 37)]
	[SelectionBase]
	[RequireComponent(typeof(RectTransform))]
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	public class SFScrollRect : UIBehaviour, IEventSystemHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IInitializePotentialDragHandler, IScrollHandler, ICanvasElement, ILayoutElement, ILayoutGroup, ILayoutController
	{
		public enum ScrollMovementType
		{
			Unrestricted = 0,
			Elastic = 1,
			Clamped = 2,
			SF2 = 3
		}

		public enum ScrollbarVisibilityMode
		{
			Permanent = 0,
			AutoHide = 1,
			AutoHideAndExpandViewport = 2
		}

		[Serializable]
		public class ScrollRectEvent : UnityEvent<Vector2>
		{
		}

		[SerializeField]
		private RectTransform m_Content;

		[SerializeField]
		private bool m_Horizontal = true;

		[SerializeField]
		private bool m_Vertical = true;

		[SerializeField]
		private ScrollMovementType m_MovementType = ScrollMovementType.Elastic;

		[SerializeField]
		private float m_Elasticity = 0.1f;

		[SerializeField]
		private bool m_Inertia = true;

		[SerializeField]
		private float m_DecelerationRate = 0.135f;

		[SerializeField]
		private float m_ScrollSensitivity = 1f;

		[SerializeField]
		private RectTransform m_Viewport;

		[SerializeField]
		private Scrollbar m_HorizontalScrollbar;

		[SerializeField]
		private Scrollbar m_VerticalScrollbar;

		[SerializeField]
		private ScrollbarVisibilityMode m_HorizontalScrollbarVisibility;

		[SerializeField]
		private ScrollbarVisibilityMode m_VerticalScrollbarVisibility;

		[SerializeField]
		private float m_HorizontalScrollbarSpacing;

		[SerializeField]
		private float m_VerticalScrollbarSpacing;

		[SerializeField]
		private ScrollRectEvent m_OnValueChanged = new ScrollRectEvent();

		private Vector2 m_PointerStartLocalCursor = Vector2.zero;

		private Vector2 m_ContentStartPosition = Vector2.zero;

		private RectTransform m_ViewRect;

		private Bounds m_ContentBounds;

		private Bounds m_ViewBounds;

		private Vector2 m_Velocity;

		private bool m_Dragging;

		private Vector2 m_PrevPosition = Vector2.zero;

		private Bounds m_PrevContentBounds;

		private Bounds m_PrevViewBounds;

		[NonSerialized]
		private bool m_HasRebuiltLayout;

		private bool m_HSliderExpand;

		private bool m_VSliderExpand;

		private float m_HSliderHeight;

		private float m_VSliderWidth;

		[NonSerialized]
		private RectTransform m_Rect;

		private RectTransform m_HorizontalScrollbarRect;

		private RectTransform m_VerticalScrollbarRect;

		private DrivenRectTransformTracker m_Tracker;

		[SerializeField]
		private float m_ScrollFactor = 1f;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private float m_FlexibleWidth;

		private readonly Vector3[] m_Corners = new Vector3[4];

		public RectTransform ContentRect
		{
			get
			{
				return get_content();
			}
			set
			{
				set_content(value);
			}
		}

		public bool Horizontal
		{
			get
			{
				return get_horizontal();
			}
			set
			{
				set_horizontal(value);
			}
		}

		public bool Vertical
		{
			get
			{
				return get_vertical();
			}
			set
			{
				set_vertical(value);
			}
		}

		public ScrollMovementType MovementType
		{
			get
			{
				return get_movementType();
			}
			set
			{
				set_movementType(value);
			}
		}

		public float Elasticity
		{
			get
			{
				return get_elasticity();
			}
			set
			{
				set_elasticity(value);
			}
		}

		public bool Inertia
		{
			get
			{
				return get_inertia();
			}
			set
			{
				set_inertia(value);
			}
		}

		public float DecelerationRate
		{
			get
			{
				return get_decelerationRate();
			}
			set
			{
				set_decelerationRate(value);
			}
		}

		public float ScrollSensitivity
		{
			get
			{
				return get_scrollSensitivity();
			}
			set
			{
				set_scrollSensitivity(value);
			}
		}

		public RectTransform Viewport
		{
			get
			{
				return get_viewport();
			}
			set
			{
				set_viewport(value);
			}
		}

		public Scrollbar HorizontalScrollbar
		{
			get
			{
				return get_horizontalScrollbar();
			}
			set
			{
				set_horizontalScrollbar(value);
			}
		}

		public Scrollbar VerticalScrollbar
		{
			get
			{
				return get_verticalScrollbar();
			}
			set
			{
				set_verticalScrollbar(value);
			}
		}

		public ScrollbarVisibilityMode HorizontalScrollbarVisibility
		{
			get
			{
				return get_horizontalScrollbarVisibility();
			}
			set
			{
				set_horizontalScrollbarVisibility(value);
			}
		}

		public ScrollbarVisibilityMode VerticalScrollbarVisibility
		{
			get
			{
				return get_verticalScrollbarVisibility();
			}
			set
			{
				set_verticalScrollbarVisibility(value);
			}
		}

		public float HorizontalScrollbarSpacing
		{
			get
			{
				return get_horizontalScrollbarSpacing();
			}
			set
			{
				set_horizontalScrollbarSpacing(value);
			}
		}

		public float VerticalScrollbarSpacing
		{
			get
			{
				return get_verticalScrollbarSpacing();
			}
			set
			{
				set_verticalScrollbarSpacing(value);
			}
		}

		public ScrollRectEvent OnValueChangedEvent
		{
			get
			{
				return get_onValueChanged();
			}
			set
			{
				set_onValueChanged(value);
			}
		}

		protected RectTransform ViewRect
		{
			get
			{
				return GetViewRect();
			}
		}

		public Vector2 VelocityProperty
		{
			get
			{
				return get_velocity();
			}
			set
			{
				set_velocity(value);
			}
		}

		private RectTransform rectTransform
		{
			get
			{
				return GetRectTransform();
			}
		}

		public float ScrollFactorProperty
		{
			get
			{
				return get_scrollFactor();
			}
			set
			{
				set_scrollFactor(value);
			}
		}

		public Vector2 NormalizedPosition
		{
			get
			{
				return get_normalizedPosition();
			}
			set
			{
				set_normalizedPosition(value);
			}
		}

		public float HorizontalNormalizedPosition
		{
			get
			{
				return get_horizontalNormalizedPosition();
			}
			set
			{
				set_horizontalNormalizedPosition(value);
			}
		}

		public float VerticalNormalizedPosition
		{
			get
			{
				return get_verticalNormalizedPosition();
			}
			set
			{
				set_verticalNormalizedPosition(value);
			}
		}

		private bool HScrollingNeeded
		{
			get
			{
				return IsHorizontalScrollingNeeded();
			}
		}

		private bool VScrollingNeeded
		{
			get
			{
				return IsVerticalScrollingNeeded();
			}
		}

		public virtual float LayoutMinWidth
		{
			get
			{
				return minWidth;
			}
		}

		public virtual float LayoutPreferredWidth
		{
			get
			{
				return preferredWidth;
			}
		}

		public virtual float LayoutFlexibleWidth
		{
			get
			{
				return flexibleWidth;
			}
			private set
			{
				SetFlexibleWidth(value);
			}
		}

		public virtual float LayoutMinHeight
		{
			get
			{
				return minHeight;
			}
		}

		public virtual float LayoutPreferredHeight
		{
			get
			{
				return preferredHeight;
			}
		}

		public virtual float LayoutFlexibleHeight
		{
			get
			{
				return flexibleHeight;
			}
		}

		public virtual int LayoutPriorityValue
		{
			get
			{
				return layoutPriority;
			}
		}

		Transform ICanvasElement.transform
		{
			get
			{
				return base.transform;
			}
		}

		protected SFScrollRect()
		{
			SetFlexibleWidth(-1f);
		}

		public RectTransform get_content()
		{
			return m_Content;
		}

		public void set_content(RectTransform value)
		{
			m_Content = value;
		}

		public bool get_horizontal()
		{
			return m_Horizontal;
		}

		public void set_horizontal(bool value)
		{
			m_Horizontal = value;
		}

		public bool get_vertical()
		{
			return m_Vertical;
		}

		public void set_vertical(bool value)
		{
			m_Vertical = value;
		}

		public ScrollMovementType get_movementType()
		{
			return m_MovementType;
		}

		public void set_movementType(ScrollMovementType value)
		{
			m_MovementType = value;
		}

		public float get_elasticity()
		{
			return m_Elasticity;
		}

		public void set_elasticity(float value)
		{
			m_Elasticity = value;
		}

		public bool get_inertia()
		{
			return m_Inertia;
		}

		public void set_inertia(bool value)
		{
			m_Inertia = value;
		}

		public float get_decelerationRate()
		{
			return m_DecelerationRate;
		}

		public void set_decelerationRate(float value)
		{
			m_DecelerationRate = value;
		}

		public float get_scrollSensitivity()
		{
			return m_ScrollSensitivity;
		}

		public void set_scrollSensitivity(float value)
		{
			m_ScrollSensitivity = value;
		}

		public RectTransform get_viewport()
		{
			return m_Viewport;
		}

		public void set_viewport(RectTransform value)
		{
			m_Viewport = value;
			SetDirtyCaching();
		}

		public Scrollbar get_horizontalScrollbar()
		{
			return m_HorizontalScrollbar;
		}

		public void set_horizontalScrollbar(Scrollbar value)
		{
			if ((bool)m_HorizontalScrollbar)
			{
				m_HorizontalScrollbar.onValueChanged.RemoveListener(SetHorizontalNormalizedPositionInternal);
			}
			m_HorizontalScrollbar = value;
			if ((bool)m_HorizontalScrollbar)
			{
				m_HorizontalScrollbar.onValueChanged.AddListener(SetHorizontalNormalizedPositionInternal);
			}
			SetDirtyCaching();
		}

		public Scrollbar get_verticalScrollbar()
		{
			return m_VerticalScrollbar;
		}

		public void set_verticalScrollbar(Scrollbar value)
		{
			if ((bool)m_VerticalScrollbar)
			{
				m_VerticalScrollbar.onValueChanged.RemoveListener(SetVerticalNormalizedPositionInternal);
			}
			m_VerticalScrollbar = value;
			if ((bool)m_VerticalScrollbar)
			{
				m_VerticalScrollbar.onValueChanged.AddListener(SetVerticalNormalizedPositionInternal);
			}
			SetDirtyCaching();
		}

		public ScrollbarVisibilityMode get_horizontalScrollbarVisibility()
		{
			return m_HorizontalScrollbarVisibility;
		}

		public void set_horizontalScrollbarVisibility(ScrollbarVisibilityMode value)
		{
			m_HorizontalScrollbarVisibility = value;
			SetDirtyCaching();
		}

		public ScrollbarVisibilityMode get_verticalScrollbarVisibility()
		{
			return m_VerticalScrollbarVisibility;
		}

		public void set_verticalScrollbarVisibility(ScrollbarVisibilityMode value)
		{
			m_VerticalScrollbarVisibility = value;
			SetDirtyCaching();
		}

		public float get_horizontalScrollbarSpacing()
		{
			return m_HorizontalScrollbarSpacing;
		}

		public void set_horizontalScrollbarSpacing(float value)
		{
			m_HorizontalScrollbarSpacing = value;
			SetDirty();
		}

		public float get_verticalScrollbarSpacing()
		{
			return m_VerticalScrollbarSpacing;
		}

		public void set_verticalScrollbarSpacing(float value)
		{
			m_VerticalScrollbarSpacing = value;
			SetDirty();
		}

		public ScrollRectEvent get_onValueChanged()
		{
			return m_OnValueChanged;
		}

		public void set_onValueChanged(ScrollRectEvent value)
		{
			m_OnValueChanged = value;
		}

		protected RectTransform GetViewRect()
		{
			if (m_ViewRect == null)
			{
				m_ViewRect = m_Viewport;
			}
			if (m_ViewRect == null)
			{
				m_ViewRect = (RectTransform)base.transform;
			}
			return m_ViewRect;
		}

		public Vector2 get_velocity()
		{
			return m_Velocity;
		}

		public void set_velocity(Vector2 value)
		{
			m_Velocity = value;
		}

		private RectTransform GetRectTransform()
		{
			if (m_Rect == null)
			{
				m_Rect = GetComponent<RectTransform>();
			}
			return m_Rect;
		}

		public float get_scrollFactor()
		{
			return m_ScrollFactor;
		}

		public void set_scrollFactor(float value)
		{
			m_ScrollFactor = value;
		}

		public virtual void Rebuild(CanvasUpdate update)
		{
			if (update == CanvasUpdate.Prelayout)
			{
				UpdateCachedData();
			}
			if (update == CanvasUpdate.PostLayout)
			{
				UpdateBounds();
				UpdateScrollbars(Vector2.zero);
				UpdatePrevData();
				m_HasRebuiltLayout = true;
			}
		}

		public virtual void LayoutComplete()
		{
		}

		public virtual void GraphicUpdateComplete()
		{
		}

		private void UpdateCachedData()
		{
			Transform transform = base.transform;
			m_HorizontalScrollbarRect = ((!(m_HorizontalScrollbar == null)) ? (m_HorizontalScrollbar.transform as RectTransform) : null);
			m_VerticalScrollbarRect = ((!(m_VerticalScrollbar == null)) ? (m_VerticalScrollbar.transform as RectTransform) : null);
			bool flag = GetViewRect().parent == transform;
			bool flag2 = !m_HorizontalScrollbarRect || m_HorizontalScrollbarRect.parent == transform;
			bool flag3 = !m_VerticalScrollbarRect || m_VerticalScrollbarRect.parent == transform;
			bool flag4 = flag && flag2 && flag3;
			m_HSliderExpand = flag4 && (bool)m_HorizontalScrollbarRect && get_horizontalScrollbarVisibility() == ScrollbarVisibilityMode.AutoHideAndExpandViewport;
			m_VSliderExpand = flag4 && (bool)m_VerticalScrollbarRect && get_verticalScrollbarVisibility() == ScrollbarVisibilityMode.AutoHideAndExpandViewport;
			m_HSliderHeight = ((!(m_HorizontalScrollbarRect == null)) ? m_HorizontalScrollbarRect.rect.height : 0f);
			m_VSliderWidth = ((!(m_VerticalScrollbarRect == null)) ? m_VerticalScrollbarRect.rect.width : 0f);
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			if ((bool)m_HorizontalScrollbar)
			{
				m_HorizontalScrollbar.onValueChanged.AddListener(SetHorizontalNormalizedPositionInternal);
			}
			if ((bool)m_VerticalScrollbar)
			{
				m_VerticalScrollbar.onValueChanged.AddListener(SetVerticalNormalizedPositionInternal);
			}
			CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
		}

		protected override void OnDisable()
		{
			CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);
			if ((bool)m_HorizontalScrollbar)
			{
				m_HorizontalScrollbar.onValueChanged.RemoveListener(SetHorizontalNormalizedPositionInternal);
			}
			if ((bool)m_VerticalScrollbar)
			{
				m_VerticalScrollbar.onValueChanged.RemoveListener(SetVerticalNormalizedPositionInternal);
			}
			m_HasRebuiltLayout = false;
			m_Tracker.Clear();
			m_Velocity = Vector2.zero;
			LayoutRebuilder.MarkLayoutForRebuild(GetRectTransform());
			base.OnDisable();
		}

		public override bool IsActive()
		{
			return base.IsActive() && m_Content != null;
		}

		private void EnsureLayoutHasRebuilt()
		{
			if (!m_HasRebuiltLayout && !CanvasUpdateRegistry.IsRebuildingLayout())
			{
				Canvas.ForceUpdateCanvases();
			}
		}

		public virtual void StopMovement()
		{
			m_Velocity = Vector2.zero;
		}

		public virtual void OnScroll(PointerEventData data)
		{
			if (!IsActive())
			{
				return;
			}
			EnsureLayoutHasRebuilt();
			UpdateBounds();
			Vector2 scrollDelta = data.scrollDelta;
			scrollDelta.y *= -1f;
			if (get_vertical() && !get_horizontal())
			{
				if (Mathf.Abs(scrollDelta.x) > Mathf.Abs(scrollDelta.y))
				{
					scrollDelta.y = scrollDelta.x;
				}
				scrollDelta.x = 0f;
			}
			if (get_horizontal() && !get_vertical())
			{
				if (Mathf.Abs(scrollDelta.y) > Mathf.Abs(scrollDelta.x))
				{
					scrollDelta.x = scrollDelta.y;
				}
				scrollDelta.y = 0f;
			}
			// Recovered prefabs use 1 canvas unit per notch, imperceptible at the
            // authored 2048px UI scale. Keep configured sensitivity as a multiplier.
            // Eclipse: notches accumulate into a target that LateUpdate eases toward,
            // instead of jumping the content a full notch in one frame.
            StopMovement();
            Vector2 anchoredPosition = (wheelActive ? wheelTarget : m_Content.anchoredPosition) + scrollDelta * m_ScrollSensitivity * 100f;
			if (m_MovementType != ScrollMovementType.Unrestricted)
			{
				anchoredPosition += CalculateOffset(anchoredPosition - m_Content.anchoredPosition);
			}
			wheelTarget = anchoredPosition;
			wheelLast = m_Content.anchoredPosition;
			wheelActive = true;
		}

		private const float WheelEaseRate = 16f;

		private Vector2 wheelTarget;

		private Vector2 wheelLast;

		private bool wheelActive;

		protected void CancelWheel()
		{
			wheelActive = false;
		}

		// Eases the content toward the accumulated mouse-wheel target (unscaled time).
		private void StepWheel(float deltaTime)
		{
			// Any other writer (drag, scrollbar, tween, code-driven scroll) takes over.
			if (m_Dragging || m_Content.anchoredPosition != wheelLast)
			{
				wheelActive = false;
				return;
			}
			if (m_MovementType != ScrollMovementType.Unrestricted)
			{
				wheelTarget += CalculateOffset(wheelTarget - m_Content.anchoredPosition);
			}
			Vector2 position = Vector2.Lerp(m_Content.anchoredPosition, wheelTarget, 1f - Mathf.Exp(-WheelEaseRate * deltaTime));
			if ((position - wheelTarget).sqrMagnitude < 0.25f)
			{
				position = wheelTarget;
				wheelActive = false;
			}
			SetContentAnchoredPosition(position);
			wheelLast = m_Content.anchoredPosition;
		}

		public virtual void OnInitializePotentialDrag(PointerEventData eventData)
		{
			if (eventData.button == PointerEventData.InputButton.Left)
			{
				m_Velocity = Vector2.zero;
			}
		}

		public virtual void OnBeginDrag(PointerEventData eventData)
		{
			if (eventData.button == PointerEventData.InputButton.Left && IsActive())
			{
				UpdateBounds();
				m_PointerStartLocalCursor = Vector2.zero;
				RectTransformUtility.ScreenPointToLocalPointInRectangle(GetViewRect(), eventData.position, eventData.pressEventCamera, out m_PointerStartLocalCursor);
				m_ContentStartPosition = m_Content.anchoredPosition;
				m_Dragging = true;
				wheelActive = false;
			}
		}

		public virtual void OnEndDrag(PointerEventData eventData)
		{
			if (eventData.button == PointerEventData.InputButton.Left)
			{
				m_Dragging = false;
			}
		}

		public virtual void OnDrag(PointerEventData eventData)
		{
			Vector2 localPoint;
			if (eventData.button != PointerEventData.InputButton.Left || !IsActive() || !RectTransformUtility.ScreenPointToLocalPointInRectangle(GetViewRect(), eventData.position, eventData.pressEventCamera, out localPoint))
			{
				return;
			}
			UpdateBounds();
			Vector2 vector = localPoint - m_PointerStartLocalCursor;
			vector *= m_ScrollFactor;
			Vector2 vector2 = m_ContentStartPosition + vector;
			Vector2 vector3 = CalculateOffset(vector2 - m_Content.anchoredPosition);
			vector2 += vector3;
			if (m_MovementType == ScrollMovementType.Elastic)
			{
				if (vector3.x != 0f)
				{
					vector2.x -= RubberDelta(vector3.x, m_ViewBounds.size.x);
				}
				if (vector3.y != 0f)
				{
					vector2.y -= RubberDelta(vector3.y, m_ViewBounds.size.y);
				}
			}
			SetContentAnchoredPosition(vector2);
		}

		protected virtual void SetContentAnchoredPosition(Vector2 contentPosition)
		{
			if (!m_Horizontal)
			{
				contentPosition.x = m_Content.anchoredPosition.x;
			}
			if (!m_Vertical)
			{
				contentPosition.y = m_Content.anchoredPosition.y;
			}
			if (contentPosition != m_Content.anchoredPosition)
			{
				m_Content.anchoredPosition = contentPosition;
				UpdateBounds();
			}
		}

		protected virtual void LateUpdate()
		{
			if (!m_Content)
			{
				return;
			}
			EnsureLayoutHasRebuilt();
			UpdateScrollbarVisibility();
			UpdateBounds();
			float unscaledDeltaTime = Time.unscaledDeltaTime;
			if (wheelActive)
			{
				StepWheel(unscaledDeltaTime);
			}
			Vector2 vector = CalculateOffset(Vector2.zero);
			if (!m_Dragging && (vector != Vector2.zero || m_Velocity != Vector2.zero) && m_MovementType != ScrollMovementType.SF2)
			{
				Vector2 anchoredPosition = m_Content.anchoredPosition;
				for (int i = 0; i < 2; i++)
				{
					if (m_MovementType == ScrollMovementType.Elastic && vector[i] != 0f)
					{
						float currentVelocity = m_Velocity[i];
						anchoredPosition[i] = Mathf.SmoothDamp(m_Content.anchoredPosition[i], m_Content.anchoredPosition[i] + vector[i], ref currentVelocity, m_Elasticity, float.PositiveInfinity, unscaledDeltaTime);
						m_Velocity[i] = currentVelocity;
					}
					else if (m_Inertia)
					{
						m_Velocity[i] *= Mathf.Pow(m_DecelerationRate, unscaledDeltaTime);
						if (Mathf.Abs(m_Velocity[i]) < 1f)
						{
							m_Velocity[i] = 0f;
						}
						anchoredPosition[i] += m_Velocity[i] * unscaledDeltaTime;
					}
					else
					{
						m_Velocity[i] = 0f;
					}
				}
				if (m_Velocity != Vector2.zero || vector != Vector2.zero)
				{
					if (m_MovementType == ScrollMovementType.Clamped)
					{
						vector = CalculateOffset(anchoredPosition - m_Content.anchoredPosition);
						anchoredPosition += vector;
					}
					SetContentAnchoredPosition(anchoredPosition);
				}
			}
			if (m_Dragging && m_Inertia)
			{
				Vector3 b = (m_Content.anchoredPosition - m_PrevPosition) / unscaledDeltaTime;
				m_Velocity = Vector3.Lerp(m_Velocity, b, unscaledDeltaTime * 10f);
			}
			if (m_ViewBounds != m_PrevViewBounds || m_ContentBounds != m_PrevContentBounds || m_Content.anchoredPosition != m_PrevPosition)
			{
				UpdateScrollbars(vector);
				m_OnValueChanged.Invoke(get_normalizedPosition());
				UpdatePrevData();
			}
		}

		private void UpdatePrevData()
		{
			if (m_Content == null)
			{
				m_PrevPosition = Vector2.zero;
			}
			else
			{
				m_PrevPosition = m_Content.anchoredPosition;
			}
			m_PrevViewBounds = m_ViewBounds;
			m_PrevContentBounds = m_ContentBounds;
		}

		private void UpdateScrollbars(Vector2 offset)
		{
			if ((bool)m_HorizontalScrollbar)
			{
				if (m_ContentBounds.size.x > 0f)
				{
					m_HorizontalScrollbar.size = Mathf.Clamp01((m_ViewBounds.size.x - Mathf.Abs(offset.x)) / m_ContentBounds.size.x);
				}
				else
				{
					m_HorizontalScrollbar.size = 1f;
				}
				m_HorizontalScrollbar.SetValueWithoutNotify(get_horizontalNormalizedPosition());
			}
			if ((bool)m_VerticalScrollbar)
			{
				if (m_ContentBounds.size.y > 0f)
				{
					m_VerticalScrollbar.size = Mathf.Clamp01((m_ViewBounds.size.y - Mathf.Abs(offset.y)) / m_ContentBounds.size.y);
				}
				else
				{
					m_VerticalScrollbar.size = 1f;
				}
				m_VerticalScrollbar.SetValueWithoutNotify(get_verticalNormalizedPosition());
			}
		}

		public Vector2 get_normalizedPosition()
		{
			return new Vector2(get_horizontalNormalizedPosition(), get_verticalNormalizedPosition());
		}

		public void set_normalizedPosition(Vector2 value)
		{
			SetNormalizedPosition(value.x, 0);
			SetNormalizedPosition(value.y, 1);
		}

		public float get_horizontalNormalizedPosition()
		{
			UpdateBounds();
			if (m_ContentBounds.size.x <= m_ViewBounds.size.x)
			{
				return (m_ViewBounds.min.x > m_ContentBounds.min.x) ? 1 : 0;
			}
			return (m_ViewBounds.min.x - m_ContentBounds.min.x) / (m_ContentBounds.size.x - m_ViewBounds.size.x);
		}

		public void set_horizontalNormalizedPosition(float value)
		{
			SetNormalizedPosition(value, 0);
		}

		public float get_verticalNormalizedPosition()
		{
			UpdateBounds();
			if (m_ContentBounds.size.y <= m_ViewBounds.size.y)
			{
				return (m_ViewBounds.min.y > m_ContentBounds.min.y) ? 1 : 0;
			}
			return (m_ViewBounds.min.y - m_ContentBounds.min.y) / (m_ContentBounds.size.y - m_ViewBounds.size.y);
		}

		public void set_verticalNormalizedPosition(float value)
		{
			SetNormalizedPosition(value, 1);
		}

		private void SetHorizontalNormalizedPositionInternal(float value)
		{
			SetNormalizedPosition(value, 0);
		}

		private void SetVerticalNormalizedPositionInternal(float value)
		{
			SetNormalizedPosition(value, 1);
		}

		private void SetNormalizedPosition(float value, int axis)
		{
			EnsureLayoutHasRebuilt();
			UpdateBounds();
			float num = m_ContentBounds.size[axis] - m_ViewBounds.size[axis];
			float num2 = m_ViewBounds.min[axis] - value * num;
			float num3 = m_Content.localPosition[axis] + num2 - m_ContentBounds.min[axis];
			Vector3 localPosition = m_Content.localPosition;
			if (Mathf.Abs(localPosition[axis] - num3) > 0.01f)
			{
				localPosition[axis] = num3;
				m_Content.localPosition = localPosition;
				m_Velocity[axis] = 0f;
				UpdateBounds();
			}
		}

		private static float RubberDelta(float overStretching, float viewSize)
		{
			return (1f - 1f / (Mathf.Abs(overStretching) * 0.55f / viewSize + 1f)) * viewSize * Mathf.Sign(overStretching);
		}

		protected override void OnRectTransformDimensionsChange()
		{
			SetDirty();
		}

		private bool IsHorizontalScrollingNeeded()
		{
			if (Application.isPlaying)
			{
				return m_ContentBounds.size.x > m_ViewBounds.size.x + 0.01f;
			}
			return true;
		}

		private bool IsVerticalScrollingNeeded()
		{
			if (Application.isPlaying)
			{
				return m_ContentBounds.size.y > m_ViewBounds.size.y + 0.01f;
			}
			return true;
		}

		public virtual void CalculateLayoutInputHorizontal()
		{
		}

		public virtual void CalculateLayoutInputVertical()
		{
		}

		public virtual float minWidth
		{
			get
			{
				return -1f;
			}
		}

#if UNITY_6000_0_OR_NEWER
		public virtual float maxWidth
		{
			get
			{
				return LayoutUtility.DefaultMaxSize;
			}
		}

#endif
		public virtual float preferredWidth
		{
			get
			{
				return -1f;
			}
		}

		public virtual float flexibleWidth
		{
			get
			{
				return m_FlexibleWidth;
			}
		}

		private void SetFlexibleWidth(float value)
		{
			m_FlexibleWidth = value;
		}

		public virtual float minHeight
		{
			get
			{
				return -1f;
			}
		}

#if UNITY_6000_0_OR_NEWER
		public virtual float maxHeight
		{
			get
			{
				return LayoutUtility.DefaultMaxSize;
			}
		}

#endif
		public virtual float preferredHeight
		{
			get
			{
				return -1f;
			}
		}

		public virtual float flexibleHeight
		{
			get
			{
				return -1f;
			}
		}

		public virtual int layoutPriority
		{
			get
			{
				return -1;
			}
		}

		public virtual void SetLayoutHorizontal()
		{
			m_Tracker.Clear();
			if (m_HSliderExpand || m_VSliderExpand)
			{
				m_Tracker.Add(this, GetViewRect(), DrivenTransformProperties.Anchors | DrivenTransformProperties.AnchoredPosition | DrivenTransformProperties.SizeDelta);
				GetViewRect().anchorMin = Vector2.zero;
				GetViewRect().anchorMax = Vector2.one;
				GetViewRect().sizeDelta = Vector2.zero;
				GetViewRect().anchoredPosition = Vector2.zero;
				LayoutRebuilder.ForceRebuildLayoutImmediate(get_content());
				m_ViewBounds = new Bounds(GetViewRect().rect.center, GetViewRect().rect.size);
				m_ContentBounds = GetBounds();
			}
			if (m_VSliderExpand && IsVerticalScrollingNeeded())
			{
				GetViewRect().sizeDelta = new Vector2(0f - (m_VSliderWidth + m_VerticalScrollbarSpacing), GetViewRect().sizeDelta.y);
				LayoutRebuilder.ForceRebuildLayoutImmediate(get_content());
				m_ViewBounds = new Bounds(GetViewRect().rect.center, GetViewRect().rect.size);
				m_ContentBounds = GetBounds();
			}
			if (m_HSliderExpand && IsHorizontalScrollingNeeded())
			{
				GetViewRect().sizeDelta = new Vector2(GetViewRect().sizeDelta.x, 0f - (m_HSliderHeight + m_HorizontalScrollbarSpacing));
				m_ViewBounds = new Bounds(GetViewRect().rect.center, GetViewRect().rect.size);
				m_ContentBounds = GetBounds();
			}
			if (m_VSliderExpand && IsVerticalScrollingNeeded() && GetViewRect().sizeDelta.x == 0f && GetViewRect().sizeDelta.y < 0f)
			{
				GetViewRect().sizeDelta = new Vector2(0f - (m_VSliderWidth + m_VerticalScrollbarSpacing), GetViewRect().sizeDelta.y);
			}
		}

		public virtual void SetLayoutVertical()
		{
			UpdateScrollbarLayout();
			m_ViewBounds = new Bounds(GetViewRect().rect.center, GetViewRect().rect.size);
			m_ContentBounds = GetBounds();
		}

		private void UpdateScrollbarVisibility()
		{
			if ((bool)m_VerticalScrollbar && m_VerticalScrollbarVisibility != ScrollbarVisibilityMode.Permanent && m_VerticalScrollbar.gameObject.activeSelf != IsVerticalScrollingNeeded())
			{
				m_VerticalScrollbar.gameObject.SetActive(IsVerticalScrollingNeeded());
			}
			if ((bool)m_HorizontalScrollbar && m_HorizontalScrollbarVisibility != ScrollbarVisibilityMode.Permanent && m_HorizontalScrollbar.gameObject.activeSelf != IsHorizontalScrollingNeeded())
			{
				m_HorizontalScrollbar.gameObject.SetActive(IsHorizontalScrollingNeeded());
			}
		}

		private void UpdateScrollbarLayout()
		{
			if (m_VSliderExpand && (bool)m_HorizontalScrollbar)
			{
				m_Tracker.Add(this, m_HorizontalScrollbarRect, DrivenTransformProperties.AnchoredPositionX | DrivenTransformProperties.AnchorMinX | DrivenTransformProperties.AnchorMaxX | DrivenTransformProperties.SizeDeltaX);
				m_HorizontalScrollbarRect.anchorMin = new Vector2(0f, m_HorizontalScrollbarRect.anchorMin.y);
				m_HorizontalScrollbarRect.anchorMax = new Vector2(1f, m_HorizontalScrollbarRect.anchorMax.y);
				m_HorizontalScrollbarRect.anchoredPosition = new Vector2(0f, m_HorizontalScrollbarRect.anchoredPosition.y);
				if (IsVerticalScrollingNeeded())
				{
					m_HorizontalScrollbarRect.sizeDelta = new Vector2(0f - (m_VSliderWidth + m_VerticalScrollbarSpacing), m_HorizontalScrollbarRect.sizeDelta.y);
				}
				else
				{
					m_HorizontalScrollbarRect.sizeDelta = new Vector2(0f, m_HorizontalScrollbarRect.sizeDelta.y);
				}
			}
			if (m_HSliderExpand && (bool)m_VerticalScrollbar)
			{
				m_Tracker.Add(this, m_VerticalScrollbarRect, DrivenTransformProperties.AnchoredPositionY | DrivenTransformProperties.AnchorMinY | DrivenTransformProperties.AnchorMaxY | DrivenTransformProperties.SizeDeltaY);
				m_VerticalScrollbarRect.anchorMin = new Vector2(m_VerticalScrollbarRect.anchorMin.x, 0f);
				m_VerticalScrollbarRect.anchorMax = new Vector2(m_VerticalScrollbarRect.anchorMax.x, 1f);
				m_VerticalScrollbarRect.anchoredPosition = new Vector2(m_VerticalScrollbarRect.anchoredPosition.x, 0f);
				if (IsHorizontalScrollingNeeded())
				{
					m_VerticalScrollbarRect.sizeDelta = new Vector2(m_VerticalScrollbarRect.sizeDelta.x, 0f - (m_HSliderHeight + m_HorizontalScrollbarSpacing));
				}
				else
				{
					m_VerticalScrollbarRect.sizeDelta = new Vector2(m_VerticalScrollbarRect.sizeDelta.x, 0f);
				}
			}
		}

		private void UpdateBounds()
		{
			m_ViewBounds = new Bounds(GetViewRect().rect.center, GetViewRect().rect.size);
			m_ContentBounds = GetBounds();
			if (!(m_Content == null))
			{
				Vector3 size = m_ContentBounds.size;
				Vector3 center = m_ContentBounds.center;
				Vector3 vector = m_ViewBounds.size - size;
				if (vector.x > 0f)
				{
					center.x -= vector.x * (m_Content.pivot.x - 0.5f);
					size.x = m_ViewBounds.size.x;
				}
				if (vector.y > 0f)
				{
					center.y -= vector.y * (m_Content.pivot.y - 0.5f);
					size.y = m_ViewBounds.size.y;
				}
				m_ContentBounds.size = size;
				m_ContentBounds.center = center;
			}
		}

		private Bounds GetBounds()
		{
			if (m_Content == null)
			{
				return default(Bounds);
			}
			Vector3 vector = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			Vector3 vector2 = new Vector3(float.MinValue, float.MinValue, float.MinValue);
			Matrix4x4 worldToLocalMatrix = GetViewRect().worldToLocalMatrix;
			m_Content.GetWorldCorners(m_Corners);
			for (int i = 0; i < 4; i++)
			{
				Vector3 lhs = worldToLocalMatrix.MultiplyPoint3x4(m_Corners[i]);
				vector = Vector3.Min(lhs, vector);
				vector2 = Vector3.Max(lhs, vector2);
			}
			Bounds result = new Bounds(vector, Vector3.zero);
			result.Encapsulate(vector2);
			return result;
		}

		private Vector2 CalculateOffset(Vector2 delta)
		{
			Vector2 zero = Vector2.zero;
			if (m_MovementType == ScrollMovementType.Unrestricted)
			{
				return zero;
			}
			Vector2 vector = m_ContentBounds.min;
			Vector2 vector2 = m_ContentBounds.max;
			if (m_Horizontal)
			{
				vector.x += delta.x;
				vector2.x += delta.x;
				if (vector.x > m_ViewBounds.min.x)
				{
					zero.x = m_ViewBounds.min.x - vector.x;
				}
				else if (vector2.x < m_ViewBounds.max.x)
				{
					zero.x = m_ViewBounds.max.x - vector2.x;
				}
			}
			if (m_Vertical)
			{
				vector.y += delta.y;
				vector2.y += delta.y;
				if (vector2.y < m_ViewBounds.max.y)
				{
					zero.y = m_ViewBounds.max.y - vector2.y;
				}
				else if (vector.y > m_ViewBounds.min.y)
				{
					zero.y = m_ViewBounds.min.y - vector.y;
				}
			}
			return zero;
		}

		protected void SetDirty()
		{
			if (IsActive())
			{
				LayoutRebuilder.MarkLayoutForRebuild(GetRectTransform());
			}
		}

		protected void SetDirtyCaching()
		{
			if (IsActive())
			{
				CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
				LayoutRebuilder.MarkLayoutForRebuild(GetRectTransform());
			}
		}

		bool ICanvasElement.IsDestroyed()
		{
			return IsDestroyed();
		}
	}
}
