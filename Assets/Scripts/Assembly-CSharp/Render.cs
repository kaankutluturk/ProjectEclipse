using System;
using System.Collections.Generic;
using Eclipse.Rendering.Interpolation;
using UnityEngine;

public class Render
{
    private readonly Eclipse.Modding.ModCameraProjection _eclipseCameraProjection = new Eclipse.Modding.ModCameraProjection();
	private enum RenderZLayer
	{
		Z_DARKNESS = 0,
		Z_RINGOUT = 1,
		Z_HOTGROUND = 2,
		Z_HIT = 3
	}

	public const float FighterDistancePadding = 300f;

	public const string ArrowSpritePath = "Textures/fight/pointers/arrow";

	public const float GroundMarkerOffset = 10f;

	private SpriteRenderer pointerArrow;

	private SpriteRenderer darknessOverlay;

	private SpriteRenderer _lightInTheDarkness;
	private Sprite _lightInTheDarknessSprite;
	private Material _lightInTheDarknessMaterial;

	private int arrowPulseFrame;

	private GameObject hitLayer;

	private LocationSelector additionalDrawsLayer;

	private GameObject hitEffectObject;

	private CocosAnimation hitAnimation;

	private ChangingSprite leftRingOutSprite;

	private ChangingSprite rightRingOutSprite;

	private SpriteRenderer perkActivationArea;

	private SpriteRenderer perkActivationAreaIcon;

	private FightTransformInterpolation _PerkActivationAreaInterpolation;

	private bool lockMinZoom;

	private float zoom;

	private float viewportScale;

	private float verticalOffset;

	private float visibleWidth;

	private float minZoom;

	private float cameraOffsetX;

	private float unusedCameraValue;

	public Location _location;

	public RenderContainer Container;

	private List<BloodEffect> bloodEffects = new List<BloodEffect>();

	private int bloodEffectFrame;

	private GameObject _UnityObject;

	public bool LockMinZoom
	{
		get
		{
			return GetLockMinZoom();
		}
		set
		{
			SetLockMinZoom(value);
		}
	}

	public GameObject RootObject
	{
		get
		{
			return GetRootObject();
		}
	}

	public ViewerModel ModelViewer
	{
		get
		{
			return GetViewerModel();
		}
	}

	public EffectsContainer BackgroundEffects
	{
		get
		{
			return GetBackgroundEffects();
		}
	}

	public EffectsContainer ForegroundEffects
	{
		get
		{
			return GetForegroundEffects();
		}
	}

	public float AutoZoom
	{
		get
		{
			return CalculateAutoZoom();
		}
	}

	public float MaxZoom
	{
		get
		{
			return GetMaxZoom();
		}
	}

	public Render(GameObject parentObject)
	{
		_UnityObject = new GameObject("Render");
		_UnityObject.transform.SetParent(parentObject.transform, false);
		_location = null;
	}

	public bool GetLockMinZoom()
	{
		return lockMinZoom;
	}

	public void SetLockMinZoom(bool value)
	{
		lockMinZoom = value;
	}

	public GameObject GetRootObject()
	{
		return _UnityObject;
	}

	public void Init(Location location)
	{
		_location = location;
		zoom = 1f;
		RefreshViewportMetrics();
		verticalOffset = (_location.height / 2f - _location.floorHeight) / 2f;
		CreateRenderContainer();
		CreateArrow();
		CreateHitEffectObjects();
		_UnityObject.transform.localScale = new Vector3(1f, -1f, 1f);
		Eclipse.Rendering.LocationAtmosphere.Attach(_UnityObject, _location);
	}

