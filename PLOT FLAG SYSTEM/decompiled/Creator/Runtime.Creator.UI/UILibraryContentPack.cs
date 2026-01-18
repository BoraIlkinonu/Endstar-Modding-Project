using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Endless.Assets;
using Endless.Creator.UI;
using Endless.GraphQl;
using Endless.Props.Assets;
using Endless.Shared.DataTypes;
using Endless.TerrainCosmetics;
using Newtonsoft.Json;
using Runtime.Shared.Matchmaking;
using UnityEngine;

namespace Runtime.Creator.UI;

public class UILibraryContentPack : UIAssetList<LibraryContentPack, UIGameAsset>
{
	protected override async Task<List<UIGameAsset>> ConvertToUiModelsAsync(List<(AssetCore asset, List<PublishedVersion> versions)> result)
	{
		if (base.VerboseLogging)
		{
			Debug.Log(string.Format("{0} ( {1}: {2} )", "ConvertToUiModelsAsync", "result", result.Count), this);
		}
		List<(SerializableGuid, string)> list = new List<(SerializableGuid, string)>();
		foreach (var item in result)
		{
			PublishedVersion publishedVersion = (from v in item.versions
				where v.State == UIPublishStates.Public.ToEndlessCloudServicesCompatibleString()
				orderby v.AssetVersion descending
				select v).FirstOrDefault();
			if (publishedVersion == null)
			{
				Debug.LogError("Could not find public version of " + item.asset.Name + " with an id of " + item.asset.AssetID, this);
			}
			else
			{
				list.Add((item.asset.AssetID, publishedVersion.AssetVersion));
			}
		}
		if (list.Count == 0)
		{
			return new List<UIGameAsset>();
		}
		try
		{
			GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetBulkAssets(list.ToArray(), null, base.VerboseLogging);
			if (base.VerboseLogging)
			{
				Debug.Log("ConvertToUiModelsAsync | Bulk request complete!", this);
			}
			if (graphQlResult.HasErrors)
			{
				throw graphQlResult.GetErrorMessage();
			}
			if (base.VerboseLogging)
			{
				Debug.Log("bulkAssetResult.RawResult: " + graphQlResult.RawResult, this);
			}
			Asset[] dataMember = graphQlResult.GetDataMember<Asset[]>();
			if (dataMember == null)
			{
				Debug.LogError("assets is null!", this);
				return new List<UIGameAsset>();
			}
			List<UIGameAsset> list2 = new List<UIGameAsset>();
			Asset[] array = dataMember;
			foreach (Asset asset in array)
			{
				try
				{
					UIGameAsset uIGameAsset;
					switch (asset.AssetType)
					{
					case "terrain-tileset-cosmetic":
					{
						TerrainTilesetCosmeticAsset terrainTilesetCosmeticAsset = graphQlResult.GetDataMember<TerrainTilesetCosmeticAsset[]>()?.FirstOrDefault((TerrainTilesetCosmeticAsset t) => t.AssetID == asset.AssetID);
						if (terrainTilesetCosmeticAsset == null)
						{
							Debug.LogError("Could not deserialize terrain asset " + asset.AssetID, this);
							Debug.Log("asset: " + asset.AssetType, this);
							Debug.Log("asset: " + JsonConvert.SerializeObject(asset), this);
							continue;
						}
						uIGameAsset = new UIGameAsset(terrainTilesetCosmeticAsset);
						break;
					}
					case "prop":
					{
						Prop prop = graphQlResult.GetDataMember<Prop[]>()?.FirstOrDefault((Prop p) => p.AssetID == asset.AssetID);
						if (prop == null)
						{
							Debug.LogError("Could not deserialize prop " + asset.AssetID, this);
							continue;
						}
						uIGameAsset = new UIGameAsset(prop);
						break;
					}
					case "audio":
					{
						AudioAsset audioAsset = graphQlResult.GetDataMember<AudioAsset[]>()?.FirstOrDefault((AudioAsset a) => a.AssetID == asset.AssetID);
						if (audioAsset == null)
						{
							Debug.LogError("Could not deserialize audio asset " + asset.AssetID, this);
							continue;
						}
						uIGameAsset = new UIGameAsset(audioAsset, UIGameAssetTypes.Ambient);
						break;
					}
					default:
						Debug.LogError("AssetType's type '" + asset.AssetType + "' is not supported.", this);
						goto end_IL_024d;
					}
					if (uIGameAsset != null)
					{
						list2.Add(uIGameAsset);
					}
					end_IL_024d:;
				}
				catch (Exception exception)
				{
					Debug.LogException(exception, this);
				}
			}
			if (base.VerboseLogging)
			{
				Debug.Log($"Converted {list2.Count} assets", this);
			}
			return list2;
		}
		catch (Exception exception2)
		{
			Debug.LogException(exception2, this);
			return new List<UIGameAsset>();
		}
	}
}
