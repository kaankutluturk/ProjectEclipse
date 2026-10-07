using System.Diagnostics;
using UnityEngine;

namespace Nekki.Audio
{
	public class AudioUnit : MonoBehaviour
	{
		private PlayCommand _command;

		private AudioSource _source;

		private Chanel _parent;

		private bool _fadingOut;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private bool _isPaused;

		private bool _wasPlaying;

		internal bool IsAudioPaused
		{
			get
			{
				return GetIsPaused();
			}
			private set
			{
				SetIsPaused(value);
			}
		}

		public bool Muted
		{
			get
			{
				return get_IsMute();
			}
			set
			{
				set_IsMute(value);
			}
		}

		internal bool IsFree
		{
			get
			{
				return GetIsFree();
			}
		}

		internal bool GetIsPaused()
		{
			return _isPaused;
		}

		private void SetIsPaused(bool value)
		{
			_isPaused = value;
		}

		public bool get_IsMute()
		{
			return _source.mute;
		}

		public void set_IsMute(bool value)
		{
			_source.mute = value;
		}

		internal void Init(Chanel parentChanel, PlayCommand command, AudioClip clip)
		{
			_command = command;
			if (!_source)
			{
				_source = base.gameObject.AddComponent<AudioSource>();
				_source.spatialBlend = 0f;
			}
			_source.clip = clip;
			_source.loop = command.GetLoop();
			_source.volume = parentChanel.GetMasterVolume() * command.GetVolume();
			_parent = parentChanel;
			SetIsPaused(false);
			_wasPlaying = false;
			_source.Play();
		}

		public void Pause()
		{
			if (!(_source == null) && !GetIsPaused())
			{
				SetIsPaused(true);
				_wasPlaying = _source.isPlaying;
				_source.Pause();
			}
		}

		public void UnPause()
		{
			if (!(_source == null) && GetIsPaused())
			{
				if (_wasPlaying)
				{
					_source.Play();
				}
				_source.volume = _parent.GetMasterVolume() * _command.GetVolume();
				SetIsPaused(false);
				_wasPlaying = false;
			}
		}

		public void Stop(bool fadeOut = false)
		{
			if ((bool)_source)
			{
				if (!fadeOut)
				{
					_source.Stop();
					SetIsPaused(false);
					_wasPlaying = false;
				}
				else
				{
					_fadingOut = true;
				}
			}
		}

		internal bool GetIsFree()
		{
			return !_source || ((bool)_source && !_source.isPlaying && !GetIsPaused());
		}

		internal void Update()
		{
			if (GetIsPaused() || GetIsFree())
			{
				return;
			}
			// Scene teardown can destroy the owning channel before this component's
			// final Update.  The decompiled code kept dereferencing the stale owner
			// every frame, producing thousands of errors and severe Editor lag.
			if (_parent == null || _command == null)
			{
				if (_source != null)
				{
					_source.Stop();
				}
				enabled = false;
				return;
			}
			if (_fadingOut)
			{
				float num = _source.volume * 0.9f;
				if ((double)num < 0.05)
				{
					Stop();
					_fadingOut = false;
				}
				else
				{
					_source.volume = num;
				}
			}
			else
			{
				_source.volume = _parent.GetMasterVolume() * _command.GetVolume();
			}
		}

		internal void ReturnToChanel()
		{
			if (_parent != null)
			{
				_parent.FreeUnit(this);
			}
		}
	}
}
