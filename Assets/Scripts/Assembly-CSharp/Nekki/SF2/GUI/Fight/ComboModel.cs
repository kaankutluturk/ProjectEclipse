using System.Collections.Generic;
using UnityEngine;

namespace Nekki.SF2.GUI.Fight
{
	public class ComboModel : SFMonoBehaviour<ComboModel.ComboChangeInfo>
	{
		public enum ComboModelEvent
		{
			ON_COMBO_CHANGE = 0
		}

		public struct ComboChangeInfo
		{
			public int ComboCount;

			public ComboTypeEvent Type;
		}

		public class ComboNode
		{
			public ComboItem Target;

			public int Count;

			public bool IsSettled;

			public bool IgnoresPause;

			public ComboTypes Type = ComboTypes.TypeCombo;
		}

		[SerializeField]
		private GameObject _comboItemPrefab;

		private ComboStatistic comboStatistic = new ComboStatistic();

		private const float NodeSpacing = 40f;

		private const float NodeMoveSpeed = 30f;

		private const float UnusedOffset = 0f;

		private float spawnX = 600f;

		private float restX = 245f;

		private Vector2 anchorMin;

		private Vector2 anchorMax;

		private readonly Vector2 firstNodePosition = new Vector2(0f, -40f);

		private List<ComboNode> nodes = new List<ComboNode>();

		private bool _fightPause;

		private int comboIdleFrames;

		private int comboTimeoutFrames;

		private int currentComboCount;

		private int hotGroundTime;

		private ScreenModel.ScreenSide modelSide;

		public ComboStatistic Statistics
		{
			get
			{
				return get_ComboStatistic();
			}
			set
			{
				set_ComboStatistic(value);
			}
		}

		public ComboStatistic get_ComboStatistic()
		{
			return comboStatistic;
		}

		public void set_ComboStatistic(ComboStatistic value)
		{
			comboStatistic = value;
		}

		public void Init(ScreenModel.ScreenSide NPEAOKLDJHA)
		{
			modelSide = NPEAOKLDJHA;
			comboTimeoutFrames = GetDisplayCount(ComboTypes.TypeCombo);
			if (modelSide == ScreenModel.ScreenSide.TYPE_LEFT)
			{
				spawnX *= -1f;
				anchorMin = new Vector2(0f, 0.5f);
				anchorMax = new Vector2(0f, 0.5f);
			}
			else
			{
				anchorMin = new Vector2(1f, 0.5f);
				anchorMax = new Vector2(1f, 0.5f);
			}
		}

		private ComboItem CreateComboItem(ComboTypes LFLGCDNKNJI)
		{
			if (_comboItemPrefab == null)
			{
				return null;
			}
			ComboItem component = Object.Instantiate(_comboItemPrefab).GetComponent<ComboItem>();
			component.Init(LFLGCDNKNJI, modelSide);
			// Labels slide 30 units per tick; show the slide smoothly between ticks.
			Eclipse.Rendering.Interpolation.TickPresentationSmoother.AttachPosition(component.gameObject,
				Eclipse.Rendering.Interpolation.TickPresentationSmoother.Clock.Camera);
			return component;
		}

		// Combo labels are presentation: they update once per displayed tick, never
		// again while a rollback re-simulates ticks that already ran.
		public ComboNode CreateCritical()
		{
			if (Eclipse.Multiplayer.VersusTickDriver.IsResimulating)
			{
				return null;
			}
			comboStatistic.CriticalCount++;
			return AddNode(CreateComboItem(ComboTypes.TypeCritical));
		}

		public ComboNode CreateShock()
		{
			if (Eclipse.Multiplayer.VersusTickDriver.IsResimulating)
			{
				return null;
			}
			comboStatistic.ShockCount++;
			return AddNode(CreateComboItem(ComboTypes.TypeShock));
		}

		public ComboNode CreateFirstStrike()
		{
			if (Eclipse.Multiplayer.VersusTickDriver.IsResimulating)
			{
				return null;
			}
			comboStatistic.FirstStrikeCount++;
			return AddNode(CreateComboItem(ComboTypes.TypeFirstStrike));
		}

		public ComboNode CreateHeadStrike()
		{
			if (Eclipse.Multiplayer.VersusTickDriver.IsResimulating)
			{
				return null;
			}
			comboStatistic.HeadStrikeCount++;
			return AddNode(CreateComboItem(ComboTypes.TypeHead));
		}

		public ComboNode CreateComboStrike(int value)
		{
			if (Eclipse.Multiplayer.VersusTickDriver.IsResimulating)
			{
				return null;
			}
			ComboItem comboItem = CreateComboItem(ComboTypes.TypeCombo);
			comboItem.UpdateCount(value);
			return AddNode(comboItem);
		}

		public ComboNode CreateHotGroundTimer(int value)
		{
			if (Eclipse.Multiplayer.VersusTickDriver.IsResimulating)
			{
				return null;
			}
			ComboItem comboItem = CreateComboItem(ComboTypes.TypeHotGroundTimer);
			comboItem.UpdateCount(value);
			return AddNode(comboItem);
		}

