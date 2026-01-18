using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Endless.Assets;
using Endless.Core;
using Endless.Core.GameStates;
using Endless.Core.UI;
using Endless.Creator;
using Endless.Data;
using Endless.Data.UI;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.RightsManagement;
using Endless.GraphQl;
using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Runtime.Gameplay.LevelEditing;
using Runtime.Shared.Matchmaking;
using Unity.Netcode;
using UnityEngine;

namespace Runtime.Core.GameStates
{
	// Token: 0x0200000B RID: 11
	public class ValidateGameLibraryGameState : GameStateBase
	{
		// Token: 0x17000006 RID: 6
		// (get) Token: 0x0600002A RID: 42 RVA: 0x000027B9 File Offset: 0x000009B9
		public override GameState StateType
		{
			get
			{
				return GameState.ValidatingLibrary;
			}
		}

		// Token: 0x0600002B RID: 43 RVA: 0x000027BC File Offset: 0x000009BC
		public ValidateGameLibraryGameState(MatchData matchData, GameState followUpState)
		{
			this.matchData = matchData;
			this.followUpState = followUpState;
			this.cancelTokenSource = new CancellationTokenSource();
		}

		// Token: 0x0600002C RID: 44 RVA: 0x000027E0 File Offset: 0x000009E0
		public override void StateEntered(GameState oldState)
		{
			MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.Display(UIScreenCoverTokens.ValidatingGameLibrary, null, true);
			NetworkBehaviourSingleton<CreatorManager>.Instance.SetRPCReceiveState(RpcReceiveState.Ignore);
			this.ValidateLibrary();
			if (base.IsServer)
			{
				NetworkBehaviourSingleton<GameStateManager>.Instance.SetClientsToValidateGameLibrary_ClientRpc(this.followUpState);
				return;
			}
			MonoBehaviourSingleton<StageManager>.Instance.SetJoinLevelId(NetworkBehaviourSingleton<GameStateManager>.Instance.LevelId);
		}

