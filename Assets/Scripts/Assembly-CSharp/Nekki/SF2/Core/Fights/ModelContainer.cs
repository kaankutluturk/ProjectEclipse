using System.Collections.Generic;
using System.Diagnostics;
using Nekki.SF2.GUI;
using UnityEngine;

namespace Nekki.SF2.Core.Fights
{
	public class ModelContainer : SFMonoBehaviour<object>
	{
		public enum ModelContainerEvent
		{
			EventAnimationEnd = 0,
			EventTryOnEnd = 1
		}

		[SerializeField]
		private Vector2 _modelPosition;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private float width;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private float height;

		private string _currentScene;

		private bool needsReapplyItems;

		private bool isRenderContainerShown;

		private bool isRenderReady;

		private StageType.Stage stageType;

		private ModelParameters modelParameters;

		private Model _playerModel;

		private RenderContainer renderContainer;

		private Location _location = new Location();

		private EquippedItemsStruct equippedItems = new EquippedItemsStruct();

		private SelectAnimation _selectAnimation = new SelectAnimation();

		private List<Model> _models = new List<Model>();

		private List<Model> pendingModels = new List<Model>();

		private List<Model> modelsToRemove = new List<Model>();

		private Color _colorModel = new Color32(40, 20, 9, byte.MaxValue);

		// Eclipse: a fighter to show instead of the saved player (versus loadout previews).
		private ModelParameters _eclipseParameters;

		private bool isRenderEnabled = true;

		public float ContainerWidth
		{
			get
			{
				return get_Width();
			}
			protected set
			{
				SetWidth(value);
			}
		}

		public float ContainerHeight
		{
			get
			{
				return get_Height();
			}
			protected set
			{
				SetHeight(value);
			}
		}

		public StageType.Stage CurrentStageType
		{
			get
			{
				return get__StageType();
			}
		}

		public float get_Width()
		{
			return width;
		}

		protected void SetWidth(float value)
		{
			width = value;
		}

		public float get_Height()
		{
			return height;
		}

		protected void SetHeight(float value)
		{
			height = value;
		}

		public StageType.Stage get__StageType()
		{
			return stageType;
		}

		public void Init(float JMLAKAKDBBL = 0f, float FEIHFIPFNKF = 0f, float LOJLAFEALJO = 0f, float ILLMIAIFBKL = 0f)
		{
			SetWidth((JMLAKAKDBBL != 0f) ? JMLAKAKDBBL : ((float)Screen.width));
			SetHeight(FEIHFIPFNKF);
			_location.gameLayer = new LocationSelector(0);
			_location.gameLayer.GetLayerObject().transform.SetParent(base.transform, false);
			RecreateRenderContainer();
			// The title's sparring previews run before any save is loaded. Their fighter comes
			// from ShowParameters, and UpdateModel replaces this saved-player placeholder anyway.
			if (ListSF.GetRoster() == null || ListSF.GetPlayerParameters() == null) return;
			modelParameters = GameUtils.GetPlayerModelParameters();
			modelParameters.AiControlled = false;
			modelParameters.UserControlled = false;
		}

		private void OnDestroy()
		{
			if (_playerModel != null)
			{
				_playerModel.RemoveEventListener(3, OnAnimationEnd);
				_playerModel.RemoveEventListener(6, OnModelAdded);
				_playerModel.RemoveEventListener(5, OnModelRemoved);
				_playerModel.RemoveEventListener(14, OnTryOnEnd);
				OnPlayerModelRemoved();
				_models.Remove(_playerModel);
				_playerModel.DestroyModel();
				_playerModel = null;
				_models.ForEach((Model DHDMNHCIPEH) =>
				{
					DHDMNHCIPEH.DestroyModel();
				});
				_models.Clear();
			}
		}

		private void RecreateRenderContainer()
		{
			if (renderContainer != null)
			{
				Object.Destroy(renderContainer.GetRootObject());
				renderContainer = null;
			}
			renderContainer = new RenderContainer();
			renderContainer.Init(_location);
			renderContainer.GetRootObject().SetActive(false);
		}

