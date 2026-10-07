using System.Collections.Generic;
using Nekki.SF2.Core.Fights.Controller;
using Nekki.SF2.GUI.Fight;
using Eclipse.Rendering.Interpolation;
using UnityEngine;

public class Camera : global::EventDispatcher<object>
{
	private class PendingBloodEffect
	{
		public Vector3f Position = new Vector3f();

		public Vector3f Impulse = new Vector3f();

		public int count;

		public bool isActive;
	}

	public enum CameraEvent
	{
		onPauseFight = 0,
		onResumeFight = 1,
		onStopEffect = 2
	}

	private const float MaxFollowSpeed = 200f;

	private const float MaxFollowDistance = 50f;

	private PendingBloodEffect _pendingBlood = new PendingBloodEffect();

	private Render _render;

	private GameUtils.HitEffect _hitEffect;

	private GameUtils.ZoomEffect _zoomEffect;

	private List<Model> _models = new List<Model>();

	private ModelNode _targetNode;

	private ModelNode _positionNode;

	private Location _location;

	private ModelNode _focusNode;

	private ModelNode _secondaryFocusNode;

	private PreFight _preFight;

	private bool _isShowPending;

	private bool _isHitPaused;

	private float _hitPauseFrames;

	private bool _isShaking;

	private float _shakeFramesLeft;

	private float _shakeDuration;

	private bool _isZooming;

	private int showZoomEffectCount;

	private bool _isEnabled;

	public GameController FightController;

	private GameObject _UnityObject;

	private readonly FightCameraInterpolation _RenderInterpolation = new FightCameraInterpolation();

	public Render ActiveRender
	{
		get
		{
			return GetRender();
		}
	}

	public PreFight PreFightView
	{
		get
		{
			return GetPreFight();
		}
	}

	public bool IsEnabled
	{
		get
		{
			return GetEnabled();
		}
		set
		{
			SetEnabled(value);
		}
	}

	public GameObject CameraObject
	{
		get
		{
			return GetCameraObject();
		}
	}

	public Vector3f FocusPosition
	{
		get
		{
			return GetFocusPosition();
		}
	}

	public Vector3f CameraTarget
	{
		get
		{
			return GetCameraTarget();
		}
		set
		{
			SetCameraTarget(value);
		}
	}

	public Camera(Transform parent)
	{
		CreateUnityObject(parent);
		_targetNode = new ModelNode("Camera");
		_positionNode = new ModelNode("Position");
		_render = null;
		_preFight = null;
		_location = null;
		_focusNode = null;
		_isHitPaused = false;
		_hitPauseFrames = 0f;
		_isShaking = false;
		_shakeFramesLeft = 0f;
		_shakeDuration = 0f;
		_hitEffect = null;
		showZoomEffectCount = 0;
		_isShowPending = false;
		_isEnabled = true;
	}

	// best guess for name
	public Render GetRender()
	{
		return _render;
	}

	public PreFight GetPreFight()
	{
		return _preFight;
	}

	public bool GetEnabled()
	{
		return _isEnabled;
	}

	public void SetEnabled(bool value)
	{
		_isEnabled = value;
	}

	public GameObject GetCameraObject()
	{
		return _UnityObject;
	}

	private float GetControllerScale()
	{
		float result = 1f;
		if (!GraphicsController.LargeControlsEnabled())
		{
			result = 25f / 33f;
		}
		return result;
	}

	private void UpdateCameraPosition()
	{
		_targetNode.SetEnd();
		ModelObject firstFighter = _render.GetViewerModel().GetFirstFighter();
		ModelObject oIEODIEHJMH2 = _render.GetViewerModel().GetSecondFighter();
		if (firstFighter != null && oIEODIEHJMH2 != null)
		{
			_targetNode.SetStart(Model.GetCameraMidpoint(firstFighter, oIEODIEHJMH2));
		}
	}

