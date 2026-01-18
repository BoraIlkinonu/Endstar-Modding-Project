using System.Threading.Tasks;
using Endless.Creator.UI;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing.Level;
using Endless.GraphQl;
using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Runtime.Shared.Matchmaking;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Creator.LevelEditing.Runtime;

public class SaveLoadManager
{
	private const float MINIMUM_DELAY_IN_SECONDS_BEFORE_SAVE = 60f;

	private bool pendingLevelSave;

	private bool saveInProgress;

	private float initialSaveTimeRequest;

	private float lastSaveTimeRequest;

	public SerializableGuid CachedLevelStateId { get; private set; }

	public string CachedLevelStateVersion { get; private set; }

	public bool PendingSave => pendingLevelSave;

	public bool SaveInProgress => saveInProgress;

	public void SetCachedLevelState(string levelId, string assetVersion)
	{
		Stage activeStage = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage;
		if ((bool)activeStage && activeStage.LevelState.AssetID == levelId)
		{
			activeStage.UpdateVersion(assetVersion);
			NetworkBehaviourSingleton<UISaveStatusManager>.Instance.UpdateLevelVersion(assetVersion, PendingSave);
		}
		CachedLevelStateId = levelId;
		CachedLevelStateVersion = assetVersion;
	}

	public void UpdateSaveLoad()
	{
		if (NetworkManager.Singleton.IsServer)
		{
			float num = Time.realtimeSinceStartup - lastSaveTimeRequest;
			float num2 = Time.realtimeSinceStartup - initialSaveTimeRequest;
			if (pendingLevelSave && !saveInProgress && (num > 60f || num2 > 300f))
			{
				pendingLevelSave = false;
				SubmitSave();
			}
		}
	}

	private async void SubmitSave()
	{
		await SubmitSaveAsync();
	}

	private async Task SubmitSaveAsync()
	{
		if (!MonoBehaviourSingleton<StageManager>.Instance || !MonoBehaviourSingleton<StageManager>.Instance.ActiveStage || MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState == null)
		{
			return;
		}
		saveInProgress = true;
		LevelState levelState = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.Copy();
		MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.Changes.Clear();
		levelState.AssetVersion = string.Empty;
		if (MatchmakingClientController.Instance != null)
		{
			NetworkBehaviourSingleton<UISaveStatusManager>.Instance.SetSaveIndicator(newValue: true);
			GraphQlResult response = await EndlessServices.Instance.CloudService.UpdateAssetAsync(levelState, MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.AssetID, useThreadForInputSerialization: true);
			if (!response.HasErrors)
			{
				var (levelId, assetVersion) = await Task.Run(delegate
				{
					LevelState levelState2 = LevelStateLoader.Load(response.GetDataMember().ToString());
					return (AssetID: levelState2.AssetID, AssetVersion: levelState2.AssetVersion);
				});
				SetCachedLevelState(levelId, assetVersion);
			}
			else
			{
				Debug.LogException(response.GetErrorMessage());
			}
		}
		saveInProgress = false;
		NetworkBehaviourSingleton<UISaveStatusManager>.Instance.SetSaveIndicator(newValue: false);
	}

	public void SaveLevel()
	{
		if (!pendingLevelSave)
		{
			initialSaveTimeRequest = Time.realtimeSinceStartup;
			lastSaveTimeRequest = Time.realtimeSinceStartup;
			pendingLevelSave = true;
			NetworkBehaviourSingleton<UISaveStatusManager>.Instance.UpdateLevelVersion(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.AssetVersion, isVersionDirty: true);
			Debug.Log("Setting to save a level!");
		}
		else
		{
			lastSaveTimeRequest = Time.realtimeSinceStartup;
		}
	}

	public async void ForceSaveIfNeeded()
	{
		await ForceSaveIfNeededAsync();
	}

	public async Task ForceSaveIfNeededAsync()
	{
		if (pendingLevelSave)
		{
			pendingLevelSave = false;
			Debug.Log("Forcing Save!");
			await SubmitSaveAsync();
		}
	}
}