	// Reuse native location layers/animation and mod atmosphere in menu previews.
	// The shop's fighter preview keeps its own authored position and renderer.
	public void UpdateMenuBackdrop(UnityEngine.Camera camera, bool advanceAnimation, float? fightCenterX = null)
	{
		if (camera == null || _location == null || _location.width <= 0f || _location.height <= 0f) return;
		float height = camera.orthographicSize * 2f;
		float scale = Mathf.Max(height / _location.height, height * camera.aspect / _location.width);
		float offset = 0f;
		if (fightCenterX.HasValue)
		{
			float halfVisible = height * camera.aspect / scale * .5f;
			float center = Mathf.Clamp(fightCenterX.Value, halfVisible, _location.width - halfVisible);
			offset = _location.width * .5f - center;
		}
		_UnityObject.transform.position = new Vector3(camera.transform.position.x, camera.transform.position.y, 100f + _location.layers.Count * 3f);
		_UnityObject.transform.localScale = new Vector3(scale, -scale, 1f);
		// As in the fight camera, pan the game layer with the fighters and let
		// distant layers move by their authored factors. Moving the whole root
		// exposes the edges of backgrounds that only cover the viewport.
		bool behindGameLayer = true;
		foreach (var layer in _location.layers)
		{
			if (layer == _location.gameLayer) behindGameLayer = false;
			float factor = layer.Factor;
			if (behindGameLayer) factor = Eclipse.Modding.ModVisuals.BackgroundLayerFactor(factor);
			layer.SetPositionX(offset * factor);
		}
		SyncAdditionalDrawsLayerTransform();
		pointerArrow.gameObject.SetActive(false);
		if (advanceAnimation) RenderLayers();
	}

	public void DestroyMenuBackdrop()
	{
		Clear();
		UnityEngine.Object.Destroy(_UnityObject);
	}

	private void RefreshViewportMetrics()
	{
		if (_location == null || _location.height <= 0f || _location.width <= 0f)
		{
			return;
		}

		int screenWidth = SystemProperties.GetScreenWidth();
		int screenHeight = SystemProperties.GetScreenHeight();
		if (screenWidth <= 0 || screenHeight <= 0)
		{
			return;
		}

		// The original mobile runtime could cache these values because its viewport
		// did not resize during a fight. Desktop windows and the Unity Game view can.
		// Keep the original camera math, but feed it the current viewport dimensions
		// so the Root Width clamp cannot expose space beyond the location edge.
		viewportScale = (float)screenHeight / _location.height;
		visibleWidth = (float)screenWidth / viewportScale;
		minZoom = visibleWidth / _location.width;
	}

	public void SpawnBloodEffects(Vector3f bloodPosition, Vector3f direction, int count)
	{
		if (Eclipse.Multiplayer.VersusTickDriver.IsResimulating)
		{
			return;
		}
		ClearBloodEffects();
		for (int i = 0; i < count; i++)
		{
			string spritePath = "textures/misc/drop_blood";
			BloodEffect bloodEffect = new BloodEffect(direction);
			bloodEffect.index = i;
			bloodEffect.LifetimeFrames = 90;
			bloodEffect.CreateSprite(spritePath, _location.modelsColor);
			bloodEffect.SetPosition(bloodPosition);
			bloodEffect.SetScale(0.4f);
			bloodEffect.SetParent(Container.GetViewerModel().GetRootObject());
			bloodEffects.Add(bloodEffect);
		}
		bloodEffectFrame = 0;
	}

	public void RenderBloodEffects()
	{
		if (bloodEffects.Count > 0)
		{
			for (int i = 0; i < bloodEffects.Count; i++)
			{
				bloodEffects[i].Render();
			}
			if (bloodEffectFrame > 90)
			{
				ClearBloodEffects();
			}
			bloodEffectFrame++;
		}
	}

	private void ClearBloodEffects()
	{
		for (int i = 0; i < bloodEffects.Count; i++)
		{
			bloodEffects[i].Destroy();
		}
		bloodEffects.Clear();
	}

	public int AddModel(ModelObject modelObject, Color color, bool flag = true)
	{
		return Container.GetViewerModel().AddModel(modelObject, color, flag);
	}

	public void RenderLayers()
	{
		List<LocationSelector> layers = _location.layers;
		foreach (LocationSelector item in layers)
		{
			item.Render();
		}
		UpdateAdditionalDrawsLayer();
	}

	// Eclipse: set by Camera.RenderInterpolatedPresentation while it redraws the
	// view between fixed ticks. Per-tick counters must not advance in that pass,
	// and fighter-derived values use interpolated fighter positions.
	internal bool PresentationPass;

