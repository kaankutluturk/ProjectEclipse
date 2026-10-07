using UnityEngine;

public class CocosAnimation : MonoBehaviour
{
	private bool _autoplay;

	private float _frameTimer;

	private float _changeSpriteTime = 0.03f;

	private CocosAnimationData _Animation;

	private GameObject _childObject;

	private SpriteRenderer _SpriteRender;

	private int _currentFrame;

	private int _totalFrames;

	private bool _IsWork;

	private int _iterations = -1;

	public bool IsAutoplay
	{
		get
		{
			return get_Autoplay();
		}
		set
		{
			set_Autoplay(value);
		}
	}

	public float FrameInterval
	{
		set
		{
			set_ChangeSpriteTime(value);
		}
	}

	public CocosAnimationData AnimationInfo
	{
		get
		{
			return get_AnimationData();
		}
	}

	public int FrameCount
	{
		get
		{
			return get_TotalFrames();
		}
	}

	public bool IsAnimationRunning
	{
		get
		{
			return get_IsWork();
		}
	}

	public int IterationCount
	{
		set
		{
			set_Iterations(value);
		}
	}

	public bool get_Autoplay()
	{
		return _autoplay;
	}

	public void set_Autoplay(bool value)
	{
		_autoplay = value;
	}

	public void set_ChangeSpriteTime(float value)
	{
		_changeSpriteTime = value;
	}

	public CocosAnimationData get_AnimationData()
	{
		return _Animation;
	}

	public int get_TotalFrames()
	{
		return _totalFrames;
	}

	public bool get_IsWork()
	{
		return _IsWork;
	}

	public void set_Iterations(int value)
	{
		_iterations = value;
	}

	public void SetSortingOrder(int value)
	{
		if (_SpriteRender != null)
			_SpriteRender.sortingOrder = value;
	}

	public bool Init(string ONEIGMLOGDC, bool MPMHHEMGHOJ)
	{
		_Animation = CocosAnimationData.Create(ONEIGMLOGDC + "_xml", MPMHHEMGHOJ);
		if (_Animation == null)
		{
			return false;
		}
		if (_childObject == null)
		{
			_childObject = new GameObject("Child");
			_childObject.transform.SetParent(base.transform, false);
			_SpriteRender = _childObject.AddComponent<SpriteRenderer>();
		}
		_Animation.LoadSprites();
		_Animation.SortFrames();
		_totalFrames = _Animation.GetFrames().Count;
		_IsWork = true;
		return true;
	}

	public void SetFirstFrame()
	{
		SetSpriteFrame(0);
	}

	private void Update()
	{
		if (_autoplay)
		{
			Render(Time.deltaTime);
		}
	}

	public void Render(float PPOFNJGPHGP)
	{
		if (_IsWork && !(_SpriteRender == null))
		{
			_frameTimer += PPOFNJGPHGP;
			while (_frameTimer >= _changeSpriteTime)
			{
				AdvanceFrame();
				_frameTimer -= _changeSpriteTime;
			}
		}
	}

	private void AdvanceFrame()
	{
		SetSpriteFrame(_currentFrame);
		_currentFrame++;
		if (_currentFrame < _totalFrames)
		{
			return;
		}
		_currentFrame = 0;
		if (_iterations != -1)
		{
			_iterations--;
			if (_iterations <= 0)
			{
				_IsWork = false;
			}
		}
	}

	private void SetSpriteFrame(int DCHCFFFFLLK)
	{
		if (DCHCFFFFLLK < _totalFrames)
		{
			CocosAnimationData.SpriteFrameCocos pBAHNJDFMBO = _Animation.GetFrames()[DCHCFFFFLLK];
			_SpriteRender.sprite = pBAHNJDFMBO.GetSprite();
			_childObject.transform.localEulerAngles = new Vector3(0f, 0f, pBAHNJDFMBO.GetRotated() ? 90 : 0);
			// Preserve the Cocos/TexturePacker frame offset exactly. The recovered
			// XML matches the original plist metadata, and the original Mono build
			// applies this offset without inverting it. Negating it makes trim
			// compensation run in the wrong direction and visibly shakes sequences.
			Vector2 frameOffset = pBAHNJDFMBO.GetOffset();
			_childObject.transform.localPosition = new Vector3(frameOffset.x, frameOffset.y, 0f);
		}
	}
}
