using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Endless.Assets;
using Endless.Gameplay.Serialization;
using Endless.GraphQl;
using Endless.ParticleSystems.Assets;
using Endless.Props.Assets;
using Endless.Props.Loaders;
using Endless.Shared.DataTypes;
using Endless.TerrainCosmetics;
using Newtonsoft.Json;
using Runtime.Shared.Matchmaking;

namespace Endless.Gameplay
{
	// Token: 0x0200009E RID: 158
	public static class EndlessAssetCache
	{
		// Token: 0x060002C6 RID: 710 RVA: 0x0000E999 File Offset: 0x0000CB99
		private static Dictionary<AssetReference, Asset> GetCache(Type type)
		{
			if (!EndlessAssetCache.assetCaches.ContainsKey(type))
			{
				EndlessAssetCache.assetCaches.Add(type, new Dictionary<AssetReference, Asset>());
			}
			return EndlessAssetCache.assetCaches[type];
		}

		// Token: 0x060002C7 RID: 711 RVA: 0x0000E9C4 File Offset: 0x0000CBC4
		public static async Task<AssetCacheResult<T>> GetLatestAssetAsync<T>(SerializableGuid assetId) where T : Asset
		{
			Type assetType = typeof(T);
			if (!EndlessAssetCache.assetTypeStrings.ContainsKey(assetType))
			{
				throw new ArgumentException(string.Format("Type {0} is unsupported!", assetType));
			}
			GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetAssetAsync(assetId, "", null, false, 10);
			AssetCacheResult<T> assetCacheResult;
			if (graphQlResult.HasErrors)
			{
				assetCacheResult = new AssetCacheResult<T>(graphQlResult, default(T));
			}
			else
			{
				T t = EndlessAssetCache.LoadAssetFromJson<T>(graphQlResult.GetDataMember().ToString());
				AssetReference assetReference = new AssetReference();
				assetReference.AssetID = t.AssetID;
				assetReference.AssetType = t.AssetType;
				assetReference.AssetVersion = t.AssetVersion;
				EndlessAssetCache.GetCache(assetType).TryAdd(assetReference, t);
				assetCacheResult = new AssetCacheResult<T>(graphQlResult, t);
			}
			return assetCacheResult;
		}

		// Token: 0x060002C8 RID: 712 RVA: 0x0000EA08 File Offset: 0x0000CC08
		public static T LoadAssetFromJson<T>(string json) where T : Asset
		{
			if (EndlessAssetCache.typeLoaders.ContainsKey(typeof(T)))
			{
				return EndlessAssetCache.typeLoaders[typeof(T)].Item1(json) as T;
			}
			return JsonConvert.DeserializeObject<T>(json);
		}

		// Token: 0x060002C9 RID: 713 RVA: 0x0000EA5C File Offset: 0x0000CC5C
		public static T[] LoadAssetArrayFromJson<T>(string json) where T : Asset
		{
			if (EndlessAssetCache.typeLoaders.ContainsKey(typeof(T)))
			{
				return EndlessAssetCache.typeLoaders[typeof(T)].Item2(json) as T[];
			}
			return JsonConvert.DeserializeObject<T[]>(json);
		}

