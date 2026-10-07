using System.Diagnostics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Nekki.SF2.GUI.Map
{
	public class MapButton : Button
	{
		[SerializeField]
		private ResolutionImageLE _image;

		[SerializeField]
		private TimerLabel _timer;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private MapButtonInfo mapButtonInfo;

		public MapButtonInfo ButtonInfo
		{
			get
			{
				return get_MapButtonInfo();
			}
			protected set
			{
				SetMapButtonInfo(value);
			}
		}

		public MapButtonInfo get_MapButtonInfo()
		{
			return mapButtonInfo;
		}

		protected void SetMapButtonInfo(MapButtonInfo value)
		{
			mapButtonInfo = value;
		}

		public void Init(MapButtonInfo buttonInfo)
		{
			SetMapButtonInfo(buttonInfo);
			ApplyButtonLayout();
			UpdateTimer();
		}

		private void ApplyButtonLayout()
		{
			if (get_MapButtonInfo() == null)
			{
				return;
			}
			if (_image != null)
			{
				if (!string.IsNullOrEmpty(get_MapButtonInfo().AtlasName))
				{
				_image.set_TexturePath(get_MapButtonInfo().AtlasName);
			}
			_image.set_SpriteName(get_MapButtonInfo().ImageName);
			_image.SetNativeSize();
			if (get_MapButtonInfo().Name == "EclipseModeOn" || get_MapButtonInfo().Name == "EclipseModeOff")
			{
				// The recovered atlas region is authored smaller than the old generic
				// map-button artwork. Enlarge the RectTransform as well as the visible
				// sprite so the switch has the intended presence and hit target.
				RectTransform imageRect = _image.rectTransform;
				imageRect.sizeDelta *= 1.5f;
			}
			}
			// Preserve the XML's horizontal anchor. Newer data anchors the Eclipse
			// switch to the right edge while keeping its authored negative offset.
			// Its vertical coordinate remains relative to the canvas centre, matching
			// the coordinate system used by the original scene.
			RectTransform rectTransform = base.transform as RectTransform;
			if (rectTransform != null)
			{
				rectTransform.anchorMin = new Vector2(get_MapButtonInfo().AnchorMinX, 0.5f);
				rectTransform.anchorMax = new Vector2(get_MapButtonInfo().AnchorMaxX, 0.5f);
				rectTransform.anchoredPosition = get_MapButtonInfo().Position;
			}
			else
			{
				base.transform.localPosition = get_MapButtonInfo().Position;
			}
		}

		private void UpdateTimer()
		{
			if (get_MapButtonInfo() != null && !(_timer == null))
			{
				RosterTimer rosterTimer = ((!string.IsNullOrEmpty(get_MapButtonInfo().Timer)) ? ListSF.GetRoster().GetTimerContainer().FindTimer(get_MapButtonInfo().Timer) : null);
				bool flag = rosterTimer != null;
				if (flag)
				{
					_timer.set_DaysStringAlias("TimeDaysShort");
					_timer.set_HoursStringAlias("TimeHourShort");
					_timer.set_MinutesStringAlias("TimeMinuteShort");
					_timer.set_SecondsStringAlias("TimeSecondsShort");
					_timer.Delimiter = " ";
					_timer.IsDays = true;
					_timer.IsDaysZero = false;
					_timer.SegmentsDate = 2;
					long num = rosterTimer.GetEndTimeSeconds();
					_timer.set_CurrentTime(num - ListSF.GetCurrentTime());
				}
				_timer.gameObject.SetActive(flag);
			}
		}

		public override void OnPointerClick(PointerEventData eventData)
		{
			base.OnPointerClick(eventData);
			ActivateAction();
		}

		public bool ActivateAction()
		{
			if (get_MapButtonInfo() != null)
			{
				QuestParameters questParameters = ListSF.GetInstance().GetQuestParameters();
				questParameters.buttonName = get_MapButtonInfo().Name;
				bool handled = ListSF.GetInstance().RaiseQuestEvent(QuestEvent.QuestEventType.QUEST_EVENT_MAP_BUTTON_PRESS);
				if (get_MapButtonInfo().Name == "EclipseModeOn" || get_MapButtonInfo().Name == "EclipseModeOff")
				{
					UnityEngine.Debug.Log("[Eclipse] button=" + get_MapButtonInfo().Name + " questHandled=" + handled);
				}
				if (handled)
				{
					ListSF.GetInstance().RunQuestActions();
				}
				return handled;
			}
			return false;
		}
	}
}