		private ComboNode AddNode(ComboItem target)
		{
			Vector2 aLLNKANPNBL = firstNodePosition;
			float x = aLLNKANPNBL.x;
			Vector2 aLLNKANPNBL2 = firstNodePosition;
			Vector2 vector = new Vector2(x, aLLNKANPNBL2.y);
			target.get_rectTransform().pivot = ((modelSide != ScreenModel.ScreenSide.TYPE_LEFT) ? new Vector2(1f, 0.5f) : new Vector2(0f, 0.5f));
			vector.x = spawnX;
			target.get_rectTransform().anchorMin = anchorMin;
			target.get_rectTransform().anchorMax = anchorMax;
			if (nodes.Count > 0)
			{
				RectTransform rectTransform = nodes[nodes.Count - 1].Target.get_rectTransform();
				RectTransform rectTransform2 = target.get_rectTransform();
				vector.y = rectTransform.localPosition.y;
				vector.y -= rectTransform.rect.height * rectTransform.pivot.y;
				vector.y -= rectTransform2.rect.height * rectTransform2.pivot.y;
				vector.y -= 40f;
			}
			target.transform.SetParent(base.transform, false);
			target.transform.localPosition = vector;
			ComboNode iNEGMMHCDGN = new ComboNode();
			iNEGMMHCDGN.Count = GetDisplayCount(target.get_ComboType());
			iNEGMMHCDGN.Target = target;
			iNEGMMHCDGN.Type = target.get_ComboType();
			iNEGMMHCDGN.IgnoresPause = target.get_ComboType() == ComboTypes.TypeHotGroundTimer;
			nodes.Add(iNEGMMHCDGN);
			return iNEGMMHCDGN;
		}

		public void AddCrazyStyle(int value)
		{
			if (Eclipse.Multiplayer.VersusTickDriver.IsResimulating)
			{
				return;
			}
			comboStatistic.MaxStyle = (FightStatistics.FightStyle)Mathf.Max((int)comboStatistic.MaxStyle, value);
			comboStatistic.StatisticCrazyStyleToString = comboStatistic.GetCrazyStyleAlias();
		}

		public void AddPerfect()
		{
			if (Eclipse.Multiplayer.VersusTickDriver.IsResimulating)
			{
				return;
			}
			comboStatistic.PerfectCount++;
		}

		private void IncrementStatistic()
		{
			comboStatistic.OtherStrikeCount++;
		}

		public void ResetComboStrike()
		{
			currentComboCount = 0;
		}

		public void UpdateHotGroundTimer(int time)
		{
			if (Eclipse.Multiplayer.VersusTickDriver.IsResimulating)
			{
				return;
			}
			hotGroundTime = time;
			ComboNode iNEGMMHCDGN = nodes.Find((ComboNode DHDMNHCIPEH) => DHDMNHCIPEH.Type == ComboTypes.TypeHotGroundTimer);
			if (iNEGMMHCDGN == null)
			{
				iNEGMMHCDGN = CreateHotGroundTimer(hotGroundTime);
			}
			iNEGMMHCDGN.Count = GetDisplayCount(iNEGMMHCDGN.Type);
			iNEGMMHCDGN.Target.UpdateCount(hotGroundTime);
		}

		public void OnFightPause(bool value)
		{
			_fightPause = value;
		}

		public void UpdateCombo(int value, int HFMKKLJGPPN)
		{
			if (Eclipse.Multiplayer.VersusTickDriver.IsResimulating)
			{
				return;
			}
			currentComboCount = value;
			if (currentComboCount > 0)
			{
				comboIdleFrames = 0;
			}
			if (currentComboCount >= GameUtils.GetComboMinHits())
			{
				ComboNode iNEGMMHCDGN = nodes.Find((ComboNode DHDMNHCIPEH) => DHDMNHCIPEH.Type == ComboTypes.TypeCombo);
				bool flag = false;
				if (iNEGMMHCDGN == null)
				{
					iNEGMMHCDGN = CreateComboStrike(currentComboCount);
					flag = true;
				}
				iNEGMMHCDGN.Count = GetDisplayCount(iNEGMMHCDGN.Type, HFMKKLJGPPN);
				iNEGMMHCDGN.Target.UpdateCount(currentComboCount);
				comboStatistic.MaxCombo = Mathf.Max(comboStatistic.MaxCombo, currentComboCount);
				RaiseComboEvent((!flag) ? ComboTypeEvent.COMBO_INCREASE : ComboTypeEvent.COMBO_START);
			}
		}

		public void RemoveAllCombo()
		{
			foreach (ComboNode item in nodes)
			{
				item.Target.gameObject.SetActive(false);
				Object.Destroy(item.Target.gameObject);
			}
			nodes.Clear();
		}

		private void RaiseComboEvent(ComboTypeEvent LFLGCDNKNJI)
		{
			CallEvent(0, new ComboChangeInfo
			{
				ComboCount = currentComboCount,
				Type = LFLGCDNKNJI
			});
		}

