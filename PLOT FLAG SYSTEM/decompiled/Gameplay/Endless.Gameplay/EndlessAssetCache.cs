using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Endless.Gameplay;

public static class EndlessAssetCache
{
	private static Dictionary<Type, Dictionary<AssetReference, Asset>> assetCaches = new Dictionary<Type, Dictionary<AssetReference, Asset>>();

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

	private static Dictionary<Type, (Func<string, Asset> singleAsset, Func<string, Asset[]> bulkAsset)> typeLoaders = new Dictionary<Type, (Func<string, Asset>, Func<string, Asset[]>)>
	{
		{
			typeof(Script),
			(ScriptLoader.LoadFromJson, ScriptLoader.LoadArrayFromJson)
		},
		{
			typeof(Prop),
			(PropLoader.LoadFromJson, PropLoader.LoadArrayFromJson)
		}
	};

	private static Dictionary<AssetReference, Asset> GetCache(Type type)
	{
		if (!assetCaches.ContainsKey(type))
		{
			assetCaches.Add(type, new Dictionary<AssetReference, Asset>());
		}
		return assetCaches[type];
	}

	public static async Task<AssetCacheResult<T>> GetLatestAssetAsync<T>(SerializableGuid assetId) where T : Asset
	{
		Type assetType = typeof(T);
		if (!assetTypeStrings.ContainsKey(assetType))
		{
			throw new ArgumentException($"Type {assetType} is unsupported!");
		}
		GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetAssetAsync(assetId);
		if (graphQlResult.HasErrors)
		{
			return new AssetCacheResult<T>(graphQlResult, null);
		}
		T val = LoadAssetFromJson<T>(graphQlResult.GetDataMember().ToString());
		AssetReference key = new AssetReference
		{
			AssetID = val.AssetID,
			AssetType = val.AssetType,
			AssetVersion = val.AssetVersion
		};
		GetCache(assetType).TryAdd(key, val);
		return new AssetCacheResult<T>(graphQlResult, val);
	}

	public static T LoadAssetFromJson<T>(string json) where T : Asset
	{
		if (typeLoaders.ContainsKey(typeof(T)))
		{
			return typeLoaders[typeof(T)].singleAsset(json) as T;
		}
		return JsonConvert.DeserializeObject<T>(json);
	}

	public static T[] LoadAssetArrayFromJson<T>(string json) where T : Asset
	{
		if (typeLoaders.ContainsKey(typeof(T)))
		{
			return typeLoaders[typeof(T)].bulkAsset(json) as T[];
		}
		return JsonConvert.DeserializeObject<T[]>(json);
	}

	public static async Task<AssetCacheResult<T>> GetAssetAsync<T>(SerializableGuid assetId, string versionId) where T : Asset
	{
		Type typeFromHandle = typeof(T);
		if (!assetTypeStrings.ContainsKey(typeFromHandle))
		{
			throw new ArgumentException($"Type {typeFromHandle} is unsupported!");
		}
		string assetType = assetTypeStrings[typeof(T)];
		if (string.IsNullOrEmpty(versionId))
		{
			throw new ArgumentException("versionId must be populated. Did you mean to use EndlessAssetCache.GetLatestAssetAsync");
		}
		AssetReference newReference = new AssetReference
		{
			AssetID = assetId,
			AssetVersion = versionId,
			AssetType = assetType
		};
		Dictionary<AssetReference, Asset> assetCache = GetCache(typeFromHandle);
		if (assetCache.TryGetValue(newReference, out var value) && value != null)
		{
			return new AssetCacheResult<T>(null, (T)value);
		}
		if (assetCache.ContainsKey(newReference))
		{
			while (assetCache.ContainsKey(newReference) && assetCache[newReference] == null)
			{
				await Task.Yield();
			}
			if (assetCache.TryGetValue(newReference, out var value2))
			{
				return new AssetCacheResult<T>(null, (T)value2);
			}
		}
		assetCache.Add(newReference, null);
		GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetAssetAsync(assetId, versionId);
		if (graphQlResult.HasErrors)
		{
			assetCache.Remove(newReference);
			return new AssetCacheResult<T>(graphQlResult, null);
		}
		T asset = (T)(assetCache[newReference] = LoadAssetFromJson<T>(graphQlResult.GetDataMember().ToString()));
		return new AssetCacheResult<T>(graphQlResult, asset);
	}

	public static async Task<BulkAssetCacheResult<T>> GetLatestBulkAssetsAsync<T>((SerializableGuid AssetId, string Version)[] assetIdentifiers) where T : Asset
	{
		Type typeFromHandle = typeof(T);
		if (!assetTypeStrings.ContainsKey(typeFromHandle))
		{
			throw new ArgumentException($"Type {typeFromHandle} is unsupported!");
		}
		return await ProcessBulkAssetRequest<T>(assetIdentifiers, typeFromHandle);
	}

	public static async Task<BulkAssetCacheResult<T>> GetBulkAssetsAsync<T>(IEnumerable<AssetReference> assetReferences) where T : Asset
	{
		return await GetBulkAssetsAsync<T>(assetReferences.Select((AssetReference entry) => ((SerializableGuid, string AssetVersion))(entry.AssetID, AssetVersion: entry.AssetVersion)).ToArray());
	}

