using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace Runtime.Creator.UI
{
	// Token: 0x0200000E RID: 14
	public class UILibraryContentPack : UIAssetList<LibraryContentPack, UIGameAsset>
	{
		// Token: 0x06000032 RID: 50 RVA: 0x00002C88 File Offset: 0x00000E88
		protected override async Task<List<UIGameAsset>> ConvertToUiModelsAsync([TupleElementNames(new string[] { "asset", "versions" })] List<ValueTuple<AssetCore, List<PublishedVersion>>> result)
		{
			if (base.VerboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "ConvertToUiModelsAsync", "result", result.Count), this);
			}
			List<ValueTuple<SerializableGuid, string>> list = new List<ValueTuple<SerializableGuid, string>>();
			foreach (ValueTuple<AssetCore, List<PublishedVersion>> valueTuple in result)
			{
				PublishedVersion publishedVersion = (from v in valueTuple.Item2
					where v.State == UIPublishStates.Public.ToEndlessCloudServicesCompatibleString()
					orderby v.AssetVersion descending
					select v).FirstOrDefault<PublishedVersion>();
				if (publishedVersion == null)
				{
					Debug.LogError("Could not find public version of " + valueTuple.Item1.Name + " with an id of " + valueTuple.Item1.AssetID, this);
				}
				else
				{
					list.Add(new ValueTuple<SerializableGuid, string>(valueTuple.Item1.AssetID, publishedVersion.AssetVersion));
				}
			}
			List<UIGameAsset> list2;
			if (list.Count == 0)
			{
				list2 = new List<UIGameAsset>();
			}
			else
			{
				try
				{
					GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetBulkAssets(list.ToArray(), null, base.VerboseLogging);
					if (base.VerboseLogging)
					{
						Debug.Log("ConvertToUiModelsAsync | Bulk request complete!", this);
					}
					if (graphQlResult.HasErrors)
					{
						throw graphQlResult.GetErrorMessage(0);
					}
					if (base.VerboseLogging)
					{
						Debug.Log("bulkAssetResult.RawResult: " + graphQlResult.RawResult, this);
					}
					Asset[] dataMember = graphQlResult.GetDataMember<Asset[]>();
					if (dataMember == null)
					{
						Debug.LogError("assets is null!", this);
						list2 = new List<UIGameAsset>();
					}
					else
					{
						List<UIGameAsset> list3 = new List<UIGameAsset>();
						Asset[] array = dataMember;
						for (int i = 0; i < array.Length; i++)
						{
							Asset asset = array[i];
							try
							{
								string assetType = asset.AssetType;
								UIGameAsset uigameAsset;
								if (!(assetType == "terrain-tileset-cosmetic"))
								{
									if (!(assetType == "prop"))
									{
										if (!(assetType == "audio"))
										{
											Debug.LogError("AssetType's type '" + asset.AssetType + "' is not supported.", this);
											goto IL_0402;
										}
										AudioAsset[] dataMember2 = graphQlResult.GetDataMember<AudioAsset[]>();
										AudioAsset audioAsset = ((dataMember2 != null) ? dataMember2.FirstOrDefault((AudioAsset a) => a.AssetID == asset.AssetID) : null);
										if (audioAsset == null)
										{
											Debug.LogError("Could not deserialize audio asset " + asset.AssetID, this);
											goto IL_0402;
										}
										uigameAsset = new UIGameAsset(audioAsset, UIGameAssetTypes.Ambient);
									}
									else
									{
										Prop[] dataMember3 = graphQlResult.GetDataMember<Prop[]>();
										Prop prop = ((dataMember3 != null) ? dataMember3.FirstOrDefault((Prop p) => p.AssetID == asset.AssetID) : null);
										if (prop == null)
										{
											Debug.LogError("Could not deserialize prop " + asset.AssetID, this);
											goto IL_0402;
										}
										uigameAsset = new UIGameAsset(prop);
									}
								}
								else
								{
									TerrainTilesetCosmeticAsset[] dataMember4 = graphQlResult.GetDataMember<TerrainTilesetCosmeticAsset[]>();
									TerrainTilesetCosmeticAsset terrainTilesetCosmeticAsset = ((dataMember4 != null) ? dataMember4.FirstOrDefault((TerrainTilesetCosmeticAsset t) => t.AssetID == asset.AssetID) : null);
									if (terrainTilesetCosmeticAsset == null)
									{
										Debug.LogError("Could not deserialize terrain asset " + asset.AssetID, this);
										Debug.Log("asset: " + asset.AssetType, this);
										Debug.Log("asset: " + JsonConvert.SerializeObject(asset), this);
										goto IL_0402;
									}
									uigameAsset = new UIGameAsset(terrainTilesetCosmeticAsset);
								}
								if (uigameAsset != null)
								{
									list3.Add(uigameAsset);
								}
							}
							catch (Exception ex)
							{
								Debug.LogException(ex, this);
							}
							IL_0402:;
						}
						if (base.VerboseLogging)
						{
							Debug.Log(string.Format("Converted {0} assets", list3.Count), this);
						}
						list2 = list3;
					}
				}
				catch (Exception ex2)
				{
					Debug.LogException(ex2, this);
					list2 = new List<UIGameAsset>();
				}
			}
			return list2;
		}
	}
}
