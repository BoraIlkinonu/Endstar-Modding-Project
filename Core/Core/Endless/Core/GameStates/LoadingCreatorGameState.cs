using System;
using System.Threading;
using System.Threading.Tasks;
using Endless.Core.UI;
using Endless.Creator;
using Endless.Data;
using Endless.Data.UI;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Core.GameStates
{
	// Token: 0x020000CE RID: 206
	public class LoadingCreatorGameState : GameStateBase
	{
		// Token: 0x17000090 RID: 144
		// (get) Token: 0x060004BB RID: 1211 RVA: 0x00016DC5 File Offset: 0x00014FC5
		public override GameState StateType
		{
			get
			{
				return GameState.LoadingCreator;
			}
		}

		// Token: 0x060004BC RID: 1212 RVA: 0x00016DC8 File Offset: 0x00014FC8
		public LoadingCreatorGameState(SerializableGuid levelId)
		{
			this.levelId = levelId;
		}

		// Token: 0x060004BD RID: 1213 RVA: 0x00016DE4 File Offset: 0x00014FE4
		public override async void StateEntered(GameState oldState)
		{
			MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.Display(UIScreenCoverTokens.LoadingCreator, null, true);
			MonoBehaviourSingleton<LoadTimeTester>.Instance.StartTracking("LoadingCreatorGameState");
			this.UpdateDisplayText("Cleaning up previous state..");
			while (NetworkBehaviourSingleton<GameStateManager>.Instance != null && NetworkBehaviourSingleton<GameStateManager>.Instance.CurrentlyProcessingState != null)
			{
				await Task.Yield();
			}
			MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("Cleanup", "LoadingCreatorGameState");
			if (!this.loadingStateCancelToken.IsCancellationRequested)
			{
				base.SetBlockingToken();
				try
				{
					MonoBehaviourSingleton<GameplayManager>.Instance.AvailableSpawnPoints = null;
					CancellationToken assetVersionCancellationToken = NetworkBehaviourSingleton<CreatorManager>.Instance.GetAssetVersionCancellationToken();
					NetworkBehaviourSingleton<CreatorManager>.Instance.EnteringCreator();
					MonoBehaviourSingleton<GameEditorAssetVersionManager>.Instance.LoadAssetUpdateStates(MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame, assetVersionCancellationToken);
					MonoBehaviourSingleton<StageManager>.Instance.PrepareForLevelChange(this.levelId);
					if (base.IsServer)
					{
						NetworkBehaviourSingleton<NetClock>.Instance.GameplayStreamActive.Value = false;
						if (oldState != GameState.ValidatingLibrary)
						{
							NetworkBehaviourSingleton<GameStateManager>.Instance.ChangeClientState_ClientRpc(GameState.LoadingCreator, this.levelId);
						}
						await MonoBehaviourSingleton<NetworkTickUtility>.Instance.WaitForNetworkTicksAsync(1);
						MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("Await network ticks", "LoadingCreatorGameState");
						NetworkScopeManager.PrepareToLoadNewLevel();
						NetworkBehaviourSingleton<GameStateManager>.Instance.SetNewLevelId(this.levelId);
						await NetworkBehaviourSingleton<CreatorManager>.Instance.PerformInitialLevelLoad(this.levelId, new Action<string>(this.UpdateDisplayText), this.loadingStateCancelToken.Token);
						MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("Load level", "LoadingCreatorGameState");
					}
					else
					{
						await NetworkBehaviourSingleton<CreatorManager>.Instance.RetrieveAndLoadServerLevel(this.loadingStateCancelToken.Token, new Action<string>(this.UpdateDisplayText));
						MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("Loading level", "LoadingCreatorGameState");
					}
					if (this.loadingStateCancelToken.IsCancellationRequested)
					{
						base.ClearBlockingStateToken();
					}
					else
					{
						if (base.IsClient)
						{
							this.UpdateDisplayText("Applying recent changes...");
							await NetworkBehaviourSingleton<CreatorManager>.Instance.ApplyCachedRpcs(this.loadingStateCancelToken.Token);
							if (this.loadingStateCancelToken.IsCancellationRequested)
							{
								base.ClearBlockingStateToken();
								return;
							}
						}
						MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("Applying Cached RPCs", "LoadingCreatorGameState");
						MonoBehaviourSingleton<LoadTimeTester>.Instance.StopTracking("LoadingCreatorGameState");
						NetworkBehaviourSingleton<GameStateManager>.Instance.SpawnCreatorCharacter_ServerRpc(default(ServerRpcParams));
						base.ClearBlockingStateToken();
						base.ChangeState(new CreatorGameState());
					}
				}
				catch (Exception ex)
				{
					base.ClearBlockingStateToken();
					ErrorHandler.HandleError(ErrorCodes.LoadingCreator_UnknownFailure, ex, true, true);
				}
			}
		}

		// Token: 0x060004BE RID: 1214 RVA: 0x00016E23 File Offset: 0x00015023
		private void UpdateDisplayText(string newText)
		{
			MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.UpdateText(UIScreenCoverTokens.LoadingCreator, "Loading Creator: " + newText);
		}

		// Token: 0x060004BF RID: 1215 RVA: 0x00016E3C File Offset: 0x0001503C
		public override void HandleExitingState(GameState newState)
		{
			Debug.Log("LoadingCreatorGameState.HandleExitingState");
			if (base.IsServer)
			{
				NetworkBehaviourSingleton<NetClock>.Instance.GameplayStreamActive.Value = true;
			}
			this.loadingStateCancelToken.Cancel();
			if (newState != GameState.Creator)
			{
				NetworkBehaviourSingleton<CreatorManager>.Instance.ClearCachedRPCs();
				NetworkBehaviourSingleton<CreatorManager>.Instance.SetRPCReceiveState(RpcReceiveState.Ignore);
			}
			if (newState == GameState.Default)
			{
				MonoBehaviourSingleton<StageManager>.Instance.LeavingSession();
			}
			MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.ReleaseToken(UIScreenCoverTokens.LoadingCreator);
		}

		// Token: 0x0400031E RID: 798
		private CancellationTokenSource loadingStateCancelToken = new CancellationTokenSource();

		// Token: 0x0400031F RID: 799
		private SerializableGuid levelId;
	}
}