	private Model _lightTarget;
	private Eclipse.Multiplayer.VersusGroundHighlight _versusPlayerTwoMarker;
	private float _lightRadius;
	private float _lightShape;

	public void UpdateArrowPointer(float positionX, float positionY)
	{
		if (arrowPulseFrame > BasicGUI.GetArrowFlashingFrames())
		{
			arrowPulseFrame = 0;
		}
		float num = 127.5f;
		Color color = pointerArrow.color;
		color.a = (num + num * Mathf.Sin((float)Math.PI / (float)BasicGUI.GetArrowFlashingFrames() * (float)arrowPulseFrame)) / 255f;
		pointerArrow.color = color;
		pointerArrow.transform.localPosition = new Vector3(positionX, positionY, -40f);
		if (!PresentationPass)
		{
			arrowPulseFrame++;
		}
	}

	public void PlayHitEffect(Vector3f hitPosition, Vector3f direction, float time, bool flag, string effectName, float scale)
	{
		if (Eclipse.Multiplayer.VersusTickDriver.IsResimulating)
		{
			return;
		}
		float num = Vector2f.GetAngle2DDegreeSigned(direction, new Vector2f(1f));
		hitAnimation = hitEffectObject.GetComponent<CocosAnimation>();
		hitAnimation.Init("textures/effects/fight/" + effectName, true);
		hitAnimation.set_Iterations(1);
		hitAnimation.set_ChangeSpriteTime(time);
		hitAnimation.set_Autoplay(true);
		hitEffectObject.transform.localPosition = new Vector3(hitPosition.GetX(), hitPosition.GetY(), 0f);
		hitEffectObject.transform.localScale = new Vector3(scale, scale, scale);
		// The fight render root is mirrored vertically (localScale.y = -1).
		// A reflection reverses rotation handedness, so compensate here or an
		// upward strike (for example an uppercut) points the effect downward.
		hitEffectObject.transform.eulerAngles = new Vector3(0f, 0f, (!float.IsNaN(num)) ? (0f - num) : 0f);
		hitEffectObject.SetActive(true);
	}

	public void SetVisible(bool value)
	{
		Container.GetRootObject().SetActive(value);
		pointerArrow.enabled = value;
	}