	private void CameraUpdate()
	{
		if (_isEnabled)
		{
			_positionNode.TimeStep(0f);
			Vector3f point = new Vector3f(_targetNode.GetEnd());
			Vector3f eMAFACPEPDK2 = new Vector3f(_targetNode.GetStart());
			Vector3f eMAFACPEPDK3 = new Vector3f(_positionNode.GetEnd());
			Vector3f eMAFACPEPDK4 = new Vector3f(_positionNode.GetStart());
			float num = 0f;
			eMAFACPEPDK4.SetZ(num);
			num = num;
			eMAFACPEPDK3.SetZ(num);
			num = num;
			eMAFACPEPDK2.SetZ(num);
			point.SetZ(num);
			Vector3f targetDelta = Vector3f.op_Subtraction(eMAFACPEPDK2, point);
			Vector3f offset = Vector3f.op_Addition(eMAFACPEPDK3, targetDelta);
			Vector3f nBMEGFBPGFE2 = Vector3f.op_Subtraction(offset, eMAFACPEPDK4);
			Vector3f eMAFACPEPDK5 = Vector3f.op_Subtraction(eMAFACPEPDK2, eMAFACPEPDK4);
			eMAFACPEPDK5.Multiply(0.15f);
			Vector3f eMAFACPEPDK6 = Vector3f.op_Addition(nBMEGFBPGFE2, eMAFACPEPDK5);
			if (eMAFACPEPDK6.GetLength2D() > 200f)
			{
				eMAFACPEPDK6.Normalize();
				eMAFACPEPDK6.Multiply(200f);
			}
			eMAFACPEPDK4.Add(eMAFACPEPDK6);
			Vector3f eMAFACPEPDK7 = Vector3f.op_Subtraction(eMAFACPEPDK4, eMAFACPEPDK3);
			float num2 = eMAFACPEPDK7.GetLength2D();
			if (num2 > 50f)
			{
				eMAFACPEPDK7.Multiply(50f / num2);
				eMAFACPEPDK4 = Vector3f.op_Addition(eMAFACPEPDK3, eMAFACPEPDK7);
			}
			_positionNode.SetStart(eMAFACPEPDK4);
		}
	}

	private void DrawPosition()
	{
		Vector3f targetPosition = _targetNode.GetStart();
		Vector3f focusPosition = _focusNode.GetStart();
		if (_isZooming)
		{
			_render.UpdatePosition(_positionNode.GetStart(), targetPosition, focusPosition.GetX(), focusPosition.GetY(), _zoomEffect.CurrentScale);
		}
		else
		{
			_render.UpdatePosition(_positionNode.GetStart(), targetPosition, focusPosition.GetX(), focusPosition.GetY());
		}
	}

	private void DrawInterpolatedPosition(float alpha)
	{
		if (_focusNode == null)
		{
			return;
		}
		_RenderInterpolation.SamplePositions(_positionNode, _targetNode, _focusNode, alpha);
		if (_isZooming)
		{
			float zoomScale = _RenderInterpolation.SampleZoomScale(alpha);
			_render.UpdatePosition(
				_RenderInterpolation.CameraPosition,
				_RenderInterpolation.CameraTarget,
				_RenderInterpolation.FocusPosition.GetX(),
				_RenderInterpolation.FocusPosition.GetY(),
				zoomScale);
		}
		else
		{
			_render.UpdatePosition(
				_RenderInterpolation.CameraPosition,
				_RenderInterpolation.CameraTarget,
				_RenderInterpolation.FocusPosition.GetX(),
				_RenderInterpolation.FocusPosition.GetY());
		}
	}

	private void DrawQuakeEffect()
	{
		if (_isShaking && _hitEffect != null)
		{
			float duration = _shakeDuration;
			float num = _shakeDuration - _shakeFramesLeft;
			float strength = _hitEffect.Type == "CriticalHit" ? Eclipse.UI.AccessibilitySettings.CriticalShake : 1f;
			float amplitudeX = _hitEffect.AmplitudeX * strength;
			float amplitudeY = _hitEffect.AmplitudeY * strength;
			float frequencyX = _hitEffect.FrequencyX;
			float frequencyY = _hitEffect.FrequencyY;
			float num2 = Mathf.Sin(frequencyX * num) * amplitudeX * (duration - num) / duration;
			float num3 = Mathf.Sin(frequencyY * num) * amplitudeY * (duration - num) / duration;
			num2 *= SystemProperties.ScaleY;
			num3 *= SystemProperties.ScaleY;
			_render.SetRootPosition(num2, num3);
		}
	}

