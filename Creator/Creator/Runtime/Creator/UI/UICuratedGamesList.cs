using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Endless.Assets;
using Endless.Creator.UI;
using Endless.Gameplay.LevelEditing.Level;
using Endless.GraphQl;
using Endless.Matchmaking;
using Endless.Shared.DataTypes;
using Runtime.Shared.Matchmaking;
using UnityEngine;

namespace Runtime.Creator.UI
{
	// Token: 0x0200000B RID: 11
	public class UICuratedGamesList : UIAssetList<CuratedGamesList, MainMenuGameModel>
	{
		// Token: 0x06000029 RID: 41 RVA: 0x00002900 File Offset: 0x00000B00
		protected override async Task<List<MainMenuGameModel>> ConvertToUiModelsAsync([TupleElementNames(new string[] { "asset", "versions" })] List<ValueTuple<AssetCore, List<PublishedVersion>>> result)
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
			List<MainMenuGameModel> list2;
			if (list.Count == 0)
			{
				list2 = new List<MainMenuGameModel>();
			}
			else
			{
				try
				{
					AssetParams assetParams = new AssetParams(null, false, MainMenuGameModel.AssetReturnArgs);
					GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetBulkAssets(list.ToArray(), assetParams, base.VerboseLogging);
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
					MainMenuGameModel[] dataMember = graphQlResult.GetDataMember<MainMenuGameModel[]>();
					if (dataMember == null)
					{
						Debug.LogError("models is null!", this);
						list2 = new List<MainMenuGameModel>();
					}
					else
					{
						list2 = dataMember.Where((MainMenuGameModel m) => m != null).ToList<MainMenuGameModel>();
					}
				}
				catch (Exception ex)
				{
					Debug.LogException(ex, this);
					list2 = new List<MainMenuGameModel>();
				}
			}
			return list2;
		}
	}
}