		// Token: 0x060002CA RID: 714 RVA: 0x0000EAAC File Offset: 0x0000CCAC
		public static async Task<AssetCacheResult<T>> GetAssetAsync<T>(SerializableGuid assetId, string versionId) where T : Asset
		{
			Type typeFromHandle = typeof(T);
			if (!EndlessAssetCache.assetTypeStrings.ContainsKey(typeFromHandle))
			{
				throw new ArgumentException(string.Format("Type {0} is unsupported!", typeFromHandle));
			}
			string text = EndlessAssetCache.assetTypeStrings[typeof(T)];
			if (string.IsNullOrEmpty(versionId))
			{
				throw new ArgumentException("versionId must be populated. Did you mean to use EndlessAssetCache.GetLatestAssetAsync");
			}
			AssetReference newReference = new AssetReference
			{
				AssetID = assetId,
				AssetVersion = versionId,
				AssetType = text
			};
			Dictionary<AssetReference, Asset> assetCache = EndlessAssetCache.GetCache(typeFromHandle);
			Asset asset;
			AssetCacheResult<T> assetCacheResult;
			if (assetCache.TryGetValue(newReference, out asset) && asset != null)
			{
				assetCacheResult = new AssetCacheResult<T>(null, (T)((object)asset));
			}
			else
			{
				if (assetCache.ContainsKey(newReference))
				{
					while (assetCache.ContainsKey(newReference) && assetCache[newReference] == null)
					{
						await Task.Yield();
					}
					Asset asset2;
					if (assetCache.TryGetValue(newReference, out asset2))
					{
						return new AssetCacheResult<T>(null, (T)((object)asset2));
					}
				}
				assetCache.Add(newReference, null);
				GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetAssetAsync(assetId, versionId, null, false, 10);
				if (graphQlResult.HasErrors)
				{
					assetCache.Remove(newReference);
					assetCacheResult = new AssetCacheResult<T>(graphQlResult, default(T));
				}
				else
				{
					T t = EndlessAssetCache.LoadAssetFromJson<T>(graphQlResult.GetDataMember().ToString());
					assetCache[newReference] = t;
					assetCacheResult = new AssetCacheResult<T>(graphQlResult, t);
				}
			}
			return assetCacheResult;
		}

		// Token: 0x060002CB RID: 715 RVA: 0x0000EAF8 File Offset: 0x0000CCF8
		public static async Task<BulkAssetCacheResult<T>> GetLatestBulkAssetsAsync<T>([TupleElementNames(new string[] { "AssetId", "Version" })] ValueTuple<SerializableGuid, string>[] assetIdentifiers) where T : Asset
		{
			Type typeFromHandle = typeof(T);
			if (!EndlessAssetCache.assetTypeStrings.ContainsKey(typeFromHandle))
			{
				throw new ArgumentException(string.Format("Type {0} is unsupported!", typeFromHandle));
			}
			return await EndlessAssetCache.ProcessBulkAssetRequest<T>(assetIdentifiers, typeFromHandle, null);
		}

		// Token: 0x060002CC RID: 716 RVA: 0x0000EB3C File Offset: 0x0000CD3C
		public static async Task<BulkAssetCacheResult<T>> GetBulkAssetsAsync<T>(IEnumerable<AssetReference> assetReferences) where T : Asset
		{
			return await EndlessAssetCache.GetBulkAssetsAsync<T>(assetReferences.Select((AssetReference entry) => new ValueTuple<SerializableGuid, string>(entry.AssetID, entry.AssetVersion)).ToArray<ValueTuple<SerializableGuid, string>>());
		}

		// Token: 0x060002CD RID: 717 RVA: 0x0000EB80 File Offset: 0x0000CD80
		public static async Task<BulkAssetCacheResult<T>> GetBulkAssetsAsync<T>([TupleElementNames(new string[] { "AssetId", "Version" })] ValueTuple<SerializableGuid, string>[] assetIdentifiers) where T : Asset
		{
			Type typeFromHandle = typeof(T);
			if (!EndlessAssetCache.assetTypeStrings.ContainsKey(typeFromHandle))
			{
				throw new ArgumentException(string.Format("Type {0} is unsupported!", typeFromHandle));
			}
			List<T> entriesAlreadyInCache = EndlessAssetCache.GetEntriesAlreadyInCache<T>(assetIdentifiers);
			BulkAssetCacheResult<T> bulkAssetCacheResult;
			if (entriesAlreadyInCache.Count == assetIdentifiers.Length)
			{
				bulkAssetCacheResult = new BulkAssetCacheResult<T>(null, entriesAlreadyInCache);
			}
			else
			{
				bulkAssetCacheResult = await EndlessAssetCache.ProcessBulkAssetRequest<T>(assetIdentifiers.Where(([TupleElementNames(new string[] { "AssetId", "Version" })] ValueTuple<SerializableGuid, string> identifier) => !entriesAlreadyInCache.Any((T entry) => entry.AssetID == identifier.Item1 && entry.AssetVersion == identifier.Item2)).ToArray<ValueTuple<SerializableGuid, string>>(), typeFromHandle, entriesAlreadyInCache);
			}
			return bulkAssetCacheResult;
		}

