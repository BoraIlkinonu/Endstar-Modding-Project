using System.Collections.Generic;
using Endless.Data;
using Endless.Gameplay.LevelEditing.Level;
using Endless.GraphQl;
using Endless.Matchmaking;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Newtonsoft.Json;
using Runtime.Shared.Matchmaking;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI;

public class UILevelStateSelectionModalView : UIEscapableModalView, IUILoadingSpinnerViewCompatible
{
	[Header("UILevelStateSelectionModalView")]
	[SerializeField]
	private UILevelAssetListModel levelStateListModel;

	private MainMenuGameModel mainMenuGameModel;

	private bool openLevelInAdminMode;

	public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

	public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

	public override void OnDespawn()
	{
		base.OnDespawn();
		levelStateListModel.Clear(triggerEvents: true);
	}

	public override void OnDisplay(params object[] modalData)
	{
		base.OnDisplay(modalData);
		mainMenuGameModel = modalData[0] as MainMenuGameModel;
		openLevelInAdminMode = (bool)modalData[1];
		if (base.VerboseLogging)
		{
			DebugUtility.Log(JsonUtility.ToJson(mainMenuGameModel), this);
		}
		levelStateListModel.SetGame(mainMenuGameModel);
		levelStateListModel.Set(new List<LevelAsset>(), triggerEvents: true);
		levelStateListModel.SetOpenLevelInAdminMode(openLevelInAdminMode);
		FetchLevels();
	}

	private async void FetchLevels()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "FetchLevels");
		}
		OnLoadingStarted.Invoke();
		SerializableGuid serializableGuid = mainMenuGameModel.AssetID;
		string empty = string.Empty;
		AssetParams assetParams = new AssetParams(LevelAsset.QueryString, populateRefs: true);
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0}: {1}", "assetId", serializableGuid), this);
			DebugUtility.Log("version: " + empty, this);
			DebugUtility.Log(string.Format("{0}: {1}", "assetParams", assetParams), this);
		}
		GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetAssetAsync(serializableGuid, empty, assetParams);
		if (graphQlResult.HasErrors)
		{
			ErrorHandler.HandleError(ErrorCodes.LevelStateSelectionModalView_FetchLevelsFailure, graphQlResult.GetErrorMessage());
			return;
		}
		var anonymousTypeObject = new
		{
			levels = new List<LevelAsset>()
		};
		var anon = JsonConvert.DeserializeAnonymousType(graphQlResult.GetDataMember().ToString(), anonymousTypeObject);
		levelStateListModel.Set(anon.levels, triggerEvents: true);
		OnLoadingEnded.Invoke();
	}
}