	private void DrawInterpolatedQuakeEffect(float alpha)
	{
		if (_isShaking && _hitEffect != null)
		{
			float duration = _shakeDuration;
			float elapsed = Mathf.Max(0f, _shakeDuration - _shakeFramesLeft - (1f - alpha));
			float x = Mathf.Sin(_hitEffect.FrequencyX * elapsed) * _hitEffect.AmplitudeX * (duration - elapsed) / duration;
			float y = Mathf.Sin(_hitEffect.FrequencyY * elapsed) * _hitEffect.AmplitudeY * (duration - elapsed) / duration;
			float strength = _hitEffect.Type == "CriticalHit" ? Eclipse.UI.AccessibilitySettings.CriticalShake : 1f;
			_render.SetRootPosition(x * SystemProperties.ScaleY * strength, y * SystemProperties.ScaleY * strength);
		}
	}

	private void DrawZoomEffect()
	{
		if (!_isZooming)
		{
			return;
		}
		float num = Mathf.Abs(_zoomEffect.TargetScale - _zoomEffect.StartScale) / ((float)_zoomEffect.EffectTime / 2f);
		if (_zoomEffect.ElapsedFrames <= _zoomEffect.EffectTime / 2)
		{
			_zoomEffect.CurrentScale -= num;
			if (_zoomEffect.CurrentScale < _zoomEffect.TargetScale)
			{
				_zoomEffect.CurrentScale = _zoomEffect.TargetScale;
			}
		}
		else
		{
			_zoomEffect.CurrentScale += num;
			if (_zoomEffect.CurrentScale > _zoomEffect.StartScale)
			{
				_zoomEffect.CurrentScale = _zoomEffect.StartScale;
			}
		}
		_zoomEffect.ElapsedFrames++;
		_RenderInterpolation.PushZoomScale(_zoomEffect.CurrentScale);
	}

	private void RenderEffect()
	{
		if (_isHitPaused)
		{
			if (_hitPauseFrames <= 0f)
			{
				_isHitPaused = false;
				OnHitPauseEnded();
			}
			_hitPauseFrames--;
		}
		if (_isShaking)
		{
			if (_shakeFramesLeft <= 0f)
			{
				_isShaking = false;
				OnShakeEnded();
			}
			_shakeFramesLeft--;
		}
		if (_isZooming)
		{
			if (showZoomEffectCount <= 0)
			{
				_isZooming = false;
			}
			showZoomEffectCount--;
		}
	}

	private void OnHitPauseEnded()
	{
		CallEvent(1, null);
		if (_pendingBlood.isActive)
		{
			_render.SpawnBloodEffects(_pendingBlood.Position, _pendingBlood.Impulse, _pendingBlood.count);
			_pendingBlood.isActive = false;
		}
	}

	private void OnShakeEnded()
	{
		_shakeDuration = 0f;
		CallEvent(2, null);
	}

	private void CreateUnityObject(Transform parent)
	{
		_UnityObject = new GameObject("Camera");
		_UnityObject.transform.SetParent(parent, false);
		_UnityObject.transform.localPosition = new Vector3(0f, 0f);
		_UnityObject.AddComponent<CameraRenderInterpolationDriver>().Init(this);
	}

	public void Clear()
	{
		_render.Clear();
		_render = null;
	}

	public virtual void Init(Location location)
	{
		_render = new Render(_UnityObject);
		_positionNode.SetAttenuation(0f);
		_targetNode.SetAttenuation(0f);
		_location = location;
		_render.Init(_location);
		Vector3f modelsCenter = _location.GetModelsCenter();
		_positionNode.SetStart(modelsCenter);
		_positionNode.SetEnd(modelsCenter);
		_targetNode.SetStart(modelsCenter);
		_targetNode.SetEnd(modelsCenter);
		_shakeFramesLeft = 0f;
		_isShaking = false;
		_hitPauseFrames = 0f;
		_isHitPaused = false;
		showZoomEffectCount = 0;
		_isZooming = false;
		_RenderInterpolation.ResetZoomScale(0f);
	}

