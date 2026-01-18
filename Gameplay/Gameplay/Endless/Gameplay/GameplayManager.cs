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

namespace Endless.Gameplay
{
	// Token: 0x020000CA RID: 202
	public class GameplayManager : MonoBehaviourSingleton<GameplayManager>
	{
		// Token: 0x1700009F RID: 159
		// (get) Token: 0x060003F1 RID: 1009 RVA: 0x00015DBE File Offset: 0x00013FBE
		// (set) Token: 0x060003F2 RID: 1010 RVA: 0x00015DC6 File Offset: 0x00013FC6
		public List<SerializableGuid> AvailableSpawnPoints { get; set; }

		// Token: 0x170000A0 RID: 160
		// (get) Token: 0x060003F3 RID: 1011 RVA: 0x00015DCF File Offset: 0x00013FCF
		// (set) Token: 0x060003F4 RID: 1012 RVA: 0x00015DD7 File Offset: 0x00013FD7
		public bool IsPlaying { get; private set; }

		// Token: 0x170000A1 RID: 161
		// (get) Token: 0x060003F5 RID: 1013 RVA: 0x00015DE0 File Offset: 0x00013FE0
		public static Collider[] TemporaryColliderArray { get; } = new Collider[30];

		// Token: 0x170000A2 RID: 162
		// (get) Token: 0x060003F6 RID: 1014 RVA: 0x00015DE7 File Offset: 0x00013FE7
		public static HashSet<HittableComponent> TemporaryHittableComponentsHashset { get; } = new HashSet<HittableComponent>();

		// Token: 0x170000A3 RID: 163
		// (get) Token: 0x060003F7 RID: 1015 RVA: 0x00015DEE File Offset: 0x00013FEE
		public static HashSet<IPhysicsTaker> TemporaryPhysicsTakerHashset { get; } = new HashSet<IPhysicsTaker>();

		// Token: 0x060003F8 RID: 1016 RVA: 0x00015DF5 File Offset: 0x00013FF5
		private void Start()
		{
			MonoBehaviourSingleton<StageManager>.Instance.OnLevelLoaded.AddListener(new UnityAction<SerializableGuid>(this.HandleLevelLoaded));
		}

		// Token: 0x060003F9 RID: 1017 RVA: 0x00015E12 File Offset: 0x00014012
		public void ChangeLevel(LevelDestination destination)
		{
			if (!destination.IsValidLevel())
			{
				return;
			}
			if (this.RequestLevelChange != null)
			{
				this.AvailableSpawnPoints = destination.TargetSpawnPointIds;
				this.RequestLevelChange(destination.TargetLevelId);
			}
		}

		// Token: 0x060003FA RID: 1018 RVA: 0x00015E44 File Offset: 0x00014044
		public async Task<bool> LoadLevel(SerializableGuid levelId, CancellationToken cancelToken, string versionNumber = "", Action<string> progressCallback = null)
		{
			Debug.Log(string.Format("GameplayManager.LoadLevel loading LevelId: {0}", levelId));
			await Task.Yield();
			if (progressCallback != null)
			{
				progressCallback("Fetching latest data...");
			}
			LevelState levelState;
			if (!MonoBehaviourSingleton<StageManager>.Instance.TryGetCachedLevel(levelId, versionNumber, out levelState))
			{
				GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetAssetAsync(levelId, versionNumber, null, false, 60);
				if (graphQlResult.HasErrors)
				{
					Exception errorMessage = graphQlResult.GetErrorMessage(0);
					ErrorHandler.HandleError((errorMessage is TimeoutException) ? ErrorCodes.GameplayManager_LevelFetchTimeout : ErrorCodes.GameplayManager_GetLevelAsset, errorMessage, true, true);
					return false;
				}
				string text = graphQlResult.GetDataMember().ToString();
				levelState = LevelStateLoader.Load(text);
				if (levelState == null)
				{
					ErrorHandler.HandleError(ErrorCodes.LoadingGameplay_MalformedLevelCaught, new Exception("Result from Get Level call was not returned as expected?\n" + text), true, true);
					return false;
				}
				if (levelState.RevisionMetaData == null)
				{
					levelState.RevisionMetaData = new RevisionMetaData();
				}
				levelState.RevisionMetaData.Changes.Clear();
			}
			if (progressCallback != null)
			{
				progressCallback("Starting level load...");
			}
			await MonoBehaviourSingleton<StageManager>.Instance.LoadLevel(levelState, true, cancelToken, progressCallback);
			return true;
		}

		// Token: 0x060003FB RID: 1019 RVA: 0x00015EA0 File Offset: 0x000140A0
		public void StartGameplay()
		{
			this.IsPlaying = true;
			MonoBehaviourSingleton<EndlessLoop>.Instance.EnterPlayMode();
			this.OnGameplayStarted.Invoke();
		}

		// Token: 0x060003FC RID: 1020 RVA: 0x00015EBE File Offset: 0x000140BE
		public void StopGameplay()
		{
			this.IsPlaying = false;
			MonoBehaviourSingleton<EndlessLoop>.Instance.ExitPlayMode();
			this.OnGameplayStopped.Invoke();
		}

		// Token: 0x060003FD RID: 1021 RVA: 0x00015EDC File Offset: 0x000140DC
		public void CleanupGameplay()
		{
			MonoBehaviourSingleton<GlobalContextsManager>.Instance.ClearContexts();
			this.OnGameplayCleanup.Invoke();
			MonoBehaviourSingleton<StageManager>.Instance.LeavingSession();
		}

		// Token: 0x060003FE RID: 1022 RVA: 0x00015EFD File Offset: 0x000140FD
		public void SavePropStates()
		{
			MonoBehaviourSingleton<EndlessLoop>.Instance.SavePropStates();
		}

		// Token: 0x060003FF RID: 1023 RVA: 0x00015F0C File Offset: 0x0001410C
		private void HandleLevelLoaded(SerializableGuid mapId)
		{
			try
			{
				this.OnLevelLoaded.Invoke(mapId);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		// Token: 0x06000400 RID: 1024 RVA: 0x00015F40 File Offset: 0x00014140
		private string GetLevelName()
		{
			return MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.Name;
		}

		// Token: 0x04000388 RID: 904
		public UnityEvent OnGameplayStarted = new UnityEvent();

		// Token: 0x04000389 RID: 905
		public UnityEvent OnGameplayStopped = new UnityEvent();

		// Token: 0x0400038A RID: 906
		public UnityEvent OnGameplayCleanup = new UnityEvent();

		// Token: 0x0400038B RID: 907
		public UnityEvent<SerializableGuid> OnLevelLoaded = new UnityEvent<SerializableGuid>();

		// Token: 0x0400038C RID: 908
		public UnityEvent<BlockTokenCollection> OnGameplayLoadingStateReadyToEnd = new UnityEvent<BlockTokenCollection>();

		// Token: 0x0400038D RID: 909
		[SerializeField]
		private TextAsset levelFile;

		// Token: 0x0400038E RID: 910
		public Action<SerializableGuid> RequestLevelChange;

		// Token: 0x0400038F RID: 911
		private bool pendingLevelSave;

		// Token: 0x04000390 RID: 912
		private bool saveInProgress;

		// Token: 0x04000391 RID: 913
		private float initialSaveTimeRequest;

		// Token: 0x04000392 RID: 914
		private float lastSaveTimeRequest;
	}
}
