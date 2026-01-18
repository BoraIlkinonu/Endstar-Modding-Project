using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Endless.Assets;
using Endless.Creator.UI;
using Endless.Gameplay.LevelEditing.Level;
using Endless.GraphQl;
using Endless.Matchmaking;
using Endless.Shared.DataTypes;
using Runtime.Shared.Matchmaking;
using UnityEngine;

namespace Runtime.Creator.UI;

public class UICuratedGamesList : UIAssetList<CuratedGamesList, MainMenuGameModel>
{
	protected override async Task<List<MainMenuGameModel>> ConvertToUiModelsAsync(List<(AssetCore asset, List<PublishedVersion> versions)> result)
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
			return new List<MainMenuGameModel>();
		}
		try
		{
			AssetParams assetParams = new AssetParams(null, populateRefs: false, MainMenuGameModel.AssetReturnArgs);
			GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetBulkAssets(list.ToArray(), assetParams, base.VerboseLogging);
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
			MainMenuGameModel[] dataMember = graphQlResult.GetDataMember<MainMenuGameModel[]>();
			if (dataMember == null)
			{
				Debug.LogError("models is null!", this);
				return new List<MainMenuGameModel>();
			}
			return dataMember.Where((MainMenuGameModel m) => m != null).ToList();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception, this);
			return new List<MainMenuGameModel>();
		}
	}
}
