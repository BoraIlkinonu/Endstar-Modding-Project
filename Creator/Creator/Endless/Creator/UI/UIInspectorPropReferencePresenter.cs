using System;
using System.Collections.Generic;
using Endless.Assets;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;

namespace Endless.Creator.UI
{
	// Token: 0x0200021E RID: 542
	public abstract class UIInspectorPropReferencePresenter<TModel> : UIInspectorReferencePresenter<TModel> where TModel : InspectorPropReference
	{
		// Token: 0x1700010E RID: 270
		// (get) Token: 0x060008BE RID: 2238 RVA: 0x0002A3B3 File Offset: 0x000285B3
		protected ReferenceFilter ReferenceFilter
		{
			get
			{
				return InspectorReferenceUtility.GetReferenceFilter(base.Model);
			}
		}

		// Token: 0x1700010F RID: 271
		// (get) Token: 0x060008BF RID: 2239 RVA: 0x0002A3C8 File Offset: 0x000285C8
		protected override IEnumerable<object> SelectionOptions
		{
			get
			{
				List<object> list = new List<object>();
				foreach (AssetReference assetReference in MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.GameLibrary.PropReferences)
				{
					PropLibrary.RuntimePropInfo runtimePropInfo;
					if (MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(assetReference, out runtimePropInfo) && !runtimePropInfo.IsMissingObject && (this.ReferenceFilter == ReferenceFilter.None || runtimePropInfo.EndlessProp.ReferenceFilter.HasFlag(this.ReferenceFilter)))
					{
						list.Add(runtimePropInfo);
					}
				}
				return list;
			}
		}

		// Token: 0x060008C0 RID: 2240 RVA: 0x0002A470 File Offset: 0x00028670
		protected override void UpdateOriginalSelection(TModel model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "UpdateOriginalSelection", "model", model), this);
			}
			if (model.IsReferenceEmpty())
			{
				return;
			}
			SerializableGuid id = InspectorReferenceUtility.GetId(model);
			foreach (AssetReference assetReference in MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.GameLibrary.PropReferences)
			{
				PropLibrary.RuntimePropInfo runtimePropInfo;
				if (MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(assetReference, out runtimePropInfo) && !runtimePropInfo.IsMissingObject && runtimePropInfo.PropData.AssetID == id)
				{
					this.originalSelection.Add(runtimePropInfo);
					break;
				}
			}
		}
	}
}