		// Token: 0x0600002D RID: 45 RVA: 0x0000283C File Offset: 0x00000A3C
		private async void ValidateLibrary()
		{
			MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("Start ValidateGameLibraryGameState", "MatchLoad");
			AssetFlagCache.Clear();
			if (this.followUpState == GameState.Creator || !MonoBehaviourSingleton<RuntimeDatabase>.Instance.RestoreActiveGame(this.matchData.ProjectId, this.matchData.Version))
			{
				GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetAssetAsync(this.matchData.ProjectId, this.matchData.Version, null, false, 10);
				if (graphQlResult.HasErrors)
				{
					Exception errorMessage = graphQlResult.GetErrorMessage(0);
					ErrorHandler.HandleError((errorMessage is TimeoutException) ? ErrorCodes.ValidateGameLibrary_GameFetchTimeout : ErrorCodes.ValidateGameState_GetGameAsset, errorMessage, true, true);
					return;
				}
				string text = graphQlResult.GetDataMember().ToString();
				Game game;
				try
				{
					game = GameLoader.Load(text);
					Debug.Log("loaded with internal version " + game.InternalVersion);
				}
				catch (Exception ex)
				{
					ErrorHandler.HandleError(ErrorCodes.ValidateGameState_LoadingGame, new Exception("Malformed game result from server. Unable to validate library", ex), true, true);
					base.ChangeState(new DefaultGameState());
					return;
				}
				Game game2 = game;
				if (game2.GameLibrary == null)
				{
					game2.GameLibrary = new GameLibrary();
				}
				bool needsSave = false;
				if (game.GameLibrary.TerrainEntries.Count == 0)
				{
					Debug.Log("Adding default terrain!");
					this.UpdateLoadingText("Updating terrain details...");
					needsSave = true;
					await MonoBehaviourSingleton<DefaultContentManager>.Instance.AddDefaultTerrain(game);
				}
				if (game.GameLibrary.PropReferences.Count == 0)
				{
					this.UpdateLoadingText("Updating prop details...");
					Debug.Log("Adding default props!");
					needsSave = true;
					await MonoBehaviourSingleton<DefaultContentManager>.Instance.AddDefaultProps(game);
				}
				if (base.IsServer && needsSave && this.followUpState == GameState.LoadingCreator)
				{
					game.RevisionMetaData.Changes.Add(new ChangeData
					{
						ChangeType = ChangeType.AutomaticAssetUpgrade,
						UserId = EndlessServices.Instance.CloudService.ActiveUserId
					});
					game.RevisionMetaData.RevisionTimestamp = DateTime.Now.Ticks;
					Version version = Version.Parse(game.AssetVersion);
					game.AssetVersion = string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build + 1);
					Debug.Log("Saving the Game with its new library");
					Debug.Log("Saved game with asset version " + game.AssetVersion);
					Debug.Log("Saved with internal version " + game.InternalVersion);
					this.UpdateLoadingText("Updating game data...");
					GraphQlResult graphQlResult2 = await EndlessServices.Instance.CloudService.UpdateAssetAsync(game, false, false);
					if (graphQlResult2.HasErrors)
					{
						ErrorHandler.HandleError(ErrorCodes.ValidateGameState_UpdatingGame, graphQlResult2.GetErrorMessage(0), true, true);
						return;
					}
					game = GameLoader.Load(graphQlResult2.GetDataMember().ToString());
					if (game == null)
					{
						ErrorHandler.HandleError(ErrorCodes.ValidateGameState_LoadGame, new Exception("Malformed game result from server. Unable to validate library"), true, true);
						return;
					}
				}
				MonoBehaviourSingleton<RuntimeDatabase>.Instance.SetGame(game);
				game = null;
			}
			this.UpdateLoadingText("Cleaning up memory...");
			MonoBehaviourSingleton<StageManager>.Instance.ActiveTerrainPalette.UnloadExcept(MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.GameLibrary.TerrainEntries);
			MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.UnloadPropsNotInGameLibrary();
			await Task.Yield();
			Resources.UnloadUnusedAssets();
			await Task.Yield();
			GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
			await Task.Yield();
			if (!this.cancelTokenSource.Token.IsCancellationRequested)
			{
				this.UpdateLoadingText("Starting Asset Load...");
				if (this.RequiresContentFlagCheck())
				{
					HashSet<ValueTuple<SerializableGuid, string>> assetsToCheckContentRestrictions = new HashSet<ValueTuple<SerializableGuid, string>>();
					foreach (TerrainUsage terrainUsage2 in MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.GameLibrary.TerrainEntries.Where((TerrainUsage terrainUsage) => terrainUsage.IsActive))
					{
						assetsToCheckContentRestrictions.Add(new ValueTuple<SerializableGuid, string>(terrainUsage2.TerrainAssetReference.AssetID, terrainUsage2.TerrainAssetReference.AssetVersion));
					}
					foreach (AssetReference assetReference in MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.GameLibrary.PropReferences)
					{
						assetsToCheckContentRestrictions.Add(new ValueTuple<SerializableGuid, string>(assetReference.AssetID, assetReference.AssetVersion));
					}
					if (!this.matchData.IsEditSession || (this.matchData.IsEditSession && !NetworkManager.Singleton.IsHost))
					{
						try
						{
							await AssetFlagCache.GetBulkAssetFlagsForAsset(assetsToCheckContentRestrictions);
						}
						catch (Exception ex2)
						{
							ErrorHandler.HandleError((ex2 is TimeoutException) ? ErrorCodes.ValidateGameLibraryGameState_UnableToFetchContentFlags_Timeout : ErrorCodes.ValidateGameLibraryGameState_UnableToFetchContentFlags, ex2, true, true);
						}
						foreach (ValueTuple<SerializableGuid, string> valueTuple in assetsToCheckContentRestrictions)
						{
							try
							{
								TaskAwaiter<bool> taskAwaiter = AssetFlagCache.IsAssetAllowed(valueTuple.Item1, valueTuple.Item2).GetAwaiter();
								if (!taskAwaiter.IsCompleted)
								{
									await taskAwaiter;
									TaskAwaiter<bool> taskAwaiter2;
									taskAwaiter = taskAwaiter2;
									taskAwaiter2 = default(TaskAwaiter<bool>);
								}
								if (!taskAwaiter.GetResult())
								{
									ErrorHandler.HandleError(ErrorCodes.LoadingIntoGame_ContentRestricted, new Exception("Unable to Validate Game Library. Reason: Content Restricted"), true, true);
								}
							}
							catch (Exception ex3)
							{
								ex3.Message.Contains("not found");
							}
						}
						HashSet<ValueTuple<SerializableGuid, string>>.Enumerator enumerator3 = default(HashSet<ValueTuple<SerializableGuid, string>>.Enumerator);
					}
					assetsToCheckContentRestrictions = null;
				}
				await MonoBehaviourSingleton<StageManager>.Instance.ActiveTerrainPalette.PreloadData(MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.GameLibrary, this.cancelTokenSource.Token, new Action<int, int>(this.TerrainLoadingUpdate));
				if (!this.cancelTokenSource.Token.IsCancellationRequested)
				{
					await MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.PreloadData(this.cancelTokenSource.Token, null, new Action<int, int>(this.PropLoadingUpdate));
					if (!this.cancelTokenSource.Token.IsCancellationRequested)
					{
						await MonoBehaviourSingleton<StageManager>.Instance.ActiveAudioLibrary.PreloadData(this.cancelTokenSource.Token, new Action<int, int>(this.AudioLoadingUpdate));
						if (!this.cancelTokenSource.Token.IsCancellationRequested)
						{
							this.UpdateLoadingText("Checking rights...");
							await MonoBehaviourSingleton<RightsManager>.Instance.HasRoleOrGreaterForAssetAsync(NetworkBehaviourSingleton<GameStateManager>.Instance.LevelId, NetworkBehaviourSingleton<GameStateManager>.Instance.GameId, EndlessServices.Instance.CloudService.ActiveUserId, Roles.Viewer, null, true);
							if (!this.cancelTokenSource.Token.IsCancellationRequested)
							{
								this.FinalizeState();
							}
						}
					}
				}
			}
		}

