using System;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Core.GameStates
{
	// Token: 0x020000CB RID: 203
	public abstract class GameStateBase
	{
		// Token: 0x1700008A RID: 138
		// (get) Token: 0x060004A2 RID: 1186 RVA: 0x00016B25 File Offset: 0x00014D25
		protected bool IsServer
		{
			get
			{
				return NetworkManager.Singleton.IsServer;
			}
		}

		// Token: 0x1700008B RID: 139
		// (get) Token: 0x060004A3 RID: 1187 RVA: 0x00016B31 File Offset: 0x00014D31
		protected bool IsClient
		{
			get
			{
				return NetworkManager.Singleton.IsClient;
			}
		}

		// Token: 0x1700008C RID: 140
		// (get) Token: 0x060004A4 RID: 1188
		public abstract GameState StateType { get; }

		// Token: 0x060004A5 RID: 1189 RVA: 0x00016B3D File Offset: 0x00014D3D
		protected void ChangeState(GameStateBase newGameState)
		{
			Action<GameStateBase> onChangeRequestCallback = this.OnChangeRequestCallback;
			if (onChangeRequestCallback == null)
			{
				return;
			}
			onChangeRequestCallback(newGameState);
		}

		// Token: 0x060004A6 RID: 1190 RVA: 0x00016B50 File Offset: 0x00014D50
		public void ForceExitState()
		{
			this.HandleForceExitState();
			this.ChangeState(new DefaultGameState());
		}

		// Token: 0x060004A7 RID: 1191 RVA: 0x0000229D File Offset: 0x0000049D
		public virtual void HandleForceExitState()
		{
		}

		// Token: 0x060004A8 RID: 1192 RVA: 0x0000229D File Offset: 0x0000049D
		public virtual void StartEnteringState(GameState oldState)
		{
		}

		// Token: 0x060004A9 RID: 1193
		public abstract void StateEntered(GameState oldState);

		// Token: 0x060004AA RID: 1194
		public abstract void HandleExitingState(GameState newState);

		// Token: 0x060004AB RID: 1195 RVA: 0x00016B63 File Offset: 0x00014D63
		protected void SetBlockingToken()
		{
			if (NetworkBehaviourSingleton<GameStateManager>.Instance.CurrentlyProcessingState != null)
			{
				Debug.LogError("Claiming non null token for state " + base.GetType().Name);
			}
			NetworkBehaviourSingleton<GameStateManager>.Instance.CurrentlyProcessingState = this;
		}

		// Token: 0x060004AC RID: 1196 RVA: 0x00016B98 File Offset: 0x00014D98
		protected void ClearBlockingStateToken()
		{
			if (NetworkBehaviourSingleton<GameStateManager>.Instance)
			{
				if (NetworkBehaviourSingleton<GameStateManager>.Instance.CurrentlyProcessingState != this)
				{
					Debug.LogError(string.Format("We were not properly marked as processing, but tried to clear from: {0}, currentProcessingState: {1}", base.GetType().Name, NetworkBehaviourSingleton<GameStateManager>.Instance.CurrentlyProcessingState));
				}
				NetworkBehaviourSingleton<GameStateManager>.Instance.CurrentlyProcessingState = null;
			}
		}

		// Token: 0x04000318 RID: 792
		public Action<GameStateBase> OnChangeRequestCallback;
	}
}
