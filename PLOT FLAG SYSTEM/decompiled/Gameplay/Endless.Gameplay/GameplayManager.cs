using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Endless.Assets;
using Endless.Data;
using Endless.Gameplay.LevelEditing.Level;
using Endless.GraphQl;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Runtime.Shared.Matchmaking;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay;

public class GameplayManager : MonoBehaviourSingleton<GameplayManager>
{
	public UnityEvent OnGameplayStarted = new UnityEvent();

	public UnityEvent OnGameplayStopped = new UnityEvent();

	public UnityEvent OnGameplayCleanup = new UnityEvent();

	public UnityEvent<SerializableGuid> OnLevelLoaded = new UnityEvent<SerializableGuid>();

	public UnityEvent<BlockTokenCollection> OnGameplayLoadingStateReadyToEnd = new UnityEvent<BlockTokenCollection>();

	[SerializeField]
	private TextAsset levelFile;

	public Action<SerializableGuid> RequestLevelChange;

	private bool pendingLevelSave;

	private bool saveInProgress;

	private float initialSaveTimeRequest;

	private float lastSaveTimeRequest;

	public List<SerializableGuid> AvailableSpawnPoints { get; set; }

	public bool IsPlaying { get; private set; }

	public static Collider[] TemporaryColliderArray { get; } = new Collider[30];

	public static HashSet<HittableComponent> TemporaryHittableComponentsHashset { get; } = new HashSet<HittableComponent>();

	public static HashSet<IPhysicsTaker> TemporaryPhysicsTakerHashset { get; } = new HashSet<IPhysicsTaker>();

	private void Start()
	{
		MonoBehaviourSingleton<StageManager>.Instance.OnLevelLoaded.AddListener(HandleLevelLoaded);
	}

	public void ChangeLevel(LevelDestination destination)
	{
		if (destination.IsValidLevel() && RequestLevelChange != null)
		{
			AvailableSpawnPoints = destination.TargetSpawnPointIds;
			RequestLevelChange(destination.TargetLevelId);
		}
	}

	public async Task<bool> LoadLevel(SerializableGuid levelId, CancellationToken cancelToken, string versionNumber = "", Action<string> progressCallback = null)
	{
		Debug.Log($"GameplayManager.LoadLevel loading LevelId: {levelId}");
		await Task.Yield();
		progressCallback?.Invoke("Fetching latest data...");
		if (!MonoBehaviourSingleton<StageManager>.Instance.TryGetCachedLevel(levelId, versionNumber, out var levelState))
		{
			GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetAssetAsync(levelId, versionNumber, null, debugQuery: false, 60);
			if (graphQlResult.HasErrors)
			{
				Exception errorMessage = graphQlResult.GetErrorMessage();
				ErrorHandler.HandleError((errorMessage is TimeoutException) ? ErrorCodes.GameplayManager_LevelFetchTimeout : ErrorCodes.GameplayManager_GetLevelAsset, errorMessage, displayModal: true, leaveMatch: true);
				return false;
			}
			string text = graphQlResult.GetDataMember().ToString();
			levelState = LevelStateLoader.Load(text);
			if (levelState == null)
			{
				ErrorHandler.HandleError(ErrorCodes.LoadingGameplay_MalformedLevelCaught, new Exception("Result from Get Level call was not returned as expected?\n" + text), displayModal: true, leaveMatch: true);
				return false;
			}
			if (levelState.RevisionMetaData == null)
			{
				levelState.RevisionMetaData = new RevisionMetaData();
			}
			levelState.RevisionMetaData.Changes.Clear();
		}
		progressCallback?.Invoke("Starting level load...");
		await MonoBehaviourSingleton<StageManager>.Instance.LoadLevel(levelState, loadLibraryPrefabs: true, cancelToken, progressCallback);
		return true;
	}

	public void StartGameplay()
	{
		IsPlaying = true;
		MonoBehaviourSingleton<EndlessLoop>.Instance.EnterPlayMode();
		OnGameplayStarted.Invoke();
	}

	public void StopGameplay()
	{
		IsPlaying = false;
		MonoBehaviourSingleton<EndlessLoop>.Instance.ExitPlayMode();
		OnGameplayStopped.Invoke();
	}

	public void CleanupGameplay()
	{
		MonoBehaviourSingleton<GlobalContextsManager>.Instance.ClearContexts();
		OnGameplayCleanup.Invoke();
		MonoBehaviourSingleton<StageManager>.Instance.LeavingSession();
	}

	public void SavePropStates()
	{
		MonoBehaviourSingleton<EndlessLoop>.Instance.SavePropStates();
	}

	private void HandleLevelLoaded(SerializableGuid mapId)
	{
		try
		{
			OnLevelLoaded.Invoke(mapId);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private string GetLevelName()
	{
		return MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.Name;
	}
}