		public void UpdateModel(ItemInfo item, StageType.Stage LGPIFNMFPAN, string MHOCFOODLLL)
		{
			stageType = LGPIFNMFPAN;
			RecreateRenderContainer();
			if (_playerModel != null)
			{
				_playerModel.RemoveEventListener(3, OnAnimationEnd);
				_playerModel.RemoveEventListener(6, OnModelAdded);
				_playerModel.RemoveEventListener(5, OnModelRemoved);
				_playerModel.RemoveEventListener(14, OnTryOnEnd);
				OnPlayerModelRemoved();
				_models.Remove(_playerModel);
				_playerModel.DestroyModel();
				_playerModel = null;
				_models.ForEach((Model DHDMNHCIPEH) =>
				{
					DHDMNHCIPEH.DestroyModel();
				});
				_models.Clear();
			}
			modelParameters = new ModelParameters(_eclipseParameters ?? GameUtils.GetPlayerModelParameters());
			modelParameters.SpawnPosition = new Vector3f(_modelPosition);
			modelParameters.AiControlled = false;
			modelParameters.UserControlled = false;
			modelParameters.SceneType = GetSceneTypeForItemType(MHOCFOODLLL);
			_currentScene = MHOCFOODLLL;
			ItemInfo dJKEECEOCJB = null;
			if (item != null)
			{
				if (item.Type.Equals("Weapon"))
				{
					dJKEECEOCJB = equippedItems.Weapon;
					modelParameters.Weapon = item;
				}
				else if (item.Type.Equals("Armor"))
				{
					dJKEECEOCJB = equippedItems.Armor;
					modelParameters.Armor = item;
				}
				else if (item.Type.Equals("Helm"))
				{
					dJKEECEOCJB = equippedItems.Helm;
					modelParameters.Helm = item;
				}
				else if (item.Type.Equals("Ranged"))
				{
					dJKEECEOCJB = equippedItems.Ranged;
					modelParameters.Ranged = item;
				}
				else if (item.Type.Equals("Magic"))
				{
					dJKEECEOCJB = equippedItems.Magic;
					modelParameters.Magic = item;
				}
				else if (item.Type.Equals("RaidConsumable") && item.SubType.Equals("RaidCharge"))
				{
					dJKEECEOCJB = equippedItems.RaidCharge;
					RaidModelParameters kAOPLEPILDH = modelParameters as RaidModelParameters;
					if (kAOPLEPILDH != null)
					{
						kAOPLEPILDH.RaidChargeItem = item;
					}
				}
				modelParameters.BuildModelDocuments();
			}
			else
			{
				dJKEECEOCJB = FindChangedItem();
			}
			modelParameters.CopyEquippedItemsTo(equippedItems);
			// A shop's forced preview item is for the saved player, not a versus fighter.
			if (_eclipseParameters == null) ApplyShopOverride(modelParameters, MHOCFOODLLL);
			if (needsReapplyItems)
			{
				needsReapplyItems = false;
				modelParameters.CopyEquippedItemsTo(equippedItems);
			}
			isRenderContainerShown = false;
			_playerModel = new Model(modelParameters);
			_playerModel.AttachToParent();
			SetModelOnListening(_playerModel);
			_selectAnimation.ClearModelsAndEvents();
			_selectAnimation.AddModel(_playerModel);
			_models.Add(_playerModel);
			ApplyStageToPlayerModel();
			Render();
			AttachModelToRenderContainer();
		}

		private void ApplyStageToPlayerModel()
		{
			_playerModel.EventData.Data = stageType;
			_playerModel.RoundStage = (int)stageType;
			UpdateAnimationParameters(_playerModel);
			_selectAnimation.CheckEvent(EventAnimation.EventAnimationType.EVENT_ROUND_STAGE, _playerModel.EventData);
		}

		private void AttachModelToRenderContainer()
		{
			renderContainer.GetViewerModel().AddModel(_playerModel.GetBodyObject(), _colorModel, true);
			renderContainer.AttachModelEffects(_playerModel);
			isRenderReady = true;
		}