		// Token: 0x0600002E RID: 46 RVA: 0x00002873 File Offset: 0x00000A73
		private bool RequiresContentFlagCheck()
		{
			return EndlessCloudService.ContentRestrictionsOnAccount.Count > 0;
		}

		// Token: 0x0600002F RID: 47 RVA: 0x00002882 File Offset: 0x00000A82
		private void AudioLoadingUpdate(int loadingSection, int loadingTotal)
		{
			this.UpdateLoadingText(string.Format("Downloading Audio Info: {0}/{1}", loadingSection, loadingTotal));
		}

		// Token: 0x06000030 RID: 48 RVA: 0x000028A0 File Offset: 0x00000AA0
		private void TerrainLoadingUpdate(int loadingSection, int loadingTotal)
		{
			this.UpdateLoadingText(string.Format("Downloading Terrain Info: {0:N0}/{1:N0}", loadingSection, loadingTotal));
		}

		// Token: 0x06000031 RID: 49 RVA: 0x000028BE File Offset: 0x00000ABE
		private void PropLoadingUpdate(int loadingSection, int loadingTotal)
		{
			this.UpdateLoadingText(string.Format("Downloading Prop Info: {0:N0}/{1:N0}", loadingSection, loadingTotal));
		}

		// Token: 0x06000032 RID: 50 RVA: 0x000028DC File Offset: 0x00000ADC
		private void UpdateLoadingText(string text)
		{
			MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.UpdateText(UIScreenCoverTokens.ValidatingGameLibrary, "Validating Game Library: " + text);
		}

		// Token: 0x06000033 RID: 51 RVA: 0x000028F8 File Offset: 0x00000AF8
		private void FinalizeState()
		{
			MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("ValidateGameLibraryGameState", "MatchLoad");
			if (!base.IsServer)
			{
				this.HandleClientChangeToState(NetworkBehaviourSingleton<GameStateManager>.Instance.SharedGameState);
				return;
			}
			GameState gameState = this.followUpState;
			if (gameState == GameState.LoadingCreator)
			{
				base.ChangeState(new LoadingCreatorGameState(this.matchData.LevelId));
				return;
			}
			if (gameState != GameState.LoadingGameplay)
			{
				ErrorHandler.HandleError(ErrorCodes.ValidateGameLibraryGameState_IncorrectServerFollowupState, new Exception(string.Format("Expected to go into {0} but is not an allowed follow up state for server.", this.followUpState)), true, true);
				return;
			}
			base.ChangeState(new LoadingGameplayGameState(this.matchData.LevelId, MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.GetLevelReferenceById(this.matchData.LevelId).AssetVersion));
		}

		// Token: 0x06000034 RID: 52 RVA: 0x000029C8 File Offset: 0x00000BC8
		private void HandleClientChangeToState(GameState targetState)
		{
			switch (targetState)
			{
			case GameState.LoadingCreator:
			case GameState.Creator:
				base.ChangeState(new LoadingCreatorGameState(NetworkBehaviourSingleton<GameStateManager>.Instance.LevelId));
				return;
			case GameState.LoadingGameplay:
			case GameState.LoadedGameplay:
			case GameState.StartingGameplay:
			case GameState.Gameplay:
			case GameState.GameplayOutro:
				base.ChangeState(new LoadingGameplayGameState(NetworkBehaviourSingleton<GameStateManager>.Instance.LevelId, MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.GetLevelReferenceById(NetworkBehaviourSingleton<GameStateManager>.Instance.LevelId).AssetVersion));
				return;
			}
			if (targetState == this.followUpState)
			{
				ErrorHandler.HandleError(ErrorCodes.ValidateGameLibraryGameState_StateChangeFinalizeLoop, new Exception(string.Format("Infinite state change detected. Attempting to load into state: {0} but it is also our follow up state that we failed to load into.", targetState)), true, true);
				return;
			}
			this.HandleClientChangeToState(this.followUpState);
		}

		// Token: 0x06000035 RID: 53 RVA: 0x00002A88 File Offset: 0x00000C88
		public override void HandleExitingState(GameState newState)
		{
			this.cancelTokenSource.Cancel();
			MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.ReleaseToken(UIScreenCoverTokens.ValidatingGameLibrary);
		}

		// Token: 0x0400001B RID: 27
		private MatchData matchData;

		// Token: 0x0400001C RID: 28
		private GameState followUpState;

		// Token: 0x0400001D RID: 29
		private CancellationTokenSource cancelTokenSource;
	}
}
