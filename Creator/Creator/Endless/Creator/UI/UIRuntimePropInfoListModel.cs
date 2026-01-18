using System;
using System.Collections.Generic;
using Endless.Assets;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Creator.UI
{
	// Token: 0x02000166 RID: 358
	public class UIRuntimePropInfoListModel : UIBaseLocalFilterableListModel<PropLibrary.RuntimePropInfo>
	{
		// Token: 0x17000086 RID: 134
		// (get) Token: 0x06000556 RID: 1366 RVA: 0x0001CDA2 File Offset: 0x0001AFA2
		// (set) Token: 0x06000557 RID: 1367 RVA: 0x0001CDAA File Offset: 0x0001AFAA
		public UIRuntimePropInfoListModel.Contexts Context { get; private set; }

		// Token: 0x17000087 RID: 135
		// (get) Token: 0x06000558 RID: 1368 RVA: 0x0001CDB3 File Offset: 0x0001AFB3
		protected override Comparison<PropLibrary.RuntimePropInfo> DefaultSort
		{
			get
			{
				return (PropLibrary.RuntimePropInfo x, PropLibrary.RuntimePropInfo y) => string.Compare(x.PropData.Name, y.PropData.Name, StringComparison.Ordinal);
			}
		}

		// Token: 0x06000559 RID: 1369 RVA: 0x0001CDD4 File Offset: 0x0001AFD4
		public void Synchronize(ReferenceFilter referenceFilter = ReferenceFilter.None, IReadOnlyList<PropLibrary.RuntimePropInfo> propsToIgnore = null)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Synchronize", new object[]
				{
					referenceFilter,
					propsToIgnore.DebugSafeCount<PropLibrary.RuntimePropInfo>()
				});
			}
			List<PropLibrary.RuntimePropInfo> list = new List<PropLibrary.RuntimePropInfo>();
			HashSet<SerializableGuid> hashSet = new HashSet<SerializableGuid>();
			if (propsToIgnore != null)
			{
				foreach (PropLibrary.RuntimePropInfo runtimePropInfo in propsToIgnore)
				{
					if (runtimePropInfo != null)
					{
						hashSet.Add(runtimePropInfo.PropData.AssetID);
					}
				}
			}
			foreach (AssetReference assetReference in MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetAssetReferences())
			{
				PropLibrary.RuntimePropInfo runtimePropInfo2;
				if (MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(assetReference, out runtimePropInfo2) && !runtimePropInfo2.IsMissingObject && !hashSet.Contains(runtimePropInfo2.PropData.AssetID) && (referenceFilter == ReferenceFilter.None || runtimePropInfo2.EndlessProp.ReferenceFilter.HasFlag(referenceFilter)))
				{
					list.Add(runtimePropInfo2);
				}
			}
			this.Set(list, true);
		}

		// Token: 0x02000167 RID: 359
		public enum Contexts
		{
			// Token: 0x040004D1 RID: 1233
			PropTool,
			// Token: 0x040004D2 RID: 1234
			InventoryLibraryReference
		}
	}
}