		/// <summary>
		/// Eclipse: shows <paramref name="parameters"/> (fully equipped) through the same menu
		/// model path as the profile scene, instead of the saved player.
		/// </summary>
		internal void ShowParameters(ModelParameters parameters, StageType.Stage stage, string scene, Color tint)
		{
			_eclipseParameters = parameters;
			_colorModel = tint;
			UpdateModel(null, stage, scene);
			// A preview is not framed by the scene camera; keep its walls well clear of the pose.
			_playerModel?.SetWalls(-4000f, 4000f, 0, 0);
		}

		/// <summary>
		/// Eclipse: where the model stands, in the container's fight coordinates (the menu
		/// scenes serialize this). The title stands its sparring fighters on a stage with it.
		/// </summary>
		internal void SetPreviewPosition(Vector2 position)
		{
			_modelPosition = position;
			ResetModelPosition();
		}

		public void PlayAnimation(string name, int AOJJBKLCHJO = 1)
		{
			TryPlayAnimation(name);
		}

		public bool TryPlayAnimation(string name)
		{
			InfoAnimation pJAHIOELGGD = AnimationData.GetAnimationByName(name);
			if (pJAHIOELGGD != null && _playerModel != null)
			{
				_playerModel.PlayAnimationDelay(pJAHIOELGGD);
				return true;
			}
			return false;
		}

		private void OnModelAdded(object data)
		{
			Model fGCODGKLHED = (Model)data;
			pendingModels.Add(fGCODGKLHED);
			SetModelOnListening(fGCODGKLHED);
			renderContainer.GetViewerModel().AddModel(fGCODGKLHED.GetBodyObject(), _colorModel, true);
			renderContainer.AttachModelEffects(fGCODGKLHED);
			UpdateAnimationParameters(fGCODGKLHED);
		}

		private void OnModelRemoved(object data)
		{
			Model fGCODGKLHED = (Model)data;
			int num = 0;
			foreach (Model item in _models)
			{
				if (item == fGCODGKLHED)
				{
					break;
				}
				num++;
			}
			modelsToRemove.AddIfNotExist(fGCODGKLHED);
		}

		private void UpdateAnimationParameters(Model CNAAFEHFGKD)
		{
			ModelObject bBGCMFGFMCL = CNAAFEHFGKD.GetBodyObject();
			bool dPKOKLCJEHI = CNAAFEHFGKD.IsPlayerModel();
			bool eMGNKKHPGCJ = CNAAFEHFGKD.GetParentModel() != null;
			List<InfoAnimation> lNKFKJKLCKP = CNAAFEHFGKD.GetAvailableAnimations();
			foreach (Model item in _models)
			{
				SyncAnimationParameters(item, lNKFKJKLCKP, bBGCMFGFMCL, dPKOKLCJEHI, eMGNKKHPGCJ);
			}
			foreach (Model item2 in pendingModels)
			{
				SyncAnimationParameters(item2, lNKFKJKLCKP, bBGCMFGFMCL, dPKOKLCJEHI, eMGNKKHPGCJ);
			}
		}

		private void SyncAnimationParameters(Model ACENLMONNPA, List<InfoAnimation> LNKFKJKLCKP, ModelObject BBGCMFGFMCL, bool DPKOKLCJEHI, bool EMGNKKHPGCJ)
		{
			List<InfoAnimation> list = ACENLMONNPA.GetAvailableAnimations();
			foreach (InfoAnimation item in list)
			{
				item.UpdateModelObjects(BBGCMFGFMCL, DPKOKLCJEHI, EMGNKKHPGCJ, BBGCMFGFMCL);
			}
			ModelObject oIEODIEHJMH = ACENLMONNPA.GetBodyObject();
			bool eKBOGDKIHIH = ACENLMONNPA.IsPlayerModel();
			bool pHADJMAONJG = ACENLMONNPA.GetParentModel() != null;
			foreach (InfoAnimation item2 in LNKFKJKLCKP)
			{
				item2.UpdateModelObjects(oIEODIEHJMH, eKBOGDKIHIH, pHADJMAONJG, oIEODIEHJMH);
			}
		}

		private bool IsItemDifferent(ItemInfo CHJGFBKFKKD, ItemInfo BGCMDCGMPPL)
		{
			if (CHJGFBKFKKD != null && BGCMDCGMPPL != null && !CHJGFBKFKKD.Name.Equals(BGCMDCGMPPL.Name))
			{
				return true;
			}
			return false;
		}