	public void UpdatePosition(Vector3f centerPosition, Vector3f otherPosition, float targetX, float targetY, float forcedZoom = 0f)
	{
        _eclipseCameraProjection.Begin();
        var camera = Fight.GetCurrentFight()?.GetEclipseCameraSettings(this);
		RefreshViewportMetrics();
		cameraOffsetX = _location.width / 2f - (camera?.CenterX.HasValue == true ? (float)camera.CenterX.Value : centerPosition.GetX());
		zoom = camera?.Zoom.HasValue == true ? (float)camera.Zoom.Value : ((!(forcedZoom > 0f)) ? CalculateAutoZoom() : forcedZoom);
		float num = 1f;
		if (GameUtils.GetCameraSettings().MaxWidth > 0f)
		{
			num = GameUtils.GetCameraSettings().MaxWidth / _location.width;
		}
		float num2 = minZoom / num;
		if (zoom < num2)
		{
			zoom = num2;
			float num3 = visibleWidth / zoom / 2f;
			float num4 = 0f - cameraOffsetX;
			float num5 = targetX - _location.width / 2f;
			float bindingLength = GameUtils.GetCameraSettings().BindingLength;
			float num6 = num5 - num4;
			if (camera?.CenterX.HasValue != true && Mathf.Abs(num6) + bindingLength > num3)
			{
				int num7 = ((num6 > 0f) ? 1 : (-1));
				float num8 = (float)(-num7) * (Mathf.Abs(num6) - num3 + bindingLength);
				cameraOffsetX += num8;
			}
		}
		float maxWidthDelta = GameUtils.GetCameraSettings().MaxWidthDelta;
		zoom = ((!lockMinZoom) ? Mathf.Max(zoom, minZoom) : minZoom);
		float num9 = (_location.width - maxWidthDelta) * zoom / 2f - visibleWidth / 2f;
		cameraOffsetX *= zoom;
		if (Mathf.Abs(cameraOffsetX) > num9)
		{
			cameraOffsetX = ((!(cameraOffsetX < 0f)) ? num9 : (0f - num9));
		}
		List<LocationSelector> layers = _location.layers;
		// Eclipse: optional background depth applies only to layers drawn behind
		// the game layer; the game layer and foreground stay as authored.
		bool behindGameLayer = true;
		foreach (LocationSelector item in layers)
		{
			if (item.GetIsGameLayer())
			{
				behindGameLayer = false;
			}
			if (item.GetIsGameLayer() || item.GetScalingEnabled())
			{
				item.SetScale(zoom);
			}
			else
			{
				float layerOffsetY = verticalOffset * (1f - zoom);
				item.SetPositionY(layerOffsetY);
			}
			float factor = item.GetFactor();
			if (behindGameLayer)
			{
				factor = Eclipse.Modding.ModVisuals.BackgroundLayerFactor(factor);
			}
			item.SetPositionX(cameraOffsetX * factor);
            if (camera != null) _eclipseCameraProjection.ApplyVertical(item, camera.OffsetY, zoom, factor);
		}
		float arrowX = cameraOffsetX - (_location.width / 2f - targetX) * zoom;
		float arrowY = _location.gameLayer.GetLayerObject().transform.localPosition.y - 2f * verticalOffset * zoom - 10f;
		var versusFight = Fight.GetCurrentFight();
		if (versusFight != null && versusFight.IsLocalVersus)
		{
			var player = versusFight.GetPlayerModel();
			var enemy = versusFight.GetEnemyModel();
			if (player != null) arrowX = cameraOffsetX - (_location.width / 2f - player.InterpolatedPivot().GetX()) * zoom;
			if (enemy != null)
			{
				if (_versusPlayerTwoMarker == null)
					_versusPlayerTwoMarker = new Eclipse.Multiplayer.VersusGroundHighlight(_UnityObject.transform);
				float x = cameraOffsetX - (_location.width / 2f - enemy.InterpolatedPivot().GetX()) * zoom;
				_versusPlayerTwoMarker.Update(x, arrowY, zoom);
			}
		}
		UpdateArrowPointer(arrowX, arrowY);
	}

	public void SetRootPosition(float x, float y)
	{
		_UnityObject.transform.localPosition = new Vector3(x, y);
	}

	public void CreateRingOutSprites(float leftMargin, float rightMargin, float frameTime, string atlasName)
	{
		if (leftRingOutSprite == null)
		{
			ChangingSprite ringOutSprite = new ChangingSprite(ChangingSprite.SpriteEffectType.AtlasBased);
			ringOutSprite.InitAtlasAnimation(atlasName, "Textures/fight/rules/ringout/", frameTime, 0f, _location.width / 2f + leftMargin, 7f + 2f * _location.floorHeight);
			ringOutSprite.SetPosition((0f - _location.width) / 4f + leftMargin / 2f, 7f + _location.floorHeight - _location.height / 2f);
			additionalDrawsLayer.AddChangingSprite(ringOutSprite, 1);
			leftRingOutSprite = ringOutSprite;
		}
		if (rightRingOutSprite == null)
		{
			ChangingSprite ringOutSprite = new ChangingSprite(ChangingSprite.SpriteEffectType.AtlasBased);
			ringOutSprite.InitAtlasAnimation(atlasName, "Textures/fight/rules/ringout/", frameTime, 0f, _location.width / 2f - rightMargin, 7f + 2f * _location.floorHeight);
			ringOutSprite.SetPosition(_location.width / 4f + rightMargin / 2f, 7f + _location.floorHeight - _location.height / 2f);
			additionalDrawsLayer.AddChangingSprite(ringOutSprite, 1);
			rightRingOutSprite = ringOutSprite;
		}
	}

	public void RemoveRingOutSprites()
	{
		if (leftRingOutSprite != null)
		{
			additionalDrawsLayer.RemoveChangingSprite(leftRingOutSprite);
			leftRingOutSprite = null;
		}
		if (rightRingOutSprite != null)
		{
			additionalDrawsLayer.RemoveChangingSprite(rightRingOutSprite);
			rightRingOutSprite = null;
		}
	}

