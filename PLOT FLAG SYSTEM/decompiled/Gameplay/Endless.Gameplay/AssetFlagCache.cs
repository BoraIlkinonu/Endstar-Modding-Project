using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Endless.Gameplay.LevelEditing.Tilesets;
using Endless.GraphQl;
using Endless.Matchmaking;
using Endless.Shared.DataTypes;
using Newtonsoft.Json;
using Runtime.Shared.Matchmaking;
using UnityEngine;

namespace Endless.Gameplay;

public static class AssetFlagCache
{
	private static Dictionary<SerializableGuid, List<ModerationFlag>> assetToContentFlagMap = new Dictionary<SerializableGuid, List<ModerationFlag>>();

	public static void Clear()
	{
		assetToContentFlagMap.Clear();
	}

	public static async Task<List<ModerationFlag>> GetAssetFlagsForAsset(SerializableGuid assetId, string assetVersion)
	{
		if (assetToContentFlagMap.TryGetValue(assetId, out var value))
		{
			return value;
		}
		GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetAssetObjectFlags(assetId);
		if (graphQlResult.HasErrors)
		{
			throw graphQlResult.GetErrorMessage();
		}
		ObjectFlagResult objectFlagResult = JsonConvert.DeserializeObject<ObjectFlagResult>(graphQlResult.GetDataMember().ToString());
		assetToContentFlagMap.Add(assetId, objectFlagResult.Moderations.Select((Moderation x) => x.flag).ToList());
		return assetToContentFlagMap[assetId];
	}

	public static async Task GetBulkAssetFlagsForAsset(IEnumerable<(SerializableGuid assetId, string assetVersion)> assets)
	{
		List<(SerializableGuid, string)> list = new List<(SerializableGuid, string)>();
		for (int i = 0; i < assets.Count(); i++)
		{
			if (!assetToContentFlagMap.ContainsKey(assets.ElementAt(i).assetId))
			{
				list.Add(assets.ElementAt(i));
			}
		}
		GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetBulkObjectFlags(list.Select<(SerializableGuid, string), string>(((SerializableGuid assetId, string assetVersion) tuple) => tuple.assetId.ToString()).ToArray(), debugQuery: true);
		if (graphQlResult.HasErrors)
		{
			throw graphQlResult.GetErrorMessage();
		}
		var anonymousTypeObject = new
		{
			results = Array.Empty<ObjectFlagResult>()
		};
		ObjectFlagResult[] results = JsonConvert.DeserializeAnonymousType(graphQlResult.GetDataMember().ToString(), anonymousTypeObject).results;
		for (int num = 0; num < results.Length; num++)
		{
			ObjectFlagResult objectFlagResult = results[num];
			assetToContentFlagMap.TryAdd(objectFlagResult.Identifier, objectFlagResult.Moderations.Select((Moderation y) => y.flag).ToList());
		}
	}

	public static async Task<bool> IsAssetAllowed(SerializableGuid assetId, string assetVersion)
	{
		if (!assetToContentFlagMap.ContainsKey(assetId))
		{
			await GetAssetFlagsForAsset(assetId, assetVersion);
		}
		if (assetToContentFlagMap.TryGetValue(assetId, out var value))
		{
			return !HaveAnyMatchingTags(EndlessCloudService.ContentRestrictionsOnAccount, value);
		}
		return false;
	}

	private static bool HaveAnyMatchingTags(List<ModerationFlag> flagsGroupOne, List<ModerationFlag> flagsGroupTwo)
	{
		return flagsGroupOne.Any((ModerationFlag flag) => flagsGroupTwo.Any((ModerationFlag sFlag) => sFlag.Id == flag.Id));
	}

	public static async Task<List<ModerationFlag>> GetFlagsForAsset(SerializableGuid assetId, string assetVersion)
	{
		if (!assetToContentFlagMap.ContainsKey(assetId))
		{
			await GetAssetFlagsForAsset(assetId, assetVersion);
		}
		return assetToContentFlagMap[assetId] ?? new List<ModerationFlag>();
	}

	public static List<ModerationFlag> GetMatchingFlags(List<ModerationFlag> flagsGroupOne, List<ModerationFlag> flagsGroupTwo)
	{
		return flagsGroupOne.Where((ModerationFlag flag) => flagsGroupTwo.Exists((ModerationFlag moderationFlag) => moderationFlag.Id == flag.Id)).ToList();
	}

	public static async Task<bool> AreAssetsAllowed(IEnumerable<AssetIdVersionKey> assetList)
	{
		List<(SerializableGuid, string)> assetVersionPairsToQuery = new List<(SerializableGuid, string)>();
		foreach (AssetIdVersionKey asset in assetList)
		{
			if (assetToContentFlagMap.TryGetValue(asset.AssetId, out var value))
			{
				if (HaveAnyMatchingTags(EndlessCloudService.ContentRestrictionsOnAccount, value))
				{
					return false;
				}
			}
			else
			{
				assetVersionPairsToQuery.Add((asset.AssetId, asset.Version));
			}
		}
		try
		{
			await GetBulkAssetFlagsForAsset(assetVersionPairsToQuery);
			bool flag = true;
			int num = 0;
			while (flag && num < assetVersionPairsToQuery.Count)
			{
				SerializableGuid item = assetVersionPairsToQuery[num].Item1;
				if (assetToContentFlagMap.TryGetValue(item, out var value2) && HaveAnyMatchingTags(EndlessCloudService.ContentRestrictionsOnAccount, value2))
				{
					flag = false;
				}
				num++;
			}
			return flag;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return false;
		}
	}
}
