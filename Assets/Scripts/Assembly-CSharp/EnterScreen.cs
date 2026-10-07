using System;
using System.Collections.Generic;
using DG.Tweening;
using Nekki.SF2.GUI;
using UnityEngine;
using UnityEngine.UI;

public class EnterScreen : MonoBehaviour
{
	[SerializeField]
	public float MIN_OPACITY;

	[SerializeField]
	public float MAX_OPACITY = 1f;

	[SerializeField]
	public float fadeTime = 1f;

	[SerializeField]
	public float expandTime = 1f;

	[SerializeField]
	public float hideTime = 1f;

	[SerializeField]
	public float endTime = 1f;

	[SerializeField]
	private LabelAlias _label;

	[SerializeField]
	private ResolutionImage _backgroundLeft;

	[SerializeField]
	private ResolutionImage _backgroundRight;

	[SerializeField]
	private Image _foreground;

	private Action _dlg;
    private DG.Tweening.Sequence _presentation;
    private bool _finished;
    private bool _musicCaptured;

	// best guess for name
	private float _originalMusicVolume;

	private float currentMusicVolume;

	public static EnterScreen Create()
	{
		EnterScreen original = Resources.Load<EnterScreen>("Prefabs/Map/EnterScreen");
		return UnityEngine.Object.Instantiate(original);
	}

	public void Init(List<KeyValuePair<string, int>> lines, Action completed)
	{
        InitLines(lines, completed, false);
    }

    public void InitResolved(List<KeyValuePair<string, int>> lines, Action completed)
    {
        InitLines(lines, completed, true);
    }

    private void InitLines(List<KeyValuePair<string, int>> lines, Action completed, bool resolved)
	{
		_dlg = completed;
		SetVisible(false);
		CaptureMusicVolume();
		SetForegroundAlpha(MIN_OPACITY);
		if (_label != null && lines.Count > 0)
		{
			SetLine(lines[0].Key, resolved);
		}
		DG.Tweening.Sequence sequence = DOTween.Sequence();
		sequence.AppendInterval(0f);
		foreach (KeyValuePair<string, int> line in lines)
		{
			sequence.AppendCallback(() =>
			{
				if (_label != null)
				{
					SetLine(line.Key, resolved);
				}
			});
			float interval = (float)line.Value / 60f;
			sequence.AppendInterval(interval);
		}
		DG.Tweening.Sequence t = MuteMusicSequence();
		DG.Tweening.Sequence s = DOTween.Sequence();
        _presentation = s;
		s.Append(t);
		s.Join(_foreground.DOFade(MAX_OPACITY, fadeTime));
		s.AppendCallback(() =>
		{
			PlayMusic();
			SetVisible(true);
		});
		s.Append(_foreground.DOFade(MIN_OPACITY, expandTime));
		s.Append(sequence);
		s.Append(_foreground.DOFade(MAX_OPACITY, endTime));
		s.AppendCallback(() =>
		{
			End();
		});
	}

	public void Init(string alias, Action completed)
	{
		_dlg = completed;
		if (_label != null)
		{
			_label.set_Alias(alias);
		}
		SetVisible(false);
		CaptureMusicVolume();
		SetForegroundAlpha(MIN_OPACITY);
		DG.Tweening.Sequence t = MuteMusicSequence();
		DG.Tweening.Sequence s = DOTween.Sequence();
        _presentation = s;
		s.Append(t);
		s.Join(_foreground.DOFade(MAX_OPACITY, fadeTime));
		s.AppendCallback(() =>
		{
			PlayMusic();
			SetVisible(true);
		});
		s.Append(_foreground.DOFade(MIN_OPACITY, expandTime));
		s.AppendInterval(hideTime);
		s.Append(_foreground.DOFade(MAX_OPACITY, endTime));
		s.AppendCallback(() =>
		{
			End();
		});
	}

	public DG.Tweening.Sequence MuteMusicSequence()
	{
		DG.Tweening.Sequence sequence = DOTween.Sequence();
		sequence.Append(DOTween.To(() => currentMusicVolume, (float volume) =>
		{
			currentMusicVolume = volume;
			Sound.SetMusicVolume(currentMusicVolume);
		}, 0f, fadeTime));
		sequence.AppendCallback(() =>
		{
			Sound.SetMusicVolume(_originalMusicVolume);
			Sound.StopMusic();
		});
		return sequence;
	}

	public void End()
	{
        if (_finished) return;
        var completed = _dlg;
        FinishPresentation(false);
        // Destroy invokes OnDisable synchronously: report success before a
        // presentation owner's disable hook can interpret it as cancellation.
        try { completed?.Invoke(); }
        finally { UnityEngine.Object.Destroy(base.gameObject); }
    }

    public void Cancel()
    {
        if (_finished) return;
        FinishPresentation();
    }

    private void FinishPresentation(bool destroyObject = true)
    {
        _finished = true;
        _dlg = null;
        _presentation?.Kill();
        _presentation = null;
		if (_musicCaptured) Sound.SetMusicVolume(_originalMusicVolume);
		if (_musicCaptured && !Sound.GetMusicMuted())
		{
			SoundController.IsBackgroundMusicIntro = false;
			SoundController.StartBackgroundMusic();
		}
		if (destroyObject) UnityEngine.Object.Destroy(base.gameObject);
	}

    private void OnDestroy()
    {
        if (!_finished) FinishPresentation(false);
    }

    private void SetLine(string text, bool resolved)
    {
        if (!resolved) { _label.set_Alias(text); return; }
        _label.set_Alias(string.Empty);
        _label.supportRichText = false;
        _label.set_text(text);
    }

	public void SetVisible(bool value)
	{
		if (_backgroundLeft != null)
		{
			_backgroundLeft.gameObject.SetActive(value);
		}
		if (_backgroundRight != null)
		{
			_backgroundRight.gameObject.SetActive(value);
		}
		if (_label != null)
		{
			_label.gameObject.SetActive(value);
		}
	}

	private void PlayMusic()
	{
		if (!Sound.GetMusicMuted())
		{
			Sound.SetMusicVolume(_originalMusicVolume);
			Sound.StopMusic();
			SoundController.IsBackgroundMusicIntro = false;
			SoundController.StartBackgroundMusic("act", false);
		}
	}

	private void CaptureMusicVolume()
	{
		if (!Sound.GetMusicMuted())
		{
			_originalMusicVolume = Sound.GetMusicVolume();
            _musicCaptured = true;
			currentMusicVolume = _originalMusicVolume;
		}
	}

	private void SetForegroundAlpha(float alpha)
	{
		if (_foreground != null)
		{
			Color color = _foreground.color;
			color.a = alpha;
			_foreground.color = color;
		}
	}
}
