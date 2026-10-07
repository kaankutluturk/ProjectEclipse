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

	public ChangingSprite(SpriteEffectType LFLGCDNKNJI)
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
		_type = LFLGCDNKNJI;
	}

	public void SetPositionX(float value)
	{
		_positionX = value;
	}

	public void SetPositionY(float value)
	{
		_positionY = value;
	}

	public virtual bool InitAtlasAnimation(string PMFEIPCHENB, string path, float time, float ILENLCMAMBH, float JMLAKAKDBBL, float FEIHFIPFNKF)
	{
		if (_type != SpriteEffectType.AtlasBased)
		{
			return false;
		}
		_animationOffset = ILENLCMAMBH;
		SpriteObject = new GameObject(PMFEIPCHENB);
		_animation = SpriteObject.AddComponent<CocosAnimation>();
		if (!_animation.Init(path + PMFEIPCHENB, true))
		{
			GameLog.Write("Anim NO " + path + PMFEIPCHENB);
			return false;
		}
		_animation.SetFirstFrame();
		_animation.set_ChangeSpriteTime(time / 60f);
		_animationDuration = time * (float)_animation.get_TotalFrames() + 1f;
		float x = _animation.get_AnimationData().GetFrames()[0].GetSourceSize().x;
		float y = _animation.get_AnimationData().GetFrames()[0].GetSourceSize().y;
		Vector3 localScale = new Vector3(JMLAKAKDBBL / x, FEIHFIPFNKF / y, 1f);
		localScale.x = (float)Math.Round(localScale.x, 4, MidpointRounding.AwayFromZero);
		localScale.y = (float)Math.Round(localScale.y, 4, MidpointRounding.AwayFromZero);
		SpriteObject.transform.localScale = localScale;
		return true;
	}

	public virtual void SetPause(float GCMMAPEFEBG)
	{
		_pauseDuration = GCMMAPEFEBG;
	}

	public virtual void InitPicture(string GPNPNHFACPO, string ODMCNMJPHFJ, string GAKBMMOOGDB, CocosAnimationData.SpriteFrameCocos PIDBGGLFBCO, float JMLAKAKDBBL, float FEIHFIPFNKF)
	{
		if (_type != SpriteEffectType.PictureBased)
		{
			return;
		}
		Sprite sprite = LocationSpriteCache.GetSprite(GPNPNHFACPO, ODMCNMJPHFJ, GAKBMMOOGDB);
		if (sprite == null)
		{
			GameLog.Write("Pic: {0}", ODMCNMJPHFJ);
			return;
		}
		SpriteObject = new GameObject(ODMCNMJPHFJ);
		SpriteRenderer spriteRenderer = SpriteObject.AddComponent<SpriteRenderer>();
		spriteRenderer.sprite = sprite;
		float num = sprite.rect.size.x;
		float num2 = sprite.rect.size.y;
		_spriteOffsetX = 0f;
		_spriteOffsetY = 0f;
		bool flag = false;
		if (PIDBGGLFBCO != null)
		{
			flag = PIDBGGLFBCO.GetRotated();
			_spriteOffsetX = PIDBGGLFBCO.GetOffset().x;
			_spriteOffsetY = PIDBGGLFBCO.GetOffset().y;
			if (flag)
			{
				float num3 = num;
				num = num2;
				num2 = num3;
			}
			num = ((!(PIDBGGLFBCO.GetSourceSize().x < num)) ? PIDBGGLFBCO.GetSourceSize().x : num);
			num2 = ((!(PIDBGGLFBCO.GetSourceSize().y < num2)) ? PIDBGGLFBCO.GetSourceSize().y : num2);
		}
		Vector3 localPosition = SpriteObject.transform.localPosition;
		SpriteObject.transform.localPosition = new Vector3(_spriteOffsetX, _spriteOffsetY, localPosition.z);
		Vector3 vector = default(Vector3);
		if (flag)
		{
			SpriteObject.transform.Rotate(0f, 0f, 90f);
			vector = new Vector3(FEIHFIPFNKF / num2, JMLAKAKDBBL / num, 1f);
		}
		else
		{
			vector = new Vector3(JMLAKAKDBBL / num, FEIHFIPFNKF / num2, 1f);
		}
		// Location projection places the qualified asset directory in the first argument.
		if (Eclipse.Modding.AssetId.TryParse(GPNPNHFACPO, out _))
		{
			vector.x *= sprite.pixelsPerUnit;
			vector.y *= sprite.pixelsPerUnit;
		}
		SpriteObject.transform.localScale = vector;
	}

	public virtual bool InitParticles(string JIPAAPBPNJM, float FNDOOJNDJDC, float GBCONNBABLL)
	{
		GameObject gameObject = Resources.Load<GameObject>(JIPAAPBPNJM);
		if (gameObject != null && gameObject != null)
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject);
			gameObject2.transform.localPosition = new Vector3(FNDOOJNDJDC, GBCONNBABLL, -0.1f);
			Particles = gameObject2.GetComponent<ParticleSystem>();
			Particles.Pause();
			return true;
		}
		GameLog.Write("Particles NO " + JIPAAPBPNJM);
		GameObject gameObject3 = new GameObject("NO " + JIPAAPBPNJM);
		Particles = gameObject3.AddComponent<ParticleSystem>();
		gameObject3.transform.localPosition = new Vector3(FNDOOJNDJDC, GBCONNBABLL, -0.1f);
		return true;
	}

	public virtual bool InitParticlesExtended(string JIPAAPBPNJM, float FNDOOJNDJDC, float GBCONNBABLL, int HJAHHPHOMDO = 0, int JAJICKINNCP = 0, int FKFCKIDMFCP = 24)
	{
		return true;
	}

	public virtual void SetPosition(float DHDMNHCIPEH, float BGEEALIPKCC)
	{
		_positionX = DHDMNHCIPEH;
		_positionY = BGEEALIPKCC;
	}

	public void AddOscillationXKeyframe(float GKIHFPFHKCI, float value, float JENJFNNFGLD)
	{
		_oscillationX.AddInterval(GKIHFPFHKCI, value, JENJFNNFGLD);
	}

	public void SetOscillationXOffset(float IPCOBJBKNAO)
	{
		_oscillationX.AdvanceTime(IPCOBJBKNAO);
	}

	public void AddOscillationYKeyframe(float GKIHFPFHKCI, float value, float JENJFNNFGLD)
	{
		_oscillationY.AddInterval(GKIHFPFHKCI, value, JENJFNNFGLD);
	}

	public void SetOscillationYOffset(float IPCOBJBKNAO)
	{
		_oscillationY.AdvanceTime(IPCOBJBKNAO);
	}

	public virtual void AddRotationKeyframe(float GKIHFPFHKCI, float value, float JENJFNNFGLD)
	{
		_rotation.AddInterval(GKIHFPFHKCI, value, JENJFNNFGLD);
	}

	public virtual void SetRotationOffset(float IPCOBJBKNAO)
	{
		_rotation.AdvanceTime(IPCOBJBKNAO);
	}

	public virtual void SetSpeed(float LKMBEJFMCHJ, float IAKJEEBPDBE)
	{
		_speedX = LKMBEJFMCHJ;
		_speedY = IAKJEEBPDBE;
	}

	public virtual void AddTransparencyKeyframe(float GKIHFPFHKCI, float value, float JENJFNNFGLD)
	{
		value = Mathf.Max(0f, value);
		value = Mathf.Min(100f, value);
		_transparency.AddInterval(GKIHFPFHKCI, value, JENJFNNFGLD);
	}

	public virtual void SetTransparencyOffset(float IPCOBJBKNAO)
	{
		_transparency.AdvanceTime(IPCOBJBKNAO);
	}

	public virtual void SetReappearX(float LHNCHOAEGEA, float KAEPJHHLLPK)
	{
		_reappearX.state = true;
		_reappearX.Min = LHNCHOAEGEA;
		_reappearX.Max = KAEPJHHLLPK;
	}

	public virtual void SetReappearY(float LHNCHOAEGEA, float KAEPJHHLLPK)
	{
		_reappearY.state = true;
		_reappearY.Min = LHNCHOAEGEA;
		_reappearY.Max = KAEPJHHLLPK;
	}

	public virtual void Render(float KBBLAECAAFG = 1f)
	{
		float num = 1f / 60f * KBBLAECAAFG;
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
		_elapsedFrames += KBBLAECAAFG;
		if (_type == SpriteEffectType.AtlasBased)
		{
			float num2 = UpdateAnimation(KBBLAECAAFG);
			if (num2 > 0f)
			{
				_animation.Render(1f / 60f * num2);
			}
		}
		_positionX += _speedX;
		float kKMDAOEBJDJ = _positionX;
		_oscillationX.AdvanceTime(num);
		kKMDAOEBJDJ += _oscillationX.GetCurrentValue();
		_positionY += _speedY;
		float fLIBIFJKJOD = _positionY;
		_oscillationY.AdvanceTime(num);
		fLIBIFJKJOD += _oscillationY.GetCurrentValue();
		Vector3 localPosition = SpriteObject.transform.localPosition;
		SpriteObject.transform.localPosition = new Vector3(kKMDAOEBJDJ + _spriteOffsetX, fLIBIFJKJOD + _spriteOffsetY, localPosition.z);
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
			if (kKMDAOEBJDJ > _reappearX.Max)
			{
				_positionX = _reappearX.Min + (kKMDAOEBJDJ - _reappearX.Max);
			}
			if (kKMDAOEBJDJ < _reappearX.Min)
			{
				_positionX = _reappearX.Max - (_reappearX.Min - kKMDAOEBJDJ);
			}
		}
		if (_reappearY.state)
		{
			if (fLIBIFJKJOD > _reappearY.Max)
			{
				_positionY = _reappearY.Min + (fLIBIFJKJOD - _reappearY.Max);
			}
			if (fLIBIFJKJOD < _reappearY.Min)
			{
				_positionY = _reappearY.Max - (_reappearY.Min - fLIBIFJKJOD);
			}
		}
	}

	public void Release()
	{
	}

	private float UpdateAnimation(float HDJFIPHOLMP)
	{
		if (HDJFIPHOLMP > _animationDuration + _pauseDuration)
		{
			HDJFIPHOLMP -= (float)(int)(HDJFIPHOLMP / (_animationDuration + _pauseDuration)) * (_animationDuration + _pauseDuration);
		}
		if (_pauseElapsed == 0f)
		{
			_animationTime += HDJFIPHOLMP;
			if (_animationTime < _animationDuration)
			{
				return HDJFIPHOLMP;
			}
			_pauseElapsed += _animationTime - _animationDuration;
			if (_pauseElapsed >= _pauseDuration)
			{
				float hDJFIPHOLMP = _pauseElapsed - _pauseDuration;
				_animationTime = 0f;
				_pauseElapsed = 0f;
				return UpdateAnimation(hDJFIPHOLMP);
			}
			return _animationDuration - (_animationTime - HDJFIPHOLMP + 1E-05f);
		}
		_pauseElapsed += HDJFIPHOLMP;
		if (_pauseElapsed < _pauseDuration)
		{
			return 0f;
		}
		float mFOJNBJIJIB = _pauseElapsed;
		_animationTime = 0f;
		_pauseElapsed = 0f;
		return UpdateAnimation(mFOJNBJIJIB - _pauseDuration);
	}
}
