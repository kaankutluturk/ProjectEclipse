using Nekki.SF2.Core.Fights.Controller;
using Nekki.SF2.GUI.Common;
using Nekki.SF2.GUI.Menu;
using UnityEngine;

namespace Nekki.SF2.GUI.Scenes
{
	public class DojoScene : Scene<DojoScene>
	{
		[SerializeField]
		private MainMenu _mainMenu;

		[SerializeField]
		public GameController gameController;

		private PaymentUI paymentUi;

		public global::Fight fight;

		private bool drawFightEnabled = true;

		public PaymentUI PaymentPanel
		{
			get
			{
				return get_PaymentUI();
			}
		}

		public override ScreenType SceneType
		{
			get
			{
				return get_SceneId();
			}
		}

		public PaymentUI get_PaymentUI()
		{
			return paymentUi;
		}

		public override ScreenType get_SceneId()
		{
			return ScreenType.ModuleDojo;
		}

		protected override void Init(object data)
		{
			base.Init(data);
			paymentUi = GetModule<PaymentUI>();
			_mainMenu.Init();
			FightList jDIPBIHBGPF = ListSF.GetCurrentZone().FindBattlesByType(BattleType.FightNone)[0].GetFightByIndex(0);
			jDIPBIHBGPF = GameUtils.GetFinalFight(jDIPBIHBGPF);
			RosterFight pIGKOIFBOME = ListSF.GetRoster().FindSavedFightRecord(jDIPBIHBGPF.FightId);
			if (pIGKOIFBOME == null)
			{
				pIGKOIFBOME = ListSF.GetRoster().CreateFight(jDIPBIHBGPF.FightId);
			}
			jDIPBIHBGPF.SetRosterFight(pIGKOIFBOME);
			fight = GameUtils.CreateFight(jDIPBIHBGPF, null, gameController);
		}

		protected override void OnSceneClosed()
		{
			base.OnSceneClosed();
			if (fight != null)
			{
				fight.Unload();
			}
		}

		public override void UpdateScene(object data)
		{
		}

		private void FixedUpdate()
		{
			if ((drawFightEnabled || Eclipse.Input.EclipseInput.GetKeyDown(KeyCode.Equals)) && fight != null)
			{
				fight.Draw();
			}
			if (Eclipse.Input.EclipseInput.GetKeyDown(KeyCode.Minus))
			{
				drawFightEnabled = !drawFightEnabled;
			}
		}
	}
}
