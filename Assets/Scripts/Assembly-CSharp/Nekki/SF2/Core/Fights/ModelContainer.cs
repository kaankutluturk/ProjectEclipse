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

		public void Init(float containerWidth = 0f, float containerHeight = 0f, float offsetX = 0f, float offsetY = 0f)
		{
			SetWidth((containerWidth != 0f) ? containerWidth : ((float)Screen.width));
			SetHeight(containerHeight);
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
				_models.ForEach((Model model) =>
				{
					model.DestroyModel();
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

		public void UpdateModel(ItemInfo item, StageType.Stage stage, string itemType)
		{
			stageType = stage;
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
				_models.ForEach((Model model) =>
				{
					model.DestroyModel();
				});
				_models.Clear();
			}
			modelParameters = new ModelParameters(_eclipseParameters ?? GameUtils.GetPlayerModelParameters());
			modelParameters.SpawnPosition = new Vector3f(_modelPosition);
			modelParameters.AiControlled = false;
			modelParameters.UserControlled = false;
			modelParameters.SceneType = GetSceneTypeForItemType(itemType);
			_currentScene = itemType;
			ItemInfo equippedItem = null;
			if (item != null)
			{
				if (item.Type.Equals("Weapon"))
				{
					equippedItem = equippedItems.Weapon;
					modelParameters.Weapon = item;
				}
				else if (item.Type.Equals("Armor"))
				{
					equippedItem = equippedItems.Armor;
					modelParameters.Armor = item;
				}
				else if (item.Type.Equals("Helm"))
				{
					equippedItem = equippedItems.Helm;
					modelParameters.Helm = item;
				}
				else if (item.Type.Equals("Ranged"))
				{
					equippedItem = equippedItems.Ranged;
					modelParameters.Ranged = item;
				}
				else if (item.Type.Equals("Magic"))
				{
					equippedItem = equippedItems.Magic;
					modelParameters.Magic = item;
				}
				else if (item.Type.Equals("RaidConsumable") && item.SubType.Equals("RaidCharge"))
				{
					equippedItem = equippedItems.RaidCharge;
					RaidModelParameters raidParameters = modelParameters as RaidModelParameters;
					if (raidParameters != null)
					{
						raidParameters.RaidChargeItem = item;
					}
				}
				modelParameters.BuildModelDocuments();
			}
			else
			{
				equippedItem = FindChangedItem();
			}
			modelParameters.CopyEquippedItemsTo(equippedItems);
			// A shop's forced preview item is for the saved player, not a versus fighter.
			if (_eclipseParameters == null) ApplyShopOverride(modelParameters, itemType);
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

		public void PlayAnimation(string name, int repeat = 1)
		{
			TryPlayAnimation(name);
		}

		public bool TryPlayAnimation(string name)
		{
			InfoAnimation animation = AnimationData.GetAnimationByName(name);
			if (animation != null && _playerModel != null)
			{
				_playerModel.PlayAnimationDelay(animation);
				return true;
			}
			return false;
		}

		private void OnModelAdded(object data)
		{
			Model model = (Model)data;
			pendingModels.Add(model);
			SetModelOnListening(model);
			renderContainer.GetViewerModel().AddModel(model.GetBodyObject(), _colorModel, true);
			renderContainer.AttachModelEffects(model);
			UpdateAnimationParameters(model);
		}

		private void OnModelRemoved(object data)
		{
			Model model = (Model)data;
			int num = 0;
			foreach (Model item in _models)
			{
				if (item == model)
				{
					break;
				}
				num++;
			}
			modelsToRemove.AddIfNotExist(model);
		}

		private void UpdateAnimationParameters(Model model)
		{
			ModelObject bodyObject = model.GetBodyObject();
			bool isPlayer = model.IsPlayerModel();
			bool hasParent = model.GetParentModel() != null;
			List<InfoAnimation> animations = model.GetAvailableAnimations();
			foreach (Model item in _models)
			{
				SyncAnimationParameters(item, animations, bodyObject, isPlayer, hasParent);
			}
			foreach (Model item2 in pendingModels)
			{
				SyncAnimationParameters(item2, animations, bodyObject, isPlayer, hasParent);
			}
		}

		private void SyncAnimationParameters(Model model, List<InfoAnimation> animations, ModelObject ownerBodyObject, bool ownerIsPlayer, bool ownerHasParent)
		{
			List<InfoAnimation> list = model.GetAvailableAnimations();
			foreach (InfoAnimation item in list)
			{
				item.UpdateModelObjects(ownerBodyObject, ownerIsPlayer, ownerHasParent, ownerBodyObject);
			}
			ModelObject bodyObject = model.GetBodyObject();
			bool isPlayer = model.IsPlayerModel();
			bool hasParent = model.GetParentModel() != null;
			foreach (InfoAnimation item2 in animations)
			{
				item2.UpdateModelObjects(bodyObject, isPlayer, hasParent, bodyObject);
			}
		}

		private bool IsItemDifferent(ItemInfo currentItem, ItemInfo otherItem)
		{
			if (currentItem != null && otherItem != null && !currentItem.Name.Equals(otherItem.Name))
			{
				return true;
			}
			return false;
		}

		public bool IsItemDiffer(ModelParameters parameters)
		{
			if (IsItemDifferent(equippedItems.Armor, parameters.Armor))
			{
				return true;
			}
			if (IsItemDifferent(equippedItems.Helm, parameters.Helm))
			{
				return true;
			}
			if (IsItemDifferent(equippedItems.Skeleton, parameters.Skeleton))
			{
				return true;
			}
			if (IsItemDifferent(equippedItems.Seal, parameters.Seal))
			{
				return true;
			}
			if (IsItemDifferent(equippedItems.Weapon, parameters.Weapon))
			{
				return true;
			}
			if (IsItemDifferent(equippedItems.Magic, parameters.Magic))
			{
				return true;
			}
			if (IsItemDifferent(equippedItems.Ranged, parameters.Ranged))
			{
				return true;
			}
			RaidModelParameters raidParameters = parameters as RaidModelParameters;
			if (raidParameters != null && IsItemDifferent(equippedItems.RaidCharge, raidParameters.RaidChargeItem))
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
			RaidModelParameters raidParameters = modelParameters as RaidModelParameters;
			if (raidParameters != null && equippedItems.RaidCharge != raidParameters.RaidChargeItem)
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

		private void RemoveModelByIndex(int index, Model model)
		{
			int count = _models.Count;
			Model target = null;
			if (count == 0 || index < 0 || count - 1 < index)
			{
				target = model;
			}
			else
			{
				target = _models[index];
				renderContainer.GetViewerModel().RemoveModel(index);
				renderContainer.DetachModelEffects(target);
				_models.Remove(target);
			}
			RemoveModel(target);
		}

		private void RemoveModel(Model model)
		{
			if (model == null)
			{
				GameLog.Error("Fight::removeModel - cant find model");
				return;
			}
			Model parentModel = model.GetParentModel();
			if (parentModel != null)
			{
				parentModel.RemoveWeaponModel((WeaponModel)model);
			}
			foreach (Model item in _models)
			{
				item.RemoveEnemy(model);
				item.SetNearestEnemy();
			}
			_selectAnimation.RemoveModel(model);
			model.DetachCurrentEffects();
			model.DestroyModel();
		}

		private void OnPlayerModelRemoved()
		{
		}

		private SceneTypes GetSceneTypeForItemType(string itemType)
		{
			switch (itemType)
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
				Vector3f resetPosition = new Vector3f(_modelPosition);
				_playerModel.SetModelPosition(resetPosition);
			}
		}

		private void ApplyShopOverride(ModelParameters targetParameters, string screenName)
		{
			ShopOverride shopOverride = GameUtils.ShopOverrides.GetOverrideByScreen(screenName);
			if (shopOverride != null)
			{
				ItemInfo overrideItem = ListSF.GetItems().GetItemByName(shopOverride.ItemName);
				modelParameters.SetItemByType(shopOverride.Type, overrideItem);
				modelParameters.BuildModelDocuments();
			}
		}

		private void SetModelOnListening(Model model)
		{
			// Eclipse: menus without a main camera (versus previews) set their walls themselves.
			if (UnityEngine.Camera.main != null)
			{
				float leftWall = UnityEngine.Camera.main.ScreenToWorldPoint(new Vector2(0f, 0f)).x - base.transform.position.x;
				float rightWall = UnityEngine.Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, 0f)).x - base.transform.position.x;
				model.SetWalls(leftWall, rightWall, 0, 0);
			}
			model.AddEventListener(3, OnAnimationEnd);
			model.AddEventListener(6, OnModelAdded);
			model.AddEventListener(5, OnModelRemoved);
			model.AddEventListener(14, OnTryOnEnd);
		}

		private void OnAnimationEnd(object data)
		{
			CallEvent(0, data);
		}

		private void OnTryOnEnd(object data)
		{
			CallEvent(1, data);
		}

		// Eclipse: Moveset Lab preview control. A paused preview advances only through PreviewStep.
		internal bool PreviewPaused { get; set; }

		internal Model PreviewModel => _playerModel;

		internal void PreviewStep(int ticks)
		{
			for (int i = 0; i < ticks; i++) Render();
		}

		// Eclipse: a paused preview shows each model's current pose (see ModelPresentation.Frozen).
		// Synced every frame so models rebuilt or spawned during a pause are frozen too.
		internal void SyncPreviewFreeze()
		{
			FreezePresentation(_playerModel);
			foreach (Model model in _models) FreezePresentation(model);
		}

		// Eclipse: the preview owns its fighter's walls. The model is built through the menu
		// path, whose walls follow the scene camera (or stay at zero without one), which put
		// a wall right behind a preview fighter and turned recoils into wall hits.
		internal void KeepPreviewWalls(float left, float right)
		{
			if (_playerModel != null && (_playerModel.GetLeftWallX() != left || _playerModel.GetRightWallX() != right))
				_playerModel.SetWalls(left, right, 0, 0);
		}

		private void FreezePresentation(Model model)
		{
			var root = model?.UnityObject;
			var presentation = root != null ? root.GetComponent<Eclipse.Rendering.ModelPresentation>() : null;
			if (presentation != null) presentation.Frozen = PreviewPaused;
		}

		private void FixedUpdate()
		{
			if (PreviewPaused) return;
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
