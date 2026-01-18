using System;
using System.Collections;
using Endless.Core.UI;
using Endless.Creator;
using Endless.Creator.LevelEditing;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Data.UI;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.GameStates
{
	// Token: 0x020000C6 RID: 198
	public class CreatorGameState : GameStateBase
	{
		// Token: 0x17000083 RID: 131
		// (get) Token: 0x06000482 RID: 1154 RVA: 0x0001648A File Offset: 0x0001468A
		public override GameState StateType
		{
			get
			{
				return GameState.Creator;
			}
		}

		// Token: 0x06000483 RID: 1155 RVA: 0x00016490 File Offset: 0x00014690
		public override void StateEntered(GameState oldState)
		{
			MonoBehaviourSingleton<CellMarker>.Instance.SetActiveState(true);
			MonoBehaviourSingleton<WorldBoundaryMarker>.Instance.SetActiveState(true);
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.ApplyMemberChanges();
			NetworkBehaviourSingleton<CreatorManager>.Instance.CreatorLoaded();
			if (base.IsServer)
			{
				NetworkBehaviourSingleton<CreatorManager>.Instance.LevelReverted.AddListener(new UnityAction(this.OnLevelReverted));
				NetworkBehaviourSingleton<GameStateManager>.Instance.OnGameplayFlipRequested.AddListener(new UnityAction(this.HandleFlipRequested));
				NetworkBehaviourSingleton<NetClock>.Instance.GameplayReadyFrame.Value = NetClock.CurrentFrame + 8U;
			}
			this.networkSyncingStartFrameCoroutine = NetworkBehaviourSingleton<GameStateManager>.Instance.StartCoroutine(this.NetworkSyncingStartFrameProcess());
			MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("CreatorGameState", "MatchLoad");
			MonoBehaviourSingleton<LoadTimeTester>.Instance.StopTracking("MatchLoad");
		}

		// Token: 0x06000484 RID: 1156 RVA: 0x00016559 File Offset: 0x00014759
		private IEnumerator NetworkSyncingStartFrameProcess()
		{
			MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.Display(UIScreenCoverTokens.WaitingForNetworkToStartCreator, null, true);
			while (NetClock.CurrentFrame < NetworkBehaviourSingleton<NetClock>.Instance.GameplayReadyFrame.Value)
			{
				yield return null;
			}
			MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.ReleaseToken(UIScreenCoverTokens.WaitingForNetworkToStartCreator);
			this.networkSyncingStartFrameCoroutine = null;
			yield break;
		}

		// Token: 0x06000485 RID: 1157 RVA: 0x00016568 File Offset: 0x00014768
		private void OnLevelReverted()
		{
			base.ChangeState(new LoadingCreatorGameState(NetworkBehaviourSingleton<GameStateManager>.Instance.LevelId));
		}

		// Token: 0x06000486 RID: 1158 RVA: 0x0001657F File Offset: 0x0001477F
		private void HandleFlipRequested()
		{
			MonoBehaviourSingleton<ToolManager>.Instance.SetActiveTool(ToolType.Empty);
			MonoBehaviourSingleton<RuntimeDatabase>.Instance.SetGame(MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame);
			base.ChangeState(new LoadingGameplayGameState(NetworkBehaviourSingleton<GameStateManager>.Instance.LevelId, ""));
		}

		// Token: 0x06000487 RID: 1159 RVA: 0x000165BC File Offset: 0x000147BC
		public override void HandleExitingState(GameState newState)
		{
			MonoBehaviourSingleton<CellMarker>.Instance.SetActiveState(false);
			MonoBehaviourSingleton<WorldBoundaryMarker>.Instance.SetActiveState(false);
			if (this.networkSyncingStartFrameCoroutine != null)
			{
				MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.ReleaseToken(UIScreenCoverTokens.WaitingForNetworkToStartCreator);
				this.networkSyncingStartFrameCoroutine = null;
			}
			MonoBehaviourSingleton<GameEditorAssetVersionManager>.Instance.SaveAndCleanup();
			if (newState == GameState.Default)
			{
				NetworkBehaviourSingleton<CreatorManager>.Instance.LeavingSession();
			}
			NetworkBehaviourSingleton<CreatorManager>.Instance.LeavingCreator(newState > GameState.Default);
			NetworkBehaviourSingleton<CreatorManager>.Instance.LevelReverted.RemoveListener(new UnityAction(this.OnLevelReverted));
			NetworkBehaviourSingleton<GameStateManager>.Instance.OnGameplayFlipRequested.RemoveListener(new UnityAction(this.HandleFlipRequested));
			if (base.IsServer)
			{
				NetworkBehaviourSingleton<NetClock>.Instance.GameplayReadyFrame.Value = uint.MaxValue;
			}
		}

		// Token: 0x0400030E RID: 782
		private Coroutine networkSyncingStartFrameCoroutine;
	}
}