	public void CreatePerkActivationArea(float areaWidth, string areaSpritePath, string iconPath)
	{
		perkActivationArea = new GameObject("PerkActivationArea").AddComponent<SpriteRenderer>();
		_PerkActivationAreaInterpolation = perkActivationArea.gameObject.AddComponent<FightTransformInterpolation>();
		perkActivationArea.sprite = ResourcesAndBundles.Load<Sprite>(areaSpritePath);
		perkActivationArea.transform.SetParent(additionalDrawsLayer.GetLayerObject().transform, false);
		perkActivationArea.gameObject.SetActive(false);
		RectTransform rectTransform = perkActivationArea.gameObject.AddComponent<RectTransform>();
		Vector2 sizeDelta = rectTransform.sizeDelta;
		rectTransform.localPosition = new Vector2(0f, sizeDelta.y - _location.floorHeight / 8f - _location.height / 2f);
		_PerkActivationAreaInterpolation.Snap(rectTransform.localPosition, rectTransform.localRotation);
		rectTransform.localScale = new Vector2(areaWidth / sizeDelta.x, _location.height * 2f / sizeDelta.y);
		if (!string.IsNullOrEmpty(iconPath))
		{
			perkActivationAreaIcon = new GameObject("PerkActivationAreaIcon").AddComponent<SpriteRenderer>();
			perkActivationAreaIcon.sprite = ResourcesAndBundles.Load<Sprite>(iconPath);
			perkActivationAreaIcon.transform.SetParent(perkActivationArea.transform, false);
			RectTransform rectTransform2 = perkActivationAreaIcon.gameObject.AddComponent<RectTransform>();
			Vector2 sizeDelta2 = rectTransform2.sizeDelta;
			rectTransform2.localPosition = new Vector2(0f, sizeDelta.x / 3f - _location.floorHeight / 8f - _location.height / 2f);
			float x = 0.75f * sizeDelta.x / areaWidth;
			float y = 0.75f * sizeDelta.y / (_location.height * 2f);
			rectTransform2.localScale = new Vector2(x, y);
		}
	}

	public void UpdatePerkActivationArea(float positionX, float alpha)
	{
		alpha /= 255f;
		if (perkActivationArea != null)
		{
			bool firstShow = !perkActivationArea.gameObject.activeSelf;
			perkActivationArea.gameObject.SetActive(true);
			Vector3 position = _PerkActivationAreaInterpolation.CurrentPosition;
			position.x = positionX;
			// Place a newly shown area directly; only later moves interpolate.
			if (firstShow) _PerkActivationAreaInterpolation.Snap(position, _PerkActivationAreaInterpolation.CurrentRotation);
			else _PerkActivationAreaInterpolation.Push(position, _PerkActivationAreaInterpolation.CurrentRotation);
			Color color = perkActivationArea.color;
			color.a = alpha;
			perkActivationArea.color = color;
			color = perkActivationAreaIcon.color;
			color.a = alpha;
			perkActivationAreaIcon.color = color;
		}
	}

	public void FadePerkActivationArea(float fadeAmount)
	{
		if (perkActivationArea != null)
		{
			float num = perkActivationArea.color.a - fadeAmount / 255f;
			if (num < 0f)
			{
				num = 0f;
			}
			Color color = perkActivationArea.color;
			color.a = num;
			perkActivationArea.color = color;
			color = perkActivationAreaIcon.color;
			color.a = num;
			perkActivationAreaIcon.color = color;
		}
	}

	public void DestroyPerkActivationArea()
	{
		if (perkActivationArea != null)
		{
			UnityEngine.Object.Destroy(perkActivationAreaIcon.gameObject);
			UnityEngine.Object.Destroy(perkActivationArea.gameObject);
			perkActivationAreaIcon = null;
			perkActivationArea = null;
			_PerkActivationAreaInterpolation = null;
		}
	}

