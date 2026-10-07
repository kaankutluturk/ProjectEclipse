using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace Nekki.SF2.GUI
{
	public class UIModule : MonoBehaviour
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		[CompilerGenerated]
		private static Action<UIModule> OnModuleActivated;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		[CompilerGenerated]
		private static Action<UIModule> OnModuleDeactivated;

		private static List<UIModule> _modules = new List<UIModule>();

		[SerializeField]
		private int _Order;

		private Canvas _sceneCanvas;

		protected bool initialized;

		protected bool shutDown;

		public bool IsModuleActive
		{
			get
			{
				return get_IsActive();
			}
		}

		public static event Action<UIModule> ModuleActivated
		{
			add
			{
				add_OnModuleActivated(value);
			}
			remove
			{
				remove_OnModuleActivated(value);
			}
		}

		public static event Action<UIModule> ModuleDeactivated
		{
			add
			{
				add_OnModuleDeactivated(value);
			}
			remove
			{
				remove_OnModuleDeactivated(value);
			}
		}

		public static void add_OnModuleActivated(Action<UIModule> value)
		{
			Action<UIModule> action = OnModuleActivated;
			Action<UIModule> action2;
			do
			{
				action2 = action;
				action = Interlocked.CompareExchange(ref OnModuleActivated, (Action<UIModule>)Delegate.Combine(action2, value), action);
			}
			while ((object)action != action2);
		}

		public static void remove_OnModuleActivated(Action<UIModule> value)
		{
			Action<UIModule> action = OnModuleActivated;
			Action<UIModule> action2;
			do
			{
				action2 = action;
				action = Interlocked.CompareExchange(ref OnModuleActivated, (Action<UIModule>)Delegate.Remove(action2, value), action);
			}
			while ((object)action != action2);
		}

		public static void add_OnModuleDeactivated(Action<UIModule> value)
		{
			Action<UIModule> action = OnModuleDeactivated;
			Action<UIModule> action2;
			do
			{
				action2 = action;
				action = Interlocked.CompareExchange(ref OnModuleDeactivated, (Action<UIModule>)Delegate.Combine(action2, value), action);
			}
			while ((object)action != action2);
		}

		public static void remove_OnModuleDeactivated(Action<UIModule> value)
		{
			Action<UIModule> action = OnModuleDeactivated;
			Action<UIModule> action2;
			do
			{
				action2 = action;
				action = Interlocked.CompareExchange(ref OnModuleDeactivated, (Action<UIModule>)Delegate.Remove(action2, value), action);
			}
			while ((object)action != action2);
		}

		public static UIModule MountModule(UIModule ILLLNBPALIO, Transform PKHKBAJOHHF, bool CMDIBEFNCOE)
		{
			if (ILLLNBPALIO == null)
			{
				return null;
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(ILLLNBPALIO.gameObject);
			gameObject.name = ILLLNBPALIO.gameObject.name;
			gameObject.transform.SetParent(PKHKBAJOHHF, false);
			gameObject.transform.localScale = Vector3.one;
			gameObject.SetActive(false);
			UIModule component = gameObject.GetComponent<UIModule>();
			component._sceneCanvas = PKHKBAJOHHF.GetComponent<Canvas>();
			if (CMDIBEFNCOE)
			{
				component.Activate();
			}
			return component;
		}

		public static T GetModule<T>() where T : UIModule
		{
			for (int i = 0; i < _modules.Count; i++)
			{
				if (_modules[i] is T)
				{
					return _modules[i] as T;
				}
			}
			return (T)null;
		}

		public static UIModule GetModuleByName(string JLEKBBJBLOE)
		{
			for (int i = 0; i < _modules.Count; i++)
			{
				if (_modules[i].name == JLEKBBJBLOE)
				{
					return _modules[i];
				}
			}
			return null;
		}

		public void Activate()
		{
			base.gameObject.SetActive(true);
			GetComponent<RectTransform>().SetSiblingIndex(_Order);
			if (!initialized)
			{
				Init();
			}
			OnModuleActivatedHook();
			CoroutineManager.get_Current().StartCoroutine(InvokeAtEndOfFrame(OnModuleActivated));
		}

		public void DeActivate()
		{
			base.gameObject.SetActive(false);
			OnModuleDeactivatedHook();
			CoroutineManager.get_Current().StartCoroutine(InvokeAtEndOfFrame(OnModuleDeactivated));
		}

		private IEnumerator InvokeAtEndOfFrame(Action<UIModule> p_event)
		{
			yield return new WaitForEndOfFrame();
			if (p_event != null)
			{
				p_event(this);
			}
		}

		public bool get_IsActive()
		{
			return base.gameObject.activeSelf;
		}

		public void MoveToSceneCanvas()
		{
			base.transform.SetParent(_sceneCanvas.transform, false);
			Activate();
		}

		protected virtual void Init()
		{
			initialized = true;
		}

		protected virtual void OnModuleShutdown()
		{
			shutDown = true;
		}

		protected virtual void OnModuleActivatedHook()
		{
		}

		protected virtual void OnModuleDeactivatedHook()
		{
		}

		private void Awake()
		{
			_modules.Add(this);
		}

		private void OnDestroy()
		{
			if (!shutDown)
			{
				OnModuleShutdown();
			}
			_modules.Remove(this);
		}
	}
}
