using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Nekki.SF2.GUI.Dialogs
{
	public class DialogCanvasController : MonoBehaviour
	{
		[SerializeField]
		private ResolutionImage _Background;

		[SerializeField]
		private GameObject _SettingsDialogPrefab;

		[SerializeField]
		private GameObject _SettingsAdvancedDialogPrefab;

		[SerializeField]
		private GameObject _StrangerDialogPrefab;

		[SerializeField]
		private GameObject _StoryDialogPrefab;

		[SerializeField]
		private GameObject _SimpleDialogPrefab;

		[SerializeField]
		private GameObject _TradeDialogPrefab;

		[SerializeField]
		private GameObject _NewsDialogPrefab;

		[SerializeField]
		private GameObject _ExitDialogPrefab;

		private Dictionary<Type, GameObject> prefabsByType = new Dictionary<Type, GameObject>();

		private static DialogCanvasController _instance;

		private static Canvas dialogsCanvas;

		public static DialogCanvasController SharedInstance
		{
			get
			{
				return get_Instance();
			}
		}

		public static Canvas CanvasInstance
		{
			get
			{
				return get_DialogsCanvas();
			}
		}

		public static DialogCanvasController get_Instance()
		{
			if (_instance == null)
			{
				GameObject original = Resources.Load<GameObject>("Prefabs/Dialogs/DialogCanvas");
				GameObject gameObject = UnityEngine.Object.Instantiate(original);
				gameObject.name = "[DialogCanvas]";
				_instance = gameObject.GetComponent<DialogCanvasController>();
				UnityEngine.Object.DontDestroyOnLoad(gameObject);
			}
			return _instance;
		}

		public static Canvas get_DialogsCanvas()
		{
			return dialogsCanvas;
		}

		private void Awake()
		{
			dialogsCanvas = GetComponent<Canvas>();
			_Background.gameObject.SetActive(false);
			RegisterDialogPrefabs();
		}

		private void RegisterDialogPrefabs()
		{
			prefabsByType.Add(typeof(SettingsDialog), _SettingsDialogPrefab);
			prefabsByType.Add(typeof(SettingsAdvancedDialog), _SettingsAdvancedDialogPrefab);
			prefabsByType.Add(typeof(StrangerDialog), _StrangerDialogPrefab);
			prefabsByType.Add(typeof(StoryDialog), _StoryDialogPrefab);
			prefabsByType.Add(typeof(SimpleDialog), _SimpleDialogPrefab);
			prefabsByType.Add(typeof(TradeDialog), _TradeDialogPrefab);
			prefabsByType.Add(typeof(NewsDialog), _NewsDialogPrefab);
			prefabsByType.Add(typeof(ExitDialog), _ExitDialogPrefab);
		}

		private void OnDestroy()
		{
			Eclipse.UI.Modding.ModUiGameBridge.SetNativeBlocked(false);
			_instance = null;
			dialogsCanvas = null;
		}

		public void BlockTouches()
		{
			Eclipse.UI.Modding.ModUiGameBridge.SetNativeBlocked(true);
			GraphicRaycaster[] collection = UnityEngine.Object.FindObjectsOfType<GraphicRaycaster>();
			List<GraphicRaycaster> list = new List<GraphicRaycaster>(collection);
			for (int i = 0; i < list.Count; i++)
			{
				list[i].enabled = false;
			}
		}

		public void BlockNotDialogTouches()
		{
			BlockTouches();
			base.gameObject.GetComponent<GraphicRaycaster>().enabled = true;
		}

		public void UnBlockTouches()
		{
			Eclipse.UI.Modding.ModUiGameBridge.SetNativeBlocked(false);
			GraphicRaycaster[] collection = UnityEngine.Object.FindObjectsOfType<GraphicRaycaster>();
			List<GraphicRaycaster> list = new List<GraphicRaycaster>(collection);
			for (int i = 0; i < list.Count; i++)
			{
				list[i].enabled = true;
			}
		}

		public T CreateDialog<T>() where T : BaseDialog
		{
			Type typeFromHandle = typeof(T);
			if (GetPrefab(typeFromHandle) == null)
			{
				GameLog.Error("Dialog prefab is empty! Name=" + typeFromHandle.ToString());
				return (T)null;
			}
			T component = UnityEngine.Object.Instantiate(GetPrefab(typeFromHandle)).GetComponent<T>();
			component.transform.SetParent(base.transform, false);
			return component;
		}

		private GameObject GetPrefab(Type IGABHEMGKKE)
		{
			if (prefabsByType.ContainsKey(IGABHEMGKKE))
			{
				return prefabsByType[IGABHEMGKKE];
			}
			return null;
		}
	}
}
