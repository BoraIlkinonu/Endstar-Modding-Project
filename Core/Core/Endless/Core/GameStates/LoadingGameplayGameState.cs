using System;
using System.Threading;
using System.Threading.Tasks;
using Endless.Core.UI;
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
	// Token: 0x020000D0 RID: 208
	public class LoadingGameplayGameState : GameStateBase
	{
		// Token: 0x17000091 RID: 145
		// (get) Token: 0x060004C2 RID: 1218 RVA: 0x00017366 File Offset: 0x00015566
		public override GameState StateType
		{
			get
			{
				return GameState.LoadingGameplay;
			}
		}

		// Token: 0x17000092 RID: 146
		// (get) Token: 0x060004C3 RID: 1219 RVA: 0x00017369 File Offset: 0x00015569
		public BlockTokenCollection BlockTokenCollection
		{
			get
			{
				return this.blockTokenCollection;
			}
		}

		// Token: 0x060004C4 RID: 1220 RVA: 0x00017371 File Offset: 0x00015571
		public LoadingGameplayGameState(SerializableGuid levelId, string versionNumber = "")
		{
			this.levelId = levelId;
			this.versionNumber = versionNumber;
			this.blockTokenCollection = new BlockTokenCollection();
		}

		// Token: 0x060004C5 RID: 1221 RVA: 0x000173A0 File Offset: 0x000155A0
		public override async void StateEntered(GameState oldState)
		{
			MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.Display(UIScreenCoverTokens.LoadingGameplay, null, true);
			this.UpdateLoadText("Cleaning up previous state..");
			while (NetworkBehaviourSingleton<GameStateManager>.Instance.CurrentlyProcessingState != null)
			{
				await Task.Yield();
			}
			if (!this.loadLevelCancellationSource.IsCancellationRequested)
			{
				base.SetBlockingToken();
				try
				{
					MonoBehaviourSingleton<StageManager>.Instance.PrepareForLevelChange(this.levelId);
					if (base.IsServer)
					{
						NetworkBehaviourSingleton<NetClock>.Instance.GameplayStreamActive.Value = false;
						NetworkBehaviourSingleton<GameStateManager>.Instance.ChangeClientState_ClientRpc(GameState.LoadingGameplay, this.levelId);
						await MonoBehaviourSingleton<NetworkTickUtility>.Instance.WaitForNetworkTicksAsync(1);
						if (oldState != GameState.ValidatingLibrary && oldState != GameState.Creator)
						{
							NetworkScopeManager.PrepareToLoadNewLevel();
						}
					}
					bool flag = await MonoBehaviourSingleton<GameplayManager>.Instance.LoadLevel(this.levelId, this.loadLevelCancellationSource.Token, this.versionNumber, new Action<string>(this.UpdateLoadText));
					if (this.loadLevelCancellationSource.IsCancellationRequested)
					{
						base.ClearBlockingStateToken();
					}
					else if (flag)
					{
						await Task.Yield();
						MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.PrepForGameplay();
						MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayLoadingStateReadyToEnd.Invoke(this.BlockTokenCollection);
						this.UpdateLoadText("Building AI navigation...");
						while (!this.BlockTokenCollection.IsPoolEmpty)
						{
							await Task.Yield();
							if (this.loadLevelCancellationSource.IsCancellationRequested)
							{
								base.ClearBlockingStateToken();
								return;
							}
						}
						base.ClearBlockingStateToken();
						MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("LoadingGameplayGameState", "MatchLoad");
						if (base.IsServer)
						{
							base.ChangeState(new LoadedGameplayGameState());
						}
						else if (NetworkBehaviourSingleton<GameStateManager>.Instance.SharedGameState == GameState.Gameplay)
						{
							NetworkBehaviourSingleton<GameStateManager>.Instance.SpawnGameplayCharacter_ServerRpc(default(ServerRpcParams));
							base.ChangeState(new GameplayGameState());
						}
						else
						{
							base.ChangeState(new LoadedGameplayGameState());
						}
					}
				}
				catch (Exception ex)
				{
					base.ClearBlockingStateToken();
					ErrorHandler.HandleError(ErrorCodes.LoadingGameplay_UnknownFailure, ex, true, true);
				}
			}
		}

		// Token: 0x060004C6 RID: 1222 RVA: 0x000173DF File Offset: 0x000155DF
		private void UpdateLoadText(string text)
		{
			MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.UpdateText(UIScreenCoverTokens.LoadingGameplay, "Loading Gameplay: " + text);
		}

		// Token: 0x060004C7 RID: 1223 RVA: 0x000173F8 File Offset: 0x000155F8
		public override void HandleExitingState(GameState newState)
		{
			Debug.Log("LoadingGameplayGameState.HandleExitingState");
			if (base.IsServer)
			{
				NetworkBehaviourSingleton<NetClock>.Instance.GameplayStreamActive.Value = true;
			}
			this.loadLevelCancellationSource.Cancel();
			if (newState != GameState.Gameplay && newState != GameState.LoadedGameplay)
			{
				MonoBehaviourSingleton<StageManager>.Instance.FlushLoadedAndSpawnedStages(base.IsServer);
			}
			MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.ReleaseToken(UIScreenCoverTokens.LoadingGameplay);
		}

		// Token: 0x04000326 RID: 806
		private BlockTokenCollection blockTokenCollection;

		// Token: 0x04000327 RID: 807
		private SerializableGuid levelId;

		// Token: 0x04000328 RID: 808
		private string versionNumber;

		// Token: 0x04000329 RID: 809
		private CancellationTokenSource loadLevelCancellationSource = new CancellationTokenSource();
	}
}
