using System.Collections.Generic;
using UnityEngine;

namespace Nekki.SF2.GUI
{
	public class ModuleHolder : MonoBehaviour
	{
		[SerializeField]
		private List<UIModule> _MountOnStart = new List<UIModule>();

		[SerializeField]
		private List<UIModule> _MountOnLater = new List<UIModule>();

		public List<UIModule> MountOnStartModules
		{
			get
			{
				return get_MountOnStart();
			}
		}

		public List<UIModule> MountOnLaterModules
		{
			get
			{
				return get_MountOnLater();
			}
		}

		public List<UIModule> get_MountOnStart()
		{
			return _MountOnStart;
		}

		public List<UIModule> get_MountOnLater()
		{
			return _MountOnLater;
		}

		public Canvas GetCanvas()
		{
			return base.gameObject.GetComponent<Canvas>();
		}

		protected virtual void OnBeforeMount()
		{
		}

		protected virtual bool ShouldMountModules()
		{
			return true;
		}

		protected virtual void Awake()
		{
			OnBeforeMount();
			MountModules();
		}

		private void MountModules()
		{
			foreach (UIModule item in _MountOnStart)
			{
				UIModule.MountModule(item, base.transform, true);
			}
			foreach (UIModule item2 in _MountOnLater)
			{
				UIModule.MountModule(item2, base.transform, false);
			}
		}

		protected virtual void OnDestroy()
		{
		}

		protected T GetModule<T>() where T : UIModule
		{
			return UIModule.GetModule<T>();
		}

		public UIModule GetModuleByName(string moduleName)
		{
			return UIModule.GetModuleByName(moduleName);
		}

		public List<UIModule> ActiveModule()
		{
			List<UIModule> list = new List<UIModule>();
			CollectActiveModules(_MountOnStart, list);
			CollectActiveModules(_MountOnLater, list);
			return list;
		}

		protected static void CollectActiveModules(List<UIModule> allModules, List<UIModule> activeModules)
		{
			for (int i = 0; i < allModules.Count; i++)
			{
				if (allModules[i].gameObject.activeSelf)
				{
					activeModules.Add(allModules[i]);
				}
			}
		}
	}
}
