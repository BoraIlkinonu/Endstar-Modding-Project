using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x02000114 RID: 276
	public class UIGameLibraryFilterListModel : UIBaseLocalFilterableListModel<UIGameAssetTypes>
	{
		// Token: 0x17000069 RID: 105
		// (get) Token: 0x06000462 RID: 1122 RVA: 0x0001A183 File Offset: 0x00018383
		public bool DoNotAllowNoneSelection
		{
			get
			{
				return this.doNotAllowNoneSelection;
			}
		}

		// Token: 0x1700006A RID: 106
		// (get) Token: 0x06000463 RID: 1123 RVA: 0x0001A18B File Offset: 0x0001838B
		protected override Comparison<UIGameAssetTypes> DefaultSort
		{
			get
			{
				return (UIGameAssetTypes x, UIGameAssetTypes y) => x.CompareTo(y);
			}
		}

		// Token: 0x06000464 RID: 1124 RVA: 0x0001A1AC File Offset: 0x000183AC
		private void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			if (this.Count == 0)
			{
				bool flag = !base.RestrictSelectionCountTo1;
				this.Initialize(flag);
			}
			if (base.RestrictSelectionCountTo1 && this.Count > 0 && this.SelectedTypedList.Count < 1)
			{
				this.Select(1, true);
			}
			base.ModelChangedUnityEvent.AddListener(new UnityAction(this.ApplyFilter));
		}

		// Token: 0x06000465 RID: 1125 RVA: 0x0001A228 File Offset: 0x00018428
		public override void Unselect(int index, bool triggerEvents)
		{
			if (this[index] != UIGameAssetTypes.None && base.RestrictSelectionCountTo1 && this.doNotAllowNoneSelection)
			{
				return;
			}
			base.Unselect(index, triggerEvents);
		}

		// Token: 0x06000466 RID: 1126 RVA: 0x0001A24C File Offset: 0x0001844C
		public void SelectFilters(UIGameAssetTypes toSelect)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SelectFilters", new object[] { toSelect });
			}
			if (this.Count == 0)
			{
				this.Initialize(true);
			}
			bool flag = toSelect == UIGameAssetTypes.None;
			base.ClearSelected(flag);
			if (flag)
			{
				return;
			}
			Array values = Enum.GetValues(typeof(UIGameAssetTypes));
			for (int i = 0; i < values.Length; i++)
			{
				UIGameAssetTypes uigameAssetTypes = (UIGameAssetTypes)values.GetValue(i);
				if (uigameAssetTypes != UIGameAssetTypes.None && (toSelect & uigameAssetTypes) == uigameAssetTypes)
				{
					if (base.VerboseLogging)
					{
						DebugUtility.Log(string.Format("Selecting {0} with an {1} of {2}", uigameAssetTypes, "index", i), this);
					}
					this.Select(i, false);
					if (base.RestrictSelectionCountTo1)
					{
						break;
					}
				}
			}
			base.TriggerModelChanged();
		}

		// Token: 0x06000467 RID: 1127 RVA: 0x0001A310 File Offset: 0x00018510
		private void Initialize(bool triggerEvents)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Initialize", new object[] { triggerEvents });
			}
			if (this.Count > 0)
			{
				return;
			}
			List<UIGameAssetTypes> list = ((UIGameAssetTypes[])Enum.GetValues(typeof(UIGameAssetTypes))).ToList<UIGameAssetTypes>();
			if (!Application.isEditor)
			{
				list.Remove(UIGameAssetTypes.SFX);
				list.Remove(UIGameAssetTypes.Ambient);
				list.Remove(UIGameAssetTypes.Music);
			}
			this.Set(list, triggerEvents);
		}

		// Token: 0x06000468 RID: 1128 RVA: 0x0001A38C File Offset: 0x0001858C
		private void ApplyFilter()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ApplyFilter", Array.Empty<object>());
			}
			UIGameAssetTypes uigameAssetTypes = UIGameAssetTypes.None;
			foreach (UIGameAssetTypes uigameAssetTypes2 in this.SelectedTypedList)
			{
				uigameAssetTypes |= uigameAssetTypes2;
			}
			foreach (InterfaceReference<IGameAssetListModel> interfaceReference in this.gameAssetListModels)
			{
				interfaceReference.Interface.SetAssetTypeFilter(uigameAssetTypes, interfaceReference.Interface.GameObject.activeInHierarchy);
			}
		}

		// Token: 0x06000469 RID: 1129 RVA: 0x0001A430 File Offset: 0x00018630
		private void ReselectUnselected(int index)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ReselectUnselected", new object[] { index });
			}
			this.Select(index, true);
		}

		// Token: 0x04000442 RID: 1090
		[Header("UIGameLibraryFilterListModel")]
		[SerializeField]
		private InterfaceReference<IGameAssetListModel>[] gameAssetListModels = Array.Empty<InterfaceReference<IGameAssetListModel>>();

		// Token: 0x04000443 RID: 1091
		[SerializeField]
		private bool doNotAllowNoneSelection;
	}
}