	public void SetDarknessAlpha(float alpha)
	{
		if (darknessOverlay != null)
		{
			darknessOverlay.color = new Color(0f, 0f, 0f, alpha / 255f);
		}
	}

	public void CreateDarknessOverlay()
	{
		if (darknessOverlay == null)
		{
			Texture2D texture2D = new Texture2D(1, 1);
			texture2D.SetPixel(0, 0, Color.black);
			texture2D.Apply();
			darknessOverlay = new GameObject("DarknessPicture").AddComponent<SpriteRenderer>();
			darknessOverlay.sprite = Sprite.Create(texture2D, Rect.MinMaxRect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
			darknessOverlay.color = new Color(1f, 1f, 1f, 0f);
			darknessOverlay.transform.localScale = new Vector2(_location.width * 1.5f, _location.height * 3f);
			darknessOverlay.transform.SetParent(additionalDrawsLayer.GetLayerObject().transform, false);
		}
	}

	public void DestroyDarknessOverlay()
	{
		if (darknessOverlay != null)
		{
			UnityEngine.Object.Destroy(darknessOverlay.gameObject);
			darknessOverlay = null;
		}
	}

	public void CreateLightInTheDarkness()
	{
		if (_lightInTheDarkness != null || _location == null) return;
		// The mask may have been destroyed with its layer; release what it used.
		RemoveLightInTheDarkness();
		Shader shader = Resources.Load<Shader>("shaders/EclipseLightInTheDarkness");
		if (shader == null) throw new InvalidOperationException("Eclipse spotlight shader is unavailable.");
		_lightInTheDarknessMaterial = new Material(shader);
		Texture2D white = Texture2D.whiteTexture;
		_lightInTheDarknessSprite = Sprite.Create(white, new Rect(0f, 0f, white.width, white.height),
			new Vector2(0.5f, 0.5f), white.width);
		_lightInTheDarkness = new GameObject("LightInTheDarkness").AddComponent<SpriteRenderer>();
		_lightInTheDarkness.transform.SetParent(additionalDrawsLayer.GetLayerObject().transform, false);
		// The later native rule stretches its mask sprite to a 2048-unit square.
		_lightInTheDarkness.transform.localScale = new Vector3(2048f, 2048f, 1f);
		_lightInTheDarkness.sprite = _lightInTheDarknessSprite;
		_lightInTheDarkness.sharedMaterial = _lightInTheDarknessMaterial;
	}

	// Tracks the fighter so the presentation pass can recentre the mask on the
	// interpolated pose instead of the raw per-tick position.
	public void UpdateLightInTheDarkness(Model target, float radius, float shape)
	{
		_lightTarget = target; _lightRadius = radius; _lightShape = shape;
		if (target != null) UpdateLightInTheDarkness(target.GetPosition(), radius, shape);
	}

	internal void RefreshLightInTheDarkness()
	{
		if (_lightTarget != null && _lightInTheDarkness != null)
			UpdateLightInTheDarkness(_lightTarget.InterpolatedPivot(), _lightRadius, _lightShape);
	}

	public void UpdateLightInTheDarkness(Vector3f position, float radius, float shape)
	{
		if (_lightInTheDarkness == null) return;
		// Fighter coordinates belong to RenderContainer, whose origin is offset
		// and mirrored inside the game layer. The mask belongs to the foreground
		// layer, so project through both transforms before writing its UV center.
		Vector3 fighterWorld = Container.GetRootObject().transform.TransformPoint(
			new Vector3(position.GetX(), position.GetY(), 0f));
		Vector3 fighterOnMask = _lightInTheDarkness.transform.InverseTransformPoint(fighterWorld);
		_lightInTheDarknessMaterial.SetVector("_Center", new Vector4(
			0.5f + fighterOnMask.x,
			0.5f + fighterOnMask.y, 0f, 0f));
		_lightInTheDarknessMaterial.SetFloat("_Radius", radius);
		_lightInTheDarknessMaterial.SetFloat("_Shape", shape);
	}

	public void RemoveLightInTheDarkness()
	{
		_lightTarget = null;
		if (_lightInTheDarkness != null) UnityEngine.Object.Destroy(_lightInTheDarkness.gameObject);
		if (_lightInTheDarknessMaterial != null) UnityEngine.Object.Destroy(_lightInTheDarknessMaterial);
		if (_lightInTheDarknessSprite != null) UnityEngine.Object.Destroy(_lightInTheDarknessSprite);
		_lightInTheDarkness = null;
		_lightInTheDarknessMaterial = null;
		_lightInTheDarknessSprite = null;
	}

	public ViewerModel GetViewerModel()
	{
		return Container.GetViewerModel();
	}

	public EffectsContainer GetBackgroundEffects()
	{
		return Container.GetBackgroundEffects();
	}

	public EffectsContainer GetForegroundEffects()
	{
		return Container.GetForegroundEffects();
	}

	public void UpdateAdditionalDrawsLayer()
	{
		SyncAdditionalDrawsLayerTransform();
		additionalDrawsLayer.Render();
	}

	public void SyncAdditionalDrawsLayerTransform()
	{
		additionalDrawsLayer.GetLayerObject().transform.localScale = _location.gameLayer.GetLayerObject().transform.localScale;
		Vector3 localPosition = _location.gameLayer.GetLayerObject().transform.localPosition;
		localPosition.z = additionalDrawsLayer.layerDepth;
		additionalDrawsLayer.GetLayerObject().transform.localPosition = localPosition;
	}

	public void AttachModelEffects(Model model)
	{
		Container.AttachModelEffects(model);
	}

	public void DetachModelEffects(Model model)
	{
		Container.DetachModelEffects(model);
	}

	public void UpdateHitEffect()
	{
		if (null != hitAnimation && !hitAnimation.get_IsWork())
		{
			hitEffectObject.SetActive(false);
			hitAnimation = null;
		}
	}

	public void UpdateEffects()
	{
		if (Container != null)
		{
			Container.UpdateEffects();
		}
	}

	public float CalculateAutoZoom()
	{
		float distance = PresentationPass ? Container.GetViewerModel().InterpolatedFighterDistance() : Container.GetViewerModel().GetFighterDistance();
		return Mathf.Min(visibleWidth / (distance + 300f), 1f);
	}

	public float GetMaxZoom()
	{
		return 1f;
	}

	private void CreateArrow()
	{
		arrowPulseFrame = 0;
		pointerArrow = new GameObject("Arrow").AddComponent<SpriteRenderer>();
		pointerArrow.flipY = true;
		pointerArrow.sprite = ResourcesAndBundles.Load<Sprite>("Textures/fight/pointers/arrow");
		pointerArrow.transform.localScale = new Vector3(0.5f, -0.5f, 1f);
		pointerArrow.transform.SetParent(_UnityObject.transform, false);
	}

	private void CreateHitEffectObjects()
	{
		string text = "textures/effects/fight/hit_blade";
		hitEffectObject = new GameObject("ConteynerHit");
		hitEffectObject.AddComponent<CocosAnimation>();
		hitEffectObject.transform.localScale = new Vector3(0.7f, 1f, 1f);
		hitLayer = new GameObject("HitLayer");
		hitLayer.transform.localPosition = Container.GetRootObject().transform.localPosition;
		hitLayer.transform.localScale = Container.GetRootObject().transform.localScale;
		hitLayer.transform.SetParent(additionalDrawsLayer.GetLayerObject().transform, false);
		hitEffectObject.transform.SetParent(hitLayer.transform, false);
	}

	private void CreateRenderContainer()
	{
		Container = new RenderContainer();
		Container.Init(_location);
		foreach (LocationSelector item in _location.layers)
		{
			item.GetLayerObject().transform.SetParent(_UnityObject.transform, false);
		}
		additionalDrawsLayer = new LocationSelector(_location.layers[_location.layers.Count - 1].layerDepth + -3);
		additionalDrawsLayer.GetLayerObject().transform.SetParent(_UnityObject.transform, false);
		UpdateAdditionalDrawsLayer();
	}

	public void Clear()
	{
		RemoveLightInTheDarkness();
		Container.Clear();
	}
}
