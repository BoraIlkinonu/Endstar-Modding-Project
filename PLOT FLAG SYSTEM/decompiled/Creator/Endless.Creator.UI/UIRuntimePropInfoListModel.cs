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
using UnityEngine;

namespace Endless.Creator.UI;

public class UIRuntimePropInfoListModel : UIBaseLocalFilterableListModel<PropLibrary.RuntimePropInfo>
{
	public enum Contexts
	{
		PropTool,
		InventoryLibraryReference
	}

	[field: SerializeField]
	public Contexts Context { get; private set; }

	protected override Comparison<PropLibrary.RuntimePropInfo> DefaultSort => (PropLibrary.RuntimePropInfo x, PropLibrary.RuntimePropInfo y) => string.Compare(x.PropData.Name, y.PropData.Name, StringComparison.Ordinal);

	public void Synchronize(ReferenceFilter referenceFilter = ReferenceFilter.None, IReadOnlyList<PropLibrary.RuntimePropInfo> propsToIgnore = null)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Synchronize", referenceFilter, propsToIgnore.DebugSafeCount());
		}
		List<PropLibrary.RuntimePropInfo> list = new List<PropLibrary.RuntimePropInfo>();
		HashSet<SerializableGuid> hashSet = new HashSet<SerializableGuid>();
		if (propsToIgnore != null)
		{
			foreach (PropLibrary.RuntimePropInfo item in propsToIgnore)
			{
				if (item != null)
				{
					hashSet.Add(item.PropData.AssetID);
				}
			}
		}
		AssetReference[] assetReferences = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetAssetReferences();
		foreach (AssetReference assetReference in assetReferences)
		{
			if (MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(assetReference, out var metadata) && !metadata.IsMissingObject && !hashSet.Contains(metadata.PropData.AssetID) && (referenceFilter == ReferenceFilter.None || metadata.EndlessProp.ReferenceFilter.HasFlag(referenceFilter)))
			{
				list.Add(metadata);
			}
		}
		Set(list, triggerEvents: true);
	}
}
