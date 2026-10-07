using System;
using UnityEngine;

public class ChangingSprite
{
	public enum SpriteEffectType
	{
		PictureBased = 0,
		AtlasBased = 1,
		ParticleBased = 2,
		None = 3
	}

	private bool _isFirstRender;

	private float _animationOffset;

	private float _elapsedFrames;

	private SpriteEffectType _type;

	private CocosAnimation _animation;

	private Interpolator _oscillationX = new Interpolator();

	private Interpolator _oscillationY = new Interpolator();

	private Interpolator _transparency = new Interpolator();

	private Interpolator _rotation = new Interpolator();

	private ChanceAndFlag _reappearX = new ChanceAndFlag();

	private ChanceAndFlag _reappearY = new ChanceAndFlag();

	private float _speedX;

	private float _speedY;

	private float _positionX;

	private float _positionY;

	private float _animationTime;

	private float _animationDuration;

	private float _pauseElapsed;

	private float _pauseDuration;

	private float _spriteOffsetX;

	private float _spriteOffsetY;

	public GameObject SpriteObject;

	public ParticleSystem Particles;

	public float PositionX
	{
		set
		{
			SetPositionX(value);
		}
	}

	public float PositionY
	{
		set
		{
			SetPositionY(value);
		}
	}

	public ChangingSprite(SpriteEffectType effectType)
	{
		_elapsedFrames = 0f;
		_speedX = 0f;
		_speedY = 0f;
		_positionX = 0f;
		_positionY = 0f;
		_isFirstRender = true;
		_animationTime = 0f;
		_animationDuration = 0f;
		_pauseElapsed = 0f;
		_pauseDuration = 0f;
		_spriteOffsetX = 0f;
		_spriteOffsetY = 0f;
		_type = effectType;
	}

	public void SetPositionX(float value)
	{
		_positionX = value;
	}

	public void SetPositionY(float value)
	{
		_positionY = value;
	}

	public virtual bool InitAtlasAnimation(string animationName, string path, float time, float animationOffset, float width, float height)
	{
		if (_type != SpriteEffectType.AtlasBased)
		{
			return false;
		}
		_animationOffset = animationOffset;
		SpriteObject = new GameObject(animationName);
		_animation = SpriteObject.AddComponent<CocosAnimation>();
		if (!_animation.Init(path + animationName, true))
		{
			GameLog.Write("Anim NO " + path + animationName);
			return false;
		}
		_animation.SetFirstFrame();
		_animation.set_ChangeSpriteTime(time / 60f);
		_animationDuration = time * (float)_animation.get_TotalFrames() + 1f;
		float x = _animation.get_AnimationData().GetFrames()[0].GetSourceSize().x;
		float y = _animation.get_AnimationData().GetFrames()[0].GetSourceSize().y;
		Vector3 localScale = new Vector3(width / x, height / y, 1f);
		localScale.x = (float)Math.Round(localScale.x, 4, MidpointRounding.AwayFromZero);
		localScale.y = (float)Math.Round(localScale.y, 4, MidpointRounding.AwayFromZero);
		SpriteObject.transform.localScale = localScale;
		return true;
	}

	public virtual void SetPause(float pauseDuration)
	{
		_pauseDuration = pauseDuration;
	}