	public static async Task<BulkAssetCacheResult<T>> GetBulkAssetsAsync<T>((SerializableGuid AssetId, string Version)[] assetIdentifiers) where T : Asset
	{
		Type typeFromHandle = typeof(T);
		if (!assetTypeStrings.ContainsKey(typeFromHandle))
		{
			throw new ArgumentException($"Type {typeFromHandle} is unsupported!");
		}
		List<T> entriesAlreadyInCache = GetEntriesAlreadyInCache<T>(assetIdentifiers);
		if (entriesAlreadyInCache.Count == assetIdentifiers.Length)
		{
			return new BulkAssetCacheResult<T>(null, entriesAlreadyInCache);
		}
		return await ProcessBulkAssetRequest(assetIdentifiers.Where(((SerializableGuid AssetId, string Version) identifier) => !entriesAlreadyInCache.Any((T entry) => (SerializableGuid)entry.AssetID == identifier.AssetId && entry.AssetVersion == identifier.Version)).ToArray(), typeFromHandle, entriesAlreadyInCache);
	}

	private static async Task<BulkAssetCacheResult<T>> ProcessBulkAssetRequest<T>((SerializableGuid AssetId, string Version)[] identifiersToQueryFor, Type assetType, List<T> entriesAlreadyInCache = null) where T : Asset
	{
		Dictionary<AssetReference, Asset> assetCache = GetCache(assetType);
		string assetStringType = assetTypeStrings[typeof(T)];
		(SerializableGuid, string)[] array = identifiersToQueryFor;
		for (int i = 0; i < array.Length; i++)
		{
			(SerializableGuid, string) tuple = array[i];
			if (!string.IsNullOrEmpty(tuple.Item2))
			{
				assetCache.TryAdd(new AssetReference
				{
					AssetID = tuple.Item1,
					AssetVersion = tuple.Item2,
					AssetType = assetStringType
				}, null);
			}
		}
		GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetBulkAssets(identifiersToQueryFor);
		if (graphQlResult.HasErrors)
		{
			array = identifiersToQueryFor;
			for (int i = 0; i < array.Length; i++)
			{
				(SerializableGuid, string) tuple2 = array[i];
				if (!string.IsNullOrEmpty(tuple2.Item2))
				{
					AssetReference key = new AssetReference
					{
						AssetID = tuple2.Item1,
						AssetVersion = tuple2.Item2,
						AssetType = assetStringType
					};
					if (assetCache[key] == null)
					{
						assetCache.Remove(key);
					}
				}
			}
			return new BulkAssetCacheResult<T>(graphQlResult, entriesAlreadyInCache);
		}
		T[] newResults = LoadAssetArrayFromJson<T>(graphQlResult.GetDataMember().ToString());
		T[] array2 = newResults;
		foreach (T val in array2)
		{
			AssetReference assetReference = val.ToAssetReference();
			if (assetTypeStrings.Values.Any((string value) => value == assetReference.AssetType))
			{
				if (assetReference.AssetType != assetStringType)
				{
					assetCache.Remove(new AssetReference
					{
						AssetID = val.AssetID,
						AssetVersion = val.AssetVersion,
						AssetType = assetStringType
					});
					continue;
				}
			}
			else
			{
				assetReference.AssetType = assetStringType;
			}
			assetCache[assetReference] = val;
		}
		foreach (var item in identifiersToQueryFor.Where(((SerializableGuid AssetId, string Version) identifier) => !newResults.Any((T result) => (SerializableGuid)result.AssetID == identifier.AssetId && result.AssetVersion == identifier.Version)))
		{
			if (!string.IsNullOrEmpty(item.Version))
			{
				AssetReference key2 = new AssetReference
				{
					AssetID = item.AssetId,
					AssetVersion = item.Version,
					AssetType = assetStringType
				};
				if (assetCache[key2] == null)
				{
					assetCache.Remove(key2);
				}
			}
		}
		if (entriesAlreadyInCache != null)
		{
			return new BulkAssetCacheResult<T>(graphQlResult, entriesAlreadyInCache.Concat(newResults).ToList());
		}
		return new BulkAssetCacheResult<T>(graphQlResult, newResults.ToList());
	}

	private static List<T> GetEntriesAlreadyInCache<T>((SerializableGuid AssetId, string Version)[] assetIdentifiers) where T : Asset
	{
		Type typeFromHandle = typeof(T);
		if (!assetTypeStrings.ContainsKey(typeFromHandle))
		{
			throw new ArgumentException($"Type {typeFromHandle} is unsupported!");
		}
		List<T> list = new List<T>();
		string assetType = assetTypeStrings[typeof(T)];
		for (int i = 0; i < assetIdentifiers.Length; i++)
		{
			(SerializableGuid, string) tuple = assetIdentifiers[i];
			if (!string.IsNullOrEmpty(tuple.Item2))
			{
				AssetReference key = new AssetReference
				{
					AssetID = tuple.Item1,
					AssetVersion = tuple.Item2,
					AssetType = assetType
				};
				if (GetCache(typeFromHandle).TryGetValue(key, out var value) && value != null)
				{
					list.Add((T)value);
				}
			}
		}
		return list;
	}

	public static void AddNewVersionToCache<T>(T asset) where T : Asset
	{
		Type typeFromHandle = typeof(T);
		if (!assetTypeStrings.ContainsKey(typeFromHandle))
		{
			throw new ArgumentException($"Type {typeFromHandle} is unsupported!");
		}
		AssetReference key = asset.ToAssetReference();
		GetCache(typeFromHandle).TryAdd(key, asset);
	}

	public static void Clear()
	{
		assetCaches.Clear();
	}
}
