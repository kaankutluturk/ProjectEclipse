using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Nekki.SF2.GUI.Common
{
	public class ModelDebugUI : UIModule
	{
		[SerializeField]
		private Text _PlayerAnimName;

		[SerializeField]
		private Text _EnemyAnimName;

		private StringBuilder playerTextBuilder;

		private StringBuilder enemyTextBuilder;

		protected override void Init()
		{
			base.Init();
			if (SceneManagerSF.GetCurrentScreen() != ScreenType.ModuleDojo && SceneManagerSF.GetCurrentScreen() != ScreenType.ModuleFight)
			{
				base.gameObject.SetActive(false);
			}
			playerTextBuilder = new StringBuilder();
			enemyTextBuilder = new StringBuilder();
		}

		private void Update()
		{
			global::Fight fight = global::Fight.GetCurrentFight();
			if (fight == null)
			{
				return;
			}
			playerTextBuilder.Clear();
			enemyTextBuilder.Clear();
			List<Model> models = fight.ActiveModels;
			for (int i = 0; i < models.Count; i++)
			{
				Model model = models[i];
				StringBuilder stringBuilder = ((!model.IsPlayerModel()) ? enemyTextBuilder : playerTextBuilder);
				if (model.Parameters.FightTactic != null && model.Parameters.FightTactic.get_Type() == Tactic.TacticType.TacticTabular)
				{
					stringBuilder.Append(AiData.GetTacticsTableName(model.GetAi().get_ResultSource()));
					stringBuilder.Append("\n");
				}
			}
			for (int j = 0; j < models.Count; j++)
			{
				Model fGCODGKLHED2 = models[j];
				StringBuilder stringBuilder2 = ((!fGCODGKLHED2.IsPlayerModel()) ? enemyTextBuilder : playerTextBuilder);
				stringBuilder2.Append(FormatModelAnimationInfo(fGCODGKLHED2));
			}
			string text = playerTextBuilder.ToString();
			string text2 = enemyTextBuilder.ToString();
			if (_PlayerAnimName.text != text)
			{
				_PlayerAnimName.text = text;
			}
			if (_EnemyAnimName.text != text2)
			{
				_EnemyAnimName.text = text2;
			}
		}

		private static string FormatModelAnimationInfo(Model model)
		{
			StringBuilder stringBuilder = new StringBuilder();
			InfoAnimation animation = model.GetCurrentAnimation();
			int num = -1;
			if (model.IsInPhysics())
			{
				num = model.GetPhysicsModule().GetFrame();
				List<string> list = model.GetPhysicsNames();
				int count = list.Count;
				for (int i = 0; i < count; i++)
				{
					stringBuilder.Append(list[i]);
					if (i < count - 1)
					{
						stringBuilder.Append(" | ");
					}
				}
			}
			else
			{
				stringBuilder.Append((animation == null) ? "----" : animation.Name);
				num = ((animation == null) ? (-1) : model.GetAnimationModule().GetCurrentFrame());
			}
			stringBuilder.Append("    ");
			if (num > -1)
			{
				stringBuilder.Append(num);
			}
			return stringBuilder.ToString();
		}
	}
}