	public virtual void InitPicture(string assetDirectory, string spriteName, string atlasName, CocosAnimationData.SpriteFrameCocos frameData, float width, float height)
	{
		if (_type != SpriteEffectType.PictureBased)
		{
			return;
		}
		Sprite sprite = LocationSpriteCache.GetSprite(assetDirectory, spriteName, atlasName);
		if (sprite == null)
		{
			GameLog.Write("Pic: {0}", spriteName);
			return;
		}
		SpriteObject = new GameObject(spriteName);
		SpriteRenderer spriteRenderer = SpriteObject.AddComponent<SpriteRenderer>();
		spriteRenderer.sprite = sprite;
		float num = sprite.rect.size.x;
		float num2 = sprite.rect.size.y;
		_spriteOffsetX = 0f;
		_spriteOffsetY = 0f;
		bool flag = false;
		if (frameData != null)
		{
			flag = frameData.GetRotated();
			_spriteOffsetX = frameData.GetOffset().x;
			_spriteOffsetY = frameData.GetOffset().y;
			if (flag)
			{
				float num3 = num;
				num = num2;
				num2 = num3;
			}
			num = ((!(frameData.GetSourceSize().x < num)) ? frameData.GetSourceSize().x : num);
			num2 = ((!(frameData.GetSourceSize().y < num2)) ? frameData.GetSourceSize().y : num2);
		}
		Vector3 localPosition = SpriteObject.transform.localPosition;
		SpriteObject.transform.localPosition = new Vector3(_spriteOffsetX, _spriteOffsetY, localPosition.z);
		Vector3 vector = default(Vector3);
		if (flag)
		{
			SpriteObject.transform.Rotate(0f, 0f, 90f);
			vector = new Vector3(height / num2, width / num, 1f);
		}
		else
		{
			vector = new Vector3(width / num, height / num2, 1f);
		}
		// Location projection places the qualified asset directory in the first argument.
		if (Eclipse.Modding.AssetId.TryParse(assetDirectory, out _))
		{
			vector.x *= sprite.pixelsPerUnit;
			vector.y *= sprite.pixelsPerUnit;
		}
		SpriteObject.transform.localScale = vector;
	}

