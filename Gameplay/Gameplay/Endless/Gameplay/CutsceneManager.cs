using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay
{
	// Token: 0x020000B9 RID: 185
	public class CutsceneManager : EndlessNetworkBehaviourSingleton<CutsceneManager>, IGameEndSubscriber
	{
		// Token: 0x1700008F RID: 143
		// (get) Token: 0x0600034D RID: 845 RVA: 0x00011D76 File Offset: 0x0000FF76
		// (set) Token: 0x0600034E RID: 846 RVA: 0x00011D7E File Offset: 0x0000FF7E
		public InputSettings CurrentCutsceneInputSettings { get; private set; }

		// Token: 0x0600034F RID: 847 RVA: 0x00011D87 File Offset: 0x0000FF87
		private void OnEnable()
		{
			MonoBehaviourSingleton<PlayerManager>.Instance.OnNewPlayerRegistered.AddListener(new UnityAction<ulong, PlayerReferenceManager>(this.HandlePlayerJoined));
			MonoBehaviourSingleton<PlayerManager>.Instance.PlayerUnregistered.AddListener(new UnityAction<ulong, PlayerReferenceManager>(this.HandlePlayerLeft));
		}

		// Token: 0x06000350 RID: 848 RVA: 0x00011DBF File Offset: 0x0000FFBF
		private void OnDisable()
		{
			MonoBehaviourSingleton<PlayerManager>.Instance.OnNewPlayerRegistered.RemoveListener(new UnityAction<ulong, PlayerReferenceManager>(this.HandlePlayerJoined));
			MonoBehaviourSingleton<PlayerManager>.Instance.PlayerUnregistered.RemoveListener(new UnityAction<ulong, PlayerReferenceManager>(this.HandlePlayerLeft));
		}

		// Token: 0x06000351 RID: 849 RVA: 0x00011DF7 File Offset: 0x0000FFF7
		private void HandlePlayerJoined(ulong playedId, PlayerReferenceManager references)
		{
			if (this.activeGlobalCutscene != null)
			{
				this.activeGlobalCutscene.AddPlayer(playedId);
			}
		}

		// Token: 0x06000352 RID: 850 RVA: 0x00011E10 File Offset: 0x00010010
		private void HandlePlayerLeft(ulong playedId, PlayerReferenceManager references)
		{
			if (this.activeGlobalCutscene != null)
			{
				this.activeGlobalCutscene.IncludedPlayers.Remove(playedId);
				return;
			}
			CutsceneManager.GroupCutsceneState state;
			if (this.playerCutsceneStates.TryGetValue(playedId, out state))
			{
				if (state.CutsceneScope == Scope.Private)
				{
					this.playerCutsceneStates.Remove(playedId);
					return;
				}
				if (state.IncludedPlayers.Count > 1)
				{
					state.IncludedPlayers.Remove(playedId);
					this.playerCutsceneStates.Remove(playedId);
					return;
				}
				Context key = this.activeLocalCutscenes.FirstOrDefault((KeyValuePair<Context, CutsceneManager.GroupCutsceneState> x) => x.Value == state).Key;
				if (key != null)
				{
					this.activeLocalCutscenes.Remove(key);
				}
				this.playerCutsceneStates.Remove(playedId);
			}
		}

		// Token: 0x06000353 RID: 851 RVA: 0x00011EE0 File Offset: 0x000100E0
		public void EnterPrivateCutscene(Context playerContext, CutsceneCamera cutsceneCamera)
		{
			if (this.activeGlobalCutscene != null)
			{
				return;
			}
			ulong ownerClientId = playerContext.WorldObject.NetworkObject.OwnerClientId;
			if (cutsceneCamera == null)
			{
				return;
			}
			CutsceneManager.GroupCutsceneState groupCutsceneState;
			if (this.playerCutsceneStates.TryGetValue(ownerClientId, out groupCutsceneState))
			{
				if (groupCutsceneState.CutsceneScope == Scope.Private)
				{
					groupCutsceneState.TransitionTo(cutsceneCamera);
					return;
				}
			}
			else
			{
				groupCutsceneState = new CutsceneManager.GroupCutsceneState(cutsceneCamera, Scope.Private, playerContext, new List<ulong> { ownerClientId });
				this.playerCutsceneStates.Add(ownerClientId, groupCutsceneState);
			}
		}

		// Token: 0x06000354 RID: 852 RVA: 0x00011F54 File Offset: 0x00010154
		public void ExitPrivateCutscene(ulong playerId, CutsceneCamera cutsceneCamera)
		{
			if (this.activeGlobalCutscene != null)
			{
				return;
			}
			CutsceneManager.GroupCutsceneState groupCutsceneState;
			if (this.playerCutsceneStates.TryGetValue(playerId, out groupCutsceneState) && groupCutsceneState.CutsceneScope == Scope.Private && groupCutsceneState.CurrentCutsceneCamera == cutsceneCamera)
			{
				groupCutsceneState.End();
				this.playerCutsceneStates.Remove(playerId);
			}
		}

		// Token: 0x06000355 RID: 853 RVA: 0x00011FA4 File Offset: 0x000101A4
		public void EnterGlobalCutscene(CutsceneCamera cutsceneCamera)
		{
			if (cutsceneCamera == null)
			{
				return;
			}
			if (this.activeGlobalCutscene != null)
			{
				this.activeGlobalCutscene.TransitionTo(cutsceneCamera);
				return;
			}
			this.activeGlobalCutscene = new CutsceneManager.GroupCutsceneState(cutsceneCamera, Scope.Global, Game.Instance.GetGameContext(), MonoBehaviourSingleton<PlayerManager>.Instance.GetPlayerIds());
			this.playerCutsceneStates.Clear();
			this.activeLocalCutscenes.Clear();
		}

		// Token: 0x06000356 RID: 854 RVA: 0x00012007 File Offset: 0x00010207
		public void ExitGlobalCutscene(CutsceneCamera cutsceneCamera)
		{
			if (this.activeGlobalCutscene != null && this.activeGlobalCutscene.CurrentCutsceneCamera == cutsceneCamera)
			{
				this.activeGlobalCutscene.End();
				this.activeGlobalCutscene = null;
			}
		}

		// Token: 0x06000357 RID: 855 RVA: 0x00012036 File Offset: 0x00010236
		public bool CheckIfManagedCutsceneExists(Context managingContext)
		{
			return this.activeGlobalCutscene == null && this.activeLocalCutscenes.ContainsKey(managingContext);
		}

		// Token: 0x06000358 RID: 856 RVA: 0x00012050 File Offset: 0x00010250
		public void EnterLocalCutscene(Context context, CutsceneCamera cutsceneCamera, Context manager)
		{
			if (this.activeGlobalCutscene != null)
			{
				return;
			}
			bool flag = context != null && context.IsPlayer();
			ulong num = (flag ? context.WorldObject.NetworkObject.OwnerClientId : 0UL);
			CutsceneManager.GroupCutsceneState groupCutsceneState;
			if (this.activeLocalCutscenes.TryGetValue(manager, out groupCutsceneState))
			{
				if (flag)
				{
					if (!groupCutsceneState.IncludedPlayers.Contains(num))
					{
						groupCutsceneState.AddPlayer(num);
						this.playerCutsceneStates.Add(num, groupCutsceneState);
						return;
					}
					if (cutsceneCamera == null)
					{
						return;
					}
					groupCutsceneState.TransitionTo(cutsceneCamera);
					return;
				}
				else
				{
					if (cutsceneCamera == null)
					{
						return;
					}
					groupCutsceneState.TransitionTo(cutsceneCamera);
					return;
				}
			}
			else
			{
				if (cutsceneCamera == null || !flag)
				{
					return;
				}
				groupCutsceneState = new CutsceneManager.GroupCutsceneState(cutsceneCamera, Scope.Local, manager, new List<ulong> { num });
				this.activeLocalCutscenes.Add(manager, groupCutsceneState);
				this.playerCutsceneStates.Add(num, groupCutsceneState);
				return;
			}
		}

		// Token: 0x06000359 RID: 857 RVA: 0x00012124 File Offset: 0x00010324
		public void ExitLocalCutscene(Context manager)
		{
			if (this.activeGlobalCutscene != null)
			{
				return;
			}
			CutsceneManager.GroupCutsceneState groupCutsceneState;
			if (this.activeLocalCutscenes.TryGetValue(manager, out groupCutsceneState))
			{
				groupCutsceneState.End();
				this.activeLocalCutscenes.Remove(manager);
				foreach (ulong num in groupCutsceneState.IncludedPlayers)
				{
					this.playerCutsceneStates.Remove(num);
				}
			}
		}

		// Token: 0x0600035A RID: 858 RVA: 0x000121AC File Offset: 0x000103AC
		[ClientRpc]
		private void EnterCutscene_ClientRpc(NetworkObjectReference targetController, CutsceneCamera.TransitionInfo transition, InputSettings inputSettings, bool invulnerable, CutsceneManager.InProgressState inProgressState, ClientRpcParams rpcParams = default(ClientRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(638214693U, rpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<NetworkObjectReference>(in targetController, default(FastBufferWriter.ForNetworkSerializable));
				fastBufferWriter.WriteValueSafe<CutsceneCamera.TransitionInfo>(in transition, default(FastBufferWriter.ForNetworkSerializable));
				fastBufferWriter.WriteValueSafe<InputSettings>(in inputSettings, default(FastBufferWriter.ForEnums));
				fastBufferWriter.WriteValueSafe<bool>(in invulnerable, default(FastBufferWriter.ForPrimitives));
				fastBufferWriter.WriteValueSafe<CutsceneManager.InProgressState>(in inProgressState, default(FastBufferWriter.ForNetworkSerializable));
				base.__endSendClientRpc(ref fastBufferWriter, 638214693U, rpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (PlayerReferenceManager.LocalInstance)
			{
				PlayerReferenceManager.LocalInstance.HittableComponent.IsDamageable = !invulnerable;
			}
			NetworkObject networkObject = targetController;
			if (networkObject != null)
			{
				CutsceneCamera componentInChildren = networkObject.GetComponentInChildren<CutsceneCamera>();
				if (componentInChildren)
				{
					MonoBehaviourSingleton<CameraController>.Instance.TransitionToCutsceneCamera(componentInChildren, transition.Type, transition.Duration, inProgressState.FollowInfo, inProgressState.LookAtInfo);
					componentInChildren.JoinInProgress(inProgressState);
					MonoBehaviourSingleton<InputManager>.Instance.SetCinematicInputState(true);
					this.CurrentCutsceneInputSettings = inputSettings;
				}
				if (this.activeExitCutsceneCoroutine != null)
				{
					base.StopCoroutine(this.activeExitCutsceneCoroutine);
				}
				this.activeExitCutsceneCoroutine = null;
			}
		}

		// Token: 0x0600035B RID: 859 RVA: 0x000123A0 File Offset: 0x000105A0
		[ClientRpc]
		private void ExitCutscene_ClientRpc(CutsceneCamera.TransitionInfo transition, ClientRpcParams rpcParams = default(ClientRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(3565536771U, rpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<CutsceneCamera.TransitionInfo>(in transition, default(FastBufferWriter.ForNetworkSerializable));
				base.__endSendClientRpc(ref fastBufferWriter, 3565536771U, rpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (this.activeExitCutsceneCoroutine != null)
			{
				base.StopCoroutine(this.activeExitCutsceneCoroutine);
			}
			this.activeExitCutsceneCoroutine = base.StartCoroutine(this.ExitCutsceneCoroutine(transition));
		}

		// Token: 0x0600035C RID: 860 RVA: 0x000124B5 File Offset: 0x000106B5
		private IEnumerator ExitCutsceneCoroutine(CutsceneCamera.TransitionInfo transition)
		{
			MonoBehaviourSingleton<CameraController>.Instance.ExitCutscene(transition.Type, transition.Duration);
			yield return new WaitForSeconds(transition.Duration);
			if (PlayerReferenceManager.LocalInstance)
			{
				PlayerReferenceManager.LocalInstance.HittableComponent.IsDamageable = true;
			}
			MonoBehaviourSingleton<InputManager>.Instance.SetCinematicInputState(false);
			yield break;
		}

		// Token: 0x0600035D RID: 861 RVA: 0x000124C4 File Offset: 0x000106C4
		public void EndlessGameEnd()
		{
			MonoBehaviourSingleton<InputManager>.Instance.SetCinematicInputState(false);
			if (base.IsServer)
			{
				if (this.activeGlobalCutscene != null)
				{
					this.activeGlobalCutscene.ResetDamageable();
				}
				foreach (KeyValuePair<ulong, CutsceneManager.GroupCutsceneState> keyValuePair in this.playerCutsceneStates)
				{
					keyValuePair.Value.ResetDamageable();
				}
				this.activeGlobalCutscene = null;
				this.playerCutsceneStates.Clear();
				this.activeLocalCutscenes.Clear();
				base.StopAllCoroutines();
				this.activeExitCutsceneCoroutine = null;
			}
		}

		// Token: 0x0600035F RID: 863 RVA: 0x0001258C File Offset: 0x0001078C
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x06000360 RID: 864 RVA: 0x000125A4 File Offset: 0x000107A4
		protected override void __initializeRpcs()
		{
			base.__registerRpc(638214693U, new NetworkBehaviour.RpcReceiveHandler(CutsceneManager.__rpc_handler_638214693), "EnterCutscene_ClientRpc");
			base.__registerRpc(3565536771U, new NetworkBehaviour.RpcReceiveHandler(CutsceneManager.__rpc_handler_3565536771), "ExitCutscene_ClientRpc");
			base.__initializeRpcs();
		}

		// Token: 0x06000361 RID: 865 RVA: 0x000125F4 File Offset: 0x000107F4
		private static void __rpc_handler_638214693(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			NetworkObjectReference networkObjectReference;
			reader.ReadValueSafe<NetworkObjectReference>(out networkObjectReference, default(FastBufferWriter.ForNetworkSerializable));
			CutsceneCamera.TransitionInfo transitionInfo;
			reader.ReadValueSafe<CutsceneCamera.TransitionInfo>(out transitionInfo, default(FastBufferWriter.ForNetworkSerializable));
			InputSettings inputSettings;
			reader.ReadValueSafe<InputSettings>(out inputSettings, default(FastBufferWriter.ForEnums));
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			CutsceneManager.InProgressState inProgressState;
			reader.ReadValueSafe<CutsceneManager.InProgressState>(out inProgressState, default(FastBufferWriter.ForNetworkSerializable));
			ClientRpcParams client = rpcParams.Client;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((CutsceneManager)target).EnterCutscene_ClientRpc(networkObjectReference, transitionInfo, inputSettings, flag, inProgressState, client);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06000362 RID: 866 RVA: 0x000126F0 File Offset: 0x000108F0
		private static void __rpc_handler_3565536771(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			CutsceneCamera.TransitionInfo transitionInfo;
			reader.ReadValueSafe<CutsceneCamera.TransitionInfo>(out transitionInfo, default(FastBufferWriter.ForNetworkSerializable));
			ClientRpcParams client = rpcParams.Client;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((CutsceneManager)target).ExitCutscene_ClientRpc(transitionInfo, client);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06000363 RID: 867 RVA: 0x0001276E File Offset: 0x0001096E
		protected internal override string __getTypeName()
		{
			return "CutsceneManager";
		}

		// Token: 0x04000320 RID: 800
		private Dictionary<Context, CutsceneManager.GroupCutsceneState> activeLocalCutscenes = new Dictionary<Context, CutsceneManager.GroupCutsceneState>();

		// Token: 0x04000321 RID: 801
		private Dictionary<ulong, CutsceneManager.GroupCutsceneState> playerCutsceneStates = new Dictionary<ulong, CutsceneManager.GroupCutsceneState>();

		// Token: 0x04000322 RID: 802
		private CutsceneManager.GroupCutsceneState activeGlobalCutscene;

		// Token: 0x04000323 RID: 803
		private Coroutine activeExitCutsceneCoroutine;

		// Token: 0x020000BA RID: 186
		public struct InProgressState : INetworkSerializable
		{
			// Token: 0x06000364 RID: 868 RVA: 0x00012775 File Offset: 0x00010975
			public InProgressState(bool lateJoin, float shotDuration, CutsceneCamera.TargetInfo follow, CutsceneCamera.TargetInfo lookAt, CutsceneCamera.MoveToInfo moveToInfo, double startTime = 0.0)
			{
				this.LateJoin = lateJoin;
				this.StartTime = startTime;
				this.ShotDuration = shotDuration;
				this.FollowInfo = follow;
				this.LookAtInfo = lookAt;
				this.MoveToInfo = moveToInfo;
			}

			// Token: 0x06000365 RID: 869 RVA: 0x000127A4 File Offset: 0x000109A4
			public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
			{
				serializer.SerializeValue<bool>(ref this.LateJoin, default(FastBufferWriter.ForPrimitives));
				serializer.SerializeValue<float>(ref this.ShotDuration, default(FastBufferWriter.ForPrimitives));
				serializer.SerializeValue<CutsceneCamera.TargetInfo>(ref this.FollowInfo, default(FastBufferWriter.ForNetworkSerializable));
				serializer.SerializeValue<CutsceneCamera.TargetInfo>(ref this.LookAtInfo, default(FastBufferWriter.ForNetworkSerializable));
				serializer.SerializeValue<CutsceneCamera.MoveToInfo>(ref this.MoveToInfo, default(FastBufferWriter.ForNetworkSerializable));
				if (this.LateJoin)
				{
					serializer.SerializeValue<double>(ref this.StartTime, default(FastBufferWriter.ForPrimitives));
				}
			}

			// Token: 0x04000325 RID: 805
			public bool LateJoin;

			// Token: 0x04000326 RID: 806
			public double StartTime;

			// Token: 0x04000327 RID: 807
			public float ShotDuration;

			// Token: 0x04000328 RID: 808
			public CutsceneCamera.TargetInfo FollowInfo;

			// Token: 0x04000329 RID: 809
			public CutsceneCamera.TargetInfo LookAtInfo;

			// Token: 0x0400032A RID: 810
			public CutsceneCamera.MoveToInfo MoveToInfo;
		}

		// Token: 0x020000BB RID: 187
		private class GroupCutsceneState
		{
			// Token: 0x17000090 RID: 144
			// (get) Token: 0x06000366 RID: 870 RVA: 0x0001283D File Offset: 0x00010A3D
			// (set) Token: 0x06000367 RID: 871 RVA: 0x00012845 File Offset: 0x00010A45
			public Scope CutsceneScope { get; private set; }

			// Token: 0x17000091 RID: 145
			// (get) Token: 0x06000368 RID: 872 RVA: 0x0001284E File Offset: 0x00010A4E
			// (set) Token: 0x06000369 RID: 873 RVA: 0x00012856 File Offset: 0x00010A56
			public CutsceneCamera CurrentCutsceneCamera { get; private set; }

			// Token: 0x17000092 RID: 146
			// (get) Token: 0x0600036A RID: 874 RVA: 0x0001285F File Offset: 0x00010A5F
			// (set) Token: 0x0600036B RID: 875 RVA: 0x00012867 File Offset: 0x00010A67
			public Context ManagingContext { get; private set; }

			// Token: 0x0600036C RID: 876 RVA: 0x00012870 File Offset: 0x00010A70
			public GroupCutsceneState(CutsceneCamera cutsceneCamera, Scope scope, Context managingContext, List<ulong> startingPlayers)
			{
				this.CutsceneScope = scope;
				this.InputSettings = cutsceneCamera.InputAllowed;
				this.Invulnerable = cutsceneCamera.InvulnerablePlayer;
				this.ManagingContext = managingContext;
				this.ShotStartTime = NetworkManager.Singleton.ServerTime.Time;
				foreach (ulong num in startingPlayers)
				{
					uint num2 = (uint)num;
					this.SetupPlayer((ulong)num2);
				}
				this.TransitionTo(cutsceneCamera);
			}

			// Token: 0x0600036D RID: 877 RVA: 0x00012918 File Offset: 0x00010B18
			public void AddPlayer(ulong playerId)
			{
				this.SetupPlayer(playerId);
				ClientRpcParams clientRpcParams = new ClientRpcParams
				{
					Send = new ClientRpcSendParams
					{
						TargetClientIds = new ulong[] { playerId }
					}
				};
				NetworkBehaviourSingleton<CutsceneManager>.Instance.EnterCutscene_ClientRpc(this.CurrentCutsceneCamera.NetworkObject, this.mostRecentTransition, this.InputSettings, this.Invulnerable, new CutsceneManager.InProgressState(true, this.CurrentCutsceneCamera.BaseShotDuration, this.followInfo, this.lookAtInfo, this.CurrentCutsceneCamera.MoveToInfo_Server, this.ShotStartTime), clientRpcParams);
			}

			// Token: 0x0600036E RID: 878 RVA: 0x000129B2 File Offset: 0x00010BB2
			private void SetupPlayer(ulong playerId)
			{
				this.IncludedPlayers.Add(playerId);
				MonoBehaviourSingleton<PlayerManager>.Instance.GetPlayerObject(playerId).HittableComponent.IsDamageable = !this.Invulnerable;
			}

			// Token: 0x0600036F RID: 879 RVA: 0x000129E0 File Offset: 0x00010BE0
			public void TransitionTo(CutsceneCamera cutsceneCamera)
			{
				this.ResetDamageable();
				if (!MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying)
				{
					return;
				}
				if (this.CurrentCutsceneCamera && !this.pendingInternalEnd)
				{
					this.pendingInternalEnd = true;
					this.CurrentCutsceneCamera.OnShotInterrupted.Invoke(this.ManagingContext);
				}
				this.followInfo = cutsceneCamera.CurrentFollowInfo_Server;
				this.lookAtInfo = cutsceneCamera.CurrentLookAtInfo_Server;
				this.mostRecentTransition = cutsceneCamera.TransitionIn_Server;
				this.InputSettings = cutsceneCamera.InputAllowed;
				this.Invulnerable = cutsceneCamera.InvulnerablePlayer;
				this.CurrentCutsceneCamera = cutsceneCamera;
				this.ShotStartTime = NetworkManager.Singleton.ServerTime.Time;
				this.pendingInternalEnd = false;
				if (this.transitionTimerCoroutine != null)
				{
					NetworkBehaviourSingleton<CutsceneManager>.Instance.StopCoroutine(this.transitionTimerCoroutine);
					this.transitionTimerCoroutine = null;
				}
				this.transitionTimerCoroutine = NetworkBehaviourSingleton<CutsceneManager>.Instance.StartCoroutine(this.TransitionTimerCoroutine(this.mostRecentTransition, cutsceneCamera.TotalShotDuration));
				ClientRpcParams clientRpcParams = new ClientRpcParams
				{
					Send = new ClientRpcSendParams
					{
						TargetClientIds = this.IncludedPlayers
					}
				};
				NetworkBehaviourSingleton<CutsceneManager>.Instance.EnterCutscene_ClientRpc(cutsceneCamera.NetworkObject, this.mostRecentTransition, this.InputSettings, this.Invulnerable, new CutsceneManager.InProgressState(false, this.CurrentCutsceneCamera.BaseShotDuration, this.followInfo, this.lookAtInfo, cutsceneCamera.MoveToInfo_Server, 0.0), clientRpcParams);
			}

			// Token: 0x06000370 RID: 880 RVA: 0x00012B50 File Offset: 0x00010D50
			public void ResetDamageable()
			{
				foreach (ulong num in this.IncludedPlayers)
				{
					MonoBehaviourSingleton<PlayerManager>.Instance.GetPlayerObject(num).HittableComponent.IsDamageable = true;
				}
			}

			// Token: 0x06000371 RID: 881 RVA: 0x00012BB4 File Offset: 0x00010DB4
			public void End()
			{
				if (!MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying)
				{
					return;
				}
				if (this.CurrentCutsceneCamera && !this.pendingInternalEnd)
				{
					this.pendingInternalEnd = true;
					this.CurrentCutsceneCamera.OnShotInterrupted.Invoke(this.ManagingContext);
				}
				this.ResetDamageable();
				ClientRpcParams clientRpcParams = new ClientRpcParams
				{
					Send = new ClientRpcSendParams
					{
						TargetClientIds = this.IncludedPlayers
					}
				};
				NetworkBehaviourSingleton<CutsceneManager>.Instance.ExitCutscene_ClientRpc(this.CurrentCutsceneCamera.TransitionOut_Server, clientRpcParams);
			}

			// Token: 0x06000372 RID: 882 RVA: 0x00012C43 File Offset: 0x00010E43
			private IEnumerator TransitionTimerCoroutine(CutsceneCamera.TransitionInfo transitionInfo, float shotDuration)
			{
				if (transitionInfo.Type == CameraTransition.Fade)
				{
					float halfDur = transitionInfo.Duration / 2f;
					yield return new WaitForSeconds(halfDur);
					this.CurrentCutsceneCamera.OnShotStarted.Invoke(this.ManagingContext);
					yield return new WaitForSeconds(halfDur);
				}
				else if (transitionInfo.Type == CameraTransition.Cut)
				{
					this.CurrentCutsceneCamera.OnShotStarted.Invoke(this.ManagingContext);
				}
				else
				{
					yield return new WaitForSeconds(transitionInfo.Duration);
					this.CurrentCutsceneCamera.OnShotStarted.Invoke(this.ManagingContext);
				}
				if (shotDuration >= 0f)
				{
					yield return new WaitForSeconds(shotDuration);
					if (!this.pendingInternalEnd)
					{
						this.pendingInternalEnd = true;
						this.CurrentCutsceneCamera.OnShotFinished.Invoke(this.ManagingContext);
					}
					yield return null;
					Scope cutsceneScope = this.CutsceneScope;
					if (cutsceneScope != Scope.Local)
					{
						if (cutsceneScope == Scope.Global)
						{
							NetworkBehaviourSingleton<CutsceneManager>.Instance.ExitGlobalCutscene(this.CurrentCutsceneCamera);
						}
						else if (this.IncludedPlayers.Count > 0)
						{
							NetworkBehaviourSingleton<CutsceneManager>.Instance.ExitPrivateCutscene(this.IncludedPlayers[0], this.CurrentCutsceneCamera);
						}
					}
					else
					{
						NetworkBehaviourSingleton<CutsceneManager>.Instance.ExitLocalCutscene(this.ManagingContext);
					}
				}
				this.transitionTimerCoroutine = null;
				yield break;
			}

			// Token: 0x0400032E RID: 814
			public List<ulong> IncludedPlayers = new List<ulong>();

			// Token: 0x0400032F RID: 815
			public InputSettings InputSettings;

			// Token: 0x04000330 RID: 816
			public bool Invulnerable;

			// Token: 0x04000331 RID: 817
			private CutsceneCamera.TransitionInfo mostRecentTransition;

			// Token: 0x04000332 RID: 818
			private double ShotStartTime;

			// Token: 0x04000333 RID: 819
			private bool pendingInternalEnd;

			// Token: 0x04000334 RID: 820
			private CutsceneCamera.TargetInfo followInfo;

			// Token: 0x04000335 RID: 821
			private CutsceneCamera.TargetInfo lookAtInfo;

			// Token: 0x04000336 RID: 822
			private Coroutine transitionTimerCoroutine;
		}
	}
}