	// The title already owns the location renderer and its fixed Unity camera.
	internal void InitTitleBackdrop(Location location, Render render)
	{
		_location = location;
		_render = render;
		// The title frames the borrowed renderer. The gameplay interpolation
		// driver otherwise applies an uninitialized fight camera in LateUpdate,
		// shifting/scaling the location layers after the title has drawn them.
		_UnityObject.GetComponent<CameraRenderInterpolationDriver>().enabled = false;
	}

	public void Render()
	{
		if (_isShowPending)
		{
			bool flag = true;
			foreach (Model item in _models)
			{
				if (item.GetCurrentFrame() == -1)
				{
					flag = false;
				}
			}
			if (flag)
			{
				ShowRender();
				_isShowPending = false;
			}
		}
		if (_isEnabled)
		{
			RenderEffect();
			UpdateCameraPosition();
			CameraUpdate();
			DrawPosition();
			DrawQuakeEffect();
			DrawZoomEffect();
		}
	}

	public void RenderInterpolatedPresentation()
	{
		if (!_isEnabled || _render == null)
		{
			return;
		}
		float alpha = FightInterpolation.CameraAlpha;
		_render.PresentationPass = true;
		try
		{
			DrawInterpolatedPosition(alpha);
			DrawInterpolatedQuakeEffect(alpha);
			_render.RefreshLightInTheDarkness();
		}
		finally { _render.PresentationPass = false; }
		_render.SyncAdditionalDrawsLayerTransform();
	}

	public void PlayEffectAnimation(Vector3f NAAPALOFBCI, Vector3f direction, float time, bool flag, string effectName, float scale)
	{
		_render.PlayHitEffect(NAAPALOFBCI, direction, time, flag, effectName, scale);
	}

	public void QueueBloodEffect(Vector3f NAAPALOFBCI, Vector3f impulse, int count = 4)
	{
		_pendingBlood.Position.Set(NAAPALOFBCI);
		_pendingBlood.Impulse.Set(impulse);
		_pendingBlood.count = count;
		_pendingBlood.isActive = true;
	}

	public void SetController(GameController value)
	{
		FightController = value;
		FightController.SetScale(GetControllerScale());
		FightController.InitController();
	}

	public void SetFightVisible(bool value)
	{
		if (_preFight != null)
		{
			_preFight.VisibleViewer(value);
		}
		FightController.gameObject.SetActive(value);
		if (value)
		{
			_isShowPending = value;
		}
		else
		{
			_render.SetVisible(value);
		}
	}

	public void RemoveObjectByIndex(int index)
	{
		_render.GetViewerModel().RemoveModel(index);
		_render.DetachModelEffects(_models[index]);
		_models.RemoveAt(index);
	}

	public void RemoveObject(Model model)
	{
		int num = _models.IndexOf(model);
		if (num != -1)
		{
			RemoveObjectByIndex(num);
		}
	}

	public int AddModel(Model model, bool isPlayer, bool isFighter)
	{
		int result = -1;
		_models.Add(model);
		_render.AttachModelEffects(model);
		if (isPlayer)
		{
			_focusNode = model.GetBodyObject().GetNodeByName(GameUtils.GetCameraSettings().BindingNode);
			_secondaryFocusNode = model.GetBodyObject().GetNodeByName(GameUtils.GetCameraSettings().CameraNode);
		}
		if (_render != null)
		{
			result = _render.AddModel(model.GetBodyObject(), _location.modelsColor, isFighter);
		}
		return result;
	}

