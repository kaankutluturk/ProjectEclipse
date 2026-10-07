public class ComboCounter : global::EventDispatcher<object>
{
	public enum ComboCounterEvent
	{
		ON_COMBO_CHANGE = 0
	}

	private int _comboCount;

	private int _hitCount;

	private int _framesSinceHit;

	private int _lastComboCount;

	private bool _isCounting;

	public int ComboCount
	{
		get
		{
			return GetComboCount();
		}
	}

	public int HitCount
	{
		get
		{
			return GetHitCount();
		}
	}

	public int LastComboCount
	{
		get
		{
			return GetLastComboCount();
		}
	}

	public bool IsCounting
	{
		get
		{
			return GetIsCounting();
		}
	}

	public int GetComboCount()
	{
		return _comboCount;
	}

	public int GetHitCount()
	{
		return _hitCount;
	}

	public int GetLastComboCount()
	{
		return _lastComboCount;
	}

	public bool GetIsCounting()
	{
		return _isCounting;
	}

	public void UpdateCombo()
	{
		if (!_isCounting)
		{
			return;
		}
		_framesSinceHit++;
		if (_framesSinceHit > GameUtils.GetComboTime())
		{
			_lastComboCount = _comboCount;
			Reset();
			if (_lastComboCount >= GameUtils.GetComboMinHits())
			{
				CallEvent(0, _comboCount);
			}
		}
	}

	public void RegisterHit()
	{
		_isCounting = true;
		_framesSinceHit = 0;
		_hitCount++;
		if (_hitCount >= GameUtils.GetComboMinHits())
		{
			_comboCount = _hitCount;
			_lastComboCount = _comboCount;
			CallEvent(0, _comboCount);
		}
	}

	public void Reset()
	{
		_isCounting = false;
		_framesSinceHit = 0;
		_hitCount = 0;
		_comboCount = 0;
	}
}
