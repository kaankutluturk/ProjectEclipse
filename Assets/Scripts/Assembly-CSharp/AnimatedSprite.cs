using System.Collections.Generic;
using UnityEngine;

public class AnimatedSprite : MonoBehaviour
{
	private List<Sprite> _Frames;

	private SpriteRenderer _SpriteRender;

	private float _elapsedTime;

	private float _changeSpriteTime;

	private int _currentFrame;

	private int _frameCount;

	private bool _IsWork;

	private int _iterations = -1;

	public List<Sprite> FrameList
	{
		get
		{
			return get_Frames();
		}
	}

	public float ChangeSpriteInterval
	{
		set
		{
			set_ChangeSpriteTime(value);
		}
	}

	public bool IsWorking
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

	public List<Sprite> get_Frames()
	{
		return _Frames;
	}

	public void set_ChangeSpriteTime(float value)
	{
		_changeSpriteTime = value;
	}

	public bool get_IsWork()
	{
		return _IsWork;
	}

	public void set_Iterations(int value)
	{
		_iterations = value;
	}

	public void SetFrames(Sprite[] frames)
	{
		_Frames = new List<Sprite>(frames);
		_frameCount = _Frames.Count;
		_IsWork = true;
	}

	public void SetFirstFrame()
	{
		SetSpriteFrame(0);
	}

	private void Awake()
	{
		_SpriteRender = base.gameObject.AddComponent<SpriteRenderer>();
	}

	public void Render(float deltaTime)
	{
		if (_IsWork && !(_SpriteRender == null))
		{
			_elapsedTime += deltaTime;
			if (_elapsedTime >= _changeSpriteTime)
			{
				AdvanceFrame();
				_elapsedTime = 0f;
			}
		}
	}

	private void AdvanceFrame()
	{
		SetSpriteFrame(_currentFrame);
		_currentFrame++;
		if (_currentFrame < _frameCount)
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

	private void SetSpriteFrame(int frameIndex)
	{
		if (frameIndex < _frameCount)
		{
			_SpriteRender.sprite = _Frames[frameIndex];
		}
	}
}