    internal bool ReplaceModel(Model expected, Model replacement, bool player)
    {
        if (expected == null || replacement == null || _render == null || _models.Contains(replacement)) return false;
        int index = _models.IndexOf(expected);
        if (index < 0 || replacement.GetBodyObject() == null) return false;
        ModelNode focus = null, secondaryFocus = null;
        if (player)
        {
            focus = replacement.GetBodyObject().GetNodeByName(GameUtils.GetCameraSettings().BindingNode);
            secondaryFocus = replacement.GetBodyObject().GetNodeByName(GameUtils.GetCameraSettings().CameraNode);
            if (focus == null || secondaryFocus == null) return false;
        }
        _render.AttachModelEffects(replacement);
        bool replaced = false;
        try
        {
            replaced = _render.GetViewerModel().ReplaceModel(index, expected.GetBodyObject(),
                replacement.GetBodyObject(), _location.modelsColor);
            if (!replaced) return false;
        }
        finally
        {
            if (!replaced) _render.DetachModelEffects(replacement);
        }
        _render.DetachModelEffects(expected);
        _models[index] = replacement;
        replacement.Index = expected.Index;
        if (player) { _focusNode = focus; _secondaryFocusNode = secondaryFocus; }
        return true;
    }

	public void ApplyHitEffect(GameUtils.HitEffect hitEffect)
	{
		if (Fight.GetCurrentFight()?.IsTitleSparring == true) return;
		if (hitEffect != null)
		{
			_hitEffect = hitEffect;
			// Mod-configured impact effect; critical hits respect the shake slider.
			if (!Eclipse.Multiplayer.VersusTickDriver.IsResimulating)
				Eclipse.Modding.ModVisuals.TriggerImpact(_hitEffect.Type,
					_hitEffect.Type == "CriticalHit" ? Eclipse.UI.AccessibilitySettings.CriticalShake : 1f);
			bool wasPaused = _isHitPaused;
			_hitPauseFrames = _hitEffect.PauseTime * (_hitEffect.Type == "CriticalHit" ? (Eclipse.Multiplayer.VersusDeterminism.Active ? Eclipse.Multiplayer.VersusDeterminism.CriticalPause : Eclipse.UI.AccessibilitySettings.CriticalPause) : 1f);
			_isShaking = true;
			_shakeFramesLeft = _hitEffect.EffectTime;
			_shakeDuration = _hitEffect.EffectTime;
            _isHitPaused = _hitPauseFrames > 0f;
            if (_isHitPaused) CallEvent(0, null);
            else if (wasPaused) OnHitPauseEnded();
		}
	}

	public void ApplyZoomEffect(GameUtils.ZoomEffect zoomEffect)
	{
		if (Fight.GetCurrentFight()?.IsTitleSparring == true) return;
		if (zoomEffect != null)
		{
			_isZooming = true;
			_zoomEffect = zoomEffect;
			float num = _render.GetMaxZoom();
			if (_zoomEffect.TargetScale < num)
			{
				_zoomEffect.TargetScale = num;
			}
			_zoomEffect.StartScale = _render.CalculateAutoZoom();
			_zoomEffect.CurrentScale = _zoomEffect.StartScale;
			_RenderInterpolation.ResetZoomScale(_zoomEffect.CurrentScale);
			_zoomEffect.ElapsedFrames = 0;
			showZoomEffectCount = _zoomEffect.EffectTime;
		}
	}

	public Vector3f GetFocusPosition()
	{
		return _focusNode.GetStart();
	}

	public void RenderBloodEffects()
	{
		_render.RenderBloodEffects();
	}

	public void RenderLocationLayers()
	{
		_render.RenderLayers();
	}

	public void AddPreFight(PreFight value)
	{
		_preFight = value;
	}

	public void ShowRender()
	{
		_render.SetVisible(true);
	}

	public void RefreshControllerScale()
	{
		FightController.SetScale(GetControllerScale());
	}

	public Vector3f GetCameraTarget()
	{
		return _targetNode.GetStart();
	}

	public void SetCameraTarget(Vector3f value)
	{
		_targetNode.SetStart(value);
	}

	public void ToggleMinScale()
	{
		_render.SetLockMinZoom(!_render.GetLockMinZoom());
	}
}