		private int GetDisplayCount(ComboTypes LFLGCDNKNJI, int HFMKKLJGPPN = 0)
		{
			switch (LFLGCDNKNJI)
			{
			case ComboTypes.TypeCombo:
				return Mathf.Max(GameUtils.GetComboTime() + HFMKKLJGPPN, 0);
			case ComboTypes.TypeHotGroundTimer:
				return GameUtils.GetHotGroundTime();
			default:
				return GameUtils.GetAnnouncementTime();
			}
		}

		private bool ShouldUpdateNode(ComboNode node)
		{
			return node.IgnoresPause || !_fightPause;
		}

		public bool MoveTo(ComboItem target, Vector2 IPMPAMAHLJG, float ALCFJHNPDGL)
		{
			Vector2 vector = target.transform.localPosition;
			if (modelSide == ScreenModel.ScreenSide.TYPE_RIGHT)
			{
				IPMPAMAHLJG.x *= -1f;
			}
			float num = ((!(IPMPAMAHLJG.x > vector.x)) ? (0f - ALCFJHNPDGL) : ALCFJHNPDGL);
			float num2 = ((!(IPMPAMAHLJG.y > vector.y)) ? (0f - ALCFJHNPDGL) : ALCFJHNPDGL);
			float num3 = Mathf.Abs(IPMPAMAHLJG.x - vector.x);
			float num4 = Mathf.Abs(IPMPAMAHLJG.y - vector.y);
			bool flag = false;
			if (num3 > 0f)
			{
				vector.x = ((!(num3 > Mathf.Abs(num))) ? IPMPAMAHLJG.x : (vector.x + num));
				flag = true;
			}
			if (num4 > 0f)
			{
				vector.y = ((!(num4 > Mathf.Abs(num2))) ? IPMPAMAHLJG.y : (vector.y + num2));
				flag = true;
			}
			if (flag)
			{
				target.transform.localPosition = vector;
			}
			return vector == IPMPAMAHLJG;
		}

		private void StackNodes()
		{
			Vector2 aLLNKANPNBL = firstNodePosition;
			float x = aLLNKANPNBL.x;
			Vector2 aLLNKANPNBL2 = firstNodePosition;
			Vector2 iPMPAMAHLJG = new Vector2(x, aLLNKANPNBL2.y);
			foreach (ComboNode item in nodes)
			{
				RectTransform rectTransform = item.Target.get_rectTransform();
				iPMPAMAHLJG.x = rectTransform.localPosition.x;
				if (item.IsSettled)
				{
					if (iPMPAMAHLJG.y != 0f)
					{
						iPMPAMAHLJG.y -= rectTransform.rect.height * rectTransform.pivot.y;
					}
					MoveTo(item.Target, iPMPAMAHLJG, 0f);
					iPMPAMAHLJG.y -= rectTransform.rect.height * rectTransform.pivot.y;
					iPMPAMAHLJG.y -= 40f;
				}
				else
				{
					iPMPAMAHLJG.y = rectTransform.localPosition.y;
					iPMPAMAHLJG.y -= rectTransform.rect.height * rectTransform.pivot.y;
					iPMPAMAHLJG.y -= 40f;
				}
			}
		}

		private void UpdateNodeMovement()
		{
			List<ComboNode> list = new List<ComboNode>();
			foreach (ComboNode item in nodes)
			{
				if (!ShouldUpdateNode(item))
				{
					continue;
				}
				if (item.Count > 0)
				{
					item.IsSettled = false;
					Vector2 iPMPAMAHLJG = item.Target.transform.localPosition;
					iPMPAMAHLJG.x = restX;
					if (MoveTo(item.Target, iPMPAMAHLJG, 30f) && (item.Type != ComboTypes.TypeHotGroundTimer || GameUtils.GetSlowMode() == 1))
					{
						item.IsSettled = true;
						item.Count--;
					}
				}
				else
				{
					item.IsSettled = false;
					Vector2 iPMPAMAHLJG2 = item.Target.transform.localPosition;
					iPMPAMAHLJG2.x = 0f - item.Target.get_rectTransform().rect.width;
					if (MoveTo(item.Target, iPMPAMAHLJG2, 30f))
					{
						list.Add(item);
					}
				}
			}
			foreach (ComboNode item2 in list)
			{
				nodes.Remove(item2);
				item2.Target.gameObject.SetActive(false);
				Object.Destroy(item2.Target.gameObject);
			}
		}

		public void Render()
		{
			if (Eclipse.Multiplayer.VersusTickDriver.IsResimulating)
			{
				return;
			}
			UpdateNodeMovement();
			StackNodes();
			if (currentComboCount <= 0)
			{
				return;
			}
			if (comboIdleFrames >= comboTimeoutFrames)
			{
				if (currentComboCount >= GameUtils.GetComboMinHits())
				{
					RaiseComboEvent(ComboTypeEvent.COMBO_STOP);
				}
				comboIdleFrames = 0;
				currentComboCount = 0;
			}
			else
			{
				comboIdleFrames++;
			}
		}
	}
}