		// Token: 0x060002CE RID: 718 RVA: 0x0000EBC4 File Offset: 0x0000CDC4
		private static async Task<BulkAssetCacheResult<T>> ProcessBulkAssetRequest<T>([TupleElementNames(new string[] { "AssetId", "Version" })] ValueTuple<SerializableGuid, string>[] identifiersToQueryFor, Type assetType, List<T> entriesAlreadyInCache = null) where T : Asset
		{
			Dictionary<AssetReference, Asset> assetCache = EndlessAssetCache.GetCache(assetType);
			string assetStringType = EndlessAssetCache.assetTypeStrings[typeof(T)];
			foreach (ValueTuple<SerializableGuid, string> valueTuple in identifiersToQueryFor)
			{
				if (!string.IsNullOrEmpty(valueTuple.Item2))
				{
					assetCache.TryAdd(new AssetReference
					{
						AssetID = valueTuple.Item1,
						AssetVersion = valueTuple.Item2,
						AssetType = assetStringType
					}, null);
				}
			}
			GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetBulkAssets(identifiersToQueryFor, null, true);
			BulkAssetCacheResult<T> bulkAssetCacheResult;
			if (graphQlResult.HasErrors)
			{
				foreach (ValueTuple<SerializableGuid, string> valueTuple2 in identifiersToQueryFor)
				{
					if (!string.IsNullOrEmpty(valueTuple2.Item2))
					{
						AssetReference assetReference3 = new AssetReference();
						assetReference3.AssetID = valueTuple2.Item1;
						assetReference3.AssetVersion = valueTuple2.Item2;
						assetReference3.AssetType = assetStringType;
						if (assetCache[assetReference3] == null)
						{
							assetCache.Remove(assetReference3);
						}
					}
				}
				bulkAssetCacheResult = new BulkAssetCacheResult<T>(graphQlResult, entriesAlreadyInCache);
			}
			else
			{
				T[] newResults = EndlessAssetCache.LoadAssetArrayFromJson<T>(graphQlResult.GetDataMember().ToString());
				T[] newResults2 = newResults;
				int i = 0;
				while (i < newResults2.Length)
				{
					T t = newResults2[i];
					AssetReference assetReference = t.ToAssetReference();
					if (!EndlessAssetCache.assetTypeStrings.Values.Any((string value) => value == assetReference.AssetType))
					{
						assetReference.AssetType = assetStringType;
						goto IL_0293;
					}
					if (!(assetReference.AssetType != assetStringType))
					{
						goto IL_0293;
					}
					assetCache.Remove(new AssetReference
					{
						AssetID = t.AssetID,
						AssetVersion = t.AssetVersion,
						AssetType = assetStringType
					});
					IL_02AC:
					i++;
					continue;
					IL_0293:
					assetCache[assetReference] = t;
					goto IL_02AC;
				}
				foreach (ValueTuple<SerializableGuid, string> valueTuple3 in identifiersToQueryFor.Where(([TupleElementNames(new string[] { "AssetId", "Version" })] ValueTuple<SerializableGuid, string> identifier) => !newResults.Any((T result) => result.AssetID == identifier.Item1 && result.AssetVersion == identifier.Item2)))
				{
					if (!string.IsNullOrEmpty(valueTuple3.Item2))
					{
						AssetReference assetReference2 = new AssetReference();
						assetReference2.AssetID = valueTuple3.Item1;
						assetReference2.AssetVersion = valueTuple3.Item2;
						assetReference2.AssetType = assetStringType;
						if (assetCache[assetReference2] == null)
						{
							assetCache.Remove(assetReference2);
						}
					}
				}
				if (entriesAlreadyInCache != null)
				{
					bulkAssetCacheResult = new BulkAssetCacheResult<T>(graphQlResult, entriesAlreadyInCache.Concat(newResults).ToList<T>());
				}
				else
				{
					bulkAssetCacheResult = new BulkAssetCacheResult<T>(graphQlResult, newResults.ToList<T>());
				}
			}
			return bulkAssetCacheResult;
		}

