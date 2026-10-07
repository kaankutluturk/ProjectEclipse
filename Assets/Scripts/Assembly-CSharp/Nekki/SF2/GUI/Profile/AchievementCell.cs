using UnityEngine;

namespace Nekki.SF2.GUI.Profile
{
	public class AchievementCell : ProfileCell
	{
		private const int CELL_KIND_ID = 3;

		[SerializeField]
		private AchievementSubItem achievSubItem;

		private void BindSubItem()
		{
			achievSubItem.ParentCell = this;
			achievSubItem.transform.SetLocalY(0f);
			achievSubItem.transform.SetLocalX(-240f);
			achievSubItem.RemoveAllEventListener();
			achievSubItem.AddEventListener(2, OnSubItemClick);
			achievSubItem.AddEventListener(10, Scene<ProfileScene>.get_Current().OnSubItemClick);
			achievSubItem.AddEventListener(12, Scene<ProfileScene>.get_Current().OnAchievementRewardTake);
		}

		public void Init(Achievement achievement, int currentValue, int cellIndex)
		{
			Clear();
			BindSubItem();
			string iconName = achievement.GetIconName();
			int buttonId = 30000 + cellIndex * 10;
			achievSubItem.Init(iconName, achievement.Name, achievement.Description, achievement.CounterValue, currentValue, buttonId, achievement);
			Scene<ProfileScene>.get_Current().SubItems.Add(achievSubItem);
		}

		public override SubItem GetFirstIcon()
		{
			return achievSubItem;
		}

		public override void UpdateState()
		{
			achievSubItem.UpdateState();
		}

		public override void Clear()
		{
			Scene<ProfileScene>.get_Current().SubItems.Remove(achievSubItem);
		}

		public void OnSubItemClick(object data)
		{
			if (DidSelectEvent != null)
			{
				DidSelectEvent.Invoke(get_RowNumber());
			}
		}

		private void OnDestroy()
		{
			RemoveAllEventListener();
			achievSubItem.RemoveAllEventListener();
		}
	}
}
