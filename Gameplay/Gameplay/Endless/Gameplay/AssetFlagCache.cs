using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Endless.Gameplay.LevelEditing.Tilesets;
using Endless.GraphQl;
using Endless.Matchmaking;
using Endless.Shared.DataTypes;
using Newtonsoft.Json;
using Runtime.Shared.Matchmaking;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000061 RID: 97
	public static class AssetFlagCache
	{
		// Token: 0x0600018E RID: 398 RVA: 0x00009CC6 File Offset: 0x00007EC6
		public static void Clear()
		{
			AssetFlagCache.assetToContentFlagMap.Clear();
		}

		// Token: 0x0600018F RID: 399 RVA: 0x00009CD4 File Offset: 0x00007ED4
		public static async Task<List<ModerationFlag>> GetAssetFlagsForAsset(SerializableGuid assetId, string assetVersion)
		{
			List<ModerationFlag> list;
			List<ModerationFlag> list2;
			if (AssetFlagCache.assetToContentFlagMap.TryGetValue(assetId, out list))
			{
				list2 = list;
			}
			else
			{
				GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetAssetObjectFlags(assetId, false);
				if (graphQlResult.HasErrors)
				{
					throw graphQlResult.GetErrorMessage(0);
				}
				ObjectFlagResult objectFlagResult = JsonConvert.DeserializeObject<ObjectFlagResult>(graphQlResult.GetDataMember().ToString());
				AssetFlagCache.assetToContentFlagMap.Add(assetId, objectFlagResult.Moderations.Select((Moderation x) => x.flag).ToList<ModerationFlag>());
				list2 = AssetFlagCache.assetToContentFlagMap[assetId];
			}
			return list2;
		}

		// Token: 0x06000190 RID: 400 RVA: 0x00009D18 File Offset: 0x00007F18
		public static async Task GetBulkAssetFlagsForAsset([TupleElementNames(new string[] { "assetId", "assetVersion" })] IEnumerable<ValueTuple<SerializableGuid, string>> assets)
		{
			List<ValueTuple<SerializableGuid, string>> list = new List<ValueTuple<SerializableGuid, string>>();
			for (int i = 0; i < assets.Count<ValueTuple<SerializableGuid, string>>(); i++)
			{
				if (!AssetFlagCache.assetToContentFlagMap.ContainsKey(assets.ElementAt(i).Item1))
				{
					list.Add(assets.ElementAt(i));
				}
			}
			GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetBulkObjectFlags(list.Select(([TupleElementNames(new string[] { "assetId", "assetVersion" })] ValueTuple<SerializableGuid, string> tuple) => tuple.Item1.ToString()).ToArray<string>(), true);
			if (graphQlResult.HasErrors)
			{
				throw graphQlResult.GetErrorMessage(0);
			}
			var <>f__AnonymousType = new
			{
				results = Array.Empty<ObjectFlagResult>()
			};
			foreach (ObjectFlagResult objectFlagResult in JsonConvert.DeserializeAnonymousType(graphQlResult.GetDataMember().ToString(), <>f__AnonymousType).results)
			{
				AssetFlagCache.assetToContentFlagMap.TryAdd(objectFlagResult.Identifier, objectFlagResult.Moderations.Select((Moderation y) => y.flag).ToList<ModerationFlag>());
			}
		}

		// Token: 0x06000191 RID: 401 RVA: 0x00009D5C File Offset: 0x00007F5C
		public static async Task<bool> IsAssetAllowed(SerializableGuid assetId, string assetVersion)
		{
			if (!AssetFlagCache.assetToContentFlagMap.ContainsKey(assetId))
			{
				await AssetFlagCache.GetAssetFlagsForAsset(assetId, assetVersion);
			}
			List<ModerationFlag> list;
			bool flag;
			if (AssetFlagCache.assetToContentFlagMap.TryGetValue(assetId, out list))
			{
				flag = !AssetFlagCache.HaveAnyMatchingTags(EndlessCloudService.ContentRestrictionsOnAccount, list);
			}
			else
			{
				flag = false;
			}
			return flag;
		}

		// Token: 0x06000192 RID: 402 RVA: 0x00009DA8 File Offset: 0x00007FA8
		private static bool HaveAnyMatchingTags(List<ModerationFlag> flagsGroupOne, List<ModerationFlag> flagsGroupTwo)
		{
			return flagsGroupOne.Any((ModerationFlag flag) => flagsGroupTwo.Any((ModerationFlag sFlag) => sFlag.Id == flag.Id));
		}

		// Token: 0x06000193 RID: 403 RVA: 0x00009DD4 File Offset: 0x00007FD4
		public static async Task<List<ModerationFlag>> GetFlagsForAsset(SerializableGuid assetId, string assetVersion)
		{
			if (!AssetFlagCache.assetToContentFlagMap.ContainsKey(assetId))
			{
				await AssetFlagCache.GetAssetFlagsForAsset(assetId, assetVersion);
			}
			return AssetFlagCache.assetToContentFlagMap[assetId] ?? new List<ModerationFlag>();
		}

		// Token: 0x06000194 RID: 404 RVA: 0x00009E20 File Offset: 0x00008020
		public static List<ModerationFlag> GetMatchingFlags(List<ModerationFlag> flagsGroupOne, List<ModerationFlag> flagsGroupTwo)
		{
			return flagsGroupOne.Where((ModerationFlag flag) => flagsGroupTwo.Exists((ModerationFlag flag2) => flag2.Id == flag.Id)).ToList<ModerationFlag>();
		}

		// Token: 0x06000195 RID: 405 RVA: 0x00009E54 File Offset: 0x00008054
		public static async Task<bool> AreAssetsAllowed(IEnumerable<AssetIdVersionKey> assetList)
		{
			List<ValueTuple<SerializableGuid, string>> assetVersionPairsToQuery = new List<ValueTuple<SerializableGuid, string>>();
			foreach (AssetIdVersionKey assetIdVersionKey in assetList)
			{
				List<ModerationFlag> list;
				if (AssetFlagCache.assetToContentFlagMap.TryGetValue(assetIdVersionKey.AssetId, out list))
				{
					if (AssetFlagCache.HaveAnyMatchingTags(EndlessCloudService.ContentRestrictionsOnAccount, list))
					{
						return false;
					}
				}
				else
				{
					assetVersionPairsToQuery.Add(new ValueTuple<SerializableGuid, string>(assetIdVersionKey.AssetId, assetIdVersionKey.Version));
				}
			}
			bool flag2;
			try
			{
				await AssetFlagCache.GetBulkAssetFlagsForAsset(assetVersionPairsToQuery);
				bool flag = true;
				int num = 0;
				while (flag && num < assetVersionPairsToQuery.Count)
				{
					SerializableGuid item = assetVersionPairsToQuery[num].Item1;
					List<ModerationFlag> list2;
					if (AssetFlagCache.assetToContentFlagMap.TryGetValue(item, out list2) && AssetFlagCache.HaveAnyMatchingTags(EndlessCloudService.ContentRestrictionsOnAccount, list2))
					{
						flag = false;
					}
					num++;
				}
				flag2 = flag;
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
				flag2 = false;
			}
			return flag2;
		}

		// Token: 0x04000172 RID: 370
		private static Dictionary<SerializableGuid, List<ModerationFlag>> assetToContentFlagMap = new Dictionary<SerializableGuid, List<ModerationFlag>>();
	}
}
