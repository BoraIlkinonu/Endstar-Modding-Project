using System;
using System.Collections.Generic;
using Endless.Assets;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.LevelEditing.Tilesets;
using Endless.Props.Assets;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIGameLibraryListModel : UIBaseLocalFilterableListModel<UIGameAsset>, IGameAssetListModel
{
	private HashSet<SerializableGuid> assetIdsToIgnore = new HashSet<SerializableGuid>();

	[field: Header("UIGameLibraryListModel")]
	[field: SerializeField]
	public AssetContexts Context { get; private set; }

	public UIGameAssetTypes AssetTypeFilter { get; private set; } = (UIGameAssetTypes)(-1);

	public GameObject GameObject => base.gameObject;

	protected override Comparison<UIGameAsset> DefaultSort => (UIGameAsset x, UIGameAsset y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal);

	private void OnEnable()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnEnable");
		}
		NetworkBehaviourSingleton<CreatorManager>.Instance.OnRepopulate += Synchronize;
		Synchronize();
	}

	protected void OnDisable()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDisable");
		}
		NetworkBehaviourSingleton<CreatorManager>.Instance.OnRepopulate -= Synchronize;
	}

	public void SetAssetIdsToIgnore(HashSet<SerializableGuid> assetIdsToIgnore)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetAssetIdsToIgnore", assetIdsToIgnore.Count);
		}
		this.assetIdsToIgnore = assetIdsToIgnore;
	}

	public void SetAssetTypeFilter(UIGameAssetTypes value, bool triggerEvents)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetAssetTypeFilter", value, triggerEvents);
		}
		AssetTypeFilter = value;
		if (triggerEvents)
		{
			Synchronize();
		}
	}

	public void Synchronize()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Synchronize");
		}
		List<UIGameAsset> list = new List<UIGameAsset>();
		if (FilterAllows(UIGameAssetTypes.Terrain))
		{
			foreach (Tileset item4 in (IEnumerable<Tileset>)MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.RuntimePalette.DistinctTilesets)
			{
				if (base.VerboseLogging)
				{
					DebugUtility.Log("tileset: " + JsonUtility.ToJson(item4), this);
				}
				if (!(item4 is FallbackTileset) || !((SerializableGuid)item4.Asset.AssetID == SerializableGuid.Empty))
				{
					UIGameAsset item = new UIGameAsset(item4);
					list.Add(item);
				}
			}
		}
		if (FilterAllows(UIGameAssetTypes.Prop))
		{
			foreach (AssetReference propReference in MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.GameLibrary.PropReferences)
			{
				if (MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(propReference.AssetID, out var metadata))
				{
					UIGameAsset item2 = new UIGameAsset(metadata);
					list.Add(item2);
				}
			}
		}
		bool flag = FilterAllows(UIGameAssetTypes.Music);
		bool flag2 = FilterAllows(UIGameAssetTypes.SFX);
		bool flag3 = FilterAllows(UIGameAssetTypes.Ambient);
		if (flag || flag2 || flag3)
		{
			foreach (AssetReference audioReference in MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.GameLibrary.AudioReferences)
			{
				if (MonoBehaviourSingleton<StageManager>.Instance.ActiveAudioLibrary.TryGetRuntimeAudioInfo(audioReference, out var metadata2) && metadata2.AudioAsset.AudioCategory switch
				{
					AudioCategory.Music => flag, 
					AudioCategory.SFX => flag2, 
					AudioCategory.Ambient => flag3, 
					_ => throw new ArgumentOutOfRangeException(), 
				})
				{
					UIGameAsset item3 = new UIGameAsset(metadata2);
					list.Add(item3);
				}
			}
		}
		Set(list, triggerEvents: true);
	}

	public override void Set(List<UIGameAsset> list, bool triggerEvents)
	{
		if (assetIdsToIgnore.Count > 0)
		{
			for (int num = list.Count - 1; num >= 0; num--)
			{
				if (assetIdsToIgnore.Contains(list[num].AssetID))
				{
					list.RemoveAt(num);
				}
			}
		}
		base.Set(list, triggerEvents);
	}

	private bool FilterAllows(UIGameAssetTypes typeToCheck)
	{
		if (AssetTypeFilter != UIGameAssetTypes.None)
		{
			return (AssetTypeFilter & typeToCheck) == typeToCheck;
		}
		return true;
	}
}
