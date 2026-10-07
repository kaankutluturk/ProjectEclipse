using System;
using Nekki.SF2.Core.Fights.Controller;
using UnityEngine;

namespace Nekki.SF2.GUI.Fight
{
	public class FightScene : Scene<FightScene>
	{
		[SerializeField]
		private PreFight preFight;

		[SerializeField]
		public GameController gameController;

		public global::Fight Fight;

		private bool isInitialized = true;

		public override ScreenType SceneType
		{
			get
			{
				return get_SceneId();
			}
		}

		public override ScreenType get_SceneId()
		{
			return ScreenType.ModuleFight;
		}

		protected override void Init(object data)
		{
			base.Init(data);
			FightList fightList = (FightList)data;
			if (preFight == null)
			{
				GameLog.Error("FightHolder.Start preFight is null");
			}
			Fight = GameUtils.CreateFight(fightList, preFight, gameController);
			GC.Collect();
		}

		private void FixedUpdate()
		{
			if (Fight != null)
			{
				Fight.Draw();
			}
		}

		protected override void OnSceneClosed()
		{
			base.OnSceneClosed();
			if (Fight != null)
			{
				Fight.Unload();
			}
		}

		public void RandomizeObscuredVars()
		{
			if (Fight != null)
			{
				Fight.RandomizeObscuredVars();
			}
		}
	}
}