		public bool IsItemDiffer(ModelParameters JCICKLIMBEF)
		{
			if (IsItemDifferent(equippedItems.Armor, JCICKLIMBEF.Armor))
			{
				return true;
			}
			if (IsItemDifferent(equippedItems.Helm, JCICKLIMBEF.Helm))
			{
				return true;
			}
			if (IsItemDifferent(equippedItems.Skeleton, JCICKLIMBEF.Skeleton))
			{
				return true;
			}
			if (IsItemDifferent(equippedItems.Seal, JCICKLIMBEF.Seal))
			{
				return true;
			}
			if (IsItemDifferent(equippedItems.Weapon, JCICKLIMBEF.Weapon))
			{
				return true;
			}
			if (IsItemDifferent(equippedItems.Magic, JCICKLIMBEF.Magic))
			{
				return true;
			}
			if (IsItemDifferent(equippedItems.Ranged, JCICKLIMBEF.Ranged))
			{
				return true;
			}
			RaidModelParameters kAOPLEPILDH = JCICKLIMBEF as RaidModelParameters;
			if (kAOPLEPILDH != null && IsItemDifferent(equippedItems.RaidCharge, kAOPLEPILDH.RaidChargeItem))
			{
				return true;
			}
			return false;
		}

		private ItemInfo FindChangedItem()
		{
			if (equippedItems.Armor != modelParameters.Armor)
			{
				return equippedItems.Armor;
			}
			if (equippedItems.Helm != modelParameters.Helm)
			{
				return equippedItems.Helm;
			}
			if (equippedItems.Skeleton != modelParameters.Skeleton)
			{
				return equippedItems.Skeleton;
			}
			if (equippedItems.Seal != modelParameters.Seal)
			{
				return equippedItems.Seal;
			}
			if (equippedItems.Weapon != modelParameters.Weapon)
			{
				return equippedItems.Weapon;
			}
			if (equippedItems.Magic != modelParameters.Magic)
			{
				return equippedItems.Magic;
			}
			if (equippedItems.Ranged != modelParameters.Ranged)
			{
				return equippedItems.Ranged;
			}
			RaidModelParameters kAOPLEPILDH = modelParameters as RaidModelParameters;
			if (kAOPLEPILDH != null && equippedItems.RaidCharge != kAOPLEPILDH.RaidChargeItem)
			{
				return equippedItems.RaidCharge;
			}
			return null;
		}

		private void RemoveQueuedModels()
		{
			foreach (Model item in modelsToRemove)
			{
				int num = 0;
				foreach (Model item2 in _models)
				{
					if (item2 == item)
					{
						break;
					}
					num++;
				}
				RemoveModelByIndex(num, item);
			}
			modelsToRemove.Clear();
		}

		private void RemoveModelByIndex(int index, Model LEKHCMIFJAO)
		{
			int count = _models.Count;
			Model fGCODGKLHED = null;
			if (count == 0 || index < 0 || count - 1 < index)
			{
				fGCODGKLHED = LEKHCMIFJAO;
			}
			else
			{
				fGCODGKLHED = _models[index];
				renderContainer.GetViewerModel().RemoveModel(index);
				renderContainer.DetachModelEffects(fGCODGKLHED);
				_models.Remove(fGCODGKLHED);
			}
			RemoveModel(fGCODGKLHED);
		}

		private void RemoveModel(Model ACENLMONNPA)
		{
			if (ACENLMONNPA == null)
			{
				GameLog.Error("Fight::removeModel - cant find model");
				return;
			}
			Model fGCODGKLHED = ACENLMONNPA.GetParentModel();
			if (fGCODGKLHED != null)
			{
				fGCODGKLHED.RemoveWeaponModel((WeaponModel)ACENLMONNPA);
			}
			foreach (Model item in _models)
			{
				item.RemoveEnemy(ACENLMONNPA);
				item.SetNearestEnemy();
			}
			_selectAnimation.RemoveModel(ACENLMONNPA);
			ACENLMONNPA.DetachCurrentEffects();
			ACENLMONNPA.DestroyModel();
		}

		private void OnPlayerModelRemoved()
		{
		}

