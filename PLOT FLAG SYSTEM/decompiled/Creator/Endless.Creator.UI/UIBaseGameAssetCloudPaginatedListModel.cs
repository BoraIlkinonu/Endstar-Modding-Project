using System;
using System.Collections.Generic;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public abstract class UIBaseGameAssetCloudPaginatedListModel : UIBaseAssetCloudPaginatedListModel<UIGameAsset>, IGameAssetListModel
{
	private static readonly Dictionary<UIGameAssetTypes, string> AssetTypeDictionary = new Dictionary<UIGameAssetTypes, string>
	{
		{
			UIGameAssetTypes.Terrain,
			"terrain-tileset-cosmetic"
		},
		{
			UIGameAssetTypes.Prop,
			"prop"
		},
		{
			UIGameAssetTypes.SFX,
			"audio"
		},
		{
			UIGameAssetTypes.Ambient,
			"audio"
		},
		{
			UIGameAssetTypes.Music,
			"audio"
		}
	};

	[SerializeField]
	private bool requestOnEnableIfCountIsZero;

	private UIGameAssetTypes assetTypeKey = UIGameAssetTypes.Terrain;

	private HashSet<SerializableGuid> assetIdsToIgnore = new HashSet<SerializableGuid>();

	protected override UIGameAsset SkeletonData { get; }

	protected override string AssetType => AssetTypeDictionary[assetTypeKey];

	protected override string AssetFilter
	{
		get
		{
			if (!assetTypeKey.IsAudio())
			{
				return base.AssetFilter;
			}
			return "(Name: \"*" + StringFilter + "*\", audio_category: " + assetTypeKey.ToString().ToLower() + ", " + base.SortQuery + ")";
		}
	}

	[field: Header("UIBaseGameAssetCloudPaginatedListModel")]
	[field: SerializeField]
	public AssetContexts Context { get; private set; }

	public GameObject GameObject => base.gameObject;

	private void OnEnable()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("OnEnable", this);
		}
		if (requestOnEnableIfCountIsZero)
		{
			Request(null);
		}
	}

	public void Synchronize()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("Synchronize", this);
		}
		Clear(triggerEvents: true);
		Request(null);
	}

	public void SetAssetIdsToIgnore(HashSet<SerializableGuid> assetIdsToIgnore)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetAssetIdsToIgnore", "assetIdsToIgnore", assetIdsToIgnore.Count), this);
		}
		this.assetIdsToIgnore = assetIdsToIgnore;
	}

	public void SetAssetTypeFilter(UIGameAssetTypes value, bool triggerRequest)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", "SetAssetTypeFilter", "value", value, "triggerRequest", triggerRequest), this);
		}
		if (assetTypeKey != value)
		{
			assetTypeKey = value;
			if (triggerRequest)
			{
				Clear(triggerEvents: true);
				Request(null);
			}
		}
	}

	public override void Set(List<UIGameAsset> list, bool triggerEvents)
	{
		if (assetIdsToIgnore.Count > 0)
		{
			list.RemoveAll((UIGameAsset item) => assetIdsToIgnore.Contains(item.AssetID));
		}
		base.Set(list, triggerEvents);
	}

	protected override UIPageRequestResult<UIGameAsset> ParseJson(string json)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("ParseJson ( json: " + json + " )", this);
		}
		return UIGameAssetDeserializer.Deserialize(json, assetTypeKey);
	}

	protected override void HandleError(Exception exception)
	{
		DebugUtility.LogException(exception, this);
	}
}