		// Token: 0x060002CF RID: 719 RVA: 0x0000EC18 File Offset: 0x0000CE18
		private static List<T> GetEntriesAlreadyInCache<T>([TupleElementNames(new string[] { "AssetId", "Version" })] ValueTuple<SerializableGuid, string>[] assetIdentifiers) where T : Asset
		{
			Type typeFromHandle = typeof(T);
			if (!EndlessAssetCache.assetTypeStrings.ContainsKey(typeFromHandle))
			{
				throw new ArgumentException(string.Format("Type {0} is unsupported!", typeFromHandle));
			}
			List<T> list = new List<T>();
			string text = EndlessAssetCache.assetTypeStrings[typeof(T)];
			foreach (ValueTuple<SerializableGuid, string> valueTuple in assetIdentifiers)
			{
				if (!string.IsNullOrEmpty(valueTuple.Item2))
				{
					AssetReference assetReference = new AssetReference
					{
						AssetID = valueTuple.Item1,
						AssetVersion = valueTuple.Item2,
						AssetType = text
					};
					Asset asset;
					if (EndlessAssetCache.GetCache(typeFromHandle).TryGetValue(assetReference, out asset) && asset != null)
					{
						list.Add((T)((object)asset));
					}
				}
			}
			return list;
		}

		// Token: 0x060002D0 RID: 720 RVA: 0x0000ECE8 File Offset: 0x0000CEE8
		public static void AddNewVersionToCache<T>(T asset) where T : Asset
		{
			Type typeFromHandle = typeof(T);
			if (!EndlessAssetCache.assetTypeStrings.ContainsKey(typeFromHandle))
			{
				throw new ArgumentException(string.Format("Type {0} is unsupported!", typeFromHandle));
			}
			AssetReference assetReference = asset.ToAssetReference();
			EndlessAssetCache.GetCache(typeFromHandle).TryAdd(assetReference, asset);
		}

		// Token: 0x060002D1 RID: 721 RVA: 0x0000ED3D File Offset: 0x0000CF3D
		public static void Clear()
		{
			EndlessAssetCache.assetCaches.Clear();
		}

		// Token: 0x0400028D RID: 653
		private static Dictionary<Type, Dictionary<AssetReference, Asset>> assetCaches = new Dictionary<Type, Dictionary<AssetReference, Asset>>();

		// Token: 0x0400028E RID: 654
		private static Dictionary<Type, string> assetTypeStrings = new Dictionary<Type, string>
		{
			{
				typeof(Prop),
				"prop"
			},
			{
				typeof(Script),
				"script"
			},
			{
				typeof(EndlessPrefabAsset),
				"endless-prefab"
			},
			{
				typeof(TerrainTilesetCosmeticAsset),
				"terrain-tileset-cosmetic"
			},
			{
				typeof(ParticleSystemAsset),
				"particle-system"
			},
			{
				typeof(AudioAsset),
				"audio"
			}
		};

		// Token: 0x0400028F RID: 655
		private static Dictionary<string, Type> reverseTypeMap = new Dictionary<string, Type>
		{
			{
				"prop",
				typeof(Prop)
			},
			{
				"script",
				typeof(Script)
			},
			{
				"endless-prefab",
				typeof(EndlessPrefabAsset)
			},
			{
				"terrain-tileset-cosmetic",
				typeof(TerrainTilesetCosmeticAsset)
			},
			{
				"particle-system",
				typeof(ParticleSystemAsset)
			}
		};

		// Token: 0x04000290 RID: 656
		[TupleElementNames(new string[] { "singleAsset", "bulkAsset" })]
		private static Dictionary<Type, ValueTuple<Func<string, Asset>, Func<string, Asset[]>>> typeLoaders = new Dictionary<Type, ValueTuple<Func<string, Asset>, Func<string, Asset[]>>>
		{
			{
				typeof(Script),
				new ValueTuple<Func<string, Asset>, Func<string, Asset[]>>(new Func<string, Asset>(ScriptLoader.LoadFromJson), new Func<string, Asset[]>(ScriptLoader.LoadArrayFromJson))
			},
			{
				typeof(Prop),
				new ValueTuple<Func<string, Asset>, Func<string, Asset[]>>(new Func<string, Asset>(PropLoader.LoadFromJson), new Func<string, Asset[]>(PropLoader.LoadArrayFromJson))
			}
		};
	}
}