		private SceneTypes GetSceneTypeForItemType(string LFLGCDNKNJI)
		{
			switch (LFLGCDNKNJI)
			{
			case "Weapon":
				return SceneTypes.SceneShopWeapon;
			case "Armor":
				return SceneTypes.SceneShopArmor;
			case "Helm":
				return SceneTypes.SceneShopHelm;
			case "Ranged":
				return SceneTypes.SceneShopMissile;
			case "Magic":
				return SceneTypes.SceneShopMagic;
			case "RealMoneyItem":
				return SceneTypes.SceneShopRuby;
			case "Consumable":
				return SceneTypes.SceneShopRuby;
			case "Free":
				return SceneTypes.SceneShopFree;
			case "RaidItemPack":
				return SceneTypes.SceneShopRaidItemPack;
			case "RaidConsumable":
				return SceneTypes.SceneShopRaidItemPack;
			case "Profile":
				return SceneTypes.SceneProfile;
			default:
				return SceneTypes.SceneNone;
			}
		}

		public void ResetModel()
		{
			_selectAnimation.Reset();
			ResetModelPosition();
			ApplyStageToPlayerModel();
		}

		public void ResetModelPosition()
		{
			if (_playerModel != null)
			{
				Vector3f mGMMDGFPBLP = new Vector3f(_modelPosition);
				_playerModel.SetModelPosition(mGMMDGFPBLP);
			}
		}

		private void ApplyShopOverride(ModelParameters JCICKLIMBEF, string NFNJJIGAKNN)
		{
			ShopOverride jHJPEFFBMFM = GameUtils.ShopOverrides.GetOverrideByScreen(NFNJJIGAKNN);
			if (jHJPEFFBMFM != null)
			{
				ItemInfo mBIJKDIEFIF = ListSF.GetItems().GetItemByName(jHJPEFFBMFM.ItemName);
				modelParameters.SetItemByType(jHJPEFFBMFM.Type, mBIJKDIEFIF);
				modelParameters.BuildModelDocuments();
			}
		}

		private void SetModelOnListening(Model ACENLMONNPA)
		{
			// Eclipse: menus without a main camera (versus previews) set their walls themselves.
			if (UnityEngine.Camera.main != null)
			{
				float nGHJOCKCCHH = UnityEngine.Camera.main.ScreenToWorldPoint(new Vector2(0f, 0f)).x - base.transform.position.x;
				float kCNCLAANGGJ = UnityEngine.Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, 0f)).x - base.transform.position.x;
				ACENLMONNPA.SetWalls(nGHJOCKCCHH, kCNCLAANGGJ, 0, 0);
			}
			ACENLMONNPA.AddEventListener(3, OnAnimationEnd);
			ACENLMONNPA.AddEventListener(6, OnModelAdded);
			ACENLMONNPA.AddEventListener(5, OnModelRemoved);
			ACENLMONNPA.AddEventListener(14, OnTryOnEnd);
		}

		private void OnAnimationEnd(object data)
		{
			CallEvent(0, data);
		}

		private void OnTryOnEnd(object data)
		{
			CallEvent(1, data);
		}

		private void FixedUpdate()
		{
			if (isRenderEnabled || Eclipse.Input.EclipseInput.GetKeyDown(KeyCode.Equals))
			{
				Render();
			}
			if (Eclipse.Input.EclipseInput.GetKeyDown(KeyCode.Minus))
			{
				isRenderEnabled = !isRenderEnabled;
			}
		}

		private void Render()
		{
			if (!isRenderReady)
			{
				return;
			}
			foreach (Model item in _models)
			{
				if (!isRenderContainerShown && item.GetCurrentFrame() != -1)
				{
					isRenderContainerShown = true;
					renderContainer.GetRootObject().SetActive(true);
				}
				item.Render();
			}
			if (pendingModels.Count > 0)
			{
				foreach (Model item2 in pendingModels)
				{
					item2.Render();
					_models.Add(item2);
				}
				pendingModels.Clear();
			}
			_selectAnimation.Render();
			renderContainer.GetBackgroundEffects().UpdateEffects();
			renderContainer.GetForegroundEffects().UpdateEffects();
			RemoveQueuedModels();
		}
	}
}