	public virtual bool InitParticles(string resourcePath, float x, float y)
	{
		GameObject gameObject = Resources.Load<GameObject>(resourcePath);
		if (gameObject != null && gameObject != null)
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject);
			gameObject2.transform.localPosition = new Vector3(x, y, -0.1f);
			Particles = gameObject2.GetComponent<ParticleSystem>();
			Particles.Pause();
			return true;
		}
		GameLog.Write("Particles NO " + resourcePath);
		GameObject gameObject3 = new GameObject("NO " + resourcePath);
		Particles = gameObject3.AddComponent<ParticleSystem>();
		gameObject3.transform.localPosition = new Vector3(x, y, -0.1f);
		return true;
	}

	public virtual bool InitParticlesExtended(string resourcePath, float x, float y, int firstOption = 0, int secondOption = 0, int thirdOption = 24)
	{
		return true;
	}

	public virtual void SetPosition(float x, float y)
	{
		_positionX = x;
		_positionY = y;
	}

	public void AddOscillationXKeyframe(float period, float value, float ease)
	{
		_oscillationX.AddInterval(period, value, ease);
	}

	public void SetOscillationXOffset(float offset)
	{
		_oscillationX.AdvanceTime(offset);
	}

	public void AddOscillationYKeyframe(float period, float value, float ease)
	{
		_oscillationY.AddInterval(period, value, ease);
	}

	public void SetOscillationYOffset(float offset)
	{
		_oscillationY.AdvanceTime(offset);
	}

	public virtual void AddRotationKeyframe(float period, float value, float ease)
	{
		_rotation.AddInterval(period, value, ease);
	}

	public virtual void SetRotationOffset(float offset)
	{
		_rotation.AdvanceTime(offset);
	}

	public virtual void SetSpeed(float speedX, float speedY)
	{
		_speedX = speedX;
		_speedY = speedY;
	}

	public virtual void AddTransparencyKeyframe(float period, float value, float ease)
	{
		value = Mathf.Max(0f, value);
		value = Mathf.Min(100f, value);
		_transparency.AddInterval(period, value, ease);
	}

	public virtual void SetTransparencyOffset(float offset)
	{
		_transparency.AdvanceTime(offset);
	}

	public virtual void SetReappearX(float min, float max)
	{
		_reappearX.state = true;
		_reappearX.Min = min;
		_reappearX.Max = max;
	}

	public virtual void SetReappearY(float min, float max)
	{
		_reappearY.state = true;
		_reappearY.Min = min;
		_reappearY.Max = max;
	}

	public virtual void Render(float timeScale = 1f)
	{
		float num = 1f / 60f * timeScale;
		if (_isFirstRender)
		{
			if (_type == SpriteEffectType.ParticleBased && Particles != null)
			{
				for (int i = 0; i < 150; i++)
				{
					Particles.Simulate(Mathf.Max(0f, num * 2f));
				}
			}
			_isFirstRender = false;
		}
		if (_type == SpriteEffectType.ParticleBased && Particles != null)
		{
			Particles.Simulate(Mathf.Max(0f, num), true, false);
			return;
		}
		_elapsedFrames += timeScale;
		if (_type == SpriteEffectType.AtlasBased)
		{
			float num2 = UpdateAnimation(timeScale);
			if (num2 > 0f)
			{
				_animation.Render(1f / 60f * num2);
			}
		}
		_positionX += _speedX;
		float currentX = _positionX;
		_oscillationX.AdvanceTime(num);
		currentX += _oscillationX.GetCurrentValue();
		_positionY += _speedY;
		float currentY = _positionY;
		_oscillationY.AdvanceTime(num);
		currentY += _oscillationY.GetCurrentValue();
		Vector3 localPosition = SpriteObject.transform.localPosition;
		SpriteObject.transform.localPosition = new Vector3(currentX + _spriteOffsetX, currentY + _spriteOffsetY, localPosition.z);
		if (_rotation.HasIntervals())
		{
			_rotation.AdvanceTime(num);
			float z = _rotation.GetCurrentValue();
			SpriteObject.transform.eulerAngles = new Vector3(0f, 0f, z);
		}
		if (_transparency.HasIntervals())
		{
			_transparency.AdvanceTime(num);
			SpriteRenderer component = SpriteObject.GetComponent<SpriteRenderer>();
			Color color = component.color;
			color.a = 2.55f * _transparency.GetCurrentValue() / 255f;
			component.color = color;
		}
		if (_reappearX.state)
		{
			if (currentX > _reappearX.Max)
			{
				_positionX = _reappearX.Min + (currentX - _reappearX.Max);
			}
			if (currentX < _reappearX.Min)
			{
				_positionX = _reappearX.Max - (_reappearX.Min - currentX);
			}
		}
		if (_reappearY.state)
		{
			if (currentY > _reappearY.Max)
			{
				_positionY = _reappearY.Min + (currentY - _reappearY.Max);
			}
			if (currentY < _reappearY.Min)
			{
				_positionY = _reappearY.Max - (_reappearY.Min - currentY);
			}
		}
	}

	public void Release()
	{
	}

	private float UpdateAnimation(float frames)
	{
		if (frames > _animationDuration + _pauseDuration)
		{
			frames -= (float)(int)(frames / (_animationDuration + _pauseDuration)) * (_animationDuration + _pauseDuration);
		}
		if (_pauseElapsed == 0f)
		{
			_animationTime += frames;
			if (_animationTime < _animationDuration)
			{
				return frames;
			}
			_pauseElapsed += _animationTime - _animationDuration;
			if (_pauseElapsed >= _pauseDuration)
			{
				float remainingFrames = _pauseElapsed - _pauseDuration;
				_animationTime = 0f;
				_pauseElapsed = 0f;
				return UpdateAnimation(remainingFrames);
			}
			return _animationDuration - (_animationTime - frames + 1E-05f);
		}
		_pauseElapsed += frames;
		if (_pauseElapsed < _pauseDuration)
		{
			return 0f;
		}
		float pauseFrames = _pauseElapsed;
		_animationTime = 0f;
		_pauseElapsed = 0f;
		return UpdateAnimation(pauseFrames - _pauseDuration);
	}
}
