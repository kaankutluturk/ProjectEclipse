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
			global::Fight gDBOMJODDEA = global::Fight.GetCurrentFight();
			if (gDBOMJODDEA == null)
			{
				return;
			}
			playerTextBuilder.Clear();
			enemyTextBuilder.Clear();
			List<Model> lNDLFINJHDB = gDBOMJODDEA.ActiveModels;
			for (int i = 0; i < lNDLFINJHDB.Count; i++)
			{
				Model fGCODGKLHED = lNDLFINJHDB[i];
				StringBuilder stringBuilder = ((!fGCODGKLHED.IsPlayerModel()) ? enemyTextBuilder : playerTextBuilder);
				if (fGCODGKLHED.Parameters.FightTactic != null && fGCODGKLHED.Parameters.FightTactic.get_Type() == Tactic.TacticType.TacticTabular)
				{
					stringBuilder.Append(AiData.GetTacticsTableName(fGCODGKLHED.GetAi().get_ResultSource()));
					stringBuilder.Append("\n");
				}
			}
			for (int j = 0; j < lNDLFINJHDB.Count; j++)
			{
				Model fGCODGKLHED2 = lNDLFINJHDB[j];
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

		private static string FormatModelAnimationInfo(Model ACENLMONNPA)
		{
			StringBuilder stringBuilder = new StringBuilder();
			InfoAnimation pJAHIOELGGD = ACENLMONNPA.GetCurrentAnimation();
			int num = -1;
			if (ACENLMONNPA.IsInPhysics())
			{
				num = ACENLMONNPA.GetPhysicsModule().GetFrame();
				List<string> list = ACENLMONNPA.GetPhysicsNames();
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
				stringBuilder.Append((pJAHIOELGGD == null) ? "----" : pJAHIOELGGD.Name);
				num = ((pJAHIOELGGD == null) ? (-1) : ACENLMONNPA.GetAnimationModule().GetCurrentFrame());
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
